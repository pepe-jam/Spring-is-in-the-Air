using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(StudioListener))]
public class FMODListenerPlayerSetter : MonoBehaviour
{
    public SpringController playerSpringController;
    private StudioListener fmodListener;
    private void Start()
    {
        if (!fmodListener)
        {
            fmodListener = GetComponent<StudioListener>();
        }

        StartCoroutine(SetAttenuationTarget());
    }

    private IEnumerator SetAttenuationTarget()
    {
        bool playerInstantiated = false;   // Player Character cannot be found if it hasn't been created yet
        while (!playerInstantiated)
        {
            var playerHead = playerSpringController.GetFaceSegment();
            if (playerHead)
            {
                fmodListener.attenuationObject = playerHead;
                playerInstantiated = true;
            }
            yield return null;
        }
    }
}
