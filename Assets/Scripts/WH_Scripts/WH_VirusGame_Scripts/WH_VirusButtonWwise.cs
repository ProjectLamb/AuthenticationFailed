using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WH_VirusButtonWwise : MonoBehaviour
{

    public void OnMyButtonClick()
    {
        AkSoundEngine.PostEvent("P1Button", gameObject);
    }

    // Start is called before the first frame update
    
}
