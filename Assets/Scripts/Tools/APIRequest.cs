using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BestHTTP;
using UnityEngine;

public class APIRequest
{
    public delegate void HTTPRequestCallBack(APIRequest apiRequest,bool isSuccess,string responseText);
    public delegate void HTTPDownProgressCallBack(APIRequest apiRequest,float progress);
    public delegate void HTTPDownCompleteCallBack(APIRequest apiRequest,bool isSuccess,int code,string savePath);
    private HTTPRequestCallBack requestCallBack;//请求完成回调
    private HTTPDownProgressCallBack downProgressCallBack;//下载进度回调
    private HTTPDownCompleteCallBack downCompleteCallBack;//下载完成回调

    //正式服务器根路径
    private static string baseUrl = "https://api.yinjis.immusician.com/v1/";
    //测试服务器根路径
    private static string testBaseUrl = "http://192.168.2.129:55555/v1/";
    //是否用测试服务器
    public static bool useTestServer = true;

    private HTTPRequest request;
    private TimeSpan requestTimeOut = TimeSpan.FromSeconds(30);//请求超时时间设置 30S

    private string downSavePath;//下载资源的本地保存路径
    private int downFileCount = -1;//需要下载资源的长度
    private int downedFileCount = 0;//已经下载资源的长度
    private float downProgressValue = 0;//下载资源的进度

    private APIRequest(string _baseUrl=null) {
		if (!string.IsNullOrEmpty(_baseUrl))
		{
			baseUrl = _baseUrl;
		}
	}

    //创建请求对象
	public static APIRequest Create(string _baseUrl=null) {
		if (useTestServer)
		{
			_baseUrl = testBaseUrl;
		}
		return new APIRequest(_baseUrl);
	}

    //设置请求超时时间
    public APIRequest SetTimeOut(double seconds) {
        if (seconds<=0)
        {
            return this;
        }
        requestTimeOut = TimeSpan.FromSeconds(seconds);
        return this;
    }

    //设置请求优先级  优先级较高的请求将比优先级较低的请求更快地从请求队列中选择。
    public APIRequest SetPriority(int priority) {
        if (request!=null)
        {
            request.Priority = priority;
        }
        return this;
    }

    #region 数据请求

    //发送请求
    public APIRequest Send(HTTPMethods method,string url,Dictionary<string,string> paramsDict, HTTPRequestCallBack sendCallBack =null) {
        requestCallBack = sendCallBack;
        if (Application.internetReachability==NetworkReachability.NotReachable)//没有网络连接
        {
            Debug.LogError("【 检查设备是否可以访问网络！！！ 】");
            requestCallBack?.Invoke(this,false,"设备无法进行网络连接");
            return this;
        }
        if (method ==HTTPMethods.Get)
        {
            SendGetMethod(url,paramsDict);
        }
        else if (method==HTTPMethods.Post)
        {
            SendPostMethod(url,paramsDict);
        }
        if (request==null)
        {
            Debug.LogError("【 创建HTTPRequest失败！！！ 】");
            requestCallBack?.Invoke(this, false, "创建HTTPRequest失败");
            return this;
        }
        SetRequestHeaders(request);
        //禁止缓存
        request.DisableCache = true;
        //设置超时时间
        request.Timeout = requestTimeOut;
        //发送请求
        request.Send();
        return this;
    }

    //终止请求
    public APIRequest Abort() {
        if (request!=null)
        {
            request.Abort();
        }
        return this;
    }

    //设置请求头
    public void SetRequestHeaders(HTTPRequest _request) {

        //_request.SetHeader("name","value");

    }

    //Get方式请求数据
    void SendGetMethod(string url,Dictionary<string,string> paramsDict) {
        string requestUrl = baseUrl + url+"?";
        if (paramsDict!=null&&paramsDict.Count>0)
        {
            int id = 0;
            foreach (var item in paramsDict.Keys)
            {
                string key = item;
                string value = paramsDict[key];
                if (id < paramsDict.Count - 1)
                {
                    requestUrl += key + "=" + value + "&";
                }
                else {
                    requestUrl += key + "=" + value;
                }
                id++;
            }
        }
        Uri uri = new Uri(requestUrl);
        request = new HTTPRequest(uri,HTTPMethods.Get, OnRequestFinished);
        
    }

    //Post方式请求数据
    void SendPostMethod(string url,Dictionary<string, string> paramsDict) {
        string requestUrl = baseUrl + url;
        Uri uri = new Uri(requestUrl);
        request = new HTTPRequest(uri,HTTPMethods.Post,OnRequestFinished);
        if (paramsDict!=null&&paramsDict.Count>0)
        {
            foreach (var item in paramsDict.Keys)
            {
                string key = item;
                string value = paramsDict[key];
                request.AddField(key,value);
            }
        }
        
    }

