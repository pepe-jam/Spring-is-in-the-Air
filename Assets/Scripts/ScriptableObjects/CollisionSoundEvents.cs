using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CollisionSoundEmitter", menuName = "ScriptableObjects/CollisionSoundEmitter", order = 1)]
    public class CollisionSoundEvents: ScriptableObject
    {
        public FmodEventPerPhysicsMaterial[] fmodEventPerPhysicsMaterial;
        public EventReference defaultFmodEvent;
        
        [Tooltip("Determines how the impact_force parameter in FMOD should be set in relation to the velocity the objects had when colliding (x axis is from 0 to max velocity)")]
        public AnimationCurve loudnessCurve;
        public string impactForceParameterName;

    }

    [Serializable]
    public struct FmodEventPerPhysicsMaterial
    {
        public PhysicsMaterial2D physicsMaterial;
        public EventReference fmodEvent;
    }
}