using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileTools
{

    public static string[] GetAllFileName(string dirctoryPath,string pattern="*",bool serachChildren=false) {
        if (!Directory.Exists(dirctoryPath))
        {
            return null;
        }
        string[] fileNameArr;
        if (serachChildren)
        {
            fileNameArr = Directory.GetFiles(dirctoryPath, pattern, SearchOption.AllDirectories);
        }
        else {
            fileNameArr = Directory.GetFiles(dirctoryPath, pattern);
        }
        for (int i = 0; i < fileNameArr.Length; i++)
        {
            string fileName = fileNameArr[i];
            fileName=Path.GetFileName(fileName);
            fileNameArr[i] = fileName;
        }
        return fileNameArr;
    }

}
