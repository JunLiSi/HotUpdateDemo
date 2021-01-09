using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class LuaManager : MonoBehaviour
{
    private static LuaManager _instance;
    public static LuaManager instance {
        get {
            if (_instance==null)
            {
                _instance = GameObject.FindObjectOfType<LuaManager>();
                if (_instance==null)
                {
                    GameObject go = new GameObject("LuaManager");
                    _instance = go.AddComponent<LuaManager>();
                    DontDestroyOnLoad(go);
                }
                _instance.InitLuaEnv();
            }
            return _instance;
        }
    }

    private string luaRootPath = "Assets/"+BundleInfo.tempLuaDirName+"/";

    private LuaEnv luaEnv;
    //ab包中所有lua脚本对应的byte[]字典
    public Dictionary<string, byte[]> luaByteDict = new Dictionary<string, byte[]>();

   
    public void Init(string[] fileArr,Action endAct) {
        StartCoroutine(LoadBundleLua(fileArr, endAct));
    }

    //初始化自定义Lua
    public void InitLua() {
        if (luaEnv == null) {
            InitLuaEnv();
        }
        DOLua("Common/InitLua");
    }

    //调用Lua脚本
    public void DOLua(string luaScriptName,LuaTable env=null) {
        if (luaEnv==null)
        {
            InitLuaEnv();
        }
        byte[] bytes = CustomLoader(ref luaScriptName);
        if (bytes != null && bytes.Length > 0)
        {
            luaEnv.DoString(bytes, luaScriptName, env);
        }
        else {
            Debug.LogError("【LuaScript:+"+luaScriptName+"】"+"未找到");
        }
        
    }

    //得到LuaEnv
    public LuaEnv GetLuaEnv() {
        return luaEnv;
    }

    //创建LuaTable
    public LuaTable NewLuaTable() {
        if (luaEnv==null)
        {
            InitLuaEnv();
        }
        return luaEnv.NewTable();
    }

    //自定义Lua脚本加载器
    public byte[] CustomLoader(ref string fileName) {
        string filePath;
        byte[] bytes=null;
        if (AppCfg.bundleModel)
        {
            filePath = fileName.Replace('.','/').ToLower();
            if (luaByteDict==null||luaByteDict.Count==0)
            {
                return bytes;
            }
            luaByteDict.TryGetValue(filePath,out bytes);
            return bytes;
        }
        else {
            filePath = fileName.Replace('.', '/').ToLower();
            luaByteDict.TryGetValue(filePath,out bytes);
            if (bytes == null)
            {
                string localFilePath = Application.dataPath + "/Lua/" + fileName.Replace('.', '/') + ".lua";
                if (File.Exists(localFilePath))
                {
                    bytes = File.ReadAllBytes(localFilePath);
                    luaByteDict[filePath] = bytes;
                    Debug.Log("【本地路径中加载lua】:" + fileName);
                }
            }
            else {
                Debug.Log("【字典中加载lua】:"+fileName);
            }
            return bytes;
        }
        
    }

    //加载AssetBundles中的Lua脚本
    IEnumerator LoadBundleLua(string[] assetNames, Action endAct)
    {
        if (assetNames == null || assetNames.Length == 0)
        {
            yield break;
        }
        string assetName;
        AssetBundle ab;
        string[] bundleNameArr;
        string bundleName;
        UnityEngine.Object luaObj;
        for (int i = 0; i < assetNames.Length; i++)
        {
            assetName = assetNames[i];
            Debug.LogError("AssetName:"+assetName);
            ab = InitBundleManager.instance.LoadAssetBundle(assetName)?.bundle;
            if (ab == null)
            {
                continue;
            }
            bundleNameArr = ab.GetAllAssetNames();
            if (bundleNameArr == null || bundleNameArr.Length == 0)
            {
                continue;
            }
            for (int j = 0; j < bundleNameArr.Length; j++)
            {
                bundleName = bundleNameArr[j];
                string[] splitStr = bundleName.Split('/');
                bundleName = luaRootPath + splitStr[splitStr.Length - 2] + "/" + splitStr[splitStr.Length - 1];
                luaObj = ab.LoadAsset(bundleName);
                TextAsset ta = (TextAsset)Instantiate(luaObj);
                bundleName = bundleName.Replace(luaRootPath, string.Empty).Replace(".lua.bytes", string.Empty);
                luaByteDict[bundleName] = ta.bytes;
            }
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("【Lua脚本初始化完毕！】");
        endAct?.Invoke();
    }

    //初始化LuaEnv
    void InitLuaEnv()
    {
        luaEnv = new LuaEnv();
        LuaEnv.CustomLoader method = CustomLoader;
        luaEnv.AddLoader(method);
    }

    private void OnDestroy()
    {
        if (luaEnv!=null)
        {
            luaEnv.Dispose();
        }
        
    }

}
