using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class ArrayRuleExpand 
{
    //得到数组的所有组合方式
    public static List<T[]> GetArrayRule<T>(this T[] array)
    {
        List<T[]> res=new List<T[]>();
        dfs(array,0,ref res);
        return res;
    }
    
    //得到列表的所有组合方式
    public static List<T[]> GetArrayRule<T>(List<T> list)
    {
        List<T[]> res=new List<T[]>();
        dfs(list.ToArray(),0,ref res);
        return res;
    }

    static void dfs<T>(T[] array,int x,ref List<T[]> allArray)
    {
        if (x == array.Length-1)
        {
            T[] arrayCopy = new T[array.Length];
            Array.Copy(array,arrayCopy,array.Length);
            allArray.Add(arrayCopy); // 添加排列方案
            return;
        }
        List<T> ilsList = new List<T>();
        for (int i = x; i < array.Length; i++)
        {
            if (ilsList.Contains(array[i]))
            {
                continue;
            }
            swap(array,i, x); // 交换，将 c[i] 固定在第 x 位
            ilsList.Add(array[i]);
            dfs(array,x + 1,ref allArray); // 开启固定第 x + 1 位字符
            swap(array,i, x); // 恢复交换
        }
            
    }
    static void swap<T>(T[] array ,int a, int b)
    {
        T tmp = array[a];
        array[a] = array[b];
        array[b] = tmp;
    }
}
