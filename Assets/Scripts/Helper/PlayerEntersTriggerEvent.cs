using System;
using UnityEngine;
using UnityEngine.Events;

namespace Helper
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerEntersTriggerEvent : MonoBehaviour
    {
        public UnityEvent playerEntersTrigger;
        public UnityEvent playerExitsTrigger;
        public void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Trigger!");
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                playerEntersTrigger.Invoke();
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            Debug.Log("Trigger Exit!");
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                playerExitsTrigger.Invoke();
            }
        }
    }
}
