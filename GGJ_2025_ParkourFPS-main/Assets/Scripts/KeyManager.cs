using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    [SerializeField]
    public bool canMovePlatforms = false;

    public static KeyManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

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
            collision.gameObject.GetComponent<ParkourController>().HasKey = false;
            collision.gameObject.GetComponent<ParkourController>().key_tick_img.SetActive(false);
            transform.GetChild(0).GetComponent<Animator>().SetBool("CanInsert", true);
            canMovePlatforms = true;
        }
    }
}
