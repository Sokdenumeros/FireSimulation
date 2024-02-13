using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUsimulationManager : MonoBehaviour
{
    public int dimx, dimy, dimz;

    public floatLoader temperatureManager;
    public floatLoader smokeManager;
    public vectorLoader velocityManager;

    public ComputeShader particleUpdater;

    //Those variables are just to test the RenderMeshInstanced
    public Material fsmat;
    public Material smokemat;
    public Mesh quadmesh;
    Matrix4x4[] instData;

    ComputeBuffer temperatureBuffer1;
    ComputeBuffer temperatureBuffer2;

    ComputeBuffer smokeBuffer1;
    ComputeBuffer smokeBuffer2;

    ComputeBuffer velocityBuffer1;
    ComputeBuffer velocityBuffer2;

    ComputeBuffer positionBuffer;
    ComputeBuffer smokepositionBuffer;
    ComputeBuffer colorBuffer;

    public int nparticles;
    // Start is called before the first frame update
    void Start()
    {
        instData = new Matrix4x4[nparticles];
        for (int i = 0; i < nparticles; ++i) instData[i] = Matrix4x4.identity;
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
    }

    // Update is called once per frame
    void Update()
    {
        bool redoBuffers = false;

        if (temperatureManager.checkTimeInterval())
        {
            temperatureBuffer1.SetData(temperatureManager.getData());
            temperatureBuffer2.SetData(temperatureManager.getNextData());
            redoBuffers = true;
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
            redoBuffers = true;
        }

        if (redoBuffers) initBuffers();
        
        updateParticles();
        paintParticlesInstanced();
    }

    private void initBuffers()
    {
        Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(0.1f, 0.1f, 0.1f));
        Vector3[] positions = new Vector3[nparticles];
        float exp;
        float[] temps = temperatureManager.getData();
        int index = 0;
        for (int z = 0; z < dimz; z += 1) for (int y = 0; y < dimy && index < nparticles; y += 1) for (int x = 0; x < dimx; x += 1)
                {
                    exp = (temps[z * dimy * dimx + y * dimx + x] - 300) / 1300;
                    if (index < nparticles && exp > 0.0)
                    {
                        positions[index] = new Vector3((float)x, (float)y, (float)z) / 100.0f;
                        instData[index] = scaleMatrix;
                        ++index;
                    }
                }
        positionBuffer.SetData(positions);

        Vector3[] smokepositions = new Vector3[nparticles];
        float[] densities = temperatureManager.getData();
        index = 0;
        for (int z = 0; z < dimz; z += 1) for (int y = 0; y < dimy && index < nparticles; y += 1) for (int x = 0; x < dimx; x += 1)
                {
                    if (index < nparticles && densities[z * dimy * dimx + y * dimx + x] > 80.0)
                    {
                        smokepositions[index] = new Vector3((float)x, (float)y, (float)z) / 100.0f;
                        ++index;
                    }
                }
        smokepositionBuffer.SetData(smokepositions);
    }

    private void updateParticles()
    {
        Vector3 big = new Vector3(dimx - 1, dimy - 1, dimz - 1);
        //float factor = (Time.time - timeList.First.Value) * 2;

        particleUpdater.SetBuffer(0, "positions", positionBuffer);
        particleUpdater.SetBuffer(0, "colors", colorBuffer);
        particleUpdater.SetBuffer(0, "temps1", temperatureBuffer1);
        particleUpdater.SetBuffer(0, "temps2", temperatureBuffer2);
        particleUpdater.SetBuffer(0, "vel1", velocityBuffer1);
        particleUpdater.SetBuffer(0, "vel2", velocityBuffer2);

        particleUpdater.SetFloat("tempfactor", temperatureManager.getInterpolationFactor());
        particleUpdater.SetFloat("velfactor", velocityManager.getInterpolationFactor());
        particleUpdater.SetFloat("deltaTime", Time.deltaTime);
        particleUpdater.SetInt("nparticles", nparticles);

        uint x, y, z;
        particleUpdater.GetKernelThreadGroupSizes(0, out x, out y, out z);
        int threadsx = (int)(nparticles / (x*y*z));
        if (nparticles % (x * y * z) > 0) threadsx++;
        particleUpdater.Dispatch(0, threadsx, 1, 1);

    }

    private void paintParticlesInstanced()
    {
        /*fsmat.SetBuffer("colorbuffer", colorBuffer);
        fsmat.SetBuffer("positionbuffer", positionBuffer);

        RenderParams rp = new RenderParams(fsmat);*/
        Material[] materials = new Material[40];

        for (int i = 0; i < 40; i++)
        {
            materials[i] = new Material(fsmat);
            materials[i].SetInt("offset", i * 511);
            materials[i].SetBuffer("colorbuffer", colorBuffer);
            materials[i].SetBuffer("positionbuffer", positionBuffer);
            Graphics.RenderMeshInstanced(new RenderParams(materials[i]), quadmesh, 0, instData, 512, 0);
        }

        Material[] smokematerials = new Material[40];
        for (int i = 0; i < 40; i++)
        {
            smokematerials[i] = new Material(smokemat);
            smokematerials[i].SetInt("offset", i * 511);
            smokematerials[i].SetBuffer("positionbuffer", smokepositionBuffer);
            smokematerials[i].SetBuffer("opacitybuffer", smokeBuffer1);
            Graphics.RenderMeshInstanced(new RenderParams(smokematerials[i]), quadmesh, 0, instData, 512, 0);
        }
        //Graphics.RenderMeshInstanced(rp, quadmesh, 0, instData, 512,0);
        //Graphics.DrawMeshInstancedIndirect(quadmesh,0,fsmat, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),positionBuffer);
    }
}
