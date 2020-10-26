using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BundleInfo 
{
    public static string luaDirName = "Lua";//Lua原文件文件夹

    public static string tempLuaDirName = "TempLua";//Lua文件使用文件夹

    public static string assetsDirName = "AssetBundles";//AssetBundle文件夹名

    public static string prefabsDirName = "BundleResources/Prefabs";//所有预制体的文件夹名

    public static string texturesDirName = "BundleResources/Textures";//所有贴图文件夹名

    public static string extName = ".unity3d";//AssetBundle文件后缀名

    public static string luaPrefixRoot = "lua/";//lua AssetBundle文件的前缀根字符

    public static string prefabsPrefixRoot = "prefabs/";//Prefabs AssetBundle文件的前缀根字符

    public static string texturesPrefixRoot = "textures/";//贴图 AssetBundle文件的前缀根字符

    public static string md5FileName = "files_md5.md5";//md5文件的名字

    public static string mapFileName = "files_map.map";//map文件的名字

}
