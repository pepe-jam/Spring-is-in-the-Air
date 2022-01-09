using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MaxSpeedometer : MonoBehaviour
{
    private float maxReachedRelativeSpeed = 0;
    private Rigidbody2D rigidbody2D;

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        var currentSpeed = rigidbody2D.velocity.magnitude / Physics2D.maxTranslationSpeed;
        if (currentSpeed > maxReachedRelativeSpeed)
        {
            maxReachedRelativeSpeed = currentSpeed;
            Debug.Log("new max speed reached: " + maxReachedRelativeSpeed);
        }
    }
}
