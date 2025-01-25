using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    public static void SaveData(SaveDatas Data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/GJSave.fun";
        FileStream Stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(Stream, Data);
        Stream.Close();
    }

    public static SaveDatas LoadData()
    {
        string path = Application.persistentDataPath + "/GJSave.fun";
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream Stream = new FileStream(path, FileMode.Open);

            SaveDatas data = formatter.Deserialize(Stream) as SaveDatas;
            Stream.Close();
            return data;
        } else
        {
            return null;
        }
    }
}
