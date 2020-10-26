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

    public static bool bundleMode = true;//加载BundleLua模式还是本地Lua模式
    private string luaRootPath = "Assets/"+BundleInfo.tempLuaDirName+"/";

	private LuaEnv luaEnv;
    //ab包中所有lua脚本对应的byte[]字典
    public Dictionary<string, byte[]> luaByteDict = new Dictionary<string, byte[]>();

   
    public void Init(string[] fileArr,Action endAct) {
        StartCoroutine(LoadBundleLua(fileArr, endAct));
    }

    //加载AssetBundles中的Lua脚本
    IEnumerator LoadBundleLua(string[] assetNames,Action endAct) {
        if (assetNames==null||assetNames.Length==0)
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
            ab = InitBundleManager.instance.LoadAssetBundle(assetName);
            if (ab==null)
            {
                continue;
            }
            bundleNameArr = ab.GetAllAssetNames();
            if (bundleNameArr==null||bundleNameArr.Length==0)
            {
                continue;
            }
            for (int j = 0; j < bundleNameArr.Length; j++)
            {
                bundleName = bundleNameArr[j];
                string[] splitStr = bundleName.Split('/');
                bundleName = luaRootPath+ splitStr[splitStr.Length - 2]+"/"+splitStr[splitStr.Length - 1];
                luaObj = ab.LoadAsset(bundleName);
                TextAsset ta = (TextAsset)Instantiate(luaObj);
                bundleName = bundleName.Replace(luaRootPath,string.Empty).Replace(".lua.bytes",string.Empty);
                luaByteDict[bundleName] = ta.bytes;
            }
            yield return new WaitForEndOfFrame();
        }
        endAct?.Invoke();
    }

    void InitLuaEnv() {
        luaEnv = new LuaEnv();
        LuaEnv.CustomLoader method = CustomLoader;
        luaEnv.AddLoader(method);
    }

    public LuaEnv GetLuaEnv() {
        return luaEnv;
    }

    public byte[] CustomLoader(ref string fileName) {
        string filePath;
        byte[] bytes=null;
        if (bundleMode)
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
            filePath = Application.dataPath + "/Lua/" + fileName.Replace('.', '/') + ".lua";
            Debug.LogError(filePath);
            if (File.Exists(filePath))
            {
                bytes = File.ReadAllBytes(filePath);
            }
            return bytes;
        }
        
    }

    private void OnDestroy()
    {
        luaEnv?.Dispose();
    }

}
