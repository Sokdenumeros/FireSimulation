using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class simulationManager : MonoBehaviour
{
    //Compute shader
    public ComputeShader particleUpdater;

    //Material and mesh to use for the particles
    public Material smokemat;
    public Mesh quadmesh;

    //This class and buffers store the particle positions in the grid (index)
    public byteLoader pman;
    ComputeBuffer pbuf;
    ComputeBuffer pbuf2;

    //This class and buffers store the particle status (HRPVU and soot_density)
    public byteLoader sman;
    ComputeBuffer sbuf;
    ComputeBuffer sbuf2;
    
    //Things that need to be sent to the GPU are queued so we can distribute them along several frames.
    Queue<int> GPUqueue = new Queue<int>();

    //World coordinates of the particles, this buffer is filled in a compute shader
    ComputeBuffer smokepositionBuffer;
    //RGB color of the particles, this buffer is filled in a compute shader
    ComputeBuffer colorBuffer;

    //These buffers store the mapping from grid indices to world coordinates
    ComputeBuffer gridx;
    ComputeBuffer gridy;
    ComputeBuffer gridz;

    //Path to the files containing the mapping from grid indices to world coordinates
    public string gridxFile;
    public string gridyFile;
    public string gridzFile;

    //PARAMETERS
    public int nparticles;      //Max number of particles to load
    public float particleSize;  //Scale factor for the quad mesh
    public float smokeparticleSize; // Scale factor for the smoke particles
    public float opacityfactor; //Multiplies the opacity of the particle
    public float tfmin;         //min value of the heat transfer function interval
    public float tfmax;         //max value of the heat transfer function interval
    public GameObject cam;      //camera object, the particles will face in the opposite direction of this object
    public bool order;          //Order to draw the particles, 0 - n or n - 0.

    //Light manager
    public LightManager lman;

    //Number of particles that have been loaded / need to be drawn
    private int index;
    
    //OTHER
    private Material mat;
    private GraphicsBuffer commandBuf;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;

    void Start()
    {
        smokepositionBuffer = new ComputeBuffer(nparticles, sizeof(float) * 3);
        colorBuffer = new ComputeBuffer(nparticles, sizeof(float) * 4);

        pman.initialize(nparticles*4);
        pbuf = new ComputeBuffer(nparticles, sizeof(int), ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        pbuf.SetData(pman.getData());
        particleUpdater.SetBuffer(0, "pbuffer", pbuf);

        pbuf2 = new ComputeBuffer(nparticles, 4, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        pbuf2.SetData(pman.getNextData());

        sman.initialize(nparticles*4);
        sbuf = new ComputeBuffer(nparticles, 4, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        sbuf.SetData(sman.getData());
        particleUpdater.SetBuffer(0, "sbuffer", sbuf);

        sbuf2 = new ComputeBuffer(nparticles, 4, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        sbuf2.SetData(sman.getNextData());

        //lman.init(pbuf,sbuf);

        index = pman.getNbytes()/4;

        particleUpdater.SetBuffer(0, "positions", smokepositionBuffer);
        particleUpdater.SetBuffer(0, "colors", colorBuffer);

        mat = new Material(smokemat);
        mat.SetBuffer("positionbuffer", smokepositionBuffer);
        mat.SetBuffer("colorbuffer", colorBuffer);
        mat.SetFloat("particleSize", particleSize);
        mat.SetFloat("smokeparticleSize", smokeparticleSize);

        commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        commandData[0].indexCountPerInstance = quadmesh.GetIndexCount(0);

        setupGrid();
    }

    void setupGrid(){
        byte[] gridData = new byte[10000];
        int nbytesx, nbytesy, nbytesz;

        using (BinaryReader reader = new BinaryReader(File.Open(gridxFile, FileMode.Open))) {
            nbytesx = reader.Read(gridData, 0, gridData.Length);
            gridx = new ComputeBuffer(nbytesx/4, sizeof(float));
            gridx.SetData(gridData,0,0,nbytesx);
            particleUpdater.SetBuffer(0, "gridx", gridx);
        }

        using (BinaryReader reader = new BinaryReader(File.Open(gridyFile, FileMode.Open))) {
            nbytesy = reader.Read(gridData, 0, gridData.Length);
            gridy = new ComputeBuffer(nbytesy/4, sizeof(float));
            gridy.SetData(gridData,0,0,nbytesy);
            particleUpdater.SetBuffer(0, "gridy", gridy);
        }

        using (BinaryReader reader = new BinaryReader(File.Open(gridzFile, FileMode.Open))) {
            nbytesz = reader.Read(gridData, 0, gridData.Length);
            gridz = new ComputeBuffer(nbytesz/4, sizeof(float));
            gridz.SetData(gridData,0,0,nbytesz);
            particleUpdater.SetBuffer(0, "gridz", gridz);
        }

        particleUpdater.SetInts("dimensions", new int[3] { nbytesx/4, nbytesy/4, nbytesz/4});
        //lman.setDimensions(nbytesx/4, nbytesy/4, nbytesz/4);
    }

    // Update is called once per frame
    void Update()
    { 
        if(pman.checkTimeInterval()) {
            ComputeBuffer aux = pbuf;
            pbuf = pbuf2;
            pbuf2 = aux;
            particleUpdater.SetBuffer(0, "pbuffer", pbuf);
            GPUqueue.Enqueue(0);
        }
        if(sman.checkTimeInterval()) {
            ComputeBuffer aux = sbuf;
            sbuf = sbuf2;
            sbuf2 = aux;
            particleUpdater.SetBuffer(0, "sbuffer", sbuf);
            GPUqueue.Enqueue(1);
        }

        if(GPUqueue.Count > 0){
            int actionid = GPUqueue.Dequeue();
            if (actionid == 0) pbuf2.SetData(pman.getNextData());
            else if(actionid == 1) sbuf2.SetData(sman.getNextData());
        }
        while(GPUqueue.Count > 2){
            int actionid = GPUqueue.Dequeue();
            if (actionid == 0) pbuf2.SetData(pman.getNextData());
            else if(actionid == 1) sbuf2.SetData(sman.getNextData());
        }

        index = pman.getNbytes()/4;
        //lman.updateLights(pbuf,sbuf,index);
        updateParticles();
        paintParticlesInstanced();
    }


    private void updateParticles()
    {
        particleUpdater.SetInt("nparticles", index);
        particleUpdater.SetFloat("interpfactor",sman.getInterpolationFactor());
        particleUpdater.SetFloat("opacityfactor",opacityfactor);
        particleUpdater.SetFloat("tfmax",tfmax);
        particleUpdater.SetFloat("tfmin",tfmin);

        uint x, y, z;
        particleUpdater.GetKernelThreadGroupSizes(0, out x, out y, out z);
        int threadsx = (int)(index / (x*y*z));
        if (index % (x * y * z) > 0) threadsx++;
        if (threadsx > 0) particleUpdater.Dispatch(0, threadsx, 1, 1);
    }

    private void paintParticlesInstanced()
    {
        commandData[0].instanceCount = (uint)index;
        commandBuf.SetData(commandData);
        
        mat.SetVector("fw", cam.transform.forward);
        mat.SetInt("nparts", index);
        if(order) mat.SetInt("order",-1);
        else mat.SetInt("order",1);
        mat.SetFloat("particleSize", particleSize);
        mat.SetFloat("smokeparticleSize", smokeparticleSize);
        RenderParams rp = new RenderParams(mat);
        Graphics.RenderMeshIndirect(rp, quadmesh, commandBuf, 1);
        
        
    }

    void OnDestroy(){
        smokepositionBuffer.Release();
        colorBuffer.Release();
        commandBuf.Release();
        pbuf.Release();
        pbuf2.Release();
        sbuf.Release();
        sbuf2.Release();
        gridx.Release();
        gridy.Release();
        gridz.Release();
    }
}
