using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeCanvasGroupOpacityOverDistance : MonoBehaviour
{
    public SpringController playerSpringController;

    public AnimationCurve visibilityOverDistance;

    public float fadeDistance = 10;
    [Range(0, 0.999999999f)]
    public float smoothingFactor;

    public float idleDistance = 20;
    // Start is called before the first frame update

    private Transform _playerTransform;
    private CanvasGroup _canvasGroup;
    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        StartCoroutine(nameof(GetPlayerTransformWhenCreated));
    }

    private IEnumerator FadeOpacityByPlayerDistance()
    {
        var previousOpacity = 0f;
        while (true)
        {
            var playerDistance = (_playerTransform.position - this.transform.position).magnitude;
            var newOpacity = Mathf.Lerp(visibilityOverDistance.Evaluate(playerDistance / fadeDistance), previousOpacity, smoothingFactor);
            _canvasGroup.alpha = newOpacity;
            previousOpacity = newOpacity;

            yield return playerDistance > idleDistance ? new WaitForSeconds(1f) : null;
        }
    }
    
    private IEnumerator GetPlayerTransformWhenCreated()
    {
        bool playerInstantiated = false;   // Player Character cannot be found if it hasn't been created yet
        while (!playerInstantiated)
        {
            var playerHead = playerSpringController.GetFaceSegment();
            if (playerHead)
            {
                _playerTransform = playerHead.transform;
                playerInstantiated = true;
            }
            yield return null;
        }
        StartCoroutine(nameof(FadeOpacityByPlayerDistance));
    }
}
