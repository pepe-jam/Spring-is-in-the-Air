using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpSystem : MonoBehaviour
{
    public bool pickedCrownSplitter = false;
    // Update is called once per frame

    public void pickCrownSplitter()
    {
        pickedCrownSplitter = true;
    }
}
