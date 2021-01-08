using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Text tipText;
    public Image progressImg;

    private const string nextSceneName = "Main";
    private AsyncOperation async = null;
    private float loadSceneProgressValue;
    private float fakeSpeed = 0.2f;

    private void Start()
    {
        AssetBundleManager.instance.UpdateAssetBundles(LoadingABProgress,LoadingABEnd);
    }

    //正在加载AB包资源
    public void LoadingABProgress(float value) {
        progressImg.fillAmount = value;
        tipText.text = "正在加载资源...";
    }

    //加载AB包结束
    public void LoadingABEnd() {
        progressImg.fillAmount = 0;
        tipText.text = "即将跳转场景...";
        StartCoroutine(InMainScene());
    }

    //异步加载进入主场景
    IEnumerator InMainScene() {
        async = SceneManager.LoadSceneAsync(nextSceneName);
        async.allowSceneActivation = false;
        while (true)
        {
            if (async.progress <= 0.9f)
            {
                loadSceneProgressValue = async.progress;
            }
            progressImg.fillAmount = loadSceneProgressValue;
            if (loadSceneProgressValue>=0.9f)
            {
                StartCoroutine(FakeProgress());
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    //加载主场景0.9~1之间的假数据
    IEnumerator FakeProgress() {
        while (loadSceneProgressValue<1)
        {
            loadSceneProgressValue += Time.deltaTime*fakeSpeed;
            if (loadSceneProgressValue>1)
            {
                loadSceneProgressValue = 1;
            }
            progressImg.fillAmount = loadSceneProgressValue;
            yield return new WaitForFixedUpdate();
        }
        async.allowSceneActivation = true;
        
    }

}
