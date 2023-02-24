using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FuseChecker : MonoBehaviour
{

    public List<XRSocketInteractor> interactors = new List<XRSocketInteractor>();

    public int fusesInserted = 0;
    public bool killable = false;

    // Update is called once per frame
    void Update()
    {
        fusesInserted = 0;
        for (int i = 0; i < interactors.Count; i++)
        {
            if (interactors[i].hasSelection)
            {
                fusesInserted++;
            }
        }
        if (fusesInserted == 5)
        {
            killable = true;
        } else
        {
            killable = false;
        }
    }
}
