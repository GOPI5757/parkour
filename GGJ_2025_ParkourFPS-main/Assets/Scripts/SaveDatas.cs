using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDatas
{
    public int currentCheckpoint;
    public int minutes;
    public int seconds;

    public SaveDatas()
    {
        currentCheckpoint = 0;
        minutes = 0;
        seconds = 0;
    }
}
