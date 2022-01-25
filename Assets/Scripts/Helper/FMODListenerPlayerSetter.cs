using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(StudioListener))]
public class FMODListenerPlayerSetter : MonoBehaviour
{
    private StudioListener fmodListener;
    private void Start()
    {
        if (!fmodListener)
        {
            fmodListener = GetComponent<StudioListener>();
        }
        fmodListener.attenuationObject = SpringController.Instance.GetFaceSegment();
    }
}
