using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[Serializable]
public class Point
{
    public float PosX;
    public float PosY;
    public float PosZ;
    public float RotX;
    public float RotY;
    public float RotZ;
}

[Serializable]
public class PointStorage
{
    public List<Point> Points;
    public void AddPoint(GameObject o)
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
        Points.Add(point);
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

public class Scanner: MonoBehaviour
{
    private ARRaycastManager rayManager;
    public GameObject objectToSpawn;
    public List<GameObject> objects;
    public Editor editor;
    private PointStorage storage;
    private bool can_add;

    void Start()
    {
        rayManager = FindObjectOfType<ARRaycastManager>();
        objects = new List<GameObject>();
        storage = new PointStorage();
        enabled = false;
        can_add = true;
    }

    public void SaveStorage()
    {
        if (PlayerPrefs.HasKey("points"))
        {
            PlayerPrefs.DeleteKey("points");
            PlayerPrefs.Save();
        }
        try
        {
            string jsonString = JsonUtility.ToJson(storage);
            Debug.Log("___SERIALIZED___ " + jsonString);
            PlayerPrefs.SetString("points", jsonString);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.Log("\n\n\n" + e.GetType() + "\n\n\n");
        }
    }

    public void Enable()
    {
        enabled = true;
    }

    void Update()
    {
        if (!can_add)
        {
            return;
        }
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Vector2 touchPos = Input.GetTouch(0).position;
            if (touchPos.y < Screen.height * 0.2)
            {
                return;
            }
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            rayManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon);
            if (hits.Count == 0)
            {
                return;
            }
            GameObject currentObject = Instantiate(objectToSpawn, hits[0].pose.position, hits[0].pose.rotation);
            
            // storage.AddPoint(pose);
            editor.EditObject(currentObject, (GameObject obj) => {
                can_add = true;
                if (storage == null)
                {
                    storage = new PointStorage();
                }
                storage.AddPoint(obj);
            });
            can_add = false;
        }
    }
}