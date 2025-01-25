using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float timeToWaitBeforePop;
    public AudioSource audioSource;

    Coroutine poppingRoutine = null;
    Rigidbody rb;

    bool collided;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void OnCollisionEnter(Collision other) 
    {
        if(other.transform.CompareTag("Player"))
        {
            poppingRoutine = StartCoroutine(PoppingBubble());
        }
    }

    private void OnCollisionExit(Collision other) 
    {
        if(other.transform.CompareTag("Player"))
        {
            StopCoroutine(poppingRoutine);
        }
    }

    public void DestroyBubble()
    {
        audioSource.transform.parent = null;
        audioSource.Play();
        rb.isKinematic = false;
        poppingRoutine = StartCoroutine(PoppingBubble());
    }

    IEnumerator PoppingBubble()
    {
        yield return new WaitForSeconds(timeToWaitBeforePop);
        Destroy(gameObject);
    }
}
