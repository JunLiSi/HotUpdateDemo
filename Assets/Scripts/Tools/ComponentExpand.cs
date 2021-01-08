using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public static class ComponentExpand
{
    public static bool IsNull(this GameObject go) {
        if (go == null) return true;
        return false;
    }


}
