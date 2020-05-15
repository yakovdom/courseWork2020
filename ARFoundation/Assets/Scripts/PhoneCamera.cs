using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCamera : MonoBehaviour
{
	private WebCamTexture Camera;
	private Texture DefaultBackground;
	private bool Stream;
	public RawImage Background;
	public AspectRatioFitter Fit;


	// Start is called before the first frame update
	private void Start()
    {
		Stream = true;
		DefaultBackground = Background.texture;

		foreach (WebCamDevice device in WebCamTexture.devices)
        {
            if (!device.isFrontFacing)
            {
				Camera = new WebCamTexture(device.name, Screen.width, Screen.height);
				Camera.Play();
				Background.texture = Camera;
				break;
            }
        }
    }

    private void Update()
    {
        if (!Stream)
        {
			Camera.Pause();
			return;
        }
		float ratio = (float)Camera.width / (float)Camera.height;
		Fit.aspectRatio = ratio;

		float scaleY = Camera.videoVerticallyMirrored ? -1f : 1f;
		Background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

		int orient = -Camera.videoRotationAngle;
		Background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }

    public void TakePhoto()
    {
		Color[] photo = Camera.GetPixels();
		int h = Camera.height;
		int w = Camera.width;
		string photostr = "" + photo.Length + " " + Screen.width * Screen.height + " ";
        for (int i = 0; i < 10; i++)
        {
			photostr += "(" + photo[i].r + ", " + photo[i].g + ", " + photo[i].b + "), ";
		}

		photostr += "\n";
		Debug.Log(photostr);
		Texture2D texture = new Texture2D(Screen.width, Screen.height);
		Color[] pixels = new Color[Screen.width * Screen.height];
        for (int i = 0; i < Screen.width; ++i)
        {
			for (int j = 0; j < Screen.height; ++j)
			{
				pixels[i * Screen.width + j] = photo[i * w + j];
			}
		}
		texture.SetPixels(pixels);
		Background.texture = texture;

        /*
		BinaryFormatter bf = new BinaryFormatter();

		Debug.Log("after Binary formatter");

		Debug.Log(Application.persistentDataPath);

		FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

		Debug.Log("Created file");


		bf.Serialize(file, photo);

		Debug.Log("after Serialize");

		file.Close();
        */
        
		Stream = false;
	}
    
	void Awake()
	{
		// Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
	}
    


	/*
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			// Don't attempt to use the camera if it is already open
			if (NativeCamera.IsCameraBusy())
				return;

			if (Input.mousePosition.x < Screen.width / 2)
			{
				// Take a picture with the camera
				// If the captured image's width and/or height is greater than 512px, down-scale it
				TakePicture(512);
			}
			else
			{
				// Record a video with the camera
				RecordVideo();
			}
		}
	}

	private void TakePicture(int maxSize)
	{
		NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
		{
			Debug.Log("Image path: " + path);
			if (path != null)
			{
				// Create a Texture2D from the captured image
				Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize);
				if (texture == null)
				{
					Debug.Log("Couldn't load texture from " + path);
					return;
				}

				// Assign texture to a temporary quad and destroy it after 5 seconds
				GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

                
				quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
				quad.transform.forward = Camera.main.transform.forward;
				quad.transform.localScale = new Vector3(1f, texture.height / (float)texture.width, 1f);

				Material material = quad.GetComponent<Renderer>().material;
				if (!material.shader.isSupported) // happens when Standard shader is not included in the build
					material.shader = Shader.Find("Legacy Shaders/Diffuse");

				material.mainTexture = texture;

				Destroy(quad, 5f);

				// If a procedural texture is not destroyed manually, 
				// it will only be freed after a scene change
				Destroy(texture, 5f);
			}
		}, maxSize);

		Debug.Log("Permission result: " + permission);
	}

	private void RecordVideo()
	{
		NativeCamera.Permission permission = NativeCamera.RecordVideo((path) =>
		{
			Debug.Log("Video path: " + path);
			if (path != null)
			{
				// Play the recorded video
				Handheld.PlayFullScreenMovie("file://" + path);
			}
		});

		Debug.Log("Permission result: " + permission);
	}
    */
}
