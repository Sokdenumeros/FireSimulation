using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public class DataLoader : MonoBehaviour
{
    //OLD VARIABLES
    public string filename;


    LinkedList<float> timeList;
    LinkedList<float[]> dataList;

    public string pre;
    public string post;

    float[] data;
    public int dimx;
    public int dimy;
    public int dimz;

    public GameObject fireParticle;

    // Start is called before the first frame update
    void Start()
    {
        dataList = new LinkedList<float[]>();
        timeList = new LinkedList<float>();

        timeList.AddLast(0.0f);
        readBinary(pre+timeList.Last.Value.ToString("0.0").Replace(',', '.')+post );
        dataList.AddLast(data);

        timeList.AddLast(0.5f);
        readBinary(pre+timeList.Last.Value.ToString("0.0").Replace(',', '.')+post );
        dataList.AddLast(data);

        timeList.AddLast(1.0f);
        readBinary(pre+timeList.Last.Value.ToString("0.0").Replace(',', '.')+post );
        dataList.AddLast(data);
        
        data = dataList.First.Value;

        //readBinary(filename);
        instObjects();
    }

    private void newTimeData(){
        dataList.AddLast(dataList.First.Value);
        dataList.RemoveFirst();
        float time = timeList.First.Value+1.5f;
        timeList.RemoveFirst();
        timeList.AddLast(time);
        data = dataList.First.Value;

        Object[] allObjects = Object.FindObjectsOfType(typeof(GameObject));
        foreach(GameObject obj in allObjects) if(obj.transform.name.StartsWith("FireParticle")) Destroy(obj);

        instObjects();
        Task.Run( () => readBinarytoList(pre+time.ToString("0.0").Replace(',', '.')+post) );
    }

    //Try to load into this list so I can check if Last is passing by reference.
    private void readBinarytoList(string filePath)
    {
        string[] values = filePath.Split(".");
        if (File.Exists(filePath))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                float[] data = dataList.Last.Value;
                for (int i = 0; i < data.Length; ++i) data[i] = reader.ReadSingle();
            }
        }
        else
        {
            Debug.Log("File not found: " + filePath);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > timeList.First.Value) newTimeData();
    }

    private void readBinary(string filePath)
    {
        string[] values = filePath.Split(".");
        dimx = int.Parse(values[values.Length - 2]);
        dimy = int.Parse(values[values.Length - 3]);
        dimz = int.Parse(values[values.Length - 4]);
        if (File.Exists(filePath))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                data = new float[dimx * dimy * dimz];
                for (int i = 0; i < data.Length; ++i) data[i] = reader.ReadSingle();
            }
        }
        else
        {
            Debug.Log("File not found: " + filePath);
        }
    }

    private void instObjects()
    {
        GameObject o;
        MeshRenderer me;
        Material mat;
        float exp;
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
                        mat.color = new Color(0, 0, 0, exp);

                        //mat.color = Color.red;//Color.Lerp(Color.red,Color.yellow,(data[z*dimy*dimx+y*dimx+x]-270)/1000);
                        mat.color = Color.Lerp(Color.red,Color.yellow,exp);
                        //mat.color.a = exp;
                        mat.color = Color.Lerp(Color.clear,mat.color,exp*2);
                    }
                }
    }
}
