using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public static class SaveLoad {
    
    public static void Save (State state, String filename)
    {
        try {
            Debug.Log("Save state " + filename);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/" + filename);
            bf.Serialize(file, state);
            file.Close();
        } catch (Exception e) { }
    }

    public static State Load (String filename)
    {
        Debug.Log("Load state " + filename);
        if (!File.Exists(Application.persistentDataPath + "/" + filename)) {
            return null;
        }
        else
        {
            try {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/" + filename, FileMode.Open);
                State state = (State)bf.Deserialize(file);
                file.Close();
                return state;
            } catch (Exception e)
            {
                return null;
            }
        }
    }

}
