using UnityEngine;
using System.Collections.Generic;

public static class CoordinateEvaluator
{
    public static Vector2 projectionRatio { get; private set; }
    public static Vector3 projectionTarget { get; private set; }

    //get terrain object coordinate ratio
    public static Vector2 NormalizeRayInput(GameObject TargetObject, Vector3 RayCoordinate)
    {
        GameObject ProjectionPlane = TargetObject.transform.GetChild(3).transform.gameObject;
        RaycastHit projectionHit;
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

    //reproject terrain ratio onto coordinates
    public static Vector3 NormalizeRayOutput(GameObject TargetObject, Vector2 projectionTransferRatio)
    {
        Vector3 PlaneX0Y0 = TargetObject.transform.GetChild(0).transform.position;
        Vector3 PlaneX1Y0 = TargetObject.transform.GetChild(1).transform.position;
        Vector3 PlaneX0Y1 = TargetObject.transform.GetChild(2).transform.position;

        float projectionHeight = Mathf.Abs(PlaneX0Y0.z - PlaneX0Y1.z); Debug.Log(projectionHeight);
        Vector3 ProjectionCoordinate = new Vector3(Mathf.Lerp(PlaneX0Y0.x, PlaneX1Y0.x, projectionTransferRatio.x),
                                                   PlaneX0Y0.y + projectionHeight,
                                                   Mathf.Lerp(PlaneX0Y0.z, PlaneX0Y1.z, projectionTransferRatio.y));
        //Debug.DrawRay(ProjectionCoordinate, Vector3.up * 1000, Color.yellow);

        //project onto the real terrain to ge the Z coordinate
        RaycastHit projectionHit;
        Physics.Raycast(ProjectionCoordinate, Vector3.down, out projectionHit, Mathf.Infinity);

        projectionTarget = new Vector3(ProjectionCoordinate.x,
                                       projectionHit.point.y,
                                       ProjectionCoordinate.z);
        //Debug.DrawRay(ProjectionCoordinate, Vector3.down * 2000, Color.yellow);
        return projectionTarget;
    }

    public static bool VerifyCloseness(List<GameObject> ObjectReferences, Vector3 position, float distance)
    {
        foreach (GameObject ThisObject in ObjectReferences)
        {
            if (Vector3.Distance(ThisObject.transform.position, position) < distance)
            {
                return true;
            }
        }
        return false;
    }
}
