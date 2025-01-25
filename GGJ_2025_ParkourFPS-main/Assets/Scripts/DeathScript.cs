using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScript : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            collision.gameObject.transform.position = new Vector3(GetSaveData().x[GetSaveData().currentCheckpoint],
                GetSaveData().y[GetSaveData().currentCheckpoint] + 2f,
                GetSaveData().z[GetSaveData().currentCheckpoint]);
        }
    }

    SaveDatas GetSaveData()
    {
        return SaveSystem.LoadData() == null ? new SaveDatas() : SaveSystem.LoadData();
    }
}
