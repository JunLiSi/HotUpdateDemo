PanelManager = class("PanelManager")
local panelClassDict={}--所有Lua脚本字典
local panelPrefabDict={}--所有面板预制体字典
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
end

function PanelManager.InstantiatePanel(panelName,parent)
    local obj = BundleManager:GetGameObject("prefabs/Panels",panelPrefabDict[panelName])
    if parent==nil then
        obj.transform:SetParent(PanelManager.GetUIRoot())
    else
        obj.transform:SetParent(parent)
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