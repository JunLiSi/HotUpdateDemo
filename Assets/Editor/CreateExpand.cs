using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateExpand : Editor
{
    [MenuItem("GameObject/UI/CircleImage")]
    static void CreateCicirImage() {
        GameObject go = Selection.activeGameObject;
        if (go==null)
        {
            return;
        }
        GameObject circleObj = new GameObject("CircleImage");
        circleObj.AddComponent<RectTransform>();
        circleObj.AddComponent<CircleImage>();
        circleObj.transform.SetParent(go.transform);
        circleObj.transform.localPosition = Vector3.zero;
        circleObj.transform.localScale = Vector3.one;
    }
}
