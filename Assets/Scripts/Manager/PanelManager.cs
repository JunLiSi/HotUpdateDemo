using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class PanelManager : MonoBehaviour
{
    private static PanelManager _instance;
    public static PanelManager instance {
        get {
            if (!_instance)
            {
                _instance = FindObjectOfType<PanelManager>();
                if (!_instance)
                {
                    GameObject obj = new GameObject("PanelManager");
                    _instance=obj.AddComponent<PanelManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    private LuaTable luaClass;

    [CSharpCallLua]
    public delegate void InstantiatePanelFun(string panelName,Transform parent);

    public InstantiatePanelFun InstantiatePanelEvent;


    public void Init(LuaTable _luaClass)
    {
        Debug.LogError("Init C# PanelManager");
        luaClass = _luaClass;
        luaClass.Get("InstantiatePanel",out InstantiatePanelEvent);
    }


    public void InstantiatePanel(string panelName,Transform parent) {
        InstantiatePanelEvent?.Invoke(panelName,parent);
    }

}
