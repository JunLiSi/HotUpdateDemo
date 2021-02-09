using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using Object = System.Object;

public class UniqueStringManager
{
    private static UniqueStringManager _instance;
    //小写字母数组 26
    private object[] lowerLetterArr = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
    //大写字母数组 26
    private object[] upperLetterArr = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
    //数字数组 10
    private object[] numberArr = {1,2,3,4,5,6,7,8,9,0};
    
    //特殊字符  4
    private object[] specialArr = {'+','-','*','/'};

    private object[][] curRuleArr; //当前规则
    private List<object[][]> allRuleList;//所有的组合规则
    private int ruleId = 0;//第几种组合规则

    private int[] curIdArr;
    private int[] maxIdArr;

    private bool isFull;
    

    public static UniqueStringManager instance
    {
        get
        {
            if (_instance==null)
            {
                _instance=new UniqueStringManager();
            }
            return _instance;
        }
    }

    private UniqueStringManager()
    {
        curRuleArr = new Object[4][];
        curRuleArr[0] = lowerLetterArr;
        curRuleArr[1] = upperLetterArr;
        curRuleArr[2] = numberArr;
        curRuleArr[3] = specialArr;
        allRuleList = curRuleArr.GetArrayRule();
        curIdArr=new int[curRuleArr.Length];
        maxIdArr = new int[curRuleArr.Length];
        isFull = false;
        OpenNextRule();
        
    }

    //新规则初始化数据
    private void RestartIdArr()
    {
        for (int i = 0; i < curIdArr.Length; i++)
        {
            curIdArr[i] = 0;
        }

        for (int i = 0; i < maxIdArr.Length; i++)
        {
            maxIdArr[i] = curRuleArr[i].Length - 1;
        }
    }

    //打开下一个规则
    private void OpenNextRule()
    {
        if (ruleId>allRuleList.Count-1)
        {
            isFull = true;
            //Debug.LogError("唯一字符串池已经全部取完");
            return;
        }
        curRuleArr = allRuleList[ruleId];
        RestartIdArr();
        ruleId++;
    }

    //取出一个字符串
    public string GetString()
    {
        if (isFull)
        {
            Debug.LogError("唯一字符串池已全部取完");
            return null;
        }
        StringBuilder str = new StringBuilder();
        for (int i = 0; i < curRuleArr.Length; i++)
        {
            object value = curRuleArr[i][curIdArr[i]];
            str.Append(value);
        }
        for (int j = curIdArr.Length-1; j >=0; j--)
        {
            curIdArr[j]++;
            if (curIdArr[j]>maxIdArr[j])
            {
                curIdArr[j] = 0;
                if (j==0)
                {
                    OpenNextRule();
                }
            }
            else
            {
                break;
            }
        }
       
        return str.ToString();
    }

    //字符串池是否已经使用完
    public bool IsFull()
    {
        return isFull;
    }

}
