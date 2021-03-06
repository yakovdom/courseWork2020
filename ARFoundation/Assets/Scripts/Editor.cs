﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class Editor : MonoBehaviour
{
    public GameObject editorInctance;
    public GridLayoutGroup grid;
    public RectTransform rect;
    public TextMeshProUGUI info;

    private GameObject currentObject;
    private Action<GameObject> callback;
    private float delta;

    void Start()
    {
        Screen.sleepTimeout = 10000;
        delta = 0.05f;
        int width = (int) ((double)rect.rect.width / 3);
        int height = (int)((double)rect.rect.height / 3);
        grid.cellSize = new Vector2(width, height);
        this.Disable();
    }

    public void Enable()
    {
        info.text = "Move object. Tap on OK to accept final position";
        editorInctance.SetActive(true);
    }

    public void Disable()
    {
        editorInctance.SetActive(false);
    }

    public void EditObject(GameObject o, Action<GameObject> cb) {
        this.Enable();
        currentObject = o;
        callback = cb;
    }

    public void OK()
    {
        callback(currentObject);
        callback = null;
        currentObject = null;
        this.Disable();
    }

    private void ChangePosition(Vector3 shift)
    {
        float x = currentObject.transform.position.x + shift.x;
        float y = currentObject.transform.position.y + shift.y;
        float z = currentObject.transform.position.z + shift.z;
        Vector3 position = new Vector3(x, y, z);
        Quaternion rotation = currentObject.transform.rotation;
        Destroy(currentObject);
        currentObject = Instantiate(currentObject, position, rotation);
    }

    public void Forward()
    {
        float alpha = currentObject.transform.rotation.eulerAngles.y;
        float dx = (float)Math.Cos(Math.PI * alpha / 180);
        float dz = (float)Math.Sin(Math.PI * alpha / 180);
        ChangePosition(new Vector3(-delta * dx, 0, delta * dz));
    }

    public void Backward()
    {
        float alpha = currentObject.transform.rotation.eulerAngles.y;
        float dx = (float)Math.Cos(Math.PI * alpha / 180);
        float dz = (float)Math.Sin(Math.PI * alpha / 180);
        ChangePosition(new Vector3(delta * dx, 0, -delta * dz));
    }

    public void Left()
    {
        float alpha = currentObject.transform.rotation.eulerAngles.y;
        float dx = (float) Math.Sin(Math.PI * alpha / 180);
        float dz = (float)Math.Cos(Math.PI * alpha / 180);
        // ChangePosition(new Vector3(-delta, 0, 0));
        ChangePosition(new Vector3(-delta * dx, 0, -delta * dz));
    }

    public void Right()
    {
        float alpha = currentObject.transform.rotation.eulerAngles.y;
        float dx = (float)Math.Sin(Math.PI * alpha / 180);
        float dz = (float)Math.Cos(Math.PI * alpha / 180);
        // ChangePosition(new Vector3(delta, 0, 0));
        ChangePosition(new Vector3(delta * dx, 0, delta * dz));
    }

    public void Up()
    {
        ChangePosition(new Vector3(0, delta, 0));
    }

    public void Down()
    {
        ChangePosition(new Vector3(0, -delta, 0));
    }

    public void Rotate1()
    {
        Vector3 position = currentObject.transform.position;
        Quaternion rotation = currentObject.transform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y - 10, rotation.eulerAngles.z);
        Destroy(currentObject);
        currentObject = Instantiate(currentObject, position, rotation);
    }

    public void Rotate2()
    {
        Vector3 position = currentObject.transform.position;
        Quaternion rotation = currentObject.transform.rotation;
        rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y + 10, rotation.eulerAngles.z);
        Destroy(currentObject);
        currentObject = Instantiate(currentObject, position, rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
