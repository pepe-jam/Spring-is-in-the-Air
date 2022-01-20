using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


public class Item : MonoBehaviour
{
    public enum InteractionType {None, PickUp};
    public InteractionType type;
    public bool clockwise = false;
    public bool bigger = true;
    

    private void Update()
    {
        Rotate();
        Resize();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(CompareTag("Pickable"))
        {
            switch(type)
            {
                case InteractionType.PickUp:
                    gameObject.SetActive(false);
                    collider.GetComponentInParent<PickUpSystem>().pickCrownSplitter();
                    break;
            }
        }
    }



    public void Rotate()
    {
        if (gameObject.transform.rotation.z < 0.17f && !clockwise)
        {
            gameObject.transform.Rotate(0f, 0f, 0.05f);
        }
        else
        {
            clockwise = true;
        }
        
        if (gameObject.transform.rotation.z > -0.17f && clockwise)
        {
            gameObject.transform.Rotate(0f, 0f, -0.05f);
        }
        else
        {
            clockwise = false;
        }
    }
    
    public void Resize()
    {
        Vector3 scaleChange = new Vector3(0.002f, 0.002f, 0);
        
        if (gameObject.transform.localScale.x > 2f && !bigger)
        {
            gameObject.transform.localScale -= scaleChange;
        }
        else if(!bigger)
        {
            bigger = !bigger;
        }
        if (gameObject.transform.localScale.x < 2.5f && bigger)
        {
            gameObject.transform.localScale += scaleChange;
        }
        else if(bigger)
        {
            bigger = !bigger;
        }
    }
}
