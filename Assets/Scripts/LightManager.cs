using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public ComputeShader lightUpdater;

    ComputeBuffer opacitybuf;
    ComputeBuffer heatbuf;
    ComputeBuffer countbuf;

    public void init(ComputeBuffer p, ComputeBuffer s){
        opacitybuf = new ComputeBuffer(1000, sizeof(float));
        heatbuf = new ComputeBuffer(1000, sizeof(float));
        countbuf = new ComputeBuffer(1000, sizeof(float));

        lightUpdater.SetBuffer(0, "totalopacity", opacitybuf);
        lightUpdater.SetBuffer(0, "totalheat", heatbuf);
        lightUpdater.SetBuffer(0, "totalcount", countbuf);

        lightUpdater.SetBuffer(0, "pbuffer", p);
        lightUpdater.SetBuffer(0, "sbuffer", s);
    }

    public void updateLights(ComputeBuffer p, ComputeBuffer s, int index)
    {
        lightUpdater.SetBuffer(0, "pbuffer", p);
        lightUpdater.SetBuffer(0, "sbuffer", s);

        uint x, y, z;
        lightUpdater.GetKernelThreadGroupSizes(0, out x, out y, out z);
        int threadsx = (int)(index / (x*y*z));
        if (index % (x * y * z) > 0) threadsx++;
        if (threadsx > 0) lightUpdater.Dispatch(0, threadsx, 1, 1);
    }

    void OnDestroy(){
        opacitybuf.Release();
        heatbuf.Release();
        countbuf.Release();
    }
}
