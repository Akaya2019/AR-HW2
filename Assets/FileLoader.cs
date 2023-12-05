using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileLoader : MonoBehaviour
{
    public static bool LoadPointCloudFile(string filePath, out Vector3[] points)
    {
        try
        {
            List<Vector3> pointList = new List<Vector3>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                // Read the number of points
                int numPoints = int.Parse(reader.ReadLine());

                // Read and parse each point
                for (int i = 0; i < numPoints; i++)
                {
                    string[] coordinates = reader.ReadLine().Split(' ');
                    if (coordinates.Length == 3)
                    {
                        float x = float.Parse(coordinates[0]);
                        float y = float.Parse(coordinates[1]);
                        float z = float.Parse(coordinates[2]);

                        pointList.Add(new Vector3(x, y, z));
                    }
                }
            }

            points = pointList.ToArray();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading point cloud file: {e.Message}");
            points = null;
            return false;
        }
    }
}
