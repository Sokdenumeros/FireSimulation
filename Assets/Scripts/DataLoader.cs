using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataLoader : MonoBehaviour
{
    public string filename;
    public float[] data;
    public int dimx;
    public int dimy;
    public int dimz;
    public GameObject fireParticle;

    // Start is called before the first frame update
    void Start()
    {
        readCSV(filename);
        instObjects();
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

    private void instObjects() {
        for (int x = 0; x < dimx; ++x) 
            for (int y = 0; y < dimy; ++y)
                for (int z = 0; z < dimz; ++z)
                {
                    GameObject o = Instantiate(fireParticle, new Vector3(x * 10, y * 10, z * 10), Quaternion.identity);
                    MeshRenderer me = o.GetComponent<MeshRenderer>();
                    Material mat = me.material;
                    mat.color = Color.red;
                }
    }
}
