using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class Viewer : MonoBehaviour
{
    private ARCameraManager cameraManager;
    private Matrix4x4 lastDisplayMatrix;
    private Camera arCamera;
    private PointStorage storage;
    private Rooms rooms;
    private bool isSet;
    private List<GameObject> models;
    private Vector3 oldCamPos;
    private Vector3 oldCamRot;
    private List<GameObject> objects;

    public ModelsManager modelsManager;
    public GameObject menuInstance;
    public List<ARAnchor> points;
    public RawImage background;
    public AspectRatioFitter Fit;

    // Start is called before the first frame update
    void Start()
    {
        cameraManager = FindObjectOfType<ARCameraManager>();
        cameraManager.enabled = true;
        cameraManager.frameReceived += Process;
        background.enabled = false;
        isSet = false;
        models = modelsManager.GetModels();
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.name == "AR Camera")
            {
                arCamera = cam;
            }
        }
        if (!PlayerPrefs.HasKey("rooms"))
        {
            return;
        }
        string jsonString = PlayerPrefs.GetString("rooms");
        rooms = JsonUtility.FromJson<Rooms>(jsonString);
        objects = new List<GameObject>();
    }

    public void OnEnable(int index)
    {
        storage = rooms.Storages[index];
        StartCoroutine(SetImage());
        menuInstance.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator SetImage()
    {
        ScreenShot sh = storage.Photo;
        bool isOk = true;
        Texture2D texture = new Texture2D(sh.w, sh.h);
        try
        {
            byte[] bytes = File.ReadAllBytes(sh.FileName);
            texture.LoadImage(bytes);
        }
        catch (Exception e)
        {
            isOk = false;
        }
        if (!isOk)
        {
            yield return new WaitForEndOfFrame();
        }
        
        
        Debug.Log("\n\n\nIn Screenshotter: " + sh.w + ", " + sh.h + ", " + texture.width + ", " + texture.height + "\n\n\n");

        Fit.aspectRatio = (float) texture.width / texture.height;
        Fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        background.texture = texture;   
        background.rectTransform.localEulerAngles = new Vector3(0, 0, 90);
        background.rectTransform.localScale = new Vector3((float)texture.height / texture.width, (float)texture.height / texture.width, 1f);
        background.enabled = true;
        oldCamPos = new Vector3(sh.PosX, sh.PosY, sh.PosZ);
        oldCamRot = new Vector3(sh.RotX, sh.RotY, sh.RotZ);
        yield return new WaitForEndOfFrame();
    }

    public void Process(ARCameraFrameEventArgs a)
    {
        lastDisplayMatrix = a.displayMatrix.Value;
    }

    private Matrix4x4 GetTransform()
    {
        Matrix4x4 matrix = CameraPositionSaver.GetCameraMatrix(lastDisplayMatrix);
        //
        //arCamera.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        //
        return arCamera.transform.localToWorldMatrix * matrix;
    }

    private string VecToStr(Vector3 v)
    {
        return v.x + " " + v.y + " " + v.z;
    }

    public void Set()
    {
        if (isSet)
        {
            return;
        }
        isSet = true;
        background.enabled = false;
        Matrix4x4 matrix = GetTransform();
        Vector3 cameraPos = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        Quaternion cameraRot = Quaternion.LookRotation(forward, upwards);
        Vector3 cam_pos_0 = oldCamPos;
        Vector3 cam_pos_1 = cameraPos;
        float alphaDeg = (cameraRot.eulerAngles.x - oldCamRot.x) % 360;
        float alpha = (float) Math.PI * (alphaDeg) / 180;

        float bettaDeg = (cameraRot.eulerAngles.y - oldCamRot.y) % 360;
        float betta = (float)Math.PI * (bettaDeg) / 180;

        float gammaDeg = (cameraRot.eulerAngles.z - oldCamRot.z) % 360;
        float gamma = (float)Math.PI * (gammaDeg) / 180;

        foreach (Point p in storage.Points)
        {
            string log = "\n\n\n";
            Vector3 pos_0 = new Vector3(p.PosX, p.PosY, p.PosZ);
            Quaternion rot_1 = Quaternion.Euler(p.RotX + alphaDeg, p.RotY + bettaDeg, p.RotZ + gammaDeg);
            Vector3 delta_0 = pos_0 - cam_pos_0;

            Vector3 delta_1 = RotateX(delta_0, -alpha);
            delta_1 = RotateY(delta_1, -betta);
            delta_1 = RotateZ(delta_1, -gamma);

            Vector3 pos_1 = cam_pos_1 + delta_1;

            log += "alpha: " + alphaDeg + "\n";
            log += "betta: " + bettaDeg + "\n";
            log += "gamma: " + gammaDeg + "\n";
            log += "delta_0: " + VecToStr(delta_0) + ", Len:" + delta_0.magnitude + "\n";
            log += "delta_1: " + VecToStr(delta_1) + ", Len:" + delta_1.magnitude + "\n";
            log += "cam_pos_0: " + VecToStr(cam_pos_0) + "\n";
            log += "cam_pos_1: " + VecToStr(cam_pos_1) + "\n";
            log += "Obj_0: " + VecToStr(pos_0) + "\n";
            log += "Obj_1: " + VecToStr(pos_1) + "\n";
            Debug.Log(log);
            try
            {
                GameObject o = Instantiate(models[p.ModelType], pos_1, rot_1);
                if (objects == null)
                {
                    objects = new List<GameObject>();
                }
                objects.Add(o);
            }
            catch (Exception e)
            {
                Debug.Log("\n\n\nEXCEPTION: " + e.Message + "\n\n\n");
            }
        }
        Debug.Log(objects.Count + " objects instantiated");
    }

    private Vector3 RotateX(Vector3 v, float alpha)
    {
        Vector3 o = Vector3.zero;
        o.x = v.x;
        o.y = (float)Math.Cos(alpha) * v.y + (float)Math.Sin(alpha) * v.z;
        o.z = (float)Math.Cos(alpha) * v.z - (float)Math.Sin(alpha) * v.y;
        return o;
    }

    private Vector3 RotateY(Vector3 v, float alpha)
    {
        Vector3 o = Vector3.zero;
        o.x = (float)Math.Cos(alpha) * v.x - (float)Math.Sin(alpha) * v.z;
        o.y = v.y;
        o.z = (float)Math.Cos(alpha) * v.z + (float)Math.Sin(alpha) * v.x;
        return o;
    }

    private Vector3 RotateZ(Vector3 v, float alpha)
    {
        Vector3 o = Vector3.zero;
        o.x = (float)Math.Cos(alpha) * v.x + (float)Math.Sin(alpha) * v.y;
        o.y = (float)Math.Cos(alpha) * v.y - (float)Math.Sin(alpha) * v.x;
        o.z = v.z;
        return o;
    }
}
