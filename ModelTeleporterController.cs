using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelTeleporterController : MonoBehaviour
{
    public Canvas DisplayCanvas;
    public Camera DisplayCamera;
    public GameObject UserController;
    [Space(10)]
    public GameObject ObjectModel;
    public GameObject ObjectReal;
    public GameObject RadioModel;
    public GameObject RadioReal;
    [Range(0, 25)]
    public int maximumObjectInstances = 5;
    [Space(10)]
    public bool displayRaycast;

    public GeometryGenerator Generator { get; private set; }
    public string instantiatedLayerName { get; private set; } = "Instantiated";

    public VisibilityEvaluator VisualizerModel { get; private set; }
    public VisibilityEvaluator VisualizerReal { get; private set; }

    public InteractionEvaluator Interactor { get; private set; }

    public RaycasterScreen Raycaster { get; private set; }
    public RaycastHitInfo hitInfo, hitInfoLeft, hitInfoRight;

    public RaycasterVrController RaycasterVrLeft { get; private set; }
    public RaycasterVrController RaycasterVrRight { get; private set; }
    public GameObject RaycasterDummy;

    private void Awake()
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

        Raycaster = this.gameObject.AddComponent<RaycasterScreen>();        
        Raycaster.AddTeleportableObject(ObjectModel);
        Raycaster.AddTeleportableObject(ObjectReal);

        RaycasterVrLeft = this.gameObject.AddComponent<RaycasterVrController>();
    }

    void Start()
    {
        Generator.GenerateProjectionPlane(ObjectModel);
        Generator.GenerateProjectionPlane(ObjectReal);

        Raycaster.setRaycaster(DisplayCamera, displayRaycast, displayRaycast, DisplayCanvas);
        RaycasterVrLeft.setRaycaster(RaycasterDummy, false, false, null);
    }

    void Update()
    {
        //hitInfo = Raycaster.Raycast();
        hitInfo = RaycasterVrLeft.Raycast();

        //teleport from real to model
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Interactor.TeleportBetweenModelReal();
        }
        //teleport around real
        if (Input.GetMouseButtonDown(0) && hitInfo.targetObject != null)
        {
            Interactor.TeleportInReal(hitInfo.targetPoint, hitInfo.targetObject);
        }
        //instantiate geometry
        if (Input.GetMouseButtonDown(1) && hitInfo.targetObject != null)
        {
            Interactor.CreateGeometry(hitInfo.targetPoint, hitInfo.targetObject);
        }
        //visualize geometry LoS
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Interactor.VisualizeGeometryLOS();
        }
    }
}