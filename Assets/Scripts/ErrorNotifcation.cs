using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorNotifcation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TextMeshProUGUI timerText;
    private int timer;
    public void SetErrorText(string text)
    {
        errorText.text = text;
    }

    public void SetTimer(double destroyTimer)
    {
        timer = (int) destroyTimer;
    }

    // Update is called once per frame
    void Update()
    {
        timerText.text = timer.ToString();
    }
}
