using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simulationManager : MonoBehaviour
{
    public GameObject fireParticle;
    public int dimx, dimy, dimz;
    LinkedList<float> timeList;    

    public floatLoader temperatureManager;

    public vectorLoader velocityManager;
    private Vector3[] velocity;
    private Vector3[] nextVelocity;

    //Those variables are just to test the RenderMeshInstanced
    public Material fsmat;
    public Mesh quadmesh;

    //Variables to test the dataElement struct
    dataElement temperatures;

    struct dataElement{
        public float[] previous;
        public float[] next;
        public float previousTime;
        public float nextTime;
        public int dimx, dimy, dimz;
        public dataElement(float[] a, float[] b, int x, int y,int  z) {
            previousTime = 0.0f;
            nextTime = 0.0f;
            previous = a;
            next = b;
            dimx = x;
            dimy = y;
            dimz = z;
        }

        public float sample(float t, float x, float y, float z){
            float timeFactor = (t-previousTime)/(nextTime-previousTime);
            float timesample0 = previous[ (int)y*dimy*dimx + (int)z*dimx + (int)x ];
            float timesample1 = next[(int)y * dimy * dimx + (int)z * dimx + (int)x];
            return (1-timeFactor)*timesample0 + timeFactor*timesample1;
        }

        public float sample(float t, Vector3 v){
            float x = v.x;
            float y = v.y;
            float z = v.z;
            float timeFactor = (t-previousTime)/(nextTime-previousTime);
            float timesample0 = previous[ (int)y*dimy*dimx + (int)z*dimx + (int)x ];
            float timesample1 = next[(int)y * dimy * dimx + (int)z * dimx + (int)x];
            return (1-timeFactor)*timesample0 + timeFactor*timesample1;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        temperatureManager.initialize();

        velocityManager.initialize();
        velocity = velocityManager.getData();
        nextVelocity = velocityManager.getNextData();

        timeList = new LinkedList<float>();
        timeList.AddLast(0.0f);
        timeList.AddLast(0.5f);
        timeList.AddLast(1.0f);

        temperatures = new dataElement(temperatureManager.getData(),temperatureManager.getNextData(),dimx,dimy,dimz);
        temperatures.previousTime = timeList.First.Value;
        temperatures.nextTime = timeList.First.Next.Value;

        instantiateParticles();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > timeList.First.Value) {

            float time = timeList.First.Value + 1.5f;
            timeList.RemoveFirst();
            timeList.AddLast(time);

            temperatures.previousTime = timeList.First.Value;
            temperatures.nextTime = timeList.First.Next.Value;

            temperatures.previous = temperatureManager.newTimeData(time);
            temperatures.next = temperatureManager.getNextData();

            velocity = velocityManager.newTimeData(time);
            nextVelocity = velocityManager.getNextData();

            Object[] allObjects = Object.FindObjectsOfType(typeof(GameObject));
            foreach(GameObject obj in allObjects) if(obj.transform.name.StartsWith("FireParticle")) Destroy(obj);

            //instObjects();
            instantiateParticles();
            //instantiateParticleObjects();
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
        float[] data = temperatures.previous;
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
        float[] temps = temperatures.previous;
        //for (int z = 0; z < dimz; z += 1) for (int y = 0; y < dimy; y += 1) for (int x = 0; x < dimx; x += 1){
        for (int z = dimz-1; z >= 0; z -= 1) for (int y = dimy-1; y >= 0; y -= 1) for (int x = dimx-1; x >= 0; x -= 1){
                    exp = (temps[z * dimy * dimx + y * dimx + x] - 300) / 1300;
                    if (exp > 0.5) particles.Add(new partInfo(new Vector3(x, z, y) / 100, temps[z * dimy * dimx + y * dimx + x]));
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
            p.temperature = temperatures.sample(Time.time,pos);
            //INTERCHANGING Y AND Z BECAUSE INDICES WERE FLIPPED
            //p.position += ((1-factor)*velocity[(int)(pos.y * dimy * dimx + pos.z * dimx + pos.x)]+factor*nextVelocity[(int)(pos.y * dimy * dimx + pos.z * dimx + pos.x)])*Time.deltaTime;
            particles[i] = p;
        }
    }

    private void paintParticles() {
        GameObject o = Instantiate(fireParticle, new Vector3(0,0,0), Quaternion.identity);
        RenderParams rp = new RenderParams(o.GetComponent<MeshRenderer>().material);
        Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(0.1f,0.1f,0.1f));
        float exp;
        Color c;
        //Matrix4x4[] instData = new Matrix4x4[particles.Count];
        for(int i = 0; i < particles.Count; ++i){
            exp = (particles[i].temperature - 300) / 1300;
            c = Color.Lerp(Color.red,Color.yellow,exp);
            rp.material.color = Color.Lerp(Color.clear,c,exp*2);
            //rp.material.color = c;
            //rp = new RenderParams(m);
            Graphics.RenderMesh(rp, quadmesh, 0, Matrix4x4.Translate(particles[i].position)*scaleMatrix);
        }
        Destroy(o);
        //Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }

    private void instantiateParticleObjects() {
        GameObject o;
        MeshRenderer me;
        float exp;
        Color c;
        for(int i = 0; i < particles.Count; ++i){
            exp = (particles[i].temperature - 300) / 1300;
            o = Instantiate(fireParticle, particles[i].position, Quaternion.identity);
            me = o.GetComponent<MeshRenderer>();
            c = Color.Lerp(Color.red,Color.yellow,exp);
            me.material.color = Color.Lerp(Color.clear,c,exp*2);
            fsmat.color = Color.Lerp(Color.clear,c,exp*2);
            me.material = fsmat;
        }
    }
}
