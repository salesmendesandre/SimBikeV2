using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI warningText;
    public void SetWarningText(string text)
    {
        warningText.text = text;
    }
}
