using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScreenShot
{
    public float PosX;
    public float PosY;
    public float PosZ;
    public float RotX;
    public float RotY;
    public float RotZ;
    public string FileName;
    public int w;
    public int h;
}

[Serializable]
public class Point
{
    public float PosX;
    public float PosY;
    public float PosZ;
    public float RotX;
    public float RotY;
    public float RotZ;
    public int ModelType;
}

[Serializable]
public class PointStorage
{
    public List<Point> Points;
    public string RoomName;
    public ScreenShot Photo;
    public void AddPoint(GameObject o, int modelType)
    {
        Vector3 pos = o.transform.position;
        Quaternion rot = o.transform.rotation;
        if (Points == null)
        {
            Points = new List<Point>();
        }
        Point point = new Point();
        point.PosX = pos.x;
        point.PosY = pos.y;
        point.PosZ = pos.z;
        point.RotX = rot.eulerAngles.x;
        point.RotY = rot.eulerAngles.y;
        point.RotZ = rot.eulerAngles.z;
        point.ModelType = modelType;
        Points.Add(point);
    }

    public void SetCurrentScreenshot()
    {
        string jsonString = PlayerPrefs.GetString("photo");
        Photo = JsonUtility.FromJson<ScreenShot>(jsonString);
    }

    public void Print()
    {
        string log = "STORAGE\n";
        foreach (Point p in Points)
        {
            log += p.PosX.ToString();
            log += ", ";
            log += p.PosY.ToString();
            log += ", ";
            log += p.PosZ.ToString();
            log += "\n";
        }
        Debug.Log(log);
    }
}

[Serializable]
public class Rooms
{
    public List<PointStorage> Storages;

    public bool ContainsName(string name)
    {
        foreach (PointStorage room in Storages)
        {
            if (room.RoomName == name)
            {
                return true;
            }
        }
        return false;
    }

    public void DeleteRoom(int index)
    {
        Debug.Log("\n\nCount: " + Storages.Count + "\n\n");
        Storages.RemoveAt(index);
        Debug.Log("\n\nCount2: " + Storages.Count + "\n\n");
    }
}