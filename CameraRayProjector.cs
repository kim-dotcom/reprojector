using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraRayProjector : MonoBehaviour
{
    public Canvas DisplayCanvas;
    public Camera DisplayCamera;
    public GameObject UserController;
    [Space(10)]
    public GameObject ObjectModel;
    public GameObject ObjectReal;
    private bool isTeleportedToReal;
    private Vector3 teleportCoordinateReal = new Vector3(-23f, 22f, 63f);
    private Vector3 teleportCoordinateModel = new Vector3(-0.0f, 2f, 1.0f);
    [Space(10)]
    public bool displayRaycast;
    private float displayRaycastSize;

    private Ray ray;
    private RaycastHit hit;
    private Vector3 centerVector = new Vector3(0.5f, 0.5f, 0);
    private Text canvasText;
    private GameObject raycastTarget;
    private Vector2 projectionRatio;
    private Vector3 projectionTarget;

    private List<Dictionary<GameObject, GameObject>> InstantiatedGeometry;
    private GameObject ParentInstantiated;
    private GameObject ParentObjectModel;
    private GameObject ParentObjectReal;
    private int instantiatedObjectCounter = 1;
    private string instantiatedGeometryLayer = "Instantiated";

    private void Awake()
    {
        InstantiateGeometryStructure();
        //TODO: precompute model/real dimensions here (size, rotation, direction...)
    }

    // Start is called before the first frame update
    void Start()
    {
        if (DisplayCamera == null)
        {
            DisplayCamera = Camera.main;
        }
        if (DisplayCanvas != null)
        {
            canvasText = DisplayCanvas.GetComponent<Text>();
        }   
        raycastTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        raycastTarget.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        raycastTarget.GetComponent<Collider>().enabled = false;
        raycastTarget.SetActive(false);

        GenerateProjectionPlane(ObjectModel);
        GenerateProjectionPlane(ObjectReal);
    }

    // Update is called once per frame
    void Update()
    {
        //raycast to the middle of the camera...
        ray = DisplayCamera.ViewportPointToRay(centerVector);
        if (Physics.Raycast(ray, out hit))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.yellow);
            canvasText.text = hit.collider.gameObject.name;
            //display the raycasty
            if (displayRaycast)
            {
                raycastTarget.SetActive(true);
                raycastTarget.transform.position = hit.point;
                displayRaycastSize = (DisplayCamera.transform.position - hit.point).magnitude * 0.02f;
                raycastTarget.transform.localScale = new Vector3(displayRaycastSize,
                                                                 displayRaycastSize,
                                                                 displayRaycastSize);
                //Debug.Log(hit.point);
            }
            //evaluate raycastHit
            if (hit.collider.gameObject == ObjectModel || hit.collider.gameObject == ObjectReal)
            {
                NormalizeRayInput(hit.collider.gameObject, hit.point);
                canvasText.text += "\r\n valid teleporter object :" + projectionRatio.x + ", " + projectionRatio.y;
                NormalizeRayOutput(ObjectReal, projectionRatio);
                canvasText.text += "\r\n teleport coordinate :" + projectionTarget.x + ", " + projectionTarget.y
                                   + ", " + projectionTarget.z;
            }
            else
            {
                canvasText.text += "\r\n invalid teleporter object";
            }
        }
        else
        {
            canvasText.text = "no target";
            raycastTarget.SetActive(false);
        }  
        
        //teleport from real to model
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (isTeleportedToReal)
            {
                TeleportToPosition(teleportCoordinateModel);
            }
            else
            {
                TeleportToPosition(teleportCoordinateReal);
            }
        }
        //teleport around real
        if(Input.GetMouseButtonDown(0) && hit.collider != null)
        {
            if (hit.collider.gameObject == ObjectModel)
            {
                TeleportToPosition(NormalizeRayOutput(ObjectReal,
                                                      NormalizeRayInput(ObjectModel, hit.point)));
            }
            else if (hit.collider.gameObject == ObjectReal)
            {
                TeleportToPosition(new Vector3 (hit.point.x, hit.point.y + 2, hit.point.z));
            }
        }
        //instantiate geometry
        if (Input.GetMouseButtonDown(1) && hit.collider != null)
        {
            if (hit.collider.gameObject == ObjectModel)
            {
                InstantiateGeometry(hit.point);
            }
            else if (hit.collider.gameObject == ObjectReal)
            {
                //TODO: refactor into a method of its own...
                Vector3 planeX0Y0 = ObjectModel.transform.GetChild(0).transform.position;
                Vector3 planeX1Y0 = ObjectModel.transform.GetChild(1).transform.position;
                Vector3 planeX0Y1 = ObjectModel.transform.GetChild(2).transform.position;
                Vector3 ProjectionCoordinate = new Vector3(Mathf.Lerp(planeX0Y0.x, planeX1Y0.x, projectionRatio.x),
                                                                      planeX0Y0.y + 0.25f, //a bit above the model desk
                                                           Mathf.Lerp(planeX0Y0.z, planeX0Y1.z, projectionRatio.y));
                RaycastHit projectionHit3;
                Physics.Raycast(ProjectionCoordinate, Vector3.down, out projectionHit3, Mathf.Infinity);                
                Vector3 ModelHit = new Vector3(ProjectionCoordinate.x, projectionHit3.point.y, ProjectionCoordinate.z);
                InstantiateGeometry(ModelHit);
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Instantiated"))
            {
                DestroyGeometry(hit.collider.gameObject);
            }
        }
    }

    //generate a ground plane for a real/model terrain so that there is basis for teleporting
    void GenerateProjectionPlane(GameObject TargetObject)
    {
        //get target object projection plane
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
        ProjectionPlane.transform.GetComponent<MeshRenderer>().enabled = false; //hide the mesh; mere collider will do
    }

    //project onto the model object (or go down onto the projection plane from hit.point)
    Vector2 NormalizeRayInput(GameObject TargetObject, Vector3 RayCoordinate)
    {
        //project onto the projection plane
        GameObject ProjectionPlane = TargetObject.transform.GetChild(3).transform.gameObject;
        RaycastHit projectionHit;
        //Ray projectionRay = new Ray(RayCoordinate, Vector3.down);
        //Physics.Raycast(projectionRay, out projectionHit);
        Physics.Raycast(RayCoordinate, Vector3.down, out projectionHit, Mathf.Infinity);

        //normalize to return an XY [0,1] output
        float planeX0 = TargetObject.transform.GetChild(0).transform.position.x;
        float planeX1 = TargetObject.transform.GetChild(1).transform.position.x;
        float planeY0 = TargetObject.transform.GetChild(0).transform.position.z;
        float planeY1 = TargetObject.transform.GetChild(2).transform.position.z;

        float planeRatioX = (projectionHit.point.x - planeX0) / (planeX1 - planeX0);
        float planeRatioY = (projectionHit.point.z - planeY0) / (planeY1 - planeY0);

        projectionRatio = new Vector2(planeRatioX, planeRatioY);
        //Debug.DrawRay(projectionHit.point, Vector3.up * 1000, Color.yellow);
        return projectionRatio;
    }

    //reproject onto the real object (or go up fro the projection plane onto the model)
    Vector3 NormalizeRayOutput(GameObject TargetObject, Vector2 projectionTransferRatio)
    {
        //get projection plane XY coordinate
        Vector3 PlaneX0Y0 = TargetObject.transform.GetChild(0).transform.position;
        Vector3 PlaneX1Y0 = TargetObject.transform.GetChild(1).transform.position;
        Vector3 PlaneX0Y1 = TargetObject.transform.GetChild(2).transform.position;
        Vector3 PlaneProjectionCoordinate;

        Vector3 ProjectionCoordinate = new Vector3(Mathf.Lerp(PlaneX0Y0.x, PlaneX1Y0.x, projectionTransferRatio.x),
                                                   PlaneX0Y0.y + 2000,
                                                   Mathf.Lerp(PlaneX0Y0.z, PlaneX0Y1.z, projectionTransferRatio.y));
        //Debug.DrawRay(ProjectionCoordinate, Vector3.up * 1000, Color.yellow);

        //project onto the real terrain to ge the Z coordinate
        RaycastHit projectionHit2;
        Physics.Raycast(ProjectionCoordinate, Vector3.down, out projectionHit2, Mathf.Infinity);

        projectionTarget = new Vector3(ProjectionCoordinate.x,
                                       projectionHit2.point.y,
                                       ProjectionCoordinate.z);
        //Debug.Log(projectionHit2.point.x + ", " + projectionHit2.point.y + ", " + projectionHit2.point.z);
        //Debug.DrawRay(ProjectionCoordinate, Vector3.down * 2000, Color.yellow);
        //raycastTarget.transform.position = projectionTarget;

        return projectionTarget;
    }

    void TeleportToPosition (Vector3 Position)
    {        
        UserController.GetComponent<CharacterController>().enabled = false;            
        //model position is one coordinate, real position can be wherever on the map
        if (Position == teleportCoordinateModel)
        {
            isTeleportedToReal = false;
            UserController.transform.rotation = Quaternion.Euler(new Vector3(0, 180f, 0));
        }
        else
        {
            isTeleportedToReal = true;
            Position = new Vector3(Position.x, Position.y + 2, Position.z); //to teleport above terrain
        }
        UserController.transform.position = Position;
        UserController.GetComponent<CharacterController>().enabled = true;
    }

    //on awake - instantiated objects will be put here
    void InstantiateGeometryStructure()
    {
        //these objects are global - to be easily referenced by the instantiate/delewte functions...
        ParentInstantiated = new GameObject("InstantiatedGeometry");
        //ParentObjectModel = new GameObject("InstantiatedModel");
        //ParentObjectReal = new GameObject("InstantiatedReal");
        //ParentObjectModel.transform.SetParent(ParentInstantiated.transform);
        //ParentObjectReal.transform.SetParent(ParentInstantiated.transform);
    }
 
    //instantiate geometry, as clicked on model, to model/real
    void InstantiateGeometry (Vector3 Position)
    {
        //group them (to delete them later through parent)
        GameObject NewInstanceGroup = new GameObject("Group_" + instantiatedObjectCounter);
        NewInstanceGroup.transform.parent = ParentInstantiated.transform;

        //position passed by function is on model (draw from here...)
        GameObject NewInstanceModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        NewInstanceModel.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        NewInstanceModel.transform.parent = NewInstanceGroup.transform;
        NewInstanceModel.transform.position = Position;
        NewInstanceModel.layer = LayerMask.NameToLayer("Instantiated");
        //recalculate to real position and draw here, too
        //Vector3 RealPosition = NormalizeRayOutput(ObjectReal, NormalizeRayInput(ObjectModel, Position));
        GameObject NewInstanceReal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        NewInstanceReal.transform.position = (NormalizeRayOutput(ObjectReal,
                                                                 NormalizeRayInput(ObjectModel,
                                                                                   NewInstanceModel.transform.position)));
        //NewInstanceReal.GetComponent<Collider>().enabled = false; //if this proves a nuisance when crossing terrain
        NewInstanceReal.transform.localScale = new Vector3(10f, 10f, 10f);
        NewInstanceReal.transform.parent = NewInstanceGroup.transform;
        NewInstanceReal.layer = LayerMask.NameToLayer("Instantiated");

        //name them and color them...
        NewInstanceModel.transform.name += "_" + instantiatedObjectCounter;
        NewInstanceReal.transform.name += "_" + instantiatedObjectCounter;
        instantiatedObjectCounter++;
        Color RandomColor = new Color(Random.Range(0.2f,0.8f), Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f));
        NewInstanceModel.GetComponent<Renderer>().material.SetColor("_Color", RandomColor);
        NewInstanceReal.GetComponent<Renderer>().material.SetColor("_Color", RandomColor); //or just do Color.red
    }

    void DestroyGeometry (GameObject TargetObject)
    {
        //do with both real/model objects
        Destroy(TargetObject.transform.parent.gameObject);
    }
}