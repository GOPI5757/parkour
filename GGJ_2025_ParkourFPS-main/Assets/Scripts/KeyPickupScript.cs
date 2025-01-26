using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class KeyPickupScript : MonoBehaviour
{
    [SerializeField]
    float RotateSpeed;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(0f, RotateSpeed * Time.deltaTime, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<ParkourController>().HasKey = true;
            collision.gameObject.GetComponent<ParkourController>().PlayASound(collision.gameObject.GetComponent<ParkourController>().keySound);
            Destroy(gameObject);
        }
    }
}
