using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using SJL;

//线段控制基类
public class LineBase : MonoBehaviour
{
    private VectorLineModel model;
    [SerializeField][Header("唯一id")]
    private int id;//唯一ID
    public int modelId {//唯一ID
        get {
            return id;
        }
    }

    public Texture2D[] texArr;
    public float lineWidth=10;
    protected float speed;
    protected float screenWidth;
    protected float screenHeight;
    protected float checkViewTime = 2;
    private float curTime = 0;
    

    protected GameObject canvasObj;
    public Transform mask;
    public Color32 lineColor = Color.gray;
    public Vector2 moveDir=Vector2.left;
    public bool isRightPivot = true;//是否是右锚点
    protected List<string> endCapNameList;
    protected bool isPlay;

    
    protected virtual void Awake()
    {
        isRightPivot = true;
        endCapNameList = new List<string>();
        canvasObj = GameObject.FindObjectOfType<Canvas>().gameObject;
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    //数据重置
    public virtual void Rewind()
    {
        
    }

    public void SetSpeed(float _speed)
    {
        speed = _speed;
    }

    protected void CreateModel() {
        if (model==null)
        {
            model = VectorLineManager.instance.CreateModel(modelId, lineWidth, true);
        }
    }

    protected VectorLineModel GetModel() {
        return model;
    }

    //创建VectorLineInfo
    internal virtual VectorLineModel.VectorLineInfo CreateLine(string lineName,string endCapName)
    {
        
        if (!canvasObj)
        {
            canvasObj = GameObject.FindObjectOfType<Canvas>().gameObject;
        }
        if (!endCapNameList.Contains(endCapName))
        {
            CreateEndCap(endCapName,EndCap.Both,texArr);
        }
        string capName = endCapName + "_"+modelId;
        if (model==null)
        {
            CreateModel();
        }
        VectorLineModel.VectorLineInfo lineInfo = model.CreateLine(lineName,mask,canvasObj,mask.gameObject,capName);
        lineInfo.lineRect.SetParent(mask);
        LineRectSetPos(lineInfo.lineRect);
        lineInfo.SetColor(lineColor);
        return lineInfo;
    }

    //LineRect出生位置设定
    public virtual void LineRectSetPos(RectTransform lineRect) {
        Vector3 temp = lineRect.localPosition;
        if (isRightPivot)
        {
            lineRect.pivot = new Vector2(1, 0.5f);
            lineRect.anchorMin = new Vector2(1, 0.5f);
            lineRect.anchorMax = new Vector2(1, 0.5f);
            lineRect.localScale = Vector3.one;
        }
        else {
            lineRect.pivot = new Vector2(0, 0.5f);
            lineRect.anchorMin = new Vector2(0, 0.5f);
            lineRect.anchorMax = new Vector2(0, 0.5f);
            lineRect.localScale = Vector3.one;
        }
        temp.x = 0; temp.y = 0; temp.z = -1;
        lineRect.localPosition = temp;

    }

    //开启移动
    public virtual void Play() {
        if (speed<=0)
        {
            Debug.LogError("【LineBase】:speed参数小于等于0，无法开启移动！！！");
            return;
        }
        isPlay = true;
    }

    //停止移动
    public virtual void Stop() {
        isPlay = false;
    }

    public virtual void Update() {
        if (isPlay&&model!=null)
        {
            for (int i = 0; i < model.infoList.Count; i++)
            {
                RectTransform rect = model.infoList[i].lineRect;
                if (!rect)
                {
                    continue;
                }
                rect.localPosition += (Vector3)moveDir * Time.deltaTime * speed;
            }
            curTime += Time.deltaTime;
            if (curTime > checkViewTime)
            {
                CheckView();
                curTime = 0;
            }
        }

    }

    //定期检测视野
    public virtual void CheckView()
    {
       
    }

    //VectorLineInfo线段失活处理
    internal virtual void DisableLine(VectorLineModel.VectorLineInfo info) {
        if (model!=null)
        {
            model.DisableLine(info);
        }
    }

    //整个模块所有线段失活处理
    public virtual void DisableAllLine() {
        if (model!=null)
        {
            model.DisableAllLine();
        }
        
    }

    //销毁真个模块的线段
    public virtual void Destroy() {
        if (model!=null)
        {
            model.Destroy();
        }
        
        isPlay = false;
        endCapNameList.Clear();
    }

    public virtual void OnDestroy() {
        Destroy();
    }

    //创建EndCap
    public virtual void CreateEndCap(string endCapName,EndCap endCap,params Texture2D[] texArr) {
        if (string.IsNullOrEmpty(endCapName))
        {
            Debug.LogError("【LineBase】:endCapName不能为空！！！");
            return;
        }
        
        string capName = endCapName + "_"+modelId;//名字规范化“xxx_modelId”避免相同名字冲突
        if (endCapNameList.Contains(capName))
        {
            Debug.Log("【LineBase】:endCapName=>>"+endCapName+"已经存在！");
            return;
        }
        if (model==null)
        {
            CreateModel();
        }
        model.CreateEndCap(capName, EndCap.Both, texArr);
        endCapNameList.Add(endCapName);
    }

    //移除EndCap
    public virtual void RemoveEndCap(string endCapName) {
        if (endCapNameList.Contains(endCapName))
        {
            string capName = endCapName + "_" + modelId;//名字规范化“xxx_modelId”避免相同名字冲突
            if (model!=null)
            {
                model.RemoveEndCap(capName);
            }
            endCapNameList.Remove(endCapName);
        }
    }


}
