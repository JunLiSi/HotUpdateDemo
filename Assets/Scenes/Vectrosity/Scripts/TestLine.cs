using System.Collections;
using System.Collections.Generic;
using SJL;
using UnityEngine;
using Vectrosity;

//线段功能测试
public class TestLine : LineBase
{
    VectorLineModel.VectorLineInfo lineInfo;
    string speedStr="100";
    string lineWidthStr = "10";
    public bool isStopAdd = false;
    private string endCapName = "TestLineEndCap";

    protected override void Awake()
    {
        base.Awake();
        CreateEndCap(endCapName, EndCap.Both, texArr);
        speed = 100;
        Play();
    }

    public void OnGUI()
    {
        VectorLineModel model = GetModel();
        if (GUI.Button(new Rect(0,0,100,50), "CreateLine"))//创建新线段
        {
            lineInfo = CreateLine("TestLine", endCapName);
            isStopAdd = true;
            Invoke("OpenAddPoint", 1);
            if (!isPlay)
            {
                Play();
            }
            
        }else
        if (GUI.Button(new Rect(100, 0, 100, 50), "Play"))//可以移动
        {
            Play();
        }
        else
        if (GUI.Button(new Rect(200, 0, 100, 50), "Stop"))//停止移动
        {
            Stop();
        }
        else if (GUI.Button(new Rect(300, 0, 100, 50), "AddPosSwitch"))//添加线段点开关
        {
            isStopAdd = !isStopAdd;
            if (!isStopAdd)
            {
                lineInfo = CreateLine("TestLine", endCapName);
                if (!isPlay)
                {
                    Play();
                }
            }
        }
        else
        if (GUI.Button(new Rect(400, 0, 100, 50), "DisableAllLine"))//所有线段失活
        {
            model?.DisableAllLine();
            lineInfo = null;
        }
        else if (GUI.Button(new Rect(500, 0, 100, 50), "DestroyAllLine"))//销毁所有线段
        {
            model?.Destroy();
        }
        else if (GUI.Button(new Rect(600, 0, 100, 50), "UpdateColor"))//改变线段颜色
        {
            float r = Random.Range(0,1f);
            float g = Random.Range(0, 1f);
            float b = Random.Range(0, 1f);
            lineColor = new Color(r,g,b,1);
            model?.SetColor(lineColor);
        }
        else if (GUI.Button(new Rect(700, 0, 100, 50), "ChangeDir"))//更改移动方向
        {
            model?.DisableAllLine();
            if (moveDir==Vector2.left)
            {
                moveDir = Vector2.right;
            }
            else if (moveDir ==Vector2.right)
            {
                moveDir = Vector2.left;
            }

            lineInfo = CreateLine("TestLine", endCapName);
            isStopAdd = true;
            Invoke("OpenAddPoint", 1);
            if (!isPlay)
            {
                Play();
            }

        }

        speedStr = GUI.TextArea(new Rect(0, 60, 150, 50),speedStr);
        if (GUI.Button(new Rect(150, 60, 100, 50), "UpdateSpeed"))//更新移动速度
        {
            speed = float.Parse(speedStr);
        }

        lineWidthStr = GUI.TextArea(new Rect(250, 60, 150, 50), lineWidthStr);//更新线段宽度
        if (GUI.Button(new Rect(400, 60, 100, 50), "UpdateWidth"))
        {
            lineWidth = float.Parse(lineWidthStr);
            if (model!=null)
            {
                model.lineWidth = lineWidth;
            }
            
        }

    }

    void OpenAddPoint() {
        isStopAdd = false;
    }

    public override void Update()
    {
        base.Update();
        if (isStopAdd||lineInfo==null||!lineInfo.lineRect||!isPlay)
        {
            return;
        }
        RectTransform rectTrans = lineInfo.lineRect;
        float x=-rectTrans.anchoredPosition.x - screenWidth * 0.5f;
        float y = Random.Range(10, 100);
        lineInfo.AddPos(new Vector2(x, y));

    }

    public override void CheckView()
    {
        base.CheckView();
        VectorLineModel model = GetModel();
        if (model==null)
        {
            return;
        }
        if (moveDir==Vector2.left)//超视野失活
        {

            for (int i = 0; i < model.infoList.Count; i++)
            {
                VectorLineModel.VectorLineInfo info = model.infoList[i];
                RectTransform rectTrans = info.lineRect;
                if (info.posList.Count == 0)
                {
                    continue;
                }
                Vector2 lastPos = info.posList[info.posList.Count - 1];
                float dis = -lastPos.x - rectTrans.anchoredPosition.x;
                if (dis > screenWidth)
                {
                    model.DisableLine(info);
                    i--;
                }
                else
                {
                    break;
                }
            }

        }
        else if (moveDir==Vector2.right)//超视野失活
        {

        }
        


        
    }

}
