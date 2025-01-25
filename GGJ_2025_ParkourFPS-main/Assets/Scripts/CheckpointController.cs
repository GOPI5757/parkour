using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    [Header("Checkpoint Control")]
    public LayerMask checkPointLayer;
    public TMP_Text CheckpointText;

    [Header("Ground Check")]
    public float playerHeight;

    void Start()
    {
        
    }

    void Update()
    {
        CheckpointText.text = GetSaveData().currentCheckpoint.ToString();
        CheckPointControl();
        ResetCheckpoint();
    }

    void CheckPointControl()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.2f, checkPointLayer);
        if (hit.transform == null) return;
        print(hit.transform.gameObject.GetComponent<CheckPointScript>().checkPoint);
        SaveDatas data = GetSaveData();
        if (data.currentCheckpoint < hit.transform.gameObject.GetComponent<CheckPointScript>().checkPoint)
        {
            data.currentCheckpoint = hit.transform.gameObject.GetComponent<CheckPointScript>().checkPoint;
            SaveSystem.SaveData(data);
            CheckpointText.text = data.currentCheckpoint.ToString();
        }
    }

    void ResetCheckpoint()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SaveDatas Data = GetSaveData();
            Data.currentCheckpoint = 0;
            SaveSystem.SaveData(Data);
        }
    }

    SaveDatas GetSaveData()
    {
        return SaveSystem.LoadData() == null ? new SaveDatas() : SaveSystem.LoadData();
    }
}
