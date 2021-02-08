using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using static SJL.VectorLineModel;

namespace SJL {

    public class VectorLineModel {

        private VectorLineManager mgr;
        internal readonly int id;
        private float _lineWidth;
        internal float lineWidth {
            get { return _lineWidth; }
            set {
                ChangeLineWidth(value);
            }
        }

        internal Color32 color;

        internal List<VectorLineInfo> infoList;

        internal List<VectorLineInfo> infoPoolList;

        public List<string> endCapNameList;

        internal VectorLineModel(int _id, VectorLineManager _mgr,float _lineWidth_=10)
        {
            mgr = _mgr;
            id = _id;
            _lineWidth = _lineWidth_;
            infoList = new List<VectorLineInfo>();
            infoPoolList = new List<VectorLineInfo>();
            endCapNameList = new List<string>();
        }

        internal VectorLineInfo CreateLine(string lineName, Transform parent,string endCapName=null)
        {
            VectorLineInfo info = SearchPool();
            if (info != null)
            {
                info.parent = parent;
                infoList.Add(info);
                return info;
            }
            info = new VectorLineInfo(this,lineWidth);
            info.Create(lineName, parent, endCapName);
            info.SetColor(color);
            infoList.Add(info);
            return info;
        }

        internal VectorLineInfo CreateLine(string lineName, Transform parent, GameObject canvasObj, GameObject maskObj,string endCapName=null)
        {
            VectorLineInfo info = SearchPool();
            if (info != null)
            {
                info.parent = parent;
                info.canvasObj = canvasObj;
                info.SetMask(maskObj);
                infoList.Add(info);
                
                return info;
            }
            info = new VectorLineInfo(this,lineWidth);
            info.Create(lineName, parent, endCapName);
            SetMask(info, maskObj);
            SetCanvas(info, canvasObj);
            info.SetColor(color);
            infoList.Add(info);
            return info;
        }

        internal bool CreateEndCap(string endCapName,EndCap endCap,params Texture2D[] texArr) {
            if (endCapNameList.Contains(endCapName))
            {
                Debug.LogError("【EndCapName】:"+endCapName+":"+"禁止重复创建！！！！");
                return false;
            }
            //try
            //{
            //    VectorLine.RemoveEndCap(endCapName);
            //}
            //catch (System.Exception ex)
            //{

            //}
            
            VectorLine.SetEndCap(endCapName,endCap,texArr);
            endCapNameList.Add(endCapName);
            return true;
        }

        internal void RemoveEndCap(string endCapName)
        {
            if (endCapNameList.Contains(endCapName))
            {
                VectorLine.RemoveEndCap(endCapName);
                endCapNameList.Remove(endCapName);
            }
        }

        VectorLineInfo SearchPool()
        {
            VectorLineInfo _info = null;
            if (infoPoolList.Count == 0)
            {
                return _info;
            }
            _info = infoPoolList[0];
            _info.AgainOpen();
            infoPoolList.RemoveAt(0);
            return _info;
        }

        internal void DisableLine(VectorLineInfo _info)
        {
            if (!_info.isDisable)
            {
                _info.Disable();
                infoPoolList.Add(_info);
                infoList.Remove(_info);
            }
        }

        internal void DisableAllLine()
        {
            foreach (var item in infoList)
            {
                if (!item.isDisable)
                {
                    item.Disable();
                    infoPoolList.Add(item);
                }
            }
            infoList.Clear();
        }

        internal void SetCanvas(VectorLineInfo info, GameObject canvasObj)
        {
            info.canvasObj = canvasObj;
        }

        internal void SetMask(VectorLineInfo info, GameObject maskObj)
        {
            info.SetMask(maskObj);
        }

        internal void SetColor(VectorLineInfo info, Color32 col)
        {
            info.SetColor(col);
        }

        internal void SetColor(Color32 col) {
            for (int i = 0; i < infoList.Count; i++)
            {
                infoList[i].SetColor(col);
            }
            for (int i = 0; i < infoPoolList.Count; i++)
            {
                infoPoolList[i].SetColor(col);
            }
        }

        void ChangeLineWidth(float width) {
            _lineWidth = width;
            for (int i = 0; i < infoList.Count; i++)
            {
                infoList[i].lineWidth = width;
            }
            for (int i = 0; i < infoPoolList.Count; i++)
            {
                infoPoolList[i].lineWidth = width;
            }
        }

        internal void AddPos(VectorLineInfo info, Vector2 pos)
        {
            info.AddPos(pos);
        }

        internal void Destroy(VectorLineInfo info) {
            if (infoList.Contains(info))
            {
                info.Destroy();
                infoList.Remove(info);
            }
            if (infoPoolList.Contains(info))
            {
                info.Destroy();
                infoPoolList.Remove(info);
            }
        }

        internal void Destroy() {
            for (int i = 0; i < endCapNameList.Count; i++)
            {
                string name = endCapNameList[i];
                VectorLine.RemoveEndCap(name);
            }
            endCapNameList.Clear();

            for (int i = 0; i < infoList.Count; i++)
            {
                VectorLineInfo vlInfo = infoList[i];
                vlInfo.Destroy();
            }
            for (int i = 0; i < infoPoolList.Count; i++)
            {
                VectorLineInfo vlInfo = infoPoolList[i];
                vlInfo.Destroy();
            }
            infoList.Clear();
            infoPoolList.Clear();
            VectorLineManager.instance.modelDict.Remove(id);
        }






        #region VectorLineInfo 类对象

        internal class VectorLineInfo
        {
            private VectorLineModel model;
            internal VectorLine vectorLine;
            internal List<Vector2> posList;
            internal string endCapName;
            internal string lineName;
            internal bool allowDisablePoint;//是否允许失活状态下添加线段点

            internal RectTransform lineRect
            {
                get
                {
                    if (vectorLine == null)
                    {
                        return null;
                    }
                    return vectorLine.rectTransform;
                }
            }

            private Joins _joins;
            internal Joins joins
            {
                set
                {
                    _joins = value;
                    vectorLine.joins = value;
                }
                get
                {
                    return _joins;
                }
            }

            private LineType _lineType;
            internal LineType lineType
            {
                set
                {
                    _lineType = value;
                    vectorLine.lineType = value;
                }
                get
                {
                    return _lineType;
                }
            }

            private float _lineWidth;
            public float lineWidth
            {
                get
                {
                    return _lineWidth;
                }
                set
                {
                    _lineWidth = value;
                    if (vectorLine!=null)
                    {
                        vectorLine.lineWidth = value;
                    }
                }
            }

            private GameObject _canvasObj;
            public GameObject canvasObj
            {
                get
                {
                    return _canvasObj;
                }
                set
                {
                    _canvasObj = value;
                    vectorLine.SetCanvas(value);
                }
            }

            private Transform _parent;
            public Transform parent
            {
                get
                {
                    return _parent;
                }
                set
                {
                    _parent = value;
                    lineRect.SetParent(value);
                }
            }

            internal bool isDisable;

            internal VectorLineInfo(VectorLineModel _model,float _lineWidth_=10)
            {
                model = _model;
                posList = new List<Vector2>();
                _joins = Joins.Weld;
                _lineType = LineType.Continuous;
                _lineWidth = _lineWidth_;
                isDisable = false;
                allowDisablePoint = true;
            }

            internal void Disable()
            {
                if (isDisable)
                {
                    return;
                }
                posList.Clear();
                vectorLine.Draw();
                lineRect?.gameObject.SetActive(false);
                isDisable = true;
            }

            internal void Destroy()
            {
                if (lineRect == null)
                {
                    return;
                }
                Debug.Log("【VectorLineInfo】:" + lineRect.name + "销毁");
                VectorLine.Destroy(ref vectorLine);
            }

            internal void Create(string _lineName, Transform parentTrans, string _endCapName = null)
            {
                if (vectorLine != null)
                {
                    Debug.LogError("【VectorLineInfo】:不要重复创建");
                    return;
                }
                lineName = _lineName;
                vectorLine = new VectorLine(_lineName, posList, lineWidth, lineType, joins);
                lineRect.SetParent(parentTrans);
                _parent = parentTrans;

                vectorLine.endPointsUpdate = 2;
                if (!string.IsNullOrEmpty(_endCapName))
                {
                    endCapName = _endCapName;
                    vectorLine.endCap = endCapName;
                    vectorLine.continuousTexture = true;
                }
            }

            internal void AgainOpen()
            {
                if (lineRect == null)
                {
                    return;
                }
                lineRect?.gameObject.SetActive(true);
                isDisable = false;
            }

            internal void SetMask(GameObject maskObj)
            {
                vectorLine.SetMask(maskObj);
            }

            internal void SetColor(Color32 col)
            {
                vectorLine.color = col;
                //vectorLine.SetColor(col);
            }

            internal void SetEndCap(string _endCapName)
            {
                if (string.IsNullOrEmpty(endCapName))
                {
                    return;
                }
                endCapName = _endCapName;
                vectorLine.endCap = _endCapName;
            }

            internal void AddPos(Vector2 pos)
            {
                if (lineRect == null || vectorLine == null)
                {
                    return;
                }
                if (isDisable)
                {
                    if (!allowDisablePoint)
                    {
                        return;
                    }
                    Debug.LogWarning("【VectorLineInfo】:失活状态添加线段点！,若不允许更改“allowDisablePoint=false”");
                }
                posList.Add(pos);
                vectorLine.Draw();
            }

        }

        #endregion

    }

    public class VectorLineManager
    {
        private static VectorLineManager _instance;
        public static VectorLineManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VectorLineManager();
                }
                return _instance;
            }
        }

        public Dictionary<int, VectorLineModel> modelDict;


        private VectorLineManager() {
            modelDict = new Dictionary<int, VectorLineModel>();
        }

        public VectorLineModel CreateModel(int id,float lineWidth=10,bool force=true) {
            if (modelDict.ContainsKey(id))
            {
                if (force)
                {
                    Debug.LogError("model:" + id + " 已经存在！！,强制创建");
                    DestroyModel(id);
                }
                else {
                    Debug.LogError("model:" + id + " 已经存在！！,不允许强制创建");
                    return null;
                }
            }
            VectorLineModel model = new VectorLineModel(id,this,lineWidth);
            modelDict.Add(id,model);
            return model;
        }

        public void DestroyModel(int id) {
            if (!modelDict.ContainsKey(id))
            {
                return;
            }
            VectorLineModel model = modelDict[id];
            model.Destroy();
        }

        public void CreateEndCap(int id,string endCapName,EndCap endCap, params Texture2D[] texArr) {
            if (!modelDict.ContainsKey(id))
            {
                Debug.LogError("未知ID:" + id);
                return;
            }
            VectorLineModel model = modelDict[id];
            model.CreateEndCap(endCapName,endCap,texArr);
        }

        internal VectorLineInfo CreateLine(int id, string lineName, Transform parent) {
            if (!modelDict.ContainsKey(id))
            {
                Debug.LogError("未知ID:"+id);
                return null;
            }
            VectorLineModel model = modelDict[id];
            VectorLineInfo info = model.CreateLine(lineName,parent);
            return info;
        }

        internal VectorLineInfo CreateLine(int id, string lineName, Transform parent, GameObject canvasObj, GameObject maskObj)
        {
            if (!modelDict.ContainsKey(id))
            {
                Debug.LogError("未知ID:" + id);
                return null;
            }
            VectorLineModel model = modelDict[id];
            VectorLineInfo info = model.CreateLine(lineName, parent,canvasObj,maskObj);
            return info;
        }

        internal void AddPos(int id,VectorLineInfo info,Vector2 pos) {
            if (!modelDict.ContainsKey(id))
            {
                return;
            }
            VectorLineModel model = modelDict[id];
            model.AddPos(info,pos);
        }

        public void DestroyInstance() {
            foreach (var item in modelDict.Values)
            {
                item.Destroy();
            }
            modelDict.Clear();
            _instance = null;
        }

    }

}


