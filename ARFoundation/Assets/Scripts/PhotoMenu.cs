using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotoMenu : MonoBehaviour
{
    public GameObject menu;
    public RectTransform rect;
    public TextMeshProUGUI info;
    public string infoText;
    // Start is called before the first frame update
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        info.text = infoText;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.rect.width);
    }

    public void OnClick()
    {
        info.text = "";
        menu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
