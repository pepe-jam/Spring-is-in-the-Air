using System;
using FMOD.Studio;
using FMODUnity;
using ScriptableObjects;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CollisionAudioPlayer : MonoBehaviour
{
    public CollisionSoundEvents collisionSoundEvents;
    public PhysicsMaterial2D[] ignoredPhysicsMaterials;
    
    private Rigidbody2D rigidbody2D;
    

    private readonly EventReference IGNORED_TOKEN = new EventReference();

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (Array.IndexOf(ignoredPhysicsMaterials, col.collider.sharedMaterial) >= 0)    // if physicsMaterial2D should be ignored
        {
            return;
        }
        EventReference eventToTrigger = GetFmodEventFromPhysicsMaterial(col.collider.sharedMaterial);
        var instance = FMODUnity.RuntimeManager.CreateInstance(eventToTrigger);
        instance.start();
        float collisionVelocity = col.rigidbody ? (col.rigidbody.velocity - rigidbody2D.velocity).magnitude : rigidbody2D.velocity.magnitude;
        var impact_force_parameter = collisionSoundEvents.loudnessCurve.Evaluate(collisionVelocity / Physics2D.maxTranslationSpeed);
        instance.setParameterByName(collisionSoundEvents.impactForceParameterName, impact_force_parameter);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject, rigidbody2D));
        RuntimeManager.AttachInstanceToGameObject(instance, transform, rigidbody2D);
        instance.release();
    }

    private EventReference GetFmodEventFromPhysicsMaterial(PhysicsMaterial2D physicsMaterial2D)
    {
        
        foreach (var pair in collisionSoundEvents.fmodEventPerPhysicsMaterial)
        {
            if (pair.physicsMaterial)
            {
                return pair.fmodEvent;
            }   
        }

        return collisionSoundEvents.defaultFmodEvent;
    }
}