    //数据请求完成回调
    void OnRequestFinished(HTTPRequest _request, HTTPResponse response) {
        int responseCode = 0;//请求返回码
        bool isSuccess = false;
        string responseText = null;

        if (_request.State == HTTPRequestStates.Finished)//请求完成
        {
            responseCode = response.StatusCode;
            isSuccess = response.IsSuccess;
            responseText = response.DataAsText;
        }
        else if (_request.State == HTTPRequestStates.TimedOut || _request.State == HTTPRequestStates.ConnectionTimedOut)//请求或连接超时
        {
            responseText = "请求超时";
        }
        else if (_request.State == HTTPRequestStates.Error)//请求报错
        {
            if (request.Exception != null)
            {
                responseText = request.Exception.Message;
                if (string.IsNullOrEmpty(responseText))
                {
                    responseText = "请求报错";
                }
            }
            else
            {
                responseText = "请求报错";
            }
        }
        else {//其他异常
            responseText = _request.State.ToString();
        }
#if UNITY_EDITOR  //编辑器模式下打印请求数据
        string debugStr = "【HTTPRequest  State="+ _request.State.ToString()+"  isSuccess="+isSuccess+"】:";
        string url = _request.Uri.AbsoluteUri+"\n";
        debugStr = "<color=blue>"+debugStr + url+"</color>" + System.Text.RegularExpressions.Regex.Unescape(responseText);
        Debug.Log(debugStr);
#endif

        requestCallBack?.Invoke(this,isSuccess,responseText);

    }

    #endregion 


    #region 资源下载

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url">资源url</param>
    /// <param name="savePath">保存本地的路径</param>
    /// <param name="progressCallBack">下载进度回调</param>
    /// <param name="completeCallBack">下载完成回调</param>
    /// <param name="forceDown">是否强制下载（是否替换本地文件）</param>
    public void DownFile(string url, string savePath, HTTPDownProgressCallBack progressCallBack, HTTPDownCompleteCallBack completeCallBack, bool forceDown = false) {
        downCompleteCallBack = completeCallBack;
        downProgressCallBack = progressCallBack;
        downSavePath = savePath;
        downFileCount = -1;
        downedFileCount = 0;
        if (string.IsNullOrEmpty(url)||string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("【 非法的下载地址或存储地址！ 】");
            downCompleteCallBack?.Invoke(this,false,300,savePath);
            return;
        }

        if (!forceDown)//非强制下载
        {
            if (File.Exists(savePath))//本地存在目标文件
            {
                if (savePath.EndsWith(".tmp", StringComparison.CurrentCulture))
                {//删除本地临时文件
                    File.Delete(savePath);
                    Debug.Log("删除文件："+savePath);
                }
                else {
                    Debug.Log("重复资源，无需下载："+savePath);
                    downProgressCallBack?.Invoke(this,1);
                    downCompleteCallBack?.Invoke(this, true, 200, savePath);
                    return;
                }
            }
            else {//新资源
                Debug.Log("新资源："+savePath);
            }
        }

        Uri uri = new Uri(url);
        request = new HTTPRequest(uri,DownRequestFinished);
        request.UseStreaming = true;
        request.StreamFragmentSize = 1024 * 512;
        request.DisableCache = true;
        request.Timeout = requestTimeOut;
        request.ConnectTimeout =requestTimeOut;
        request.Send();
    }

    //下载资源请求结束回调
    private void DownRequestFinished(HTTPRequest _request, HTTPResponse response) {

        HTTPRequestStates states = _request.State;
        if (states==HTTPRequestStates.Processing)//数据请求中
        {
            if (downFileCount == -1)
            {
                string value = response.GetFirstHeaderValue("content-length");
                if (!string.IsNullOrEmpty(value))
                {
                    downFileCount= int.Parse(value);
                    Debug.Log("GetFirstHeaderValue content-length = " + downFileCount);
                }
            }
            List<byte[]> bytes;
            bytes = response.GetStreamedFragments();
            SaveFile(bytes);
            downProgressValue = downedFileCount / (float)downFileCount;
            downProgressCallBack?.Invoke(this,downProgressValue);
        }
        else
        if (states == HTTPRequestStates.Finished)//数据请求完成
        {
            if (response.IsSuccess)
            {
                List<byte[]> bytes;
                bytes = response.GetStreamedFragments();
                SaveFile(bytes);
                if (response.IsStreamingFinished)
                {
                    downProgressCallBack?.Invoke(this, 1);
                    downCompleteCallBack?.Invoke(this,true,response.StatusCode,downSavePath);
                }
                else {
                    Debug.LogError("【HTTPRequestStates.Finished成功，Response.IsStreamingFinished失败】");
                    downCompleteCallBack?.Invoke(this, false, response.StatusCode, downSavePath);
                }
            }
            else {

                downCompleteCallBack?.Invoke(this, false, response.StatusCode, downSavePath);
                string logMsg = string.Format("{0} - {1} - {2}", response.StatusCode, response.Message, _request.Uri.AbsoluteUri);
                Debug.LogError(logMsg);
            }
        }
        else {//数据请求失败
            Debug.LogError("DownFile失败："+_request.State.ToString());
            downCompleteCallBack?.Invoke(this,false,-1, downSavePath);
        }

    }

    //保存资源到本地
    private void SaveFile(List<byte[]> bytes) {
        if (bytes==null||bytes.Count<=0)
        {
            return;
        }

        using (FileStream fs = new FileStream(downSavePath,FileMode.Append)) {
            for (int i = 0; i < bytes.Count; i++)
            {
                byte[] byteArr = bytes[i];
                fs.Write(byteArr,0,byteArr.Length);
                downedFileCount+=byteArr.Length;
            }
        }

    }

    #endregion 


}
