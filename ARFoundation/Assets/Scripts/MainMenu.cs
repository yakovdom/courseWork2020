using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Scan()
    {
        SceneManager.LoadScene(1);
    }

    public void View()
    {
        SceneManager.LoadScene(2);
    }

    public void Camera()
    {
        SceneManager.LoadScene(3);
    }

    public void Quit()
    {
        Application.Quit();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
