using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePlayerMovement : MonoBehaviour
{
    public void PausePlayerMovementDueToDialogue()
    {
        SpringController.Instance._inDialogue = true;
    }

    public void ResumePlayerMovementDueToDialogue()
    {
        SpringController.Instance._inDialogue = false;
    }
}
