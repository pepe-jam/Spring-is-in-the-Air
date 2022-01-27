using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class LevelTransitionAnimation : MonoBehaviour
{
    [SerializeField] private UnityEvent transitionCompleted;
    
    [Header("Animation Parameters")]
    [SerializeField] private Transform catapultingNPC;
    [SerializeField] private Transform jumpingOffPosition;    // should be a child of catapultingNPC
    [SerializeField] private Animator sceneTransitionAnimator;

    // all time-based parameters are in seconds
    [SerializeField] private float moveIntoPlaceDuration = 1;
    [SerializeField] private float chargingDuration = 1;
    [SerializeField] private float superjumpHeight = 20;
    [SerializeField] private float superjumpDuration = 1;
    [SerializeField] private float pauseDuration = 1;

    [Header("Audio")] 
    [SerializeField] private StudioEventEmitter LevelCompletedSFX;
    [SerializeField] private StudioEventEmitter chargingSFX;
    
    public void StartAnimation()
    {
        StartCoroutine(nameof(PlayAnimation));
    }

    private IEnumerator PlayAnimation()
    {
        // move into position
        
        var bottomPlayerJoint = SpringController.Instance.GetBottomSegment();
        bottomPlayerJoint.GetComponent<Rigidbody2D>().isKinematic = true;
        var startPosition = bottomPlayerJoint.transform.position;
        var startTime = Time.time;
        var velocity = Vector3.zero;
        var gettingInPosition = 0f;
        
        while (gettingInPosition <= 1f)
        {
            velocity = MovePlayerInPosition(bottomPlayerJoint, velocity, gettingInPosition);
            gettingInPosition = (Time.time - startTime) / moveIntoPlaceDuration;
            yield return null;
        }

        yield return new WaitForSeconds(pauseDuration);

        // Charge
        chargingSFX.Play();
        var charge = 0f;
        var startChargeTime = Time.time;
        var originalNPCScale = catapultingNPC.localScale;
        while (charge <= 1f)
        {
            catapultingNPC.localScale = Vector3.Scale(originalNPCScale, new Vector3(1, 1 - (charge / 2), 1));
            velocity = MovePlayerInPosition(bottomPlayerJoint, velocity, gettingInPosition);
            charge = (Time.time - startChargeTime) / chargingDuration;
            SpringController.Instance.Charge();
            yield return null;
        }
        
        yield return new WaitForSeconds(pauseDuration);

        // Jump
        LevelCompletedSFX.Play();
        catapultingNPC.localScale = originalNPCScale;
        
        bottomPlayerJoint.GetComponent<Rigidbody2D>().isKinematic = false;
        SpringController.Instance._jumpCharge = 0;
        
        var topPlayerJoint = SpringController.Instance.GetTopSegment();
        topPlayerJoint.GetComponent<Rigidbody2D>().isKinematic = true;
        var startingPosition = topPlayerJoint.transform.position;
        var targetPosition = startingPosition +
                             new Vector3(startingPosition.x, startingPosition.y + superjumpHeight, startingPosition.z);
        var jumpStartTime = Time.time;
        float jumpProgress = 0f;
        sceneTransitionAnimator.SetTrigger("unloadScene");
        while (jumpProgress <= 1f)
        {
            topPlayerJoint.transform.position = Vector3.Lerp(startingPosition, targetPosition, jumpProgress);
            jumpProgress = (Time.time - jumpStartTime) / superjumpDuration;
            yield return null;
        }
        PlayerPrefs.DeleteKey("position_x");
        PlayerPrefs.DeleteKey("position_y");
        PlayerPrefs.DeleteKey("crownshard_collected");
        transitionCompleted.Invoke();
    }

    private Vector3 MovePlayerInPosition(GameObject bottomPlayerJoint, Vector3 velocity, float gettingInPosition)
    {
        bottomPlayerJoint.transform.position = Vector3.SmoothDamp(bottomPlayerJoint.transform.position,
            jumpingOffPosition.position, ref velocity,
            (1 - gettingInPosition) * moveIntoPlaceDuration);
        return velocity;
    }
}
