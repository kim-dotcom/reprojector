//class VisibilityVisualizer draws lines between a list of objects (green line = is line of sight, red = isn't)
//  method ComputeVisualization() determines whether there is a clear line of sight between each of two objects
//  method DrawVisualization() then draws these
//
//usage: instantiate/call the class from elsewhere, per handling objects that need to be checked for visibility
//  the per-object logic is to be handled in a  high-level controller/factory class (this is a low-level class)

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityEvaluator : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> VisibilityPoints;
    [SerializeField]
    private Color ColorVisible = Color.green;
    [SerializeField]
    private Color ColorInvisible = Color.red;
    private Material DefaultMaterial;
    private float lineWidth = 1f;

    private bool[,] visibilityMatrix;
    private Ray visibilityRay;
    private GameObject LineRendererContainer;

    public void Awake()
    {
        VisibilityPoints = new List<GameObject>();
        DefaultMaterial = Resources.Load<Material>("LineMaterial");
    }

    public void AddVisibilityPoint(GameObject NewObject)
    {
        VisibilityPoints.Add(NewObject);
    }

    public void RemoveVisibilityPoint(GameObject RemovalObject)
    {
        VisibilityPoints.Remove(RemovalObject);        
    }

    public void RemoveVisibilityPointById(int id)
    {
        if (id < VisibilityPoints.Count)
        {
            VisibilityPoints.Remove(VisibilityPoints[id]);
        }        
    }

    public void SetColorVisible(Color color)
    {
        ColorVisible = color;
    }

    public void SetColorInvisible(Color color)
    {
        ColorInvisible = color;
    }

    public void SetLineWidth(float width)
    {
        lineWidth =  Mathf.Abs(width);
    }

    public bool[,] GetVisibilityMatrix()
    {
        return visibilityMatrix;
    }

    public void SetVisibilityMatrix(bool[,] matrix)
    {
        visibilityMatrix = matrix;
    }

    public bool ComputeVisualization()
    {
        int numObjects = VisibilityPoints.Count;
        ReinitVisualization(numObjects);

        if (numObjects < 2)
        {
            return false;
        }
        
        for (int i = 0; i < numObjects; i++)
        {
            for (int j= i+1; j < numObjects; j++)
            {
                if (Physics.Linecast(VisibilityPoints[i].gameObject.transform.position,
                                     VisibilityPoints[j].gameObject.transform.position, out RaycastHit hit))
                {
                    if (hit.collider != null && hit.collider.gameObject == VisibilityPoints[j].transform.gameObject)
                    {
                        visibilityMatrix[i, j] = true; //prefab has to have colliders set up here (not children/parent)
                    }                    
                }
            }
        }
        return true;
    }

    public void DrawVisualization(float lineWidth)
    {
        int numObjects = VisibilityPoints.Count;

        for (int i = 0; i < numObjects; i++)
        {
            for (int j = i + 1; j < numObjects; j++)
            {
                GameObject lineObject = new GameObject();
                lineObject.name = "LineObject_" + i + "_" + j;
                lineObject.transform.parent = LineRendererContainer.transform;
                LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
                lineRenderer.SetPosition(0, VisibilityPoints[i].gameObject.transform.position);
                lineRenderer.SetPosition(1, VisibilityPoints[j].gameObject.transform.position);
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;

                //lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.material = DefaultMaterial;
                if (visibilityMatrix[i,j] == true)
                {
                    lineRenderer.startColor = ColorVisible;
                    lineRenderer.endColor = ColorVisible;
                }
                else
                {
                    lineRenderer.startColor = ColorInvisible;
                    lineRenderer.endColor = ColorInvisible;
                }
            }
        }
    }

    public void ReinitVisualization(int arrayLength)
    {
        if (visibilityMatrix != null)
        {
            Array.Clear(visibilityMatrix, 0, visibilityMatrix.Length);
        }
        if (arrayLength > 1)
        {
            visibilityMatrix = new bool[arrayLength, arrayLength];
        }
        RemoveVisualization();
        LineRendererContainer = new GameObject();
        System.Random randomSeed = new System.Random();
        LineRendererContainer.name = "VisibilityLineContainer_" + randomSeed.Next(10000);
        LineRendererContainer.transform.parent = this.gameObject.transform;
    }

    public void RemoveVisualization()
    {
        if (LineRendererContainer != null)
        {
            Destroy(LineRendererContainer);
        }
    }

    //Unit test: attach this class to a GameObject; Inspector: add objects to the VisibilityPoints List; play; press F1
    //public void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.F1))
    //    {
    //        //var RemovableObject = VisibilityPoints[VisibilityPoints.Count - 1];
    //        //RemoveVisibilityPoint(RemovableObject);
    //        if (ComputeVisualization())
    //        {
    //            DrawVisualization(1f);
    //        }
    //        //AddVisibilityPoint(RemovableObject);
    //    }
    //}
}
