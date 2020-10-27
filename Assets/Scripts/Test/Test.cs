using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private string fontRootPath="Assets/"+BundleInfo.fontsDirName+"/";
    private string fontExt = ".ttf";

    private string audioRootPath = "Assets/" + BundleInfo.audiosDirName + "/";
    private string audioExt = ".mp3";

    private void Awake()
    {
        InitAssetBundle();
    }

    void InitAssetBundle() {

        AssetBundleManager.instance.InitOtherBundle(()=> {
            TestInitGameObject();
            TestInitFont();
            TestInitAudio();
            AssetBundleManager.instance.InitLuaBundle(TestInitLua);
        });
    }


    //测试加载Ab包中的预制体 
    void TestInitGameObject()
    {
        GameObject go = InitBundleManager.instance.GetGameObject("prefabs/1.unity3d", "1/Sphere");
        go.transform.position = Vector3.zero + Vector3.left * 2;
        go.transform.localScale = Vector3.one;

        GameObject go1 = InitBundleManager.instance.GetGameObject("prefabs/2.unity3d", "2/Cube");
        go1.transform.position = Vector3.zero + Vector3.right * 2;
        go1.transform.localScale = Vector3.one;
    }

    //测试加载Ab包中的Lua脚本 
    void TestInitLua()
    {
        XLua.LuaEnv luaEnv = LuaManager.instance.GetLuaEnv();
        luaEnv.DoString("require 'MainLua/Main'");
    }

    //测试加载Ab包中的字体
    void TestInitFont() {
        Font font = null;
        string itemName = fontRootPath + "1/PingFangThin" + fontExt;
        font = InitBundleManager.instance.LoadAsset<Font>("fonts/1.unity3d", itemName);
        Debug.Log(font);
    }

    //测试加载Ab包中的音频
    void TestInitAudio() {
        AudioClip clip = null;
        string itemName = audioRootPath + "1/Close"+audioExt;
        clip = InitBundleManager.instance.LoadAsset<AudioClip>("audios/1.unity3d", itemName);
        Debug.Log(clip);
    }

    ////测试加载Ab包中的图片
    void TestInitTexture() {

    }

}
