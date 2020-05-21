using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionSaver
{
    public static Matrix4x4 GetCameraMatrix(Matrix4x4 lastDisplayMatrix)
    {
		Matrix4x4 matrix = lastDisplayMatrix;

		// This matrix transforms a 2D UV coordinate based on the device's orientation.
		// It will rotate, flip, but maintain values in the 0-1 range. This is technically
		// just a 3x3 matrix stored in a 4x4

		// These are the matrices provided in specific phone orientations:

#if UNITY_ANDROID

        // 1 0 0 Landscape Left (upside down)
		// 0 1 0
		// 0 0 0 
		if (Mathf.RoundToInt(matrix[0,0]) == 1 && Mathf.RoundToInt(matrix[1,1]) == 1)
		{
			matrix = Matrix4x4.Rotate( Quaternion.Euler(0,0,180) );
		}

		//-1 0 1 Landscape Right
		// 0-1 1
		// 0 0 0
		else if (Mathf.RoundToInt(matrix[0,0]) == -1 && Mathf.RoundToInt(matrix[1,1]) == -1)
		{
			matrix = Matrix4x4.identity;
		}

		// 0 1 0 Portrait
		//-1 0 1
		// 0 0 0
		else if (Mathf.RoundToInt(matrix[0,1]) == 1 && Mathf.RoundToInt(matrix[1,0]) == -1)
		{
			matrix = Matrix4x4.Rotate( Quaternion.Euler(0,0,90) );
		}

		// 0-1 1 Portrait (upside down)
		// 1 0 0
		// 0 0 0
		else if (Mathf.RoundToInt(matrix[0,1]) == -1 && Mathf.RoundToInt(matrix[1,0]) == 1)
		{
			matrix = Matrix4x4.Rotate( Quaternion.Euler(0,0,-90) );
		}

#elif UNITY_IOS

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
#endif

		else
		{
			Debug.LogWarningFormat("Unexpected Matrix provided from ARFoundation!\n{0}", matrix.ToString());
		}
		//
		//arCamera.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		//
		return matrix;
	}
}
