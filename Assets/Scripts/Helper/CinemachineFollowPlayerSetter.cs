using System.Collections;
using Cinemachine;
using UnityEngine;

/*
 * Because the Player Character is generated via script, it cannot be set as the follow target of Cinemachine's virtual cameras in the editor.
 * This script tries to find the Player Character's head and gives its Transform component to virtual cameras.
 */
public class CinemachineFollowPlayerSetter : MonoBehaviour
{
    public SpringController playerSpringController;
    public CinemachineVirtualCamera[] virtualCameras;
    // Start is called before the first frame update
    void Start()
    {
        // Safety mechanism for when the user forgets to assign any VirtualCameras in the Inspector
        if (virtualCameras == null || virtualCameras.Length == 0)
        {
            virtualCameras = new[] {GetComponent<CinemachineVirtualCamera>()};
        }

        StartCoroutine(nameof(SetFollowPlayer));
    }

    private IEnumerator SetFollowPlayer()
    {
        bool playerInstantiated = false;   // Player Character cannot be found if it hasn't been created yet
        while (!playerInstantiated)
        {
            var playerHead = playerSpringController.GetFaceSegment();
            if (playerHead)
            {
                playerInstantiated = true;
                foreach (var vcam in virtualCameras)
                {
                    vcam.Follow = playerHead.transform;
                }
            }
            yield return null;
        }
    }
}
