using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class simulationManager : MonoBehaviour
{
    //DRAW ASSETS
    public ComputeShader particleUpdater;
    public Material smokemat;
    public Mesh quadmesh;

    public byteLoader pman;
    ComputeBuffer pbuf;
    public byteLoader sman;
    ComputeBuffer sbuf;

    ComputeBuffer smokepositionBuffer;
    ComputeBuffer colorBuffer;

    public string gridxFile;
    public string gridyFile;
    public string gridzFile;

    ComputeBuffer gridx;
    ComputeBuffer gridy;
    ComputeBuffer gridz;

    //PARAMETERS
    public int nparticles;
    public float particleSize;
    public GameObject cam;

    //OTHER
    private int index;
    private Material mat;
    private GraphicsBuffer commandBuf;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    public bool order;

    void Start()
    {
        smokepositionBuffer = new ComputeBuffer(nparticles, sizeof(float) * 3);
        colorBuffer = new ComputeBuffer(nparticles, sizeof(float) * 4);

        pman.initialize(nparticles*4);
        pbuf = new ComputeBuffer(nparticles, sizeof(int), ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        pbuf.SetData(pman.getData());
        particleUpdater.SetBuffer(0, "pbuffer", pbuf);

        sman.initialize(nparticles*4);
        sbuf = new ComputeBuffer(nparticles, 4, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        sbuf.SetData(sman.getData());
        particleUpdater.SetBuffer(0, "sbuffer", sbuf);

        index = pman.getNbytes()/4;

        particleUpdater.SetBuffer(0, "positions", smokepositionBuffer);
        particleUpdater.SetBuffer(0, "colors", colorBuffer);

        mat = new Material(smokemat);
        mat.SetBuffer("positionbuffer", smokepositionBuffer);
        mat.SetBuffer("colorbuffer", colorBuffer);
        mat.SetFloat("particleSize", particleSize);

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
    }

    // Update is called once per frame
    void Update()
    { 
        if(pman.checkTimeInterval()) {
            pbuf.SetData(pman.getData());
            particleUpdater.SetBuffer(0, "pbuffer", pbuf);
        }

        if(sman.checkTimeInterval()) {
            sbuf.SetData(sman.getData());
            particleUpdater.SetBuffer(0, "sbuffer", sbuf);
        }

        index = pman.getNbytes()/4;
        updateParticles();
        paintParticlesInstanced();
    }


    private void updateParticles()
    {
        //particleUpdater.SetFloat("tempfactor", temperatureManager.getInterpolationFactor());
        //particleUpdater.SetFloat("velfactor", velocityManager.getInterpolationFactor());
        //particleUpdater.SetFloat("smokfactor", smokeManager.getInterpolationFactor());
        //particleUpdater.SetFloat("deltaTime", Time.deltaTime);
        particleUpdater.SetInt("nparticles", index);

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
        
        mat.SetVector("camposition", cam.transform.position);
        mat.SetInt("nparts", index);
        if(order) mat.SetInt("order",-1);
        else mat.SetInt("order",1);
        RenderParams rp = new RenderParams(mat);
        //rp.worldBounds = new Bounds(-10000*Vector3.one, 10000*Vector3.one); // use tighter bounds for better FOV culling
        //rp.matProps = new MaterialPropertyBlock();
        //rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Scale(new Vector3(particleSize, particleSize, particleSize)));
        Graphics.RenderMeshIndirect(rp, quadmesh, commandBuf, 1);
        
        
    }

    void OnDestroy(){
        smokepositionBuffer.Release();
        colorBuffer.Release();
        commandBuf.Release();
        pbuf.Release();
        sbuf.Release();
        gridx.Release();
        gridy.Release();
        gridz.Release();
    }
}
