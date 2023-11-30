using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simulationManager : MonoBehaviour
{
    public GameObject fireParticle;
    public int dimx, dimy, dimz;
    LinkedList<float> timeList;    

    public floatLoader temperatureManager;
    private float[] temperature;

    public vectorLoader velocityManager;
    private Vector3[] velocity;

    public 
    // Start is called before the first frame update
    void Start()
    {
        temperatureManager.initialize();
        temperature = temperatureManager.getData();
        velocityManager.initialize();
        velocity = velocityManager.getData();
        timeList = new LinkedList<float>();
        timeList.AddLast(0.0f);
        timeList.AddLast(0.5f);
        timeList.AddLast(1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > timeList.First.Value) {

            float time = timeList.First.Value + 1.5f;
            timeList.RemoveFirst();
            timeList.AddLast(time);

            temperature = temperatureManager.newTimeData(time);
            velocity = velocityManager.newTimeData(time);

            Object[] allObjects = Object.FindObjectsOfType(typeof(GameObject));
            foreach(GameObject obj in allObjects) if(obj.transform.name.StartsWith("FireParticle")) Destroy(obj);

            instObjects();
        }
    }

    /*private void instObjects()
    {
        GameObject o;
        MeshRenderer me;
        Material mat;
        float exp;
        float[] data = temperature;
        for (int z = 0; z < dimz; z += 1)
            for (int y = 0; y < dimy; y += 1)
                for (int x = 0; x < dimx; x += 1)
                {
                    exp = (data[z * dimy * dimx + y * dimx + x] - 300) / 1300;
                    if (exp > 0)
                    {
                        o = Instantiate(fireParticle, new Vector3(x, z, y) / 100, Quaternion.identity);
                        me = o.GetComponent<MeshRenderer>();
                        mat = me.material;
                        
                        //should never be above 1, for some reason the ronan example has values e+16
                        //mat.color = new Color(0, 0, 0, exp);

                        //mat.color = Color.red;//Color.Lerp(Color.red,Color.yellow,(data[z*dimy*dimx+y*dimx+x]-270)/1000);
                        mat.color = Color.Lerp(Color.red,Color.yellow,exp);
                        //mat.color.a = exp;
                        mat.color = Color.Lerp(Color.clear,mat.color,exp*2);
                    }
                }
    }

    struct partInfo{
        public Vector3 position;
        public float temperature;

        public partInfo(Vector3 p, float t) {
        this.position = p;
        this.temperature = t;
        }
    }
    private List<partInfo> particles;

    private void instantiateParticles() {

        particles = new List<partInfo>(dimx*dimy*dimz/10);
        float exp;
        for (int z = 0; z < dimz; z += 1)
            for (int y = 0; y < dimy; y += 1)
                for (int x = 0; x < dimx; x += 1)
                {
                    exp = (temperature[z * dimy * dimx + y * dimx + x] - 300) / 1300;
                    if (exp > 0) particles.Add(new partInfo(new Vector3(x, z, y) / 100, temperature[z * dimy * dimx + y * dimx + x]));
                }
        
    }

    //MAYBE I SHOULD TRANSPOSE THE DATA ARRAY IN PYTHON IN A DIFFERENT WAY AT SOME POINT
    private void updateParticles() {
        partInfo p;
        Vector3 pos;
        Vector3 big = new Vector3(dimx,dimy,dimz);
        for(int i = 0; i < particles.Count; ++i) {
            p = particles[i];
            pos = p.position*100.0f;
            pos = Vector3.Max(pos, Vector3.zero);
            pos = Vector3.Min(pos, big);
            //INTERCHANGING Y AND Z BECAUSE INDICES WERE FLIPPED
            p.position += velocity[(int)(pos.y * dimy * dimx + pos.z * dimx + pos.x)]*Time.deltaTime;
            particles[i] = p;

        }
    }

    private void paintParticles() {
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }*/
}
