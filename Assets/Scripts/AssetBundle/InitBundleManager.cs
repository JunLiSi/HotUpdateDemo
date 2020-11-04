using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleInfo
{
    public AssetBundle bundle;
    public int referencedCount;
}

//实例AssetBundle控制类
public class InitBundleManager : MonoBehaviour
{
    private static InitBundleManager _instance;
    public static InitBundleManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<InitBundleManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("InitBundleManager");
                    _instance = go.AddComponent<InitBundleManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private Dictionary<string, AssetBundleInfo> assetBundleInfoDict = new Dictionary<string, AssetBundleInfo>();
    private Dictionary<string, UnityEngine.Object> prefabMapDict = new Dictionary<string, UnityEngine.Object>();

    private string prefabRootPath = "Assets/" + BundleInfo.prefabsDirName + "/";

    private string abRootPath = Application.streamingAssetsPath + "/AssetBundles/";

    private string fontRootPath = "Assets/" + BundleInfo.fontsDirName + "/";
    private string fontExt = ".ttf";
    private string audioRootPath = "Assets/" + BundleInfo.audiosDirName + "/";
    private string audioExt = ".mp3";

    private AssetBundle _mainAb;
    public AssetBundle mainAb
    {
        get
        {
            if (_mainAb == null)
            {
                _mainAb = LoadAssetBundle("AssetBundles")?.bundle;
            }
            return _mainAb;
        }
    }

    private AssetBundleManifest _manifest;
    public AssetBundleManifest manifest
    {
        get
        {
            if (_manifest == null)
            {
                _manifest = mainAb?.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
            return _manifest;
        }

    }

    public void Init(string[] fileArr, Action endAct)
    {

        abRootPath = Application.persistentDataPath + "/AssetBundles/";
        StartCoroutine(InitBundle(fileArr, endAct));
    }

    //Init ab包资源
    IEnumerator InitBundle(string[] fileArr, Action endAct)
    {
        if (fileArr == null || fileArr.Length == 0)
        {
            yield break;
        }
        AssetBundleInfo info;
        string fileName;
        AssetBundle ab;
        for (int i = 0; i < fileArr.Length; i++)
        {
            fileName = fileArr[i];
            LoadAssetBundle(fileName);
            yield return new WaitForEndOfFrame();
        }

        endAct?.Invoke();

    }

    //加载Ab包
    public AssetBundleInfo LoadAssetBundle(string assetBundleName)
    {
        AssetBundle ab = null;
        AssetBundleInfo abInfo;
        assetBundleInfoDict.TryGetValue(assetBundleName, out abInfo);
        if (abInfo != null)
        {
            return abInfo;
        }
        abInfo = new AssetBundleInfo();
        string fullPath = abRootPath + assetBundleName;
        ab = AssetBundle.LoadFromFile(fullPath);
        if (ab == null)
        {
            return null;
        }
        abInfo.bundle = ab;
        abInfo.referencedCount = 0;
        assetBundleInfoDict[assetBundleName] = abInfo;
        return abInfo;
    }

    /// <summary>
    /// 实例化物体
    /// </summary>
    /// <param name="assetName">ab包名字</param>
    /// <param name="itemName">预制体路径名</param>
    /// <returns></returns>
    public GameObject GetGameObject(string assetName, string itemName)
    {
        GameObject obj;
        UnityEngine.Object prefab = GetPrefab(assetName, itemName);
        if (prefab == null)
        {
            Debug.Log("预制体加载失败！assetName:" + assetName + "  itemName:" + itemName);
            return null;
        }
        obj = Instantiate(prefab) as GameObject;
        return obj;
    }

    /// 载入素材
    public T LoadAsset<T>(string assetName, string itemName) where T : UnityEngine.Object
    {
        assetName = assetName.ToLower();
        AssetBundleInfo bundleInfo = LoadAssetBundle(assetName);
        if (bundleInfo == null || bundleInfo.bundle == null)
        {
            Debug.LogError("assetName:" + assetName + " 加载AssetBundle失败");
            return null;
        }
        return bundleInfo.bundle.LoadAsset<T>(itemName);
    }

    /// <summary>
    /// 得到预制体
    /// </summary>
    /// <param name="assetName">ab包名字</param>
    /// <param name="itemName">预制体路径名</param>
    /// <returns></returns>
    public UnityEngine.Object GetPrefab(string assetName, string itemName)
    {
        if (string.IsNullOrEmpty(assetName) || string.IsNullOrEmpty(itemName))
        {
            return null;
        }
        string prefabKey = assetName + "_" + itemName;
        UnityEngine.Object prefab;
        string useAssetName;
        AssetBundleInfo info;
        AssetBundle bundle;
        prefabMapDict.TryGetValue(prefabKey, out prefab);
        if (prefab)
        {
            return prefab;
        }
        useAssetName = prefabRootPath + itemName + ".prefab";
        //Debug.Log(useAssetName);
        prefab = LoadAsset<UnityEngine.Object>(assetName, useAssetName);
        if (!prefab)
        {
            return null;
        }
        LoadDependencies(assetName);
        prefabMapDict[prefabKey] = prefab;
        return prefab;
    }

    //得到音频
    public AudioClip GetAudioClip(string assetName, string itemName)
    {
        AudioClip clip = null;
        itemName = audioRootPath + itemName + audioExt;
        clip = InitBundleManager.instance.LoadAsset<AudioClip>(assetName, itemName);
        return clip;
    }

    //得到字体
    public Font GetFont(string assetName, string itemName)
    {
        Font font = null;
        itemName = fontRootPath + itemName + fontExt;
        font = InitBundleManager.instance.LoadAsset<Font>(assetName, itemName);
        return font;
    }

    //得到图片
    public Sprite GetSprite(string assetName, string itemName)
    {
        return null;
    }

    /// 载入依赖
    void LoadDependencies(string name)
    {
        if (manifest == null)
        {
            return;
        }

        string[] dependencies = manifest.GetAllDependencies(name);
        if (dependencies.Length == 0) return;

        for (int i = 0; i < dependencies.Length; i++)
        {
            Debug.Log("依赖：" + dependencies[i]);
            LoadAssetBundle(dependencies[i]);
        }
    }

}
