using System;
using System.Collections;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;



public class Screenshotter : MonoBehaviour
{
    private ARCameraManager cameraManager;
    public XRCameraImage image;
    public RawImage background;
    public AspectRatioFitter Fit;
    private Matrix4x4 lastDisplayMatrix;
    private Camera arCamera;


    // Start is called before the first frame update
    void Start()
    {
        cameraManager = FindObjectOfType<ARCameraManager>();
        cameraManager.enabled = true;
        cameraManager.frameReceived += Process;
        if (background != null)
        {
            background.enabled = false;
        }
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.name == "AR Camera")
            {
                arCamera = cam;
            }
        }
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


    // Update is called once per frame
    void Update()
    {
    }

    public unsafe void GetScreenShot()
    {
        if (!cameraManager.TryGetLatestImage(out image))
        {
            return;
        }
        var format = TextureFormat.RGBA32;

        Texture2D texture = new Texture2D(image.width, image.height, format, false);

        var conversionParams = new XRCameraImageConversionParams {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = CameraImageTransformation.MirrorY
        };

        var rawTextureData = texture.GetRawTextureData<byte>();
        try
        {
            IntPtr ptr = new IntPtr(rawTextureData.GetUnsafePtr());
            image.Convert(conversionParams, ptr, rawTextureData.Length);
        }
        finally
        {
            // We must dispose of the XRCameraImage after we're finished
            // with it to avoid leaking native resources.
            image.Dispose();
        }
        // Apply the updated texture data to our texture
        texture.Apply();

        // Set the RawImage's texture so we can visualize it
        float ratio = (float)texture.height / texture.width;
        if (Fit != null) {
            Fit.aspectRatio = 1f / ratio;
            Fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }
        if (background != null)
        {
            background.texture = texture;
            background.rectTransform.localEulerAngles = new Vector3(0, 0, 90);
            background.rectTransform.localScale = new Vector3(ratio, ratio, 1f);
            background.enabled = true;
        }
        StartCoroutine(SaveTexture(texture));
    }

    private void FillPositionAndRotation(ScreenShot sh)
    {
        Matrix4x4 matrix = GetTransform();
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        Quaternion rot = Quaternion.LookRotation(forward, upwards);

        Vector3 cameraPos = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        Vector3 eulerAngles = rot.eulerAngles;

        sh.PosX = cameraPos.x;
        sh.PosY = cameraPos.y;
        sh.PosZ = cameraPos.z;
        sh.RotX = eulerAngles.x;
        sh.RotY = eulerAngles.y;
        sh.RotZ = eulerAngles.z;
    }

    IEnumerator SaveTexture(Texture2D texture)
    {
        ScreenShot sh = new ScreenShot();
        FillPositionAndRotation(sh);

        string fileName = "Photo";
        string screenshotFilename;
        string date = System.DateTime.Now.ToString("ddMMyyHHmmss");
        screenshotFilename = fileName + "_" + date + ".png";
        string path = Application.persistentDataPath + "/" + screenshotFilename;
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        sh.FileName = path;
        sh.w = texture.width;
        sh.h = texture.height;
        string jsonString = JsonUtility.ToJson(sh);
        Debug.Log("___SERIALIZED___ " + jsonString);
        if (PlayerPrefs.HasKey("photo"))
        {
            PlayerPrefs.DeleteKey("photo");
            PlayerPrefs.Save();
        }
        try
        {
            PlayerPrefs.SetString("photo", jsonString);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.Log("\n\n\n" + e.GetType() + "\n\n\n");
        }
        yield return new WaitForEndOfFrame();
    }
}
