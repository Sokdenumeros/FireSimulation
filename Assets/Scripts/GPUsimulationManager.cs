using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    ComputeBuffer smokeBuffer1;
    ComputeBuffer smokeBuffer2;

    ComputeBuffer velocityBuffer1;
    ComputeBuffer velocityBuffer2;

    ComputeBuffer positionBuffer;
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

    void Start()
    {
        
        //instData = new Matrix4x4[512];
        //for (int i = 0; i < 512; ++i) instData[i] = Matrix4x4.Scale(new Vector3(particleSize, particleSize, particleSize));
        

        temperatureBuffer1 = new ComputeBuffer(dimx*dimy*dimz, sizeof(float));
        temperatureBuffer2 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float));

        smokeBuffer1 = new ComputeBuffer(dimx*dimy*dimz, sizeof(float));
        smokeBuffer2 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float));

        velocityBuffer1 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float) * 3);
        velocityBuffer2 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float) * 3);

        positionBuffer = new ComputeBuffer(nparticles, sizeof(float) * 3);
        smokepositionBuffer = new ComputeBuffer(nparticles, sizeof(float) * 3);
        colorBuffer = new ComputeBuffer(nparticles, sizeof(float) * 4);

        particleUpdater.SetInts("dimensions", new int[3] { dimx, dimy, dimz });

        temperatureManager.initialize();
        temperatureBuffer1.SetData(temperatureManager.getData());
        temperatureBuffer2.SetData(temperatureManager.getNextData());

        smokeManager.initialize();
        smokeBuffer1.SetData(smokeManager.getData());
        smokeBuffer2.SetData(smokeManager.getNextData());

        velocityManager.initialize();
        velocityBuffer1.SetData(velocityManager.getData());
        velocityBuffer2.SetData(velocityManager.getNextData());

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

        if (temperatureManager.checkTimeInterval())
        {
            temperatureBuffer1.SetData(temperatureManager.getData());
            temperatureBuffer2.SetData(temperatureManager.getNextData());
            //redoBuffers = true;
        }

        if (smokeManager.checkTimeInterval())
        {
            smokeBuffer1.SetData(smokeManager.getData());
            smokeBuffer2.SetData(smokeManager.getNextData());
            redoBuffers = true;
        }

        if (velocityManager.checkTimeInterval())
        {
            velocityBuffer1.SetData(velocityManager.getData());
            velocityBuffer2.SetData(velocityManager.getNextData());
            //redoBuffers = true;
        }

        if (redoBuffers) initBuffers();
        updateParticles();
        paintParticlesInstanced();
    }

    private void initBuffers()
    {
        Vector3[] smokepositions = new Vector3[nparticles];
        float[] densities = smokeManager.getData();
        index = 0;
        for (int z = dimz-1; z > -1; z -= 1) for (int y = 0; y < dimy && index < nparticles; y += 1) for (int x = 0; x < dimx; x += 1)
                {
                    if (index < nparticles && densities[z * dimy * dimx + y * dimx + x] > 0)
                    {
                        smokepositions[index] = new Vector3((float)x, (float)y, (float)z) / 100.0f;
                        ++index;
                    }
                }
        smokepositionBuffer.SetData(smokepositions);
        Debug.Log(index);
    }

    private void updateParticles()
    {
        particleUpdater.SetBuffer(0, "temps1", temperatureBuffer1);
        particleUpdater.SetBuffer(0, "temps2", temperatureBuffer2);
        particleUpdater.SetBuffer(0, "vel1", velocityBuffer1);
        particleUpdater.SetBuffer(0, "vel2", velocityBuffer2);
        particleUpdater.SetBuffer(0, "smok1", smokeBuffer1);
        particleUpdater.SetBuffer(0, "smok2", smokeBuffer2);

        particleUpdater.SetFloat("tempfactor", temperatureManager.getInterpolationFactor());
        particleUpdater.SetFloat("velfactor", velocityManager.getInterpolationFactor());
        particleUpdater.SetFloat("smokfactor", smokeManager.getInterpolationFactor());
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
        smokeBuffer1.Release();
        smokeBuffer2.Release();

        velocityBuffer1.Release();
        velocityBuffer2.Release();

        positionBuffer.Release();
        smokepositionBuffer.Release();
        colorBuffer.Release();
        commandBuf.Release();
    }
}
