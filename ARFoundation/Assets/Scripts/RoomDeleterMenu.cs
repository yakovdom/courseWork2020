using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomDeleterMenu : MonoBehaviour
{
    // Start is called before the first frame update

    public Button left;
    public Button right;
    public Text roomName;
    public GridLayoutGroup grid;
    public RectTransform Rect;
    private int index;
    private List<string> names;
    private Rooms rooms;

    void Start()
    {
        index = 0;
        int width = (int)((double)Rect.rect.width / 2);
        int height = (int)((double)Rect.rect.height / 2);
        grid.cellSize = new Vector2(width, height);
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

    public void Delete()
    {
        if (names != null)
        {
            rooms.DeleteRoom(index);
            names.RemoveAt(index);
            if (index == names.Count)
            {
                index--;
            }

            if (index < 0)
            {
                roomName.text = "No rooms created";
            } else
            {
                roomName.text = names[index];
            }

            if (PlayerPrefs.HasKey("rooms"))
            {
                PlayerPrefs.DeleteKey("rooms");
                PlayerPrefs.Save();
            }
            try
            {
                string jsonString = JsonUtility.ToJson(rooms);
                Debug.Log("___SERIALIZED___ " + jsonString);
                PlayerPrefs.SetString("rooms", jsonString);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.Log("\n\n\n" + e.GetType() + "\n\n\n");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
