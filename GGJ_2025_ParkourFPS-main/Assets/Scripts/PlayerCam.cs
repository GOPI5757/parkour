using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{

    public ParkourController parkourController;

    public void SlideCompleted()
    {
        parkourController.SlideAnimFinished();
    }
}
