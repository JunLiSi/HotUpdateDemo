using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DownFileInfo {
    public string fileUrl;
    public string savePath;
    public long size;
    public string fileName;
}

public class ObjInfo {
    public string name;//资源名字
    public string assetName;//在Bundle对应的AssetName
    public string localPath;//测试时的本地路径
    public string formalPath;//正式资源路径

}

public class AssetBundleManager : MonoBehaviour
{
    private static AssetBundleManager _instance;
    public static AssetBundleManager instance {
        get {
            if (_instance==null)
            {
                _instance = GameObject.FindObjectOfType<AssetBundleManager>();
                if (_instance==null)
                {
                    GameObject obj = new GameObject("AssetBundleManager");
                    _instance = obj.AddComponent<AssetBundleManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    //本地AB包根目录
    string localRootPath;
    //服务端AB包根目录
    private string serverRootPath ;
    private string serverMd5Path = BundleInfo.md5FileName;
    public List<DownFileInfo> downInfoList = new List<DownFileInfo>();
    public List<string> otherAssetNameList = new List<string>();
    public List<string> luaAssetNameList = new List<string>();
    // key:lua脚本名字 
    private Dictionary<string, ObjInfo> luaDict = new Dictionary<string, ObjInfo>();
    // key:资源名字
    private Dictionary<string, ObjInfo> objDict = new Dictionary<string, ObjInfo>();

    private void Awake()
    {
        serverRootPath = BundleInfo.serverRootPath;
        localRootPath = Application.persistentDataPath + "/AssetBundles/";
    }

    //加载服务端Md5文件
    public string[] LoadServerMd5() {
        string[] serverMd5StrArr = File.ReadAllLines(serverRootPath+serverMd5Path);
        return serverMd5StrArr;
    }

    //更新AssetBundles
    public void UpdateAssetBundles(Action<float> progressAct,Action endAct) {
        string[] serverMd5StrArr = LoadServerMd5();
        if (serverMd5StrArr==null||serverMd5StrArr.Length==0)
        {
            Debug.LogError("服务器Md5文件内容为空！！！！");
            return;
        }
        downInfoList.Clear();

        DownFileInfo md5Info = new DownFileInfo();//下载服务器Md5文件
        md5Info.fileUrl = serverRootPath + serverMd5Path;
        md5Info.savePath = localRootPath + serverMd5Path;
        downInfoList.Add(md5Info);

        DownFileInfo mapInfo = new DownFileInfo();//下载服务器Map文件
        mapInfo.fileUrl = serverRootPath + BundleInfo.mapFileName;
        mapInfo.savePath = localRootPath + BundleInfo.mapFileName;
        downInfoList.Add(mapInfo);

        string fileName;
        string serverMd5;
        string serverFilePath;
        string localFilePath;
        string localMd5;
        long size;
        bool needDownLoad = false;

        for (int i = 0; i < serverMd5StrArr.Length; i++)
        {
            string lineStr = serverMd5StrArr[i];
            fileName = lineStr.Split('|')[0];
            serverMd5 = lineStr.Split('|')[1];
            size = long.Parse(lineStr.Split('|')[2]);
            localFilePath = localRootPath + fileName;
            needDownLoad = false;
            if (!File.Exists(localFilePath))//本地不存在的文件，需要下载 
            {
                string dir = Path.GetDirectoryName(localFilePath);
                if (!Directory.Exists(dir))//若当前文件夹不存在先创建文件夹，方便后续下载
                {
                    Directory.CreateDirectory(dir);
                }
                needDownLoad = true;
            }
            else {
                localMd5 = Md5Tools.FileToMd5(localFilePath);
                if (!localMd5.Equals(serverMd5))//本地和服务端md5数据不相同，需要下载
                {
                    needDownLoad = true;
                }

            }

            if (needDownLoad)//需要下载文件
            {
                serverFilePath = serverRootPath + fileName;
                DownFileInfo info = new DownFileInfo();
                info.fileUrl = serverFilePath;
                info.savePath = localFilePath;
                info.fileName = fileName;
                info.size = size;
                downInfoList.Add(info);
            }

        }
        StartCoroutine(DownAssetBundles(progressAct,endAct));

    }

    //下载Ab包资源
    IEnumerator DownAssetBundles(Action<float> progressAct,Action endAct) {
        if (downInfoList==null||downInfoList.Count==0)
        {
            Debug.Log("无下载内容");
            endAct?.Invoke();
            yield break;
        }
        string fileUrl;
        string saveUrl;
        string fileName;
        for (int i = 0; i < downInfoList.Count; i++)
        {
            DownFileInfo info = downInfoList[i];
            fileUrl = info.fileUrl;
            saveUrl = info.savePath;
            Debug.Log("下载或更新：" + saveUrl);
            File.Copy(fileUrl,saveUrl,true);
            yield return new WaitForFixedUpdate();
            progressAct?.Invoke((float)(i+1)/downInfoList.Count);
        }
        downInfoList.Clear();
        InitAssetNameList();
        InitObjInfoDict();
        endAct?.Invoke();
    }

    //把Lua ab包和非lua ab包区分出来
    void InitAssetNameList() {
        luaAssetNameList.Clear();
        otherAssetNameList.Clear();
        string path = Application.persistentDataPath + "/"+BundleInfo.assetsDirName+"/"+BundleInfo.md5FileName;
        string[] lines = File.ReadAllLines(path);
        if (lines==null||lines.Length==0)
        {
            return;
        }
        string line;
        string assetName;

        for (int i = 0; i < lines.Length; i++)
        {
            line = lines[i];
            assetName = line.Split('|')[0];
            if (assetName.StartsWith("lua/"))
            {
                luaAssetNameList.Add(assetName);
            }
            else {
                otherAssetNameList.Add(assetName);
            }
        }
    }

    public void InitLuaBundle(Action endAct) {
        if (AppCfg.bundleModel)//使用Bundle中的Lua文件
        {
            LuaManager.instance.Init(luaAssetNameList.ToArray(), endAct);
        }
        else {
            LuaManager.instance.InitLua();
            endAct?.Invoke();
        }
        
    }

    public void InitOtherBundle(Action endAct) {
        InitBundleManager.instance.Init(otherAssetNameList.ToArray(),endAct);
    }

    //根据资源名字得到资源信息
    public ObjInfo GetObjInfo(string name,bool isLua=false) {
        ObjInfo info=null;
        if (isLua)//Lua资源
        {
            if (luaDict.ContainsKey(name))
            {
                info = luaDict[name];
            }
        }
        else {//非Lua资源
            if (objDict.ContainsKey(name))
            {
                info = objDict[name];
            }
            else {
                foreach (var item in objDict.Keys)
                {
                    Debug.LogError(item);
                }
                Debug.LogError(name + "：资源未找到");
            }
        }
        //if (info==null)
        //{
        //    Debug.LogError(name+"：资源未找到");
        //}
        return info;
    }

    //根据Map表存储资源名字对应的资源信息
    private void InitObjInfoDict() {

        localRootPath = Application.persistentDataPath + "/AssetBundles/";
        string mapPath = localRootPath + BundleInfo.mapFileName;
        if (!File.Exists(mapPath))
        {
            Debug.LogError("Map文件异常！！！");
            return;
        }
        luaDict.Clear();
        objDict.Clear();
        string[] mapLineStrArr = File.ReadAllLines(mapPath);
        string lineStr;
        string luaPath;
        string objPath;
        string[] luaPathSplit;
        string[] objPathSplit;
        string luaScriptName;
        string objName;
        string assetName;
        for (int i = 0; i < mapLineStrArr.Length; i++)
        {
            lineStr = mapLineStrArr[i];
            ObjInfo info = new ObjInfo();
            if (lineStr.StartsWith(BundleInfo.tempLuaDirName, StringComparison.CurrentCulture))//Lua资源
            {
                luaPath = lineStr.Split('|')[0];
                luaPathSplit = luaPath.Split('/');
                luaScriptName = luaPathSplit[luaPathSplit.Length - 1];
                luaScriptName = luaScriptName.Split('.')[0];
                assetName = lineStr.Split('|')[1];
                info.assetName = assetName;
                info.name = luaScriptName;
                info.formalPath = luaPath;
                info.localPath =luaPath.Replace("TempLua", "Lua");
                luaDict[luaScriptName] = info;
            }
            else {//其他资源
                objPath = lineStr.Split('|')[0];
                objPathSplit = objPath.Split('/');
                objName = objPathSplit[objPathSplit.Length - 1];
                objName = objName.Split('.')[0];
                assetName = lineStr.Split('|')[1];
                info.assetName = assetName;
                info.name = objName;
                info.formalPath =  objPath;
                info.localPath = objPath;
                objDict[objName] = info;
            }
            Debug.Log("【objName:】" + info.name + "   【format:】" + info.formalPath + "  【local:】" + info.localPath);
        }

    }

}
