using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Viewer : MonoBehaviour
{
    private ARCameraManager cameraManager;
    private Matrix4x4 lastDisplayMatrix;
    private Camera arCamera;
    private ARAnchorManager refPointManager;
    private PointStorage storage;
    private bool isSet;
    public List<ARAnchor> points;
    public RawImage background;
    public AspectRatioFitter Fit;
    public GameObject objectToSpawn;
    private Vector3 oldCamPos;
    private Vector3 oldCamRot;
    private List<GameObject> objects;
    //
    private ARRaycastManager rayManager;
    public Text debug;

    // Start is called before the first frame update
    void Start()
    {
        cameraManager = FindObjectOfType<ARCameraManager>();
        cameraManager.enabled = true;
        cameraManager.frameReceived += Process;
        background.enabled = false;
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.name == "AR Camera")
            {
                arCamera = cam;
            }
        }
        StartCoroutine(SetImage());
        if (!PlayerPrefs.HasKey("points"))
        {
            return;
        }
        string jsonString = PlayerPrefs.GetString("points");
        storage = JsonUtility.FromJson<PointStorage>(jsonString);
        objects = new List<GameObject>();
        //
        rayManager = FindObjectOfType<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 touchPos = new Vector2(Screen.width / 2, Screen.height / 2);
        
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        rayManager.Raycast(touchPos, hits, TrackableType.PlaneWithinPolygon);
        if (hits.Count == 0)
        {
            return;
        }
        debug.text = VecToStr(hits[0].pose.position);
    }

    private IEnumerator SetImage()
    {
        if (!PlayerPrefs.HasKey("photo"))
        {
            yield return new WaitForEndOfFrame();
        }
        string jsonString = PlayerPrefs.GetString("photo");
        ScreenShot sh = JsonUtility.FromJson<ScreenShot>(jsonString);
        byte[] bytes = File.ReadAllBytes(sh.FileName);
        Texture2D texture = new Texture2D(sh.w, sh.h);
        texture.LoadImage(bytes);
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
        Matrix4x4 matrix = lastDisplayMatrix;

        // This matrix transforms a 2D UV coordinate based on the device's orientation.
        // It will rotate, flip, but maintain values in the 0-1 range. This is technically
        // just a 3x3 matrix stored in a 4x4

        // These are the matrices provided in specific phone orientations:


        // 0-.6 0 Portrait
        //-1  1 0 The source image is upside down as well, so this is identity
        // .8 1 
        if (Mathf.RoundToInt(matrix[0, 0]) == 0)
        {
            matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90));
        }

        //-1  0 0 Landscape Right
        // 0 .6 0
        // 1 .2 1
        else if (Mathf.RoundToInt(matrix[0, 0]) == -1)
        {
            matrix = Matrix4x4.identity;
        }

        // 1  0 0 Landscape Left
        // 0-.6 0
        // 0 .8 1
        else if (Mathf.RoundToInt(matrix[0, 0]) == 1)
        {
            matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180));
        }

        else
        {
            Debug.LogWarningFormat("Unexpected Matrix provided from ARFoundation!\n{0}", matrix.ToString());
        }
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
        Debug.Log("\n\n\nROTATION: " + cameraRot.eulerAngles.ToString() + "\n\n\n");
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
            /*
            Vector3 delta_1 = Vector3.zero;
            delta_1.x = (float) Math.Cos(betta) * delta_0.x + (float) Math.Sin(betta) * delta_0.z;
            delta_1.y = delta_0.y;
            delta_1.z = (float) Math.Cos(betta) * delta_0.z - (float) Math.Sin(betta) * delta_0.x;
            */

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
            GameObject o = Instantiate(objectToSpawn, pos_1, rot_1);
            objects.Add(o);
        }
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
