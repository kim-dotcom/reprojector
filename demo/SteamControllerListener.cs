using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SteamControllerListener : MonoBehaviour
{
    public SteamVR_Action_Boolean TeleportPress;
    public SteamVR_Action_Boolean CreatePress;

    void Start()
    {
        TeleportPress.AddOnStateDownListener(OnTeleportPress, SteamVR_Input_Sources.Any);
        CreatePress.AddOnStateDownListener(OnCreatePress, SteamVR_Input_Sources.Any);
    }

    void OnTeleportPress(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("Teleport trigger pressed");
    }

    void OnCreatePress(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("Create trigger pressed");
    }

    void OnDestroy()
    {
        TeleportPress.RemoveOnStateDownListener(OnTeleportPress, SteamVR_Input_Sources.Any);
        CreatePress.RemoveOnStateDownListener(OnCreatePress, SteamVR_Input_Sources.Any);
    }
}
