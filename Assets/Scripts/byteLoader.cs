using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public class byteLoader : MonoBehaviour
{

    public float timestep;
    LinkedList<int> bytes;
    LinkedList<float> timeList;
    LinkedList<byte[]> dataList;
    public string pre;
    public string post;
    private bool reading;

    public byte[] getData(){
        checkTimeInterval();
        return dataList.First.Value;
    }

    public int getNbytes(){
        checkTimeInterval();
        return bytes.First.Value;
    }

    public bool checkTimeInterval(){
        if (Time.time >= timeList.First.Value + timestep) { newTimeData(timeList.Last.Value + timestep); return true; }
        return false;
    }
    public float getInterpolationFactor() { return (Time.time - timeList.First.Value) / timestep; }
    public void initialize(int nbytes)
    {
        bytes = new LinkedList<int>();
        dataList = new LinkedList<byte[]>();
        timeList = new LinkedList<float>();
        
        for (int i = 0; i < 4; ++i){
            dataList.AddLast(new byte[nbytes]);
            bytes.AddLast(nbytes);
            readBinarytoList(timestep*i);
            timeList.AddLast(timestep*i);
        }
    }

    private byte[] newTimeData(float time){
        timeList.AddLast(time);
        timeList.RemoveFirst();

        bytes.AddLast(bytes.First.Value);
        bytes.RemoveFirst();

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
            byte[] data = dataList.Last.Value;
            bytes.Last.Value = reader.Read(data, 0, data.Length);
        }
        reading = false;
    }
}
