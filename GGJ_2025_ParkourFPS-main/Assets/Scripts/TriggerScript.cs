using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerScript : MonoBehaviour
{

    [SerializeField]
    bool isLeft;

    [SerializeField]
    bool isForward;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(isForward)
        {
            Player.Instance.canGoForward = false;
            
        }
        if(collision.gameObject.tag == "Walls")
        {
            if(isLeft)
            {
                Player.Instance.canGoLeft = false;
            } else if(!isForward)
            {
                Player.Instance.canGoRight = false;
            }
            Player.Instance.rb.useGravity = false;
            Player.Instance.canAffectMouseY = false;
            if(Player.Instance.x < 0f)
            {
                Player.Instance.x = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "slopeSlide")
        {
            Player.Instance.CamParent.GetComponent<Animator>().SetBool("IsSlope", true);
            Player.Instance.IsSlope = true;
        }

        if(other.gameObject.tag == "sliderEntry")
        {
            Player.Instance.IsEnteringSlide = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "slopeSlide")
        {
            Player.Instance.CamParent.GetComponent<Animator>().SetBool("IsSlope", false);
            Player.Instance.IsSlope = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (isForward)
        {
            Player.Instance.canGoForward = true;
        }

        if (collision.gameObject.tag == "Walls")
        {
            if(isLeft)
            {
                Player.Instance.canGoLeft = true;
            }
            else
            {
                Player.Instance.canGoRight = true;
            }

            Player.Instance.rb.useGravity = true;
            Player.Instance.canAffectMouseY = true;
        }
    }
}
