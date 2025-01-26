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
        GameObject[] Checkpoint_platforms = GameObject.FindGameObjectsWithTag("Checkpoints");
        for(int i = 0; i < Checkpoint_platforms.Length; i++)
        {
            for(int j = 0; j < Checkpoint_platforms.Length; j++)
            {
                int i_cp = Checkpoint_platforms[i].GetComponent<CheckPointScript>().checkPoint;
                int j_cp = Checkpoint_platforms[j].GetComponent<CheckPointScript>().checkPoint;
                
                if(i_cp < j_cp)
                {
                    GameObject temp = Checkpoint_platforms[i];
                    Checkpoint_platforms[i] = Checkpoint_platforms[j];
                    Checkpoint_platforms[j] = temp;
                }
            }
        }
        SaveDatas datas = GetSaveData();
        datas.TotalCheckpoints = Checkpoint_platforms.Length;
        datas.x = new float[datas.TotalCheckpoints];
        datas.y = new float[datas.TotalCheckpoints];
        datas.z = new float[datas.TotalCheckpoints];
        for (int i = 0; i < Checkpoint_platforms.Length; i++)
        {
            datas.x[i] = Checkpoint_platforms[i].transform.position.x;
            datas.y[i] = Checkpoint_platforms[i].transform.position.y;
            datas.z[i] = Checkpoint_platforms[i].transform.position.z;
        }
        SaveSystem.SaveData(datas);

        Respawn();
    }

    void Update()
    {
        CheckpointText.text = GetSaveData().currentCheckpoint.ToString();
        CheckPointControl();
        if (Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }
    }

    void CheckPointControl()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.2f, checkPointLayer);
        if (hit.transform == null) return;
        SaveDatas data = GetSaveData();
        if (data.currentCheckpoint < hit.transform.gameObject.GetComponent<CheckPointScript>().checkPoint)
        {
            data.currentCheckpoint = hit.transform.gameObject.GetComponent<CheckPointScript>().checkPoint;
            SaveSystem.SaveData(data);
            CheckpointText.text = data.currentCheckpoint.ToString();
        }
    }

    public void Respawn()
    {

        transform.position = new Vector3(GetSaveData().x[GetSaveData().currentCheckpoint],
            GetSaveData().y[GetSaveData().currentCheckpoint] + 2f,
            GetSaveData().z[GetSaveData().currentCheckpoint]);

    }

    SaveDatas GetSaveData()
    {
        return SaveSystem.LoadData() == null ? new SaveDatas() : SaveSystem.LoadData();
    }
}
