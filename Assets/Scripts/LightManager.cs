using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public ComputeShader lightUpdater;
    public GameObject lightprefab;

    ComputeBuffer opacitybuf;
    ComputeBuffer heatbuf;
    ComputeBuffer countbuf;
    Light[] lights;
    uint[] opacities;
    uint[] heat;
    uint[] count;
    uint[] clear;

    public void init(ComputeBuffer p, ComputeBuffer s){
        lights = new Light[1000];

        for(int x = 0; x < 10; ++x) for(int y = 0; y < 10; ++y) for(int z = 0; z < 10; ++z) {
            lights[x*100+y*10+z] = Instantiate(lightprefab, new Vector3(x, y, z), Quaternion.identity).GetComponent<Light>();
        }
        
        opacitybuf = new ComputeBuffer(1000, sizeof(uint));
        heatbuf = new ComputeBuffer(1000, sizeof(uint));
        countbuf = new ComputeBuffer(1000, sizeof(uint));

        opacities = new uint[1000];
        heat = new uint[1000];
        count = new uint[1000];
        clear = new uint[1000];
        for(int i = 0; i < 1000; ++i) clear[i] = 0;

        lightUpdater.SetBuffer(0, "totalopacity", opacitybuf);
        lightUpdater.SetBuffer(0, "totalheat", heatbuf);
        lightUpdater.SetBuffer(0, "totalcount", countbuf);

        lightUpdater.SetBuffer(0, "pbuffer", p);
        lightUpdater.SetBuffer(0, "sbuffer", s);
    }

    public void setDimensions(int dx, int dy, int dz){
        lightUpdater.SetInts("dimensions", new int[3] { dx, dy, dz});
    }

    public void updateLights(ComputeBuffer p, ComputeBuffer s, int index)
    {
        lightUpdater.SetBuffer(0, "pbuffer", p);
        lightUpdater.SetBuffer(0, "sbuffer", s);
        lightUpdater.SetInt("nparticles", index);

        uint x, y, z;
        lightUpdater.GetKernelThreadGroupSizes(0, out x, out y, out z);
        int threadsx = (int)(index / (x*y*z));
        if (index % (x * y * z) > 0) threadsx++;
        if (threadsx > 0) lightUpdater.Dispatch(0, threadsx, 1, 1);

        //opacitybuf.GetData(opacities);
        heatbuf.GetData(heat);
        countbuf.GetData(count);
        for(int xx = 0; xx < 10; ++xx) for(int yy = 0; yy < 10; ++yy) for(int zz = 0; zz < 10; ++zz) {
            if (heat[xx*100+yy*10+zz] == 0) lights[xx*100+yy*10+zz].gameObject.SetActive(false);
            else lights[xx*100+yy*10+zz].gameObject.SetActive(true);
            lights[xx*100+yy*10+zz].intensity = heat[xx*100+yy*10+zz]/count[xx*100+yy*10+zz];
        }
        heatbuf.SetData(clear);
        countbuf.SetData(clear);
    }

    void OnDestroy(){
        opacitybuf.Release();
        heatbuf.Release();
        countbuf.Release();
    }
}
