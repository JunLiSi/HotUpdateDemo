using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class LuaBehaviour : MonoBehaviour
{
    [CSharpCallLua]
    public delegate void CallBack(LuaTable luaTable, GameObject go);

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


    public void Init(LuaTable tb) {
        luaEnv = LuaManager.instance.GetLuaEnv();
        mLuaTable = tb;
        if (mLuaTable==null)
        {
            return;
        }
        mLuaTable["gameObject"] = gameObject;
        mLuaTable["transform"] = transform;
        mLuaTable["luahaviour"] = this;

        mLuaTable["Awake"] = luaAwake;
        mLuaTable["Start"] = luaStart;
    }

    #region Behaviour函数

    public void OnEnable()
    {
        luaOnEnable?.Invoke(mLuaTable,gameObject);
    }

    public void Awake()
    {
        luaAwake?.Invoke(mLuaTable,gameObject);
    }

    public void Start()
    {
        luaStart?.Invoke(mLuaTable,gameObject);
    }

    public void FixedUpdate()
    {
        luaFixedUpdate?.Invoke(mLuaTable,gameObject);
    }

    public void Update()
    {
        luaUpdate?.Invoke(mLuaTable,gameObject);
    }

    public void LateUpdate()
    {
        luaLateUpdate?.Invoke(mLuaTable,gameObject);
    }

    public void OnDisable()
    {
        luaOnDisable?.Invoke(mLuaTable,gameObject);
    }

    public void OnDestroy()
    {
        luaOnDestroy?.Invoke(mLuaTable, gameObject);
    }

    #endregion 
}
