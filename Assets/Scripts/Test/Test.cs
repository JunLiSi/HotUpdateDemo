using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private AudioClip clip;

    private void Awake()
    {
        InitAssetBundle();
      
    }

    void InitAssetBundle()
    {

        AssetBundleManager.instance.InitOtherBundle(() => {
            TestInitGameObject();
            TestInitFont();
            clip = GetAudio();
            AssetBundleManager.instance.InitLuaBundle(TestInitLua);
        });
    }

    float audioSlider;
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 80), new GUIContent("PlayAudio")))
        {
            if (!AudioManager.instance.GetClip())
            {
                AudioManager.instance.Play(clip, (float value) =>
                {
                    audioSlider = value;
                }, () =>
                {
                    audioSlider = 1;
                    Debug.LogError("Over");
                }, true);
            }
            else
            {
                AudioManager.instance.Play();
            }

        }
        else if (GUI.Button(new Rect(10, 90, 150, 80), new GUIContent("ContinueAudio")))
        {
            AudioManager.instance.Continue();
        }
        else if (GUI.Button(new Rect(10, 170, 150, 80), new GUIContent("StopAudio")))
        {
            AudioManager.instance.Stop();
        }
        else if (GUI.Button(new Rect(10, 250, 150, 80), new GUIContent("ShowMainPanel")))
        {
            PanelManager.instance.ShowPanel("MainPanel",null);
        }
        else if (GUI.Button(new Rect(160, 250, 150, 80), new GUIContent("ShowSettingPanel")))
        {
            PanelManager.instance.ShowPanel("SettingPanel", null);
        }
        else if (GUI.Button(new Rect(10, 330, 150, 80), new GUIContent("HideMainPanel")))
        {
            if (PanelManager.instance.IsShow("MainPanel"))
            {
                PanelManager.instance.HidePanel("MainPanel");
            }
            else {
                Debug.LogError("没有打开状态的MainPanel");
            }
            
        }
        else if (GUI.Button(new Rect(160, 330, 150, 80), new GUIContent("HideSettingPanel")))
        {
            if (PanelManager.instance.IsShow("SettingPanel"))
            {
                PanelManager.instance.HidePanel("SettingPanel");
            }
            else
            {
                Debug.LogError("没有打开状态的SettingPanel");
            }
            
        }
        else if (GUI.Button(new Rect(10, 410, 150, 80), new GUIContent("DestroyMainPanel")))
        {
            PanelManager.instance.HidePanel("MainPanel",true);
        }
        else if (GUI.Button(new Rect(160, 410, 150, 80), new GUIContent("DestroySettingPanel")))
        {
            PanelManager.instance.HidePanel("SettingPanel", true);
        }
        audioSlider = GUI.HorizontalSlider(new Rect(160, 45, 300, 80), audioSlider, 0.0f, 1.0f);
        GUI.TextArea(new Rect(480, 30, 40, 40), audioSlider.ToString("f1"));

        if (Input.GetMouseButton(1))
        {
            Debug.LogError("取出一个唯一字符串："+UniqueStringManager.instance.GetString());
        }
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
        //XLua.LuaEnv luaEnv = LuaManager.instance.GetLuaEnv();
        //luaEnv.DoString("require 'MainLua/Main'");
    }

    //测试加载Ab包中的字体
    void TestInitFont()
    {
        Font font = null;
        string assetName = "fonts/1.unity3d";
        string itemName = "1/PingFangThin";
        font = InitBundleManager.instance.GetFont(assetName, itemName);
        Debug.Log(font);
    }

    //测试加载Ab包中的音频
    AudioClip GetAudio()
    {
        AudioClip clip = null;
        string assetName = "audios/bg.unity3d";
        string itemName = "Bg/keepBg";
        clip = InitBundleManager.instance.GetAudioClip(assetName, itemName);
        return clip;
    }

    ////测试加载Ab包中的图片
    void TestInitTexture()
    {

    }

}
