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

public class AssetBundleManager : MonoBehaviour
{
    //本地AB包根目录
    string localRootPath = Application.streamingAssetsPath+ "/AssetBundles/";
    //服务端AB包根目录
    public string serverRootPath {
        get {
            string dataPath = Application.dataPath;
            string pattern = "Assets$";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);
            dataPath = regex.Replace(dataPath,string.Empty);
            dataPath += "TestServer/AssetBundles/";
            Debug.Log("本地服务器地址："+dataPath);
            return dataPath;
        }
    }
    private string serverMd5Path = BundleInfo.md5FileName;
    public List<DownFileInfo> downInfoList = new List<DownFileInfo>();
    public List<string> otherAssetNameList = new List<string>();
    public List<string> luaAssetNameList = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        UpdateAssetBundles();
    }

    //加载服务端Md5文件
    public string[] LoadServerMd5() {
        string[] serverMd5StrArr = File.ReadAllLines(serverRootPath+serverMd5Path);
        return serverMd5StrArr;
    }

    //更新AssetBundles
    public void UpdateAssetBundles() {
        string[] serverMd5StrArr = LoadServerMd5();
        if (serverMd5StrArr==null||serverMd5StrArr.Length==0)
        {
            Debug.Log("无可用更新");
            StartCoroutine(DownAssetBundles(DownFinish));
            return;
        }
        downInfoList.Clear();
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
        StartCoroutine(DownAssetBundles(DownFinish));

    }

    //下载Ab包资源
    IEnumerator DownAssetBundles(Action endAct) {
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
        }
        downInfoList.Clear();
        endAct?.Invoke();
    }

    //把Lua ab包和非lua ab包区分出来
    void InitAssetNameList() {
        string path = Application.streamingAssetsPath + "/"+BundleInfo.assetsDirName+"/"+BundleInfo.md5FileName;
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

    //Ab包更新完成
    void DownFinish()
    {
        InitAssetNameList();
        InitBundleManager.instance.Init(otherAssetNameList.ToArray(), () =>
        {
            TestInitGameObject();
            LuaManager.instance.Init(luaAssetNameList.ToArray(), TestInitLua);
        });
    }

    //测试加载Ab包中的预制体 随便改
    void TestInitGameObject()
    {
        GameObject go = InitBundleManager.instance.GetGameObject("prefabs/1.unity3d", "1/Cube");
        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one;
    }

    //测试加载Ab包中的Lua脚本 随便改
    void TestInitLua()
    {
        Debug.Log("测试Main.lua脚本执行状态");
        XLua.LuaEnv luaEnv = LuaManager.instance.GetLuaEnv();
        luaEnv.DoString("require 'MainLua/Main'");
    }
}
