using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public struct ModelRealCollection
{
    public GameObject ObjectModel;
    public GameObject ObjectReal;
    public GameObject PlacementModel;
    public GameObject PlacementReal;
    [Range(0, 25)]
    public int maxInstances;

    public ModelRealCollection(GameObject ObjModel, GameObject ObjReal,
                               GameObject PlacementModel, GameObject PlacementReal, int instances)
    {
        this.ObjectModel = ObjModel;
        this.ObjectReal = ObjReal;
        this.PlacementModel = PlacementModel;
        this.PlacementReal = PlacementReal;
        this.maxInstances = instances;
    }
}

public class ModelTeleporterController : MonoBehaviour
{
    public Canvas DisplayCanvas;
    public Camera DisplayCamera;
    public GameObject UserController;
    [Space(10)]
    public ModelRealCollection[] TerrainCollection;
    protected int currentTerrainCollection = 0;
    
    [Space(10)]
    public bool displayRaycast;

    public GeometryGenerator Generator { get; protected set; }
    public string instantiatedLayerName { get; protected set; } = "Instantiated";

    public VisibilityEvaluator VisualizerModel { get; protected set; }
    public VisibilityEvaluator VisualizerReal { get; protected set; }

    public InteractionEvaluator Interactor { get; protected set; }

    public RaycasterScreen Raycaster { get; protected set; }
    public RaycastHitInfo hitInfo;

    public int GetCurrentTerrainCollection()
    {
        return currentTerrainCollection;
    }
    public GameObject GetCurrentTerrainModel()
    {
        return TerrainCollection[currentTerrainCollection].ObjectModel;
    }

    public GameObject GetCurrentTerrainReal()
    {
        return TerrainCollection[currentTerrainCollection].ObjectReal;
    }

    public GameObject GetCurrentPlacementModel()
    {
        return TerrainCollection[currentTerrainCollection].PlacementModel;
    }

    public GameObject GetCurrentPlacementReal()
    {
        return TerrainCollection[currentTerrainCollection].PlacementReal;
    }

    public int GetCurrentMaxInstances()
    {
        return TerrainCollection[currentTerrainCollection].maxInstances;
    }

    public virtual void Awake()
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
        Raycaster.AddTeleportableObject(TerrainCollection[0].ObjectModel);
        Raycaster.AddTeleportableObject(TerrainCollection[0].ObjectReal);
    }

    public virtual void Start()
    {
        Generator.GenerateProjectionPlane(TerrainCollection[0].ObjectModel);
        Generator.GenerateProjectionPlane(TerrainCollection[0].ObjectReal);
        this.ShowTerrain(0);

        Raycaster.setRaycaster(DisplayCamera, displayRaycast, displayRaycast, DisplayCanvas);
    }

    public virtual void Update()
    {
        hitInfo = Raycaster.Raycast();
        this.ListenToKeyboardMouseControls();        
    }

    protected void ReinitTerrains(int terrainInArray)
    {
        if (terrainInArray >= 0 && terrainInArray < TerrainCollection.Length)
        {
            this.ShowTerrain(terrainInArray);

            Generator.DestroyAllGeometry();
            Generator.GenerateProjectionPlane(TerrainCollection[terrainInArray].ObjectModel);
            Generator.GenerateProjectionPlane(TerrainCollection[terrainInArray].ObjectReal);
            VisualizerModel.RemoveVisualization();
            VisualizerReal.RemoveVisualization();
            VisualizerModel.RemoveVisibilityPointsAll();
            VisualizerReal.RemoveVisibilityPointsAll();
            currentTerrainCollection = terrainInArray;
            Interactor.ResetTeleportOnTerrainChange();
        }
    }

    protected void ShowTerrain(int terrainInArray)
    {
        for (int i = 0; i < TerrainCollection.Length; i++)
        {
            if (i == terrainInArray)
            {
                TerrainCollection[i].ObjectModel.SetActive(true);
                TerrainCollection[i].ObjectReal.SetActive(true);
            }
            else
            {
                TerrainCollection[i].ObjectModel.SetActive(false);
                TerrainCollection[i].ObjectReal.SetActive(false);
            }
        }
    }

    protected void ListenToKeyboardMouseControls()
    {
        //visualize geometry LoS
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Interactor.VisualizeGeometryLOS();
        }
        //teleport from real to model
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Interactor.TeleportBetweenModelReal();
        }
        //switch to previous terrain
        if (Input.GetKeyDown(KeyCode.F9))
        {
            this.ReinitTerrains(currentTerrainCollection - 1);
        }
        //switch to next terrain
        if (Input.GetKeyDown(KeyCode.F12))
        {
            this.ReinitTerrains(currentTerrainCollection + 1);
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
    }
}