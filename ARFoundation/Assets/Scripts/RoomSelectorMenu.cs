using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomSelectorMenu : MonoBehaviour
{
    // Start is called before the first frame update

    public Button left;
    public Button right;
    public Text roomName;
    public GameObject background;
    public GameObject menuInstance;
    public GameObject roomNameInstance;
    public Viewer viewer;
    private int index;
    private List<string> names;
    private Rooms rooms;

    void Start()
    {
        index = 0;
        if (PlayerPrefs.HasKey("rooms"))
        {
            string jsonString = PlayerPrefs.GetString("rooms");
            Debug.Log("\n\nRooms" + jsonString + "\n\n");
            rooms = JsonUtility.FromJson<Rooms>(jsonString);
            names = new List<string>();
            foreach (PointStorage room in rooms.Storages)
            {
                names.Add(room.RoomName);
            }
            roomName.text = names[index];
        }
        else
        {
            Debug.Log("\n\nRooms are empty\n\n");
            roomName.text = "No rooms created";
        }
    }

    public void OnLeft()
    {
        if (index == 0)
        {
            return;
        }
        index--;
        roomName.text = names[index];
    }

    public void OnRight()
    {
        if (index == names.Count - 1)
        {
            return;
        }
        index++;
        roomName.text = names[index];
    }

    public void Back()
    {
        SceneManager.LoadScene(0);
    }

    public void Select()
    {
        if (names.Count == 0)
        {
            return;
        }
        background.SetActive(false);
        menuInstance.SetActive(false);
        roomNameInstance.SetActive(false);
        viewer.OnEnable(index);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
