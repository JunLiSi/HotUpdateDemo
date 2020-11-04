using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Md5Tools
{
    //文件转化为Md5
    public static string FileToMd5(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (System.Exception ex)
        {
            throw new System.Exception("md5file() fail, error:" + ex.Message);
        }
    }
}
