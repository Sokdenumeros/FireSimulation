using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUsimulationManager : MonoBehaviour
{
    public int dimx, dimy, dimz;
    LinkedList<float> timeList;

    public floatLoader temperatureManager;
    public vectorLoader velocityManager;

    public ComputeShader particleUpdater;

    //Those variables are just to test the RenderMeshInstanced
    public Material fsmat;
    public Mesh quadmesh;
    Matrix4x4[] instData;

    ComputeBuffer temperatureBuffer1;
    ComputeBuffer temperatureBuffer2;

    ComputeBuffer velocityBuffer1;
    ComputeBuffer velocityBuffer2;

    ComputeBuffer positionBuffer;
    ComputeBuffer colorBuffer;

    public int nparticles;
    // Start is called before the first frame update
    void Start()
    {
        instData = new Matrix4x4[nparticles];
        for (int i = 0; i < nparticles; ++i) instData[i] = Matrix4x4.identity;
        temperatureBuffer1 = new ComputeBuffer(dimx*dimy*dimz, sizeof(float));
        temperatureBuffer2 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float));
        velocityBuffer1 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float) * 3);
        velocityBuffer2 = new ComputeBuffer(dimx * dimy * dimz, sizeof(float) * 3);
        positionBuffer = new ComputeBuffer(nparticles, sizeof(float) * 3);
        colorBuffer = new ComputeBuffer(nparticles, sizeof(float) * 4);

        particleUpdater.SetInts("dimensions", new int[3] { dimx, dimy, dimz });

        temperatureManager.initialize();
        temperatureBuffer1.SetData(temperatureManager.getData());
        temperatureBuffer2.SetData(temperatureManager.getNextData());

        velocityManager.initialize();
        velocityBuffer1.SetData(velocityManager.getData());
        velocityBuffer2.SetData(velocityManager.getNextData());

        timeList = new LinkedList<float>();
        timeList.AddLast(0.0f);
        timeList.AddLast(0.5f);
        timeList.AddLast(1.0f);

        initBuffers();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > timeList.First.Value)
        {

            float time = timeList.First.Value + 1.5f;
            timeList.RemoveFirst();
            timeList.AddLast(time);

            temperatureBuffer1.SetData(temperatureManager.newTimeData(time));
            temperatureBuffer2.SetData(temperatureManager.getNextData());

            velocityBuffer1.SetData(velocityManager.newTimeData(time));
            velocityBuffer2.SetData(velocityManager.getNextData());

            initBuffers();
            //updateParticles();
        }
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
                        instData[index] = Matrix4x4.identity;
                        ++index;
                    }
                }
        for (int i = 0; i < nparticles; ++i)
        {
            instData[i] = Matrix4x4.identity;
            positions[index] = new Vector3(0, 1, 0);
        }
        positionBuffer.SetData(positions);
    }

    private void updateParticles()
    {
        Vector3 big = new Vector3(dimx - 1, dimy - 1, dimz - 1);
        float factor = (Time.time - timeList.First.Value) * 2;

        particleUpdater.SetBuffer(0, "positions", positionBuffer);
        particleUpdater.SetBuffer(0, "colors", colorBuffer);
        particleUpdater.SetBuffer(0, "temps1", temperatureBuffer1);
        particleUpdater.SetBuffer(0, "temps2", temperatureBuffer2);
        particleUpdater.SetBuffer(0, "vel1", velocityBuffer1);
        particleUpdater.SetBuffer(0, "vel2", velocityBuffer2);

        particleUpdater.SetFloat("factor", factor);
        particleUpdater.SetFloat("deltaTime", Time.deltaTime);

        uint x, y, z;
        particleUpdater.GetKernelThreadGroupSizes(0, out x, out y, out z);
        particleUpdater.Dispatch(0, nparticles/2, 2, 1);

    }

    private void paintParticlesInstanced()
    {
        fsmat.SetBuffer("colorbuffer", colorBuffer);
        fsmat.SetBuffer("positionbuffer", positionBuffer);

        RenderParams rp = new RenderParams(fsmat);
        Material[] materials = new Material[40];

        for (int i = 0; i < 40; i++)
        {
            //fsmat.SetInt("offset", i*512);
            //rp = new RenderParams(fsmat);
            materials[i] = new Material(fsmat);
            materials[i].SetInt("offset", i * 511);
            materials[i].SetBuffer("colorbuffer", colorBuffer);
            materials[i].SetBuffer("positionbuffer", positionBuffer);
            Graphics.RenderMeshInstanced(new RenderParams(materials[i]), quadmesh, 0, instData, 512, 0);
        }
        //Graphics.RenderMeshInstanced(rp, quadmesh, 0, instData, 512,0);
        //Graphics.DrawMeshInstancedIndirect(quadmesh,0,fsmat, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),positionBuffer);
    }
}
