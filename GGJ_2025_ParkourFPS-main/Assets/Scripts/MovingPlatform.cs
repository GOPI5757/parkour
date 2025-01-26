using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    Vector3 TargetPos;

    Vector3 StartPos;
    Vector3 FinalTargetPos;

    float elapsedTime, elapsedWaitTime;
    public float LerpTime, WaitTime;

    bool IsWaiting = false;

    float MoveTime = 0f;

    public float StartMoveTimer = 2f;
    bool canMove = false;

    void Start()
    {
        StartPos = transform.position;
        FinalTargetPos = new Vector3(transform.position.x + TargetPos.x, transform.position.y + TargetPos.y, transform.position.z + TargetPos.z);
    }

    void Update()
    {
        PlatformWork();
        if(!canMove && KeyManager.instance.canMovePlatforms)
        {
            if(MoveTime <= StartMoveTimer)
            {
                MoveTime += Time.deltaTime;
            } else
            {
                canMove = true;
            }
        }
    }

    void PlatformWork()
    {
        if (!KeyManager.instance.canMovePlatforms || !canMove) return;
        if (!IsWaiting)
        {
            float t = elapsedTime / LerpTime;

            float x = Mathf.Lerp(StartPos.x, FinalTargetPos.x, t);
            float y = Mathf.Lerp(StartPos.y, FinalTargetPos.y, t);
            float z = Mathf.Lerp(StartPos.z, FinalTargetPos.z, t);
            transform.position = new Vector3(x, y, z);
            elapsedTime += Time.deltaTime;

            if (t >= 1)
            {
                elapsedTime = 0f;
                IsWaiting = true;
            }
        }
        else
        {
            elapsedWaitTime += Time.deltaTime;
            if (elapsedWaitTime > WaitTime)
            {
                elapsedWaitTime = 0f;
                IsWaiting = false;
                Vector3 temp = StartPos;
                StartPos = FinalTargetPos;
                FinalTargetPos = temp;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            collision.gameObject.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.transform.SetParent(null);
        }
    }
}
