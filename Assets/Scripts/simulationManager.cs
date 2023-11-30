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
}
