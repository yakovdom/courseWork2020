using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScannerMenu : MonoBehaviour
{
    public GameObject Save;
    public GameObject RoomName;
    public Scanner scanner;

    public void Back()
    {
        scanner.Disable();
        if (!Save.active)
        {
            Save.SetActive(true);
            RoomName.SetActive(true);
            // scanner.Disable();
            return;
        }
        SceneManager.LoadScene(0);
    }

    // Start is called before the first frame update
    void Start()
    {
        Save.SetActive(false);
        RoomName.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
