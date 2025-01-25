using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDatas
{
    public int currentCheckpoint;
    public int TotalCheckpoints;
    public int minutes;
    public int seconds;

    public float[] x;
    public float[] y;
    public float[] z;

    public SaveDatas()
    {
        currentCheckpoint = 0;
        minutes = 0;
        seconds = 0;
        TotalCheckpoints = 0;
        x = new float[TotalCheckpoints];
        y = new float[TotalCheckpoints];
        z = new float[TotalCheckpoints];
    }
}
