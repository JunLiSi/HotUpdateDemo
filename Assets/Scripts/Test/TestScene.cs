using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScene : MonoBehaviour
{
    private Toggle debugToggle;
    private void Awake()
    {
        OnInitPanel();
        OnInitEvent();
    }

    private void OnInitPanel() {
        debugToggle = transform.Find("DebugToggle").GetComponent<Toggle>();
    }

    private void OnInitEvent() {
        debugToggle.onValueChanged.AddListener(DebugToggleChange);
    }

    private void DebugToggleChange(bool isOn) {
        Debug.unityLogger.logEnabled = isOn;
    }

}
