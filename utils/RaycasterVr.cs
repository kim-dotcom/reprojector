using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//raycasts wherever the controller is pointing 
public class RaycasterVr : Raycaster
{
    public float pointingDistance = 1000f; // Distance of the Raycast
    public LayerMask raycastLayer; // Layermask for the objects that can be hit by the Raycast

    private Transform controllerTransform; // Transform of the VR controller
    private LineRenderer lineRenderer; // Line Renderer component for drawing the Raycast

    private Color c1 = Color.white;
    private Color c2 = new Color(1, 1, 1, 0);

    private RaycastHit vrHit;
    private GameObject vrHitTarget;
    private Vector3 raycastStart;
    private Vector3 raycastEnd;

    void Awake()
    {
        // Get the Line Renderer component
        this.gameObject.AddComponent<LineRenderer>();
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = c1;
        lineRenderer.endColor = c2;

        // Get the Transform component of the VR controller
        controllerTransform = GetComponent<Transform>();
    }

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
        if (vrHitTarget != null)
        {
            DisplayRaycastPoint(raycastEnd);
            return new RaycastHitInfo(raycastEnd, vrHitTarget);
        }
        else
        {
            return new RaycastHitInfo(Vector3.zero, null);
        }
    }

    private void DisplayRaycastPoint(Vector3 point)
    {
        if (displayRaycast)
        {
            raycastTargetPoint.transform.position = point;
            pointSize = 0.2f;
            raycastTargetPoint.transform.localScale = new Vector3(pointSize, pointSize, pointSize);
        }
    }

    void Update()
    {        
        raycastStart = controllerTransform.position;        
        raycastEnd = controllerTransform.position + (controllerTransform.forward * pointingDistance);
                
        if (Physics.Raycast(raycastStart, controllerTransform.forward, out vrHit, pointingDistance, raycastLayer))
        {            
            raycastEnd = vrHit.point;
            vrHitTarget = vrHit.collider.gameObject;
        }
        else
        {
            vrHitTarget = null;
        }

        // Set the positions of the Line Renderer to draw the Raycast
        lineRenderer.SetPosition(0, raycastStart);
        lineRenderer.SetPosition(1, raycastEnd);
    }
}









