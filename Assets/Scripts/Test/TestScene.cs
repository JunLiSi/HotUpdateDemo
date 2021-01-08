using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScene : MonoBehaviour
{
    private string debugKey = "DEBUG_ISON";
    private int debugValue;
    private Toggle debugToggle;
    private Reporter reporter;
    private void Awake()
    {
        OnInitPanel();
        OnInitEvent();
    }

    private void OnEnable()
    {
        debugValue= PlayerPrefs.GetInt(debugKey);
        DebugToggleChange(debugValue==1);

    }

    private void OnInitPanel() {
        debugToggle = transform.Find("DebugToggle").GetComponent<Toggle>();
        reporter = Object.FindObjectOfType<Reporter>();
    }

    private void OnInitEvent() {
        debugToggle.onValueChanged.AddListener(DebugToggleChange);
    }

    private void DebugToggleChange(bool isOn) {
        PlayerPrefs.SetInt(debugKey,isOn?1:0);
        Debug.unityLogger.logEnabled = isOn;
        reporter.gameObject.SetActive(isOn);
    }

}
