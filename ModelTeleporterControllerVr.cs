//ModelTeleporterControllerVR, version 20230223. Tested with Unity 2020.3, XR plugin 4.2.1, Steam VR 2.7.3
//This ModelTeleporterController inherited class for VR is significantly redone, to accomodate SteamVR.
//Most methods are overriden, as necessary. VR raycaster is implemented internally - not a separate class.
//
//Usage:
//  - Display camera to Steam controller VR camera
//  - User Controller to Steam VR "Player"
//  - Have at least one terrain collection (model, real, and model/real instantiable objects)
//  - RaycasterVrController goes to Player > SteamVRObjects > Righthand (can be left, if linerenderer origin adjusted)
//  - Also link the buttons to GameObjects (TeleportMetaphor x2, Next, Prev, LOS), if extra functionality is intended
//  - Lastly, link the Steam VR actions (CreatePress, TeleportPress)
//      To do so, first create these two actions in Steam VR Input. Map them to trigger and grab buttons (Binding UI).
//
//To get rid of pesky Vive controller haptics (terrain collision-based), go to HapticCollider.cs, comment out this:
//  hand.hand.TriggerHapticPulse(length, 100, intensity); (line 279, near the end of the class)
//
//Known bugs:
//  - Controller-in-hand is missing (it is just hand). Steam VR issue - oh well...
//  - Touch-based movement mapped to touchpad (Character Controller, Vector2 movement) is possible but visually bugs out
//      I.e., the hand/controller model glitches out of place with this implementation (feasible without the model)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ModelTeleporterControllerVr : ModelTeleporterController
{
    public GameObject RaycasterVrController;
    public LayerMask raycastLayer;
    [Space(10)]
    public GameObject TeleportMetaphorModel; //this is "point up to the sky and teleport" - a huge collider there
    public GameObject TeleportMetaphorReal;  //this is intended the same
    public GameObject ButtonNext;
    public GameObject ButtonPrev;
    public GameObject ButtonLOS;
    [Space(10)]
    public SteamVR_Action_Boolean CreatePress;
    public SteamVR_Action_Boolean TeleportPress;

    private LineRenderer lineRenderer;
    private Color colorWhite = Color.white;
    private Color colorFadeout = new Color(1, 1, 1, 0);
    private Color colorHighlight = Color.red;

    private Vector3 raycastStart;
    private Vector3 raycastStartVisualFix;
    private Vector3 raycastEnd;
    private GameObject raycastGameObject;
    private Text canvasText;

    public override void Awake()
    {
        Generator = this.gameObject.AddComponent<GeometryGenerator>();
        Generator.SetMainController(this, instantiatedLayerName);
        Generator.InstantiateGeometryStructure();

        this.gameObject.AddComponent<VisibilityEvaluator>();
        this.gameObject.AddComponent<VisibilityEvaluator>();
        VisibilityEvaluator[] visualizers = this.GetComponents<VisibilityEvaluator>();
        VisualizerModel = visualizers[0];
        VisualizerReal = visualizers[1];
        //TODO: precompute model/real dimensions here (size, rotation, direction...)

        Interactor = this.gameObject.AddComponent<InteractionEvaluator>();
        Interactor.SetMainController(this);
        Interactor.SetUserController(UserController);
        Interactor.SetDefaultCoordinatesForVr();

        this.gameObject.AddComponent<LineRenderer>();
        canvasText = DisplayCanvas.GetComponent<Text>();
    }

    public override void Start()
    {
        Generator.GenerateProjectionPlane(TerrainCollection[0].ObjectModel);
        Generator.GenerateProjectionPlane(TerrainCollection[0].ObjectReal);

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = colorWhite;
        lineRenderer.endColor = colorFadeout;                

        TeleportPress.AddOnStateDownListener(OnTeleportPress, SteamVR_Input_Sources.Any);
        CreatePress.AddOnStateDownListener(OnCreatePress, SteamVR_Input_Sources.Any);

        this.ShowTerrain(0);
    }

    public override void Update()
    {
        raycastStart = RaycasterVrController.transform.position;
        raycastEnd = RaycasterVrController.transform.position + (RaycasterVrController.transform.forward * 1000);
        raycastGameObject = null;

        RaycastHit hit;
        if (Physics.Raycast(raycastStart, RaycasterVrController.transform.forward, out hit, Mathf.Infinity, raycastLayer))
        {
            raycastEnd = hit.point;
            raycastGameObject = hit.collider.gameObject;
            //Debug.Log(raycastEnd + " " + raycastGameObject);            
            if (raycastGameObject.layer == LayerMask.NameToLayer(instantiatedLayerName))
            {
                lineRenderer.startColor = colorHighlight;
            }
            else
            {
                lineRenderer.startColor = colorWhite;
            }
        }

        if (displayRaycast)
        {
            if (raycastGameObject == null)
            {
                canvasText.text = "no target";
            }
            else
            {
                canvasText.text = raycastGameObject.name;
            }
        }

        // Set the positions of the Line Renderer to draw the Raycast
        raycastStartVisualFix = raycastStart - (RaycasterVrController.transform.forward * 0.15f)
                              + (RaycasterVrController.transform.right * 0.025f);
        lineRenderer.SetPosition(0, raycastStartVisualFix);
        lineRenderer.SetPosition(1, raycastEnd);

        //hitInfo = Raycaster.Raycast();
        hitInfo = new RaycastHitInfo(raycastEnd, raycastGameObject);

        this.ListenToKeyboardMouseControls();
    }

    void OnCreatePress(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        //Debug.Log("Create trigger pressed");
        if (hitInfo.targetObject != null)
        {
            Interactor.CreateGeometry(hitInfo.targetPoint, hitInfo.targetObject);
        }
    }

    void OnTeleportPress(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        //Debug.Log("Teleport trigger pressed");
        if (hitInfo.targetObject != null &&
            hitInfo.targetObject != TeleportMetaphorModel && hitInfo.targetObject != TeleportMetaphorReal &&
            hitInfo.targetObject != ButtonNext && hitInfo.targetObject != ButtonPrev &&
            hitInfo.targetObject != ButtonLOS)
        {
            Interactor.TeleportInReal(hitInfo.targetPoint, hitInfo.targetObject);
        }
        else if (hitInfo.targetObject == TeleportMetaphorModel || hitInfo.targetObject == TeleportMetaphorReal)
        {
            Interactor.TeleportBetweenModelReal();
        }
        else if (hitInfo.targetObject == ButtonNext)
        {
            this.ReinitTerrains(currentTerrainCollection + 1);
        }
        else if (hitInfo.targetObject == ButtonPrev)
        {
            this.ReinitTerrains(currentTerrainCollection - 1);
        }
        else if (hitInfo.targetObject == ButtonLOS)
        {
            Interactor.VisualizeGeometryLOS();
        }
    }

    void OnDestroy()
    {
        CreatePress.RemoveOnStateDownListener(OnCreatePress, SteamVR_Input_Sources.Any);
        TeleportPress.RemoveOnStateDownListener(OnTeleportPress, SteamVR_Input_Sources.Any);      
    }
}