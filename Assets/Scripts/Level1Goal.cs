using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LevelTransitionAnimation), typeof(Collider2D))]
public class Level1Goal : MonoBehaviour
{
    [SerializeField] private UnityEvent levelCompletedDialogue;
    [SerializeField] private UnityEvent stillMissingCrownShard;
    [SerializeField] private UnityEvent zoomOnCrownShard;
    [SerializeField] private UnityEvent afterZoomOnCrownShard;

    public float dialogueCooldown = 5;

    private LevelTransitionAnimation levelTransitionAnimation;
    private Collider2D trigger;
    private int progress = 0;

    void Start()
    {
        levelTransitionAnimation = GetComponent<LevelTransitionAnimation>();
        trigger = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            SpringController.Instance._inDialogue = true;
            trigger.enabled = false;

            
            if (PlayerPrefs.HasKey("crownshard_collected"))
            {
                levelCompletedDialogue.Invoke();
            }
            else
            {
                stillMissingCrownShard.Invoke();
            }
        }
    }

    public void AfterDialogue()
    {
        StartCoroutine(nameof(AfterDialogueCoroutine));
    }

    private IEnumerator AfterDialogueCoroutine()
    {
        trigger.enabled = false;
        yield return new WaitForSeconds(0.1f);  // yarn can't start a new dialogue immediately after the currently running one has finished, so we'll have to wait a little 
         if (PlayerPrefs.HasKey("crownshard_collected"))
        {
            trigger.enabled = false;
            levelTransitionAnimation.StartAnimation();
        }
        else
        {
            switch (progress)
            {
                case 0:
                    zoomOnCrownShard.Invoke();
                    break;
                case 1: 
                    afterZoomOnCrownShard.Invoke();
                    SpringController.Instance._inDialogue = false;
                    yield return new WaitForSeconds(dialogueCooldown);
                    progress = -1;
                    trigger.enabled = true;
                    break;
                default:
                    progress = -1;
                    SpringController.Instance._inDialogue = false;
                    yield return new WaitForSeconds(dialogueCooldown);
                    trigger.enabled = true;
                    break;
            }

            progress++;
        }
    }
}
