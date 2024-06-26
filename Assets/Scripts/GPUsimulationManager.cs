using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GPUsimulationManager : MonoBehaviour
{
    //DATA MANAGERS
    public floatLoader temperatureManager;
    public floatLoader smokeManager;
    public vectorLoader velocityManager;

    //DRAW ASSETS
    public ComputeShader particleUpdater;
    public Material smokemat;
    public Mesh quadmesh;
    //Matrix4x4[] instData;

    //BUFFERS
    ComputeBuffer temperatureBuffer1;
    ComputeBuffer temperatureBuffer2;
    ComputeBuffer temperatureBuffer3;

    ComputeBuffer smokeBuffer1;
    ComputeBuffer smokeBuffer2;
    ComputeBuffer smokeBuffer3;

    ComputeBuffer velocityBuffer1;
    ComputeBuffer velocityBuffer2;
    ComputeBuffer velocityBuffer3;

    ComputeBuffer smokepositionBuffer;
    ComputeBuffer colorBuffer;

    //PARAMETERS
    public int nparticles;
    public float particleSize;
    public GameObject cam;
    public int dimx, dimy, dimz;

    //OTHER
    private int index;
    private Material mat;
    private GraphicsBuffer commandBuf;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;

    //TESTING
    public byteLoader pman;
    ComputeBuffer pbuf;
    public byteLoader sman;
    ComputeBuffer sbuf;
    public byteLoader hman;
    ComputeBuffer hbuf;

    void Start()
    {
        
        //instData = new Matrix4x4[512];
        //for (int i = 0; i < 512; ++i) instData[i] = Matrix4x4.Scale(new Vector3(particleSize, particleSize, particleSize));
        /*
        temperatureBuffer1 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float),ComputeBufferType.Default ,ComputeBufferMode.Dynamic);
        temperatureBuffer2 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float),ComputeBufferType.Default ,ComputeBufferMode.Dynamic);
        temperatureBuffer3 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float),ComputeBufferType.Default ,ComputeBufferMode.Dynamic);

        smokeBuffer1 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float),ComputeBufferType.Default ,ComputeBufferMode.Dynamic);
        smokeBuffer2 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float),ComputeBufferType.Default ,ComputeBufferMode.Dynamic);
        smokeBuffer3 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float),ComputeBufferType.Default ,ComputeBufferMode.Dynamic);

        velocityBuffer1 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float) * 3, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        velocityBuffer2 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float) * 3, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        velocityBuffer3 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float) * 3, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        */
        smokepositionBuffer = new ComputeBuffer(700000, sizeof(float) * 3);
        colorBuffer = new ComputeBuffer(700000, sizeof(float) * 4);

        particleUpdater.SetInts("dimensions", new int[3] { dimx, dimy, dimz });
        /*
        temperatureManager.initialize();
        temperatureBuffer1.SetData(temperatureManager.getData());
        temperatureBuffer2.SetData(temperatureManager.getNextData());
        temperatureBuffer3.SetData(temperatureManager.getThirdData());

        smokeManager.initialize();
        smokeBuffer1.SetData(smokeManager.getData());
        smokeBuffer2.SetData(smokeManager.getNextData());
        smokeBuffer3.SetData(smokeManager.getThirdData());

        velocityManager.initialize();
        velocityBuffer1.SetData(velocityManager.getData());
        velocityBuffer2.SetData(velocityManager.getNextData());
        velocityBuffer3.SetData(velocityManager.getThirdData());
        */
        pman.initialize(700000*4);
        pbuf = new ComputeBuffer(700000, sizeof(int), ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        pbuf.SetData(pman.getData());
        particleUpdater.SetBuffer(0, "pbuffer", pbuf);

        sman.initialize(700000*4);
        sbuf = new ComputeBuffer(700000, 4, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        sbuf.SetData(sman.getData());
        particleUpdater.SetBuffer(0, "sbuffer", sbuf);

        hman.initialize(700000*4);
        hbuf = new ComputeBuffer(700000, 4, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
        hbuf.SetData(hman.getData());
        particleUpdater.SetBuffer(0, "hbuffer", hbuf);

        initBuffers();

        particleUpdater.SetBuffer(0, "positions", smokepositionBuffer);
        particleUpdater.SetBuffer(0, "colors", colorBuffer);

        mat = new Material(smokemat);
        mat.SetBuffer("positionbuffer", smokepositionBuffer);
        mat.SetBuffer("colorbuffer", colorBuffer);
        mat.SetFloat("particleSize", particleSize);

        commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        commandData[0].indexCountPerInstance = quadmesh.GetIndexCount(0);

        
    }

    // Update is called once per frame
    void Update()
    {
        bool redoBuffers = false;
        /*
        if (temperatureManager.checkTimeInterval())
        {
            ComputeBuffer aux = temperatureBuffer1;
            temperatureBuffer1 = temperatureBuffer2;
            temperatureBuffer2 = temperatureBuffer3;
            temperatureBuffer3 = aux;
            temperatureBuffer3.SetData(temperatureManager.getThirdData());
        }

        if (smokeManager.checkTimeInterval())
        {
            ComputeBuffer aux = smokeBuffer1;
            smokeBuffer1 = smokeBuffer2;
            smokeBuffer2 = smokeBuffer3;
            smokeBuffer3 = aux;
            smokeBuffer3.SetData(smokeManager.getThirdData());
            redoBuffers = true;
        }

        if (velocityManager.checkTimeInterval())
        {
            ComputeBuffer aux = velocityBuffer1;
            velocityBuffer1 = velocityBuffer2;
            velocityBuffer2 = velocityBuffer3;
            velocityBuffer3 = aux;
            velocityBuffer3.SetData(velocityManager.getThirdData());
        }
        */
        if(pman.checkTimeInterval()) {
            pbuf.SetData(pman.getData());
            particleUpdater.SetBuffer(0, "pbuffer", pbuf);
            redoBuffers = true;
        }

        if(sman.checkTimeInterval()) {
            sbuf.SetData(sman.getData());
            particleUpdater.SetBuffer(0, "sbuffer", sbuf);
        }

        if(hman.checkTimeInterval()) {
            double d = Time.realtimeSinceStartupAsDouble;
            hbuf.SetData(hman.getData());
            d = Time.realtimeSinceStartupAsDouble - d;
            particleUpdater.SetBuffer(0, "hbuffer", hbuf);
            Debug.Log(d);
        }

        if (redoBuffers) initBuffers();
        updateParticles();
        paintParticlesInstanced();
    }

    private void initBuffers()
    {
        index = pman.getNbytes()/4; return;
        /*Vector3[] smokepositions = new Vector3[nparticles];
        float[] densities = smokeManager.getData();
        smokepositions[0] = new Vector3(0.0f,0.0f,0.0f);
        index = 1;
        for (int z = dimz-1; z > -1; z -= 3) for (int y = 0; y < dimy && index < nparticles; y += 3) for (int x = 0; x < dimx; x += 3)
                {
                    if (index < nparticles && densities[z * dimy * dimx + y * dimx + x] > 9)
                    {
                        smokepositions[index] = new Vector3((float)x, (float)y, (float)z) / 100.0f;
                        ++index;
                    }
                }
        smokepositionBuffer.SetData(smokepositions);
        Debug.Log(index);*/
    }

    private void updateParticles()
    {
    /*
        particleUpdater.SetBuffer(0, "temps1", temperatureBuffer1);
        particleUpdater.SetBuffer(0, "temps2", temperatureBuffer2);
        particleUpdater.SetBuffer(0, "vel1", velocityBuffer1);
        particleUpdater.SetBuffer(0, "vel2", velocityBuffer2);
        particleUpdater.SetBuffer(0, "smok1", smokeBuffer1);
        particleUpdater.SetBuffer(0, "smok2", smokeBuffer2);
        */
        //particleUpdater.SetFloat("tempfactor", temperatureManager.getInterpolationFactor());
        //particleUpdater.SetFloat("velfactor", velocityManager.getInterpolationFactor());
        //particleUpdater.SetFloat("smokfactor", smokeManager.getInterpolationFactor());
        particleUpdater.SetFloat("deltaTime", Time.deltaTime);
        particleUpdater.SetInt("nparticles", index);

        uint x, y, z;
        particleUpdater.GetKernelThreadGroupSizes(0, out x, out y, out z);
        int threadsx = (int)(index / (x*y*z));
        if (index % (x * y * z) > 0) threadsx++;
        if (threadsx > 0) particleUpdater.Dispatch(0, threadsx, 1, 1);

    }

    private void paintParticlesInstanced()
    {
        /*
        Material[] smokematerials = new Material[(int)index/511 + 1];
        for (int i = 0; i*511 < index; i++)
        {
            smokematerials[i] = new Material(smokemat);
            smokematerials[i].SetInt("offset", i * 511);
            smokematerials[i].SetBuffer("positionbuffer", smokepositionBuffer);
            smokematerials[i].SetBuffer("colorbuffer", colorBuffer);
            smokematerials[i].SetVector("camposition", cam.transform.position);
            Graphics.RenderMeshInstanced(new RenderParams(smokematerials[i]), quadmesh, 0, instData, 512, 0);
        }*/
        
        commandData[0].instanceCount = (uint)index;
        commandBuf.SetData(commandData);
        
        mat.SetVector("camposition", cam.transform.position);
        RenderParams rp = new RenderParams(mat);
        //rp.worldBounds = new Bounds(-10000*Vector3.one, 10000*Vector3.one); // use tighter bounds for better FOV culling
        //rp.matProps = new MaterialPropertyBlock();
        //rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Scale(new Vector3(particleSize, particleSize, particleSize)));
        Graphics.RenderMeshIndirect(rp, quadmesh, commandBuf, 1);
        
        
    }

    void OnDestroy(){
        temperatureBuffer1.Release();
        temperatureBuffer2.Release();
        temperatureBuffer3.Release();

        smokeBuffer1.Release();
        smokeBuffer2.Release();
        smokeBuffer3.Release();

        velocityBuffer1.Release();
        velocityBuffer2.Release();
        velocityBuffer3.Release();

        smokepositionBuffer.Release();
        colorBuffer.Release();
        commandBuf.Release();
    }
}
