using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class LuaBehaviour : MonoBehaviour
{
    public string luaScriptName;
    [CSharpCallLua]
    public delegate void CallBack();

    private LuaEnv luaEnv=null;
    private LuaTable mLuaTable = null;
    private CallBack luaAwake;
    private CallBack luaStart;
    private CallBack luaUpdate;
    private CallBack luaOnDestroy;
    private CallBack luaFixedUpdate;
    private CallBack luaLateUpdate;
    private CallBack luaOnEnable;
    private CallBack luaOnDisable;

    public void Init(string luaName) {
        luaScriptName = luaName;
        luaEnv = LuaManager.instance.GetLuaEnv();
        mLuaTable = luaEnv.NewTable();
        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        LuaTable meta = luaEnv.NewTable();
        meta.Set("__index",luaEnv.Global);
        mLuaTable.SetMetaTable(meta);
        meta.Dispose();
        mLuaTable.Set("transform",transform);
        mLuaTable.Set("gameObject", gameObject);
        Debug.LogError("加载Lua:"+luaScriptName);
        LuaManager.instance.DOLua(luaScriptName,mLuaTable);

        mLuaTable.Get("Awake", out luaAwake);
        mLuaTable.Get("OnEnable", out luaOnEnable);
        mLuaTable.Get("Start", out luaStart);
        mLuaTable.Get("Update", out luaUpdate);
        mLuaTable.Get("LateUpdate", out luaLateUpdate);
        mLuaTable.Get("OnDisable", out luaOnDisable);
        mLuaTable.Get("OnDestroy", out luaOnDestroy);
        luaAwake?.Invoke();
        luaOnEnable?.Invoke();
    }

    #region Behaviour函数

    public void Awake()
    {
        luaAwake?.Invoke();
    }

    public void OnEnable()
    {
        luaOnEnable?.Invoke();
    }

    public void Start()
    {
        luaStart?.Invoke();
    }

    public void FixedUpdate()
    {
        luaFixedUpdate?.Invoke();
    }

    public void Update()
    {
        luaUpdate?.Invoke();
    }

    public void LateUpdate()
    {
        luaLateUpdate?.Invoke();
    }

    public void OnDisable()
    {
        luaOnDisable?.Invoke();
    }

    public void OnDestroy()
    {
        luaOnDestroy?.Invoke();
        luaOnDestroy = null;
        luaAwake = null;
        luaOnEnable = null;
        luaStart = null;
        luaOnDisable = null;
        luaUpdate = null;
        luaLateUpdate = null;
        luaFixedUpdate = null;
        mLuaTable?.Dispose();
    }

    #endregion 
}
