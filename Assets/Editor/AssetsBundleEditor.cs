using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetsBundleEditor : Editor
{
    public static List<AssetBundleBuild> abList = new List<AssetBundleBuild>();

    //本地测试服务端AssetBundles根路径
    public static string serverRootPath = BundleInfo.serverRootPath;


    [MenuItem("SJL/BuildAssetBundle/BuildAndroid")]
    static void BuildAndroid() {//出Android包体
        Build(BuildTarget.Android);
    }

    [MenuItem("SJL/BuildAssetBundle/BuildiOS")]
    static void BuildiOS()
    {//出iOS包体
        Build(BuildTarget.iOS);
    }

    [MenuItem("SJL/BuildAssetBundle/BuildWindows")]
    static void BuildWindows()
    {//出Windows包体
        Build(BuildTarget.StandaloneWindows64);
    }

    //AssetBundles转移至本地测试服务器
    [MenuItem("SJL/BuildAssetBundle/AssetBundlesToTestServer")]
    static void AssetBundlesToTestServer()
    {
        string localBundlePath = GetStreamingAssets() + "/" + BundleInfo.assetsDirName;
        string serverBundlePath = serverRootPath;
        if (Directory.Exists(serverRootPath))
        {
            Directory.Delete(serverRootPath, true);
        }
        string[] localFiles = Directory.GetFiles(localBundlePath, "*", SearchOption.AllDirectories);
        string localFilePath;
        string serverFilePath;
        for (int i = 0; i < localFiles.Length; i++)
        {
            localFilePath = localFiles[i];
            if (localFilePath.EndsWith(".meta"))
            {
                continue;
            }
            serverFilePath = localFilePath.Replace(localBundlePath, serverBundlePath);
            string dir = Path.GetDirectoryName(serverFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.Copy(localFilePath, serverFilePath);
        }
        AssetDatabase.Refresh();
        Debug.Log("AB包转移至本地测试服务器成功：\n" + System.DateTime.Now.ToString("yyyy-MM-dd||hh:mm:ss"));
    }

    static string GetStreamingAssets() {
        return Application.streamingAssetsPath;
    }

    //打Bundle包
    static void Build(BuildTarget buildTarget) {

        string assetsBundlePath = GetStreamingAssets() + "/" + BundleInfo.assetsDirName;
        if (Directory.Exists(assetsBundlePath))
        {
            Directory.Delete(assetsBundlePath, true);
        }
        Directory.CreateDirectory(assetsBundlePath);
        abList.Clear();
        LuaCopyToTempLua();
        InitLuaABList();
        InitPrefabsABList();
        InitTexturesABList();
        InitAudiosABList();
        InitFontsABList();
        BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression;
        BuildPipeline.BuildAssetBundles(assetsBundlePath, abList.ToArray(), BuildAssetBundleOptions.None, buildTarget);
        CreateMd5File();
        CreateMapFile();
        AssetDatabase.Refresh();
        Debug.Log("打AB包成功：\n"+System.DateTime.Now.ToString("yyyy-MM-dd||hh:mm:ss")+"-----"+ buildTarget);
    }

    //Lua文件夹下的lua文件转移至TempLua文件夹下
    static void LuaCopyToTempLua() {
        string luaDir = Application.dataPath + "/" + BundleInfo.luaDirName;
        string tempDir = Application.dataPath + "/" + BundleInfo.tempLuaDirName;
        if (!Directory.Exists(luaDir))
        {
            return;
        }
        
        string[] files = Directory.GetFiles(luaDir, "*.lua", SearchOption.AllDirectories);
        if (files == null||files.Length==0)
        {
            return;
        }

        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir,true);
        }

        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i].Replace("\\", "/");
            string dirPath = Path.GetDirectoryName(filePath).Replace("\\", "/");
            string tempDirPath = tempDir + dirPath.Replace(luaDir, string.Empty);
            if (!Directory.Exists(tempDirPath))
            {
                Directory.CreateDirectory(tempDirPath);
            }
            string tempFilePath = tempDirPath + filePath.Replace(dirPath, string.Empty) + ".bytes";
            File.Copy(filePath, tempFilePath, true);

        }
        AssetDatabase.Refresh();
    }

    //将Lua添加到AssetBundleBuild列表
    static void InitLuaABList() {
        string tempLuaDirPath = Application.dataPath + "/" + BundleInfo.tempLuaDirName;
        string[] dirArr = Directory.GetDirectories(tempLuaDirPath);
        string bundleName = BundleInfo.luaPrefixRoot + BundleInfo.tempLuaDirName.ToLower() + BundleInfo.extName;
        AddABList(bundleName,"Assets/"+BundleInfo.tempLuaDirName,"*.bytes");
        for (int i = 0; i < dirArr.Length; i++)
        {
            string dirPath = dirArr[i].Replace("\\","/");
            bundleName = BundleInfo.luaPrefixRoot+dirPath.Replace(tempLuaDirPath,"").Replace("/","").ToLower()+BundleInfo.extName;
            string path = "Assets"+dirPath.Replace(Application.dataPath, "");
            AddABList(bundleName,path,"*.bytes");
        }
        
    }

    //将预制体添加到AssetBundleBuild列表
    static void InitPrefabsABList() {
        string prefabsDirPath = Application.dataPath + "/" + BundleInfo.prefabsDirName;
        string[] dirArr = Directory.GetDirectories(prefabsDirPath);
        string bundleName = BundleInfo.prefabsPrefixRoot+BundleInfo.prefabsDirName.ToLower() + BundleInfo.extName;
        AddABList(bundleName,"Assets/"+BundleInfo.prefabsDirName, "*.prefab");
        for (int i = 0; i < dirArr.Length; i++)
        {
            string dirPath = dirArr[i].Replace("\\", "/");
            bundleName = BundleInfo.prefabsPrefixRoot+dirPath.Replace(prefabsDirPath, "").Replace("/", "").ToLower() + BundleInfo.extName;
            string path = "Assets" + dirPath.Replace(Application.dataPath, "");
            AddABList(bundleName, path, "*.prefab");
        }
    }

    //将贴图添加到AssetBundleBuild列表
    static void InitTexturesABList() {
        string textruesDirPath = Application.dataPath + "/" + BundleInfo.texturesDirName;
        string[] dirArr = Directory.GetDirectories(textruesDirPath);
        string bundleName = BundleInfo.texturesPrefixRoot + BundleInfo.texturesDirName.ToLower() + BundleInfo.extName;
        AddABList(bundleName, "Assets/" + BundleInfo.texturesDirName, "*");
        for (int i = 0; i < dirArr.Length; i++)
        {
            string dirPath = dirArr[i].Replace("\\", "/");
            bundleName = BundleInfo.texturesPrefixRoot + dirPath.Replace(textruesDirPath, "").Replace("/", "").ToLower() + BundleInfo.extName;
            string path = "Assets" + dirPath.Replace(Application.dataPath, "");
            AddABList(bundleName, path, "*");
        }
    }

    //将字体添加到AssetBundleBuild列表
    static void InitFontsABList() {
        string fontsDirPath = Application.dataPath + "/" + BundleInfo.fontsDirName;
        string[] dirArr = Directory.GetDirectories(fontsDirPath);
        string bundleName = BundleInfo.fontsPrefixRoot + BundleInfo.fontsDirName.ToLower() + BundleInfo.extName;
        AddABList(bundleName, "Assets/" + BundleInfo.fontsDirName, "*.ttf");
        for (int i = 0; i < dirArr.Length; i++)
        {
            string dirPath = dirArr[i].Replace("\\", "/");
            bundleName = BundleInfo.fontsPrefixRoot + dirPath.Replace(fontsDirPath, "").Replace("/", "").ToLower() + BundleInfo.extName;
            string path = "Assets" + dirPath.Replace(Application.dataPath, "");
            AddABList(bundleName, path, "*");
        }
    }

    //将音频添加到AssetBundleBuild列表
    static void InitAudiosABList() {
        string audiosDirPath = Application.dataPath + "/" + BundleInfo.audiosDirName;
        string[] dirArr = Directory.GetDirectories(audiosDirPath);
        string bundleName = BundleInfo.audiosPrefixRoot + BundleInfo.audiosDirName.ToLower() + BundleInfo.extName;
        AddABList(bundleName, "Assets/" + BundleInfo.audiosDirName, "*.mp3");
        for (int i = 0; i < dirArr.Length; i++)
        {
            string dirPath = dirArr[i].Replace("\\", "/");
            bundleName = BundleInfo.audiosPrefixRoot + dirPath.Replace(audiosDirPath, "").Replace("/", "").ToLower() + BundleInfo.extName;
            string path = "Assets" + dirPath.Replace(Application.dataPath, "");
            AddABList(bundleName, path, "*");
        }
    }

    /// <summary>
    /// 添加文件至AssetBundleBuild列表
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="path">文件夹相对路径（例如：Assets/...）</param>
    /// <param name="pattern">筛查条件</param>
    static void AddABList(string bundleName,string path,string pattern) {
        string[] files = Directory.GetFiles(path,pattern);
        if (files==null||files.Length==0)
        {
            return;
        }
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = files[i].Replace("\\","/");
            Debug.Log("【BundleName：+" + bundleName + "  Path:" + files[i] + "】");
        }
        AssetBundleBuild abBuild = new AssetBundleBuild();
        abBuild.assetBundleName = bundleName;
        abBuild.assetNames = files;
        abList.Add(abBuild);
    }

    //创建Md5文件
    static void CreateMd5File() {
        string assetBundlePath = GetStreamingAssets() + "/" + BundleInfo.assetsDirName;
        string mainBundle = assetBundlePath + "/AssetBundles";
        string md5FilePath = assetBundlePath + "/" + BundleInfo.md5FileName;
        if (File.Exists(md5FilePath))
        {
            File.Delete(md5FilePath);
        }

        List<string> mFiles = new List<string>();
        string[] files = GetDirFiles(assetBundlePath,new string[] {".meta",".DS_Store"},"*"+BundleInfo.extName);
        if (files==null||files.Length==0)
        {
            return;
        }
        mFiles.Add(mainBundle);
        foreach (var item in files)
        {
            mFiles.Add(item);
        }

        FileStream fs = new FileStream(md5FilePath,FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);

        for (int i = 0; i < mFiles.Count; i++)
        {
            string file = mFiles[i].Replace("\\","/");
            if (string.IsNullOrEmpty(file)||file.EndsWith(".meta")||file.Contains(".DS_Store"))
            {
                continue;
            }
            string md5 = Md5Tools.FileToMd5(file);
            long size = GetFileSize(file);
            string fileName = file.Replace(assetBundlePath+"/",string.Empty);
            string str =fileName+"|"+ md5 + "|" + size;
            sw.WriteLine(str);
        }

        sw.Close();
        fs.Close();
    }

    //创建Map文件
    static void CreateMapFile()
    {
        string assetBundlePath = GetStreamingAssets() + "/" + BundleInfo.assetsDirName;
        string mapFilePath = assetBundlePath + "/" + BundleInfo.mapFileName;
        if (abList == null || abList.Count == 0)
        {
            return;
        }
        if (File.Exists(mapFilePath))
        {
            File.Delete(mapFilePath);
        }

        FileStream fs = new FileStream(mapFilePath, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);

        string[] filesArr = null;
        string abName = null;
        string fileName = null;

        foreach (AssetBundleBuild ab in abList)
        {
            filesArr = ab.assetNames;
            abName = ab.assetBundleName;
            for (int i = 0; i < filesArr.Length; i++)
            {
                fileName = filesArr[i];
                if (fileName.EndsWith(".meta") || fileName.EndsWith(".DS_Store"))
                {
                    continue;
                }
                fileName = fileName.Replace("Assets/", string.Empty);
                sw.WriteLine(fileName + "|" + abName);
            }

        }
        sw.Close();
        fs.Close();
    }

    //获取文件大小
    static long GetFileSize(string filePath) {
        long size = 0;
        if (!File.Exists(filePath))
        {
            size = -1;
        }
        else
        {
            FileInfo info = new FileInfo(filePath);
            size = info.Length;
        }
        return size;
    }

    /// <summary>
    /// 得到文件夹下所有的文件
    /// </summary>
    /// <param name="dirPath">文件夹</param>
    /// <param name="useLessSuffixArr">需要剔除的后缀名</param>
    /// <returns></returns>
    static string[] GetDirFiles(string dirPath,string[] useLessSuffixArr,string pattern="*") {
        List<string> fileList = new List<string>();
        string[] files = Directory.GetFiles(dirPath,pattern,SearchOption.AllDirectories);
        if (files==null||files.Length==0)
        {
            return null;
        }
        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];
            bool isTrue = true;
            for (int j = 0; j < useLessSuffixArr.Length; j++)
            {
                string extension = useLessSuffixArr[j];
                if (Path.GetExtension(file)==extension)
                {
                    isTrue = false;
                    break;
                }
            }
            if (isTrue)
            {
                fileList.Add(file);
            }
        }
        return fileList.ToArray();
    }
    
}
