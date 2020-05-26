using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;





public class Scanner: MonoBehaviour
{
    private ARRaycastManager rayManager;
    private PointStorage storage;
    private Rooms rooms;
    private bool can_add;
    private GameObject current;
    private List<GameObject> models;
    private int modelIndex;

    public ModelsManager modelsManager;
    public List<GameObject> objects;
    public Editor editor;
    public InputField roomName;
    public TextMeshProUGUI info;
    public GameObject left;
    public GameObject right;

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        rayManager = FindObjectOfType<ARRaycastManager>();
        objects = new List<GameObject>();
        storage = new PointStorage();
        enabled = false;
        can_add = true;
        models = modelsManager.GetModels();
        modelIndex = 0;
        SetUpArrows(false);
        if (PlayerPrefs.HasKey("rooms"))
        {
            string jsonString = PlayerPrefs.GetString("rooms");
            Debug.Log("\n\nRooms" + jsonString + "\n\n");
            rooms = JsonUtility.FromJson<Rooms>(jsonString);
        } else
        {
            Debug.Log("\n\nRooms are empty\n\n");
            rooms = new Rooms();
            rooms.Storages = new List<PointStorage>();
        }
    }

    public void SaveStorage()
    {
        
        string name = roomName.text;
        if (rooms.ContainsName(name))
        {
            Debug.Log("\n\nName is not free\n\n");
            return;
        }

        if (PlayerPrefs.HasKey("rooms"))
        {
            PlayerPrefs.DeleteKey("rooms");
            PlayerPrefs.Save();
        }
        try
        {
            storage.RoomName = name;
            storage.SetCurrentScreenshot();
            rooms.Storages.Add(storage);
            string jsonString = JsonUtility.ToJson(rooms);
            Debug.Log("___SERIALIZED___ " + jsonString);
            PlayerPrefs.SetString("rooms", jsonString);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.Log("\n\n\n" + e.GetType() + "\n\n\n");
        }
        SceneManager.LoadScene(0);
    }

    private void SetUpArrows(bool mode)
    {
        left.SetActive(mode);
        right.SetActive(mode);
    }

    
    public void Enable()
    {
        info.text = "Move your device to scan planes";
        SetUpArrows(true);
        enabled = true;
        can_add = true;
    }

    public void Disable()
    {
        editor.Disable();
        enabled = false;
        can_add = false;
        SetUpArrows(false);
        if (current != null)
        {
            Destroy(current);
        }
    }

    public void OnLeft()
    {
        if (modelIndex > 0)
        {
            modelIndex--;
        }
    }

    public void OnRight()
    {
        if (modelIndex < models.Count - 1)
        {
            modelIndex++;
        }
    }

    void Update()
    {
        if (current != null)
        {
            Destroy(current);
        }
        if (!can_add)
        {
            return;
        }
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            if (Input.GetTouch(0).position.x < Screen.width * 0.1)
            {
                OnLeft();
                return;
            }
            if (Input.GetTouch(0).position.x > Screen.width * 0.9)
            {
                OnRight();
                return;
            }
            if (Input.GetTouch(0).position.y < Screen.height * 0.2)
            {
                return;
            }
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            // Vector2 pos = Input.GetTouch(0).position;
            Vector2 pos = new Vector2(Screen.width / 2, Screen.height / 2);

            rayManager.Raycast(pos, hits, TrackableType.PlaneWithinPolygon);
            if (hits.Count == 0)
            {
                return;
            }
            GameObject currentObject = Instantiate(models[modelIndex], hits[0].pose.position, hits[0].pose.rotation);
            editor.EditObject(currentObject, (GameObject obj) => {
                can_add = true;
                SetUpArrows(true);
                if (storage == null)
                {
                    storage = new PointStorage();
                }
                storage.AddPoint(obj, modelIndex);
            });
            SetUpArrows(false);
            can_add = false;
        } else
        {
            Vector2 pos = new Vector2(Screen.width / 2, Screen.height / 2);
            
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            rayManager.Raycast(pos, hits, TrackableType.PlaneWithinPolygon);
            if (hits.Count == 0)
            {
                info.text = "Move your device to scan planes";
                return;
            }

            info.text = "Tap on the center of the screen to put object";
            current = Instantiate(models[modelIndex], hits[0].pose.position, hits[0].pose.rotation);
        }
    }
}