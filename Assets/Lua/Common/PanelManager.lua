PanelManager = class("PanelManager")
local panelClassDict={}--所有Lua脚本字典
local panelPrefabDict={}--所有面板预制体字典
local panelPool={}--面板池
local viewPanelArr={}--显示着的面板集合
local UIRoot

function PanelManager:ctor()
    for panelName,path in pairs(BaseScriptsPath) do
        local luaPath = path..panelName
        panelClassDict[panelName]=luaPath
    end
    for panelName,path in pairs(BasePrefabsPath) do
        local prefabPath = path..panelName
        panelPrefabDict[panelName]=prefabPath
    end
    CS.PanelManager.instance:Init(PanelManager)
end

--实例化面板
function PanelManager.InstantiatePanel(panelName,parent)
    local obj = BundleManager:GetGameObject("prefabs/Panels",panelPrefabDict[panelName])
    local behaviour = obj:GetComponent("LuaBehaviour")
    if behaviour==nil then
        behaviour = obj:AddComponent(typeof(CS.LuaBehaviour))
    end
    behaviour:Init(panelClassDict[panelName])
    if parent==nil then
        obj.transform:SetParent(PanelManager.GetUIRoot())
    else
        obj.transform:SetParent(parent)
    end
    return obj
end

--展示面板
function PanelManager.ShowPanel(panelName,parent)
    local panelInfo = PanelManager.GetPanelPoolFirstInfo(panelName,true)
    if panelInfo==nil then
        local panelObj=PanelManager.InstantiatePanel(panelName,parent)
        panelObj:SetActive(true)
        panelInfo={}
        panelInfo.panelObj=panelObj
        panelInfo.class=panelClassDict[panelName]
    else
        panelObj=panelInfo.panelObj
        panelObj:SetActive(true)
        if parent==nil then
            panelObj.transform:SetParent(PanelManager.GetUIRoot())
        else
            panelObj.transform:SetParent(parent)
        end
        
    end
    PanelManager.AddViewPanel(panelName,panelInfo)
end

--隐藏面板
function PanelManager.HidePanel(panelName,onDestroy)
    if onDestroy==nil then
        onDestroy=false
    end
    local panelInfo = PanelManager.GetViewPanelLastInfo(panelName,true)
    if panelInfo==nil then
        return false
    end
    local panelObj = panelInfo.panelObj
    if onDestroy==true then
        Destroy(panelObj)
    else
        panelObj:SetActive(false)
        PanelManager.AddPanelPool(panelName,panelInfo)
    end
    return true

end

--向面板池中增加一个面板信息
function PanelManager.AddPanelPool(panelName,panelInfo)
    if panelPool==nil then
        panelPool={}
        local panelInfoArr ={panelInfo}
        panelPool[panelName]=panelInfoArr
        return
    end
    local count=0
    if panelPool[panelName]~=nil then
        count = #panelPool[panelName]
    end
    if panelPool[panelName]==nil or count==0 then
        local panelInfoArr ={panelInfo}
        panelPool[panelName]=panelInfoArr
        return
    end
    table.insert(panelPool[panelName],count,panelInfo)
end

--根据名字取出最先缓存的面板 isLoop =true时若第一个面板不存在则向下遍历
function PanelManager.GetPanelPoolFirstInfo(panelName,isLoop)
    if panelPool==nil or panelPool[panelName]==nil then--不存在的面板
        return nil
    end
    
    local viewCount = #panelPool[panelName]
    if viewCount==0 then--没有已缓存的面板
        return  nil
    else
        local panelObj = panelPool[panelName][1].panelObj
        if panelObj==nil then--第一个缓存的面板不明原因已经销毁
            table.remove(panelPool[panelName],1)
            if isLoop==true then--是否遍历下一个缓存面板
                return PanelManager.GetPanelPoolFirstInfo(panelName,isLoop)
            else
                return nil
            end
        else--找到缓存面板
            local panelInfo = panelPool[panelName][1]
            table.remove(panelPool[panelName],1)
            return panelInfo
        end

    end

end

--增加一个显示面板
function PanelManager.AddViewPanel(panelName,panelInfo)
    if viewPanelArr==nil then
        viewPanelArr={}
        local panelInfoArr ={panelInfo}
        viewPanelArr[panelName]=panelInfoArr
        return
    end
    local count = 0
    if viewPanelArr[panelName]~=nil then
        count = #viewPanelArr[panelName]
    end
    if viewPanelArr[panelName]==nil or count==0 then
        local panelInfoArr ={panelInfo}
        viewPanelArr[panelName]=panelInfoArr
        return
    end
    table.insert(viewPanelArr[panelName],count,panelInfo)
end

--根据面板名字取出最后一个实例化出来的面板  isLoop =true时若最后一个面板不存在则向上遍历
function PanelManager.GetViewPanelLastInfo(panelName,isLoop)
    if viewPanelArr==nil or viewPanelArr[panelName]==nil then--不存在的面板
        return nil
    end
    
    local viewCount = #viewPanelArr[panelName]
    if viewCount==0 then--没有待隐藏面板
        return  nil
    else
        local panelObj = viewPanelArr[panelName][viewCount].panelObj
        if panelObj==nil then--最后一个待隐藏面板不明原因已经销毁
            table.remove(viewPanelArr[panelName],viewCount)
            if isLoop==true then--是否遍历下一个待隐藏面板
                return PanelManager.GetViewPanelLastInfo(panelName,isLoop)
            else
                return nil
            end
        else--找到待隐藏面板
            local panelInfo = viewPanelArr[panelName][viewCount]
            table.remove(viewPanelArr[panelName],viewCount)
            return panelInfo
        end

    end

end

function PanelManager.GetUIRoot()
    UIRoot=nil
	local scene = CS.UnityEngine.SceneManagement.SceneManager.GetActiveScene()
	local objs = scene:GetRootGameObjects()
	for i = 0, objs.Length - 1 do
		local root = objs[i]:GetComponentInChildren(typeof(CS.UnityEngine.Canvas))
		if root ~= nil then
			UIRoot = root.transform
			break
		end
	end
	return UIRoot
end
