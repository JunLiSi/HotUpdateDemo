require "Common.AssetsBase"
require "Common.ClassFunction"
require "Common.AssetsBase"
PanelManager=require "Common.PanelManager"

System = CS.System
Time = CS.UnityEngine.Time
Debug = CS.UnityEngine.Debug
GameObject = CS.UnityEngine.GameObject
Object=CS.UnityEngine.Object
Destroy=Object.Destroy
Vector3 = CS.UnityEngine.Vector3
Vector4 = CS.UnityEngine.Vector4
Vector2 = CS.UnityEngine.Vector2
Quaternion = CS.UnityEngine.Quaternion
Color = CS.UnityEngine.Color
Color32 = CS.UnityEngine.Color32
Instantiate = GameObject.Instantiate
LayerMask = CS.UnityEngine.LayerMask
Resources = CS.UnityEngine.Resources
Transfrom = CS.UnityEngine.Transfrom
Sprite = CS.UnityEngine.Sprite
Texture2D = CS.UnityEngine.Texture2D
Screen = CS.UnityEngine.Screen
PlayerPrefs = CS.UnityEngine.PlayerPrefs
Directory = CS.System.IO.Directory
File = CS.System.IO.File
Application = CS.UnityEngine.Application
BundleManager=CS.InitBundleManager.instance
PanelManager:new()