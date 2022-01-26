using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpSystem : MonoBehaviour
{
    public bool hasCrownShard { get; private set; } = false;
    // Update is called once per frame

    public void pickCrownSplitter()
    {
        hasCrownShard = true;
    }
}
