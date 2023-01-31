using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct RaycastHitInfo
{
    public Vector3 targetPoint;
    public GameObject targetObject;

    public RaycastHitInfo (Vector3 point, GameObject obj)
    {
        this.targetPoint = point;
        this.targetObject = obj;
    }
}

public abstract class Raycaster : MonoBehaviour
{
    protected Ray ray;
    protected RaycastHit hit;
    protected Vector3 hitPoint;
    protected GameObject hitObject;

    protected GameObject Origin;

    protected bool displayRaycast;    
    protected GameObject raycastTargetPoint;
    protected float pointSize;

    protected bool displayCanvasInfo;
    protected Canvas DisplayCanvas;
    protected Text canvasText;

    protected List<GameObject> TeleportableObjects;

    void Awake()
    {
        TeleportableObjects = new List<GameObject>();
        raycastTargetPoint = new GameObject();
        raycastTargetPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        raycastTargetPoint.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        raycastTargetPoint.GetComponent<Collider>().enabled = false;
        raycastTargetPoint.SetActive(false);
    }

    public void AddTeleportableObject(GameObject newObject)
    {
            TeleportableObjects.Add(newObject);    
    }

    public abstract RaycastHitInfo Raycast();
}

//raycasts to the middle of the camera...
public class RaycasterScreen : Raycaster
{
    private Vector3 centerVector = new Vector3(0.5f, 0.5f, 0);    
    public Camera DisplayCamera;
    
    public void setRaycaster(Camera origin, bool displayRaycast, bool displayCanvasInfo, Canvas canvas)
    {
        this.DisplayCamera = origin;
        this.displayRaycast = displayRaycast;
        this.displayCanvasInfo = displayCanvasInfo;
        if (canvas != null)
        {
            this.DisplayCanvas = canvas;
            canvasText = DisplayCanvas.GetComponent<Text>();
        }        
        raycastTargetPoint.SetActive(displayRaycast);
        raycastTargetPoint.transform.position = new Vector3(-10000f, -10000f, -10000f);
    }

    
    public override RaycastHitInfo Raycast ()
    {
        ray = DisplayCamera.ViewportPointToRay(centerVector);
        if (Physics.Raycast(ray, out hit))
        {
            DisplayRaycastPoint(hit.point);
            DisplayRaycastData(hit.point, hit.collider.gameObject);
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.yellow);
            return new RaycastHitInfo(hit.point, hit.collider.gameObject);
        }
        else
        {            
            DisplayRaycastData(Vector3.zero, null);
            return new RaycastHitInfo(Vector3.zero, null);
        }
    }

    private void DisplayRaycastPoint(Vector3 point)
    {
        if (displayRaycast)
        {
            raycastTargetPoint.transform.position = point;
            pointSize = (DisplayCamera.transform.position - point).magnitude * 0.02f;
            raycastTargetPoint.transform.localScale = new Vector3(pointSize, pointSize, pointSize);
        }
    }

    private void DisplayRaycastData(Vector3 point, GameObject target)
    {
        if (target == null)
        {
            canvasText.text = "no target";
        }
        else
        {
            canvasText.text = hit.collider.gameObject.name;

            if (TeleportableObjects.Contains(target))
            {
                Vector2 projectionRatio = CoordinateEvaluator.NormalizeRayInput(target, point);
                canvasText.text += "\r\n valid teleporter object :" + projectionRatio.x + ", " + projectionRatio.y;
                //Vector3 projectionTarget = CoordinateEvaluator.NormalizeRayOutput(ObjectReal, projectionRatio);
                //canvasText.text += "\r\n teleport coordinate :" + projectionTarget.x + ", " + projectionTarget.y
                //                                   + ", " + projectionTarget.z;
            }
            else
            {
                canvasText.text += "\r\n invalid teleporter object";
            }
        }        
    }
}

//raycasts wherever the controller is pointing 
public class RaycasterVrController : Raycaster
{

    public void setRaycaster(GameObject VrController, bool displayRaycast, bool displayCanvasInfo, Canvas canvas)
    {
        this.Origin = VrController;
        this.displayRaycast = displayRaycast;
        this.displayCanvasInfo = displayCanvasInfo;
        if (canvas != null)
        {
            this.DisplayCanvas = canvas;
            canvasText = DisplayCanvas.GetComponent<Text>();
        }
        raycastTargetPoint.SetActive(displayRaycast);
        raycastTargetPoint.transform.position = new Vector3(-10000f, -10000f, -10000f);
    }

    //TODO...
    public override RaycastHitInfo Raycast()
    {
        RaycastHit vrHit;
        Vector3 raycastDir = Origin.transform.position - Origin.transform.GetChild(0).transform.position;

        Debug.DrawRay(Origin.transform.position, raycastDir, Color.yellow);

        Physics.Raycast(Origin.transform.position, raycastDir, out vrHit, Mathf.Infinity);        
        if (Physics.Raycast(ray, out vrHit))
        {
            return new RaycastHitInfo(vrHit.point, vrHit.collider.gameObject);
        }
        else
        {
            return new RaycastHitInfo(Vector3.zero, null);
        }
    }
}