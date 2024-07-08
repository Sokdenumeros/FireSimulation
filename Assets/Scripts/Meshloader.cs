using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public struct BoundingBox
{
    public float x_start;
    public float x_end;
    public float y_start;
    public float y_end;
    public float z_start;
    public float z_end;
}

public class Meshloader : MonoBehaviour
{

    public string filePath;

    void CreateCubeForBoundingBox(BoundingBox box)
    {
        // Calculate the center of the bounding box
        float centerX = (box.x_start + box.x_end) / 2;
        float centerY = (box.y_start + box.y_end) / 2;
        float centerZ = (box.z_start + box.z_end) / 2;

        // Calculate the size of the bounding box
        float sizeX = Mathf.Abs(box.x_end - box.x_start);
        float sizeY = Mathf.Abs(box.y_end - box.y_start);
        float sizeZ = Mathf.Abs(box.z_end - box.z_start);

        // Create a cube
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Set the cube's position and size
        cube.transform.position = new Vector3(centerX, centerY, centerZ);
        cube.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);

    }

    // Start is called before the first frame update
    void Start()
    {        
        
        List<BoundingBox> boundingBoxes = new List<BoundingBox>();

        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                float x_start = float.Parse(reader.ReadLine(),CultureInfo.InvariantCulture);
                float x_end = float.Parse(reader.ReadLine(),CultureInfo.InvariantCulture);
                float z_start = float.Parse(reader.ReadLine(),CultureInfo.InvariantCulture);
                float z_end = float.Parse(reader.ReadLine(),CultureInfo.InvariantCulture);
                float y_start = float.Parse(reader.ReadLine(),CultureInfo.InvariantCulture);
                float y_end = float.Parse(reader.ReadLine(),CultureInfo.InvariantCulture);

                // Create a BoundingBox struct and add it to the list
                BoundingBox box = new BoundingBox
                {
                    x_start = x_start,
                    x_end = x_end,
                    y_start = y_start,
                    y_end = y_end,
                    z_start = z_start,
                    z_end = z_end
                };

                boundingBoxes.Add(box);
            }
        }

        foreach (var box in boundingBoxes)
        {
            CreateCubeForBoundingBox(box);
        }
    }
}

