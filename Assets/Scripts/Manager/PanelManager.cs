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
    [CSharpCallLua]
    public delegate void HidePanelFun(string panelName,bool onDestroy);
    [CSharpCallLua]
    public delegate bool ReturnBoolDelegate(string panelName);
    [CSharpCallLua]
    public delegate Transform GetUIRootDelegate();

    public InstantiatePanelFun ShowPanelEvent;
    public HidePanelFun HidePanelEvent;
    public GetUIRootDelegate GetUIRootEvent;
    public ReturnBoolDelegate IsShowEvent;

    public void Init(LuaTable _luaClass)
    {
        Debug.LogError("Init C# PanelManager");
        luaClass = _luaClass;
        luaClass.Get("ShowPanel", out ShowPanelEvent);
        luaClass.Get("HidePanel", out HidePanelEvent);
        luaClass.Get("GetUIRoot", out GetUIRootEvent);
        luaClass.Get("IsShow", out IsShowEvent);
        //ShowPanelEvent =luaClass.Get<InstantiatePanelFun>("ShowPanel");
        //HidePanelEvent = luaClass.Get<HidePanelFun>("HidePanel");
    }


    public void ShowPanel(string panelName,Transform parent) {
        ShowPanelEvent?.Invoke(panelName,parent);
    }

    public void HidePanel(string panelName,bool onDestroy=false) {
        HidePanelEvent?.Invoke(panelName, onDestroy);
    }

    public Transform GetUIRoot() {
       return GetUIRootEvent==null?null:GetUIRootEvent();
    }

    public bool IsShow(string panelName) {
        return IsShowEvent==null?false:IsShowEvent(panelName);
    }

}
