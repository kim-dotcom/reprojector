using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryGenerator : MonoBehaviour
{
    private GameObject ParentInstantiated;
    private GameObject ParentObjectModel;
    private GameObject ParentObjectReal;
    public List<GameObject> ObjectModelReferences { get; private set; }
    public List<GameObject> ObjectRealReferences { get; private set; }
    private int instantiatedObjectCounter = 1;
    public string instantiatedLayerName { get; private set; } = "Instantiated";

    private ModelTeleporterController MainController;

    public void SetMainController(ModelTeleporterController Controller, string layer)
    {
        this.MainController = Controller;
        this.instantiatedLayerName = layer;
    }

    //generate a ground plane for a real/model terrain so that there is basis for teleporting
    public void GenerateProjectionPlane(GameObject TargetObject)
    {
        Vector3 planeX0Y0 = TargetObject.transform.GetChild(0).transform.position;
        Vector3 planeX1Y0 = TargetObject.transform.GetChild(1).transform.position;
        Vector3 planeX0Y1 = TargetObject.transform.GetChild(2).transform.position;
        Vector3 planeX1Y1 = new Vector3(planeX1Y0.x, planeX0Y1.y, planeX0Y1.z); //TODO: reproject for non-flat planes
        Vector3 planeAverage = (Vector3.Lerp(planeX0Y1, planeX1Y0, 0.5f));
        float planeSizeX = Vector3.Distance(planeX0Y0, planeX1Y0);
        float planeSizeY = Vector3.Distance(planeX0Y0, planeX0Y1);
        GameObject ProjectionPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ProjectionPlane.transform.position = planeAverage;
        ProjectionPlane.transform.localScale = new Vector3(planeSizeX * 0.1f, 1, planeSizeY * 0.1f);
        ProjectionPlane.transform.parent = TargetObject.transform;
        ProjectionPlane.transform.name = TargetObject.transform.name + "_ProjectionPlane";
        ProjectionPlane.transform.GetComponent<MeshRenderer>().enabled = false; //hide the mesh; mesh collider will do
    }

    public void InstantiateGeometryStructure()
    {
        ParentInstantiated = new GameObject("InstantiatedGeometry");
        ObjectModelReferences = new List<GameObject>();
        ObjectRealReferences = new List<GameObject>();
    }

    //instantiate geometry, as clicked on model, to model/real
    public void InstantiateGeometry(Vector3 Position)
    {
        //group them (to delete them later through parent)
        GameObject NewInstanceGroup = new GameObject("Group_" + instantiatedObjectCounter);
        NewInstanceGroup.transform.parent = ParentInstantiated.transform;

        //position passed by function is on model (draw from here...)
        GameObject NewInstanceModel = Instantiate(MainController.GetCurrentPlacementModel());
        NewInstanceModel.transform.parent = NewInstanceGroup.transform;
        NewInstanceModel.transform.position = Position;
        SetLayer(NewInstanceModel, instantiatedLayerName);
        //recalculate to real position and draw here, too
        GameObject NewInstanceReal = Instantiate(MainController.GetCurrentPlacementReal());
        Vector3 NormalizedInput = CoordinateEvaluator.NormalizeRayInput(MainController.GetCurrentTerrainModel(),
                                                                        NewInstanceModel.transform.position);
        NewInstanceReal.transform.position =
            CoordinateEvaluator.NormalizeRayOutput(MainController.GetCurrentTerrainReal(), NormalizedInput);
        NewInstanceReal.transform.parent = NewInstanceGroup.transform;
        SetLayer(NewInstanceReal, instantiatedLayerName);

        //name them and color them...
        NewInstanceModel.transform.name += "_" + instantiatedObjectCounter;
        NewInstanceReal.transform.name += "_" + instantiatedObjectCounter;
        instantiatedObjectCounter++;

        //connect the visibility points
        GameObject VisibilityNodeModel = NewInstanceModel.transform.GetChild(1).gameObject;
        GameObject VisibilityNodeReal = NewInstanceReal.transform.GetChild(1).gameObject;
        MainController.VisualizerModel.AddVisibilityPoint(VisibilityNodeModel);
        MainController.VisualizerReal.AddVisibilityPoint(VisibilityNodeReal);
        VisibilityNodeReal.GetComponent<Interactible>().SetPreset(Interactible.Preset.RadioTower);
        VisibilityNodeModel.GetComponent<Interactible>().SetPreset(Interactible.Preset.RadioTower);
        VisibilityNodeModel.GetComponent<Interactible>().SetTeleportationPoint(
            VisibilityNodeReal.GetComponent<Interactible>().TeleportationPoint);

        //add references
        ObjectModelReferences.Add(NewInstanceModel);
        ObjectRealReferences.Add(NewInstanceReal);
    }

    //destroys a model (or real) instantiated geometry and its counterpart
    public void DestroyGeometry(GameObject TargetObject)
    {
        int index = TargetObject.transform.parent.parent.GetSiblingIndex();
        MainController.VisualizerReal.RemoveVisibilityPointById(index);
        MainController.VisualizerModel.RemoveVisibilityPointById(index);
        ObjectModelReferences.RemoveAt(index);
        ObjectRealReferences.RemoveAt(index);
        Destroy(TargetObject.transform.parent.parent.gameObject);
    }

    //used to sweep all instantiated geometry when transitioning to another terrain collection
    public void DestroyAllGeometry()
    {
        foreach (Transform child in ParentInstantiated.transform)
        {
            Destroy(child.gameObject);            
        }
        ObjectModelReferences.Clear();
        ObjectRealReferences.Clear();
        this.DestroyProjectionPlane(MainController.GetCurrentTerrainModel());
        this.DestroyProjectionPlane(MainController.GetCurrentTerrainReal());
    }

    public void DestroyProjectionPlane(GameObject ProjectedObject)
    {
        if (ProjectedObject.transform.GetChild(3) != null)
        {
            Destroy(ProjectedObject.transform.GetChild(3).gameObject);
        }
    }

    public void SetLayer(GameObject TargetObject, string layer)
    {
        TargetObject.layer = LayerMask.NameToLayer(layer);
        foreach (Transform child in TargetObject.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer(layer);
        }
    }

    public bool VerifyInstantiationAllowed()
    {
        return (ObjectModelReferences.Count < MainController.GetCurrentMaxInstances()) ? true : false;
    }
}
