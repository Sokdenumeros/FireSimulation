using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public class vectorLoader : MonoBehaviour
{
    int dimx;
    int dimy;
    int dimz;
    LinkedList<float> timeList;
    LinkedList<Vector3[]> dataList;
    public string pre;
    public string post;

    //IF TIME I AM ASKING FOR IS NOT IN TIMELIT RETURN ERROR
    //PROBABLY SHOULD MAKE IT SO THAT I CAN ASK FOR A CERTAIN TIME
    public Vector3[] getData(){return dataList.First.Value;}

    public void initialize()
    {
        dataList = new LinkedList<Vector3[]>();
        timeList = new LinkedList<float>();
        
        string[] values = post.Split(".");
        dimx = int.Parse(values[values.Length - 2]);
        dimy = int.Parse(values[values.Length - 3]);
        dimz = int.Parse(values[values.Length - 4]);

        dataList.AddLast(new Vector3[dimx * dimy * dimz]);
        readBinarytoList(0.0f);
        timeList.AddLast(0.0f);

        dataList.AddLast(new Vector3[dimx * dimy * dimz]);
        readBinarytoList(0.5f);
        timeList.AddLast(0.5f);

        dataList.AddLast(new Vector3[dimx * dimy * dimz]);
        readBinarytoList(1.0f);
        timeList.AddLast(1.0f);
    }

    //IF NO OTHER TIME READY, RETUNR AN ERROR
    public Vector3[] newTimeData(float time){
        timeList.AddLast(time);
        timeList.RemoveFirst();

        dataList.AddLast(dataList.First.Value);
        dataList.RemoveFirst();
        Task.Run( () => readBinarytoList(time) );
        return dataList.First.Value;
    }

    private void readBinarytoList(float time)
    {
        string filePath = pre+time.ToString("0.0").Replace(',', '.')+post;
        if (! File.Exists(filePath))
        {
            Debug.Log("File not found: " + filePath);
            return;
        }

        using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
        {
            float u, v, w;
            Vector3[] data = dataList.Last.Value;
            for (int i = 0; i < data.Length; ++i) {
                u = reader.ReadSingle();
                v = reader.ReadSingle();
                w = reader.ReadSingle();
                data[i] = new Vector3(u,v,w);
            }
        }
    }
}
