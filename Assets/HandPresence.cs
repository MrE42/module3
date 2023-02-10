using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPresence : MonoBehaviour
{
    
    public XRController leftHand;
    public XRController rightHand;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hand Presence");

    }

    // Update is called once per frame
    void Update()
    {
        //get & print the primary button value
        if (rightHand.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue) && primaryButtonValue) 
            Debug.Log($"Pressing Primary");
        
        //get & print the trigger value
        if (rightHand.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue) && triggerValue >= 0.1f) 
            Debug.Log($"Trigger Pressed, value {triggerValue}");

        //get & print the grip value
        if (rightHand.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue) && gripValue >= 0.1f) 
            Debug.Log($"Grip Pressed, value {gripValue}");
        
        if (rightHand.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxisValue) && primary2DAxisValue != Vector2.zero) Debug.Log("Primary Touchpad" + primary2DAxisValue);
    }
}
