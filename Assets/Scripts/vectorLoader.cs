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
    public float timestep;
    LinkedList<float> timeList;
    LinkedList<Vector3[]> dataList;
    public string pre;
    public string post;
    private bool reading;

    //IF TIME I AM ASKING FOR IS NOT IN TIMELIT RETURN ERROR
    //PROBABLY SHOULD MAKE IT SO THAT I CAN ASK FOR A CERTAIN TIME
    public Vector3[] getData(){
        checkTimeInterval();
        return dataList.First.Value;
    }

    public Vector3[] getNextData(){
        checkTimeInterval();
        return dataList.First.Next.Value;
    }

    public Vector3[] getThirdData(){
        checkTimeInterval();
        return dataList.First.Next.Next.Value;
    }

    public bool checkTimeInterval(){
        if (Time.time >= timeList.First.Value + timestep) { newTimeData(timeList.Last.Value + timestep); return true; }
        return false;
    }

    public float getInterpolationFactor() { return (Time.time - timeList.First.Value) / timestep; }
    public void initialize()
    {
        dataList = new LinkedList<Vector3[]>();
        timeList = new LinkedList<float>();
        
        string[] values = post.Split(".");
        dimx = int.Parse(values[values.Length - 2]);
        dimy = int.Parse(values[values.Length - 3]);
        dimz = int.Parse(values[values.Length - 4]);

        for (int i = 0; i < 4; ++i){
            dataList.AddLast(new Vector3[dimx * dimy * dimz]);
            readBinarytoList(timestep*i);
            timeList.AddLast(timestep*i);
        }
    }

    //IF NO OTHER TIME READY, RETUNR AN ERROR
    private Vector3[] newTimeData(float time){
        timeList.AddLast(time);
        timeList.RemoveFirst();

        dataList.AddLast(dataList.First.Value);
        dataList.RemoveFirst();
        if(reading) Debug.LogError("STILL READING");
        reading = true;
        Task.Run( () => readBinarytoList(time) );
        return dataList.First.Value;
    }

    private void readBinarytoList(float time)
    {
        string filePath = pre+time.ToString("0.0").Replace(',', '.')+post;
        if (! File.Exists(filePath))
        {
            Debug.Log("File not found: " + filePath);
            reading = false;
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
        reading = false;
    }
}
