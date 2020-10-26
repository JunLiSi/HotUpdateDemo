using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleInfo {
    public AssetBundle bundle;
    public int referencedCount;
}

public class InitBundleManager:MonoBehaviour
{
    private static InitBundleManager _instance;
    public static InitBundleManager instance {
        get {
            if (_instance==null)
            {
                _instance = GameObject.FindObjectOfType<InitBundleManager>();
                if (_instance ==null)
                {
                    GameObject go = new GameObject("InitBundleManager");
                    _instance = go.AddComponent<InitBundleManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public Dictionary<string, AssetBundleInfo> assetBundleInfoDict=new Dictionary<string, AssetBundleInfo>();
    public Dictionary<string, GameObject> prefabMapDict = new Dictionary<string, GameObject>();

    public string prefabRootPath = "Assets/BundleResources/Prefabs/";

    public string abRootPath = Application.streamingAssetsPath + "/AssetBundles/";

    public void Init(string[] fileArr,Action endAct) {
        StartCoroutine(InitBundle(fileArr,endAct));
    }

    //Init ab包资源
    IEnumerator InitBundle(string[] fileArr,Action endAct) {
        if (fileArr ==null||fileArr.Length==0)
        {
            yield break;
        }
        AssetBundleInfo info;
        string fileName;
        AssetBundle ab;
        for (int i = 0; i < fileArr.Length; i++)
        {
            fileName = fileArr[i];
            ab = LoadAssetBundle(fileName);
            if (ab!=null)
            {
                info = new AssetBundleInfo();
                info.bundle = ab;
                info.referencedCount = 0;
                assetBundleInfoDict[fileName] = info;
            }
            yield return new WaitForEndOfFrame();
        }

        endAct?.Invoke();
    }

    //加载Ab包
    public AssetBundle LoadAssetBundle(string assetBundleName) {
        AssetBundle ab = null;
        AssetBundleInfo abInfo;
        assetBundleInfoDict.TryGetValue(assetBundleName, out abInfo);
        if (abInfo!=null)
        {
            ab = abInfo.bundle;
            return ab;
        }
        string fullPath = abRootPath + assetBundleName;
        ab = AssetBundle.LoadFromFile(fullPath);
        return ab;
    }

    /// <summary>
    /// 实例预制体
    /// </summary>
    /// <param name="assetName">ab包名字</param>
    /// <param name="itemName">预制体路径名</param>
    /// <returns></returns>
    public GameObject GetGameObject(string assetName,string itemName) {
        if (string.IsNullOrEmpty(assetName))
        {
            return null;
        }
        AssetBundleInfo info;
        AssetBundle bundle;
        UnityEngine.Object obj;
        GameObject instantiateObj;
        string useAssetName;
        prefabMapDict.TryGetValue(assetName,out instantiateObj);
        if (instantiateObj)
        {
            return instantiateObj;
        }
        assetBundleInfoDict.TryGetValue(assetName, out info);
        if (info == null)
        {
            return null;
        }
        useAssetName = prefabRootPath +itemName+".prefab";
        bundle = info.bundle;
        Debug.Log(useAssetName);
        obj = bundle.LoadAsset(useAssetName);
        instantiateObj = (GameObject)Instantiate(obj);
        prefabMapDict[assetName] = instantiateObj;
        return instantiateObj;
    }


}
