//Usage: attach this to a VR controller.

using UnityEngine;

public class RaycasterPointerVr : MonoBehaviour
{
    public float pointingDistance = 10f; // Distance of the Raycast
    public LayerMask raycastLayer; // Layermask for the objects that can be hit by the Raycast

    private Transform controllerTransform; // Transform of the VR controller
    private LineRenderer lineRenderer; // Line Renderer component for drawing the Raycast

    private Color c1 = Color.white;
    private Color c2 = new Color(1, 1, 1, 0);

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

    void Update()
    {
        // Set the starting point of the Raycast to the position of the VR controller
        Vector3 raycastStart = controllerTransform.position;

        // Set the end point of the Raycast to the forward direction of the VR controller, multiplied by the Raycast distance
        Vector3 raycastEnd = controllerTransform.position + (controllerTransform.forward * pointingDistance);

        // Create a Raycast hit variable
        RaycastHit hit;

        // Cast a Raycast from the controller forward
        if (Physics.Raycast(raycastStart, controllerTransform.forward, out hit, pointingDistance, raycastLayer))
        {
            // If the Raycast hit an object, set the end point of the Line Renderer to the hit point
            raycastEnd = hit.point;
        }

        // Set the positions of the Line Renderer to draw the Raycast
        lineRenderer.SetPosition(0, raycastStart);
        lineRenderer.SetPosition(1, raycastEnd);
    }
}