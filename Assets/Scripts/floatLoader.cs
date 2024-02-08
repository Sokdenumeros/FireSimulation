using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public class floatLoader : MonoBehaviour
{
    int dimx;
    int dimy;
    int dimz;
    public float timestep;
    LinkedList<float> timeList;
    LinkedList<float[]> dataList;
    public string pre;
    public string post;

    //IF TIME I AM ASKING FOR IS NOT IN TIMELIT RETURN ERROR
    //PROBABLY SHOULD MAKE IT SO THAT I CAN ASK FOR A CERTAIN TIME
    public float[] getData(){
        checkTimeInterval();
        return dataList.First.Value;
    }

    public float[] getNextData(){
        checkTimeInterval();
        return dataList.First.Next.Value;
    }

    public bool checkTimeInterval(){
        if (Time.time >= timeList.First.Value + timestep) { newTimeData(timeList.Last.Value + timestep); return true; }
        return false;
    }

    public float getInterpolationFactor() { return (Time.time - timeList.First.Value) / timestep; }
    public void initialize()
    {
        dataList = new LinkedList<float[]>();
        timeList = new LinkedList<float>();
        
        string[] values = post.Split(".");
        dimx = int.Parse(values[values.Length - 2]);
        dimy = int.Parse(values[values.Length - 3]);
        dimz = int.Parse(values[values.Length - 4]);

        //for(int i = 0; i < 3; ++i){}
        dataList.AddLast(new float[dimx * dimy * dimz]);
        readBinarytoList(0.0f);
        timeList.AddLast(0.0f);

        dataList.AddLast(new float[dimx * dimy * dimz]);
        readBinarytoList(timestep);
        timeList.AddLast(timestep);

        dataList.AddLast(new float[dimx * dimy * dimz]);
        readBinarytoList(timestep*2);
        timeList.AddLast(timestep*2);
    }

    //IF NO OTHER TIME READY, RETURN AN ERROR
    private float[] newTimeData(float time){
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
            float[] data = dataList.Last.Value;
            for (int i = 0; i < data.Length; ++i) data[i] = reader.ReadSingle();
        }
    }

    
}
