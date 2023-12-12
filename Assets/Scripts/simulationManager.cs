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
    private float[] nextTemperature;

    public vectorLoader velocityManager;
    private Vector3[] velocity;
    private Vector3[] nextVelocity;

    //Those variables are just to test the RenderMeshInstanced
    public Material fsmat;
    public Mesh quadmesh;

    public 
    // Start is called before the first frame update
    void Start()
    {
        temperatureManager.initialize();
        temperature = temperatureManager.getData();
        nextTemperature = temperatureManager.getNextData();
        velocityManager.initialize();
        velocity = velocityManager.getData();
        nextVelocity = velocityManager.getNextData();
        timeList = new LinkedList<float>();
        timeList.AddLast(0.0f);
        timeList.AddLast(0.5f);
        timeList.AddLast(1.0f);
        instantiateParticles();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > timeList.First.Value) {

            float time = timeList.First.Value + 1.5f;
            timeList.RemoveFirst();
            timeList.AddLast(time);

            temperature = temperatureManager.newTimeData(time);
            nextTemperature = temperatureManager.getNextData();
            velocity = velocityManager.newTimeData(time);
            nextVelocity = velocityManager.getNextData();

            Object[] allObjects = Object.FindObjectsOfType(typeof(GameObject));
            foreach(GameObject obj in allObjects) if(obj.transform.name.StartsWith("FireParticle")) Destroy(obj);

            //instObjects();
            instantiateParticles();
        }
        updateParticles();
        paintParticles();
    }

    private void instObjects()
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

        particles = new List<partInfo>(dimx*dimy*dimz);
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
        float factor = (Time.time-timeList.First.Value)*2;
        for(int i = 0; i < particles.Count; ++i) {
            p = particles[i];
            pos = p.position*100.0f;
            pos = Vector3.Max(pos, Vector3.zero);
            pos = Vector3.Min(pos, big);
            //INTERCHANGING Y AND Z BECAUSE INDICES WERE FLIPPED
            p.position += ((1-factor)*velocity[(int)(pos.y * dimy * dimx + pos.z * dimx + pos.x)]+factor*nextVelocity[(int)(pos.y * dimy * dimx + pos.z * dimx + pos.x)])*Time.deltaTime;
            particles[i] = p;

        }
    }

    private void paintParticles() {
        RenderParams rp = new RenderParams(fsmat);
        Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(0.1f,0.1f,0.1f));
        float exp;
        Color c;
        //Matrix4x4[] instData = new Matrix4x4[particles.Count];
        for(int i = 0; i < particles.Count; ++i){
            exp = (particles[i].temperature - 300) / 1300;
            c = Color.Lerp(Color.red,Color.yellow,exp);
            rp.material.color = Color.Lerp(Color.clear,c,exp*2);
            rp.material.color = c;
            //rp = new RenderParams(m);
            Graphics.RenderMesh(rp, quadmesh, 0, Matrix4x4.Translate(particles[i].position)*scaleMatrix);
        }
        //Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }
}
