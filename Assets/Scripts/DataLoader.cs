using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataLoader : MonoBehaviour
{
    public string filename;
    float[] data;
    public int dimx;
    public int dimy;
    public int dimz;
    public GameObject fireParticle;

    // Start is called before the first frame update
    void Start()
    {
        //readCSV(filename);
        readBinary(filename);
        instObjects();
        //Debug.Log("Temp: " + data[102*dimy*dimx+101*dimx+100]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void readCSV(string filePath) {
        if (File.Exists(filePath))
        {
            string text = File.ReadAllText(filePath);
            string[] values = text.Split(",");
            dimx = int.Parse(values[0]);
            dimy = int.Parse(values[1]);
            dimz = int.Parse(values[2]);
            data = new float[dimx * dimy * dimz];
            for (int i = 3; i < values.Length; ++i)
            {
                data[i-3] = float.Parse(values[i]);
            }
        }
        else
        {
            Debug.Log("File not found: " + filePath);
        }
    }

    private void readBinary(string filePath) {
        string[] values = filePath.Split(".");
        dimx = int.Parse(values[3]);
        dimy = int.Parse(values[2]);
        dimz = int.Parse(values[1]);
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

    private void instObjects() {
        GameObject o;
        MeshRenderer me;
        Material mat;
        float exp;
        for (int z = 0; z < dimz; z+=3) 
            for (int y = 0; y < dimy; y+=3)
                for (int x = 0; x < dimx; x+=3)
                {
                    exp = (data[z*dimy*dimx+y*dimx+x]-300)/1000;
                    if(exp > 0){
                    o = Instantiate(fireParticle, new Vector3(x, z, y)/100, Quaternion.identity);
                    me = o.GetComponent<MeshRenderer>();
                    mat = me.material;
                    //mat.color = Color.red;//Color.Lerp(Color.red,Color.yellow,(data[z*dimy*dimx+y*dimx+x]-270)/1000);
                    mat.color = new Color(1,0,0,exp);
                    //Debug.Log("Data: " + data[z*dimy*dimx+y*dimx+x]);
                    //Debug.Log("Expression: " + exp);
                    }
                }
    }
}
