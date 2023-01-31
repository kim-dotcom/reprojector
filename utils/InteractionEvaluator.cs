using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionEvaluator : MonoBehaviour
{
    public bool isTeleportedToReal { get; private set; }
    private Vector3 teleportCoordinateModelDefault = new Vector3(-0.0f, 2f, 1.0f);
    private Vector3 teleportCoordinateRealDefault = new Vector3(-23f, 22f, 63f);
    private Vector3 teleportCoordinateRealLast = new Vector3();
    private GameObject UserController;
    private ModelTeleporterController MainController;

    public void SetUserController(GameObject Controller)
    {
        this.UserController = Controller;
    }

    public void SetMainController(ModelTeleporterController Controller)
    {
        this.MainController = Controller;
    }

    public void TeleportBetweenModelReal()
    {
        if (isTeleportedToReal)
        {
            teleportCoordinateRealLast = UserController.transform.position;
            TeleportToPosition(teleportCoordinateModelDefault);
        }
        else
        {
            if (teleportCoordinateRealLast == Vector3.zero)
            {
                TeleportToPosition(teleportCoordinateRealDefault);
            }
            else
            {
                TeleportToPosition(teleportCoordinateRealLast);
            }
        }
    }

    public void TeleportInReal(Vector3 hit, GameObject HitObject)
    {
        if (HitObject == MainController.ObjectModel)
        {
            Vector3 NormalizedInput = CoordinateEvaluator.NormalizeRayInput(MainController.ObjectModel, hit);
            TeleportToPosition(CoordinateEvaluator.NormalizeRayOutput(MainController.ObjectReal, NormalizedInput));
        }
        else if (HitObject == MainController.ObjectReal)
        {
            TeleportToPosition(new Vector3(hit.x, hit.y + 0.5f, hit.z));
        }
        else if (HitObject.TryGetComponent(out Interactible interactible))
        {
            if (interactible.isTeleportableToPoint)
            {
                TeleportToPosition(interactible.TeleportationPoint);
            }
        }
    }

    private void TeleportToPosition(Vector3 Position)
    {
        UserController.GetComponent<CharacterController>().enabled = false;
        //model position is one coordinate, real position can be wherever on the map
        if (Position == teleportCoordinateModelDefault)
        {
            isTeleportedToReal = false;
            UserController.transform.rotation = Quaternion.Euler(new Vector3(0, 180f, 0));
        }
        else
        {
            isTeleportedToReal = true;
            Position = new Vector3(Position.x, Position.y + 0.5f, Position.z); //to teleport above terrain
        }
        UserController.transform.position = Position;
        UserController.GetComponent<CharacterController>().enabled = true;
    }

    //TODO: move this functon (or a part of it) to a geometry spawner class
    public void CreateGeometry(Vector3 hit, GameObject HitObject)
    {
        if (HitObject == MainController.ObjectModel && MainController.Generator.VerifyInstantiationAllowed())
        {
            float minimumInstantiationDistanceModel = 0.1f;
            if (!CoordinateEvaluator.VerifyCloseness(MainController.Generator.ObjectModelReferences, hit,
                                                     minimumInstantiationDistanceModel))
            {
                MainController.Generator.InstantiateGeometry(hit);
            }
        }
        else if (HitObject == MainController.ObjectReal && MainController.Generator.VerifyInstantiationAllowed())
        {
            float minimumInstantiationDistanceUser = 8f;
            float minimumInstantiationDistanceTowers = minimumInstantiationDistanceUser * 2;
            bool isCloseToTowers = CoordinateEvaluator.VerifyCloseness(MainController.Generator.ObjectRealReferences,
                                                                       hit, minimumInstantiationDistanceTowers);
            List<GameObject> UserList = new List<GameObject>();
            UserList.Add(UserController);
            bool isCloseToUser = CoordinateEvaluator.VerifyCloseness(UserList, hit, minimumInstantiationDistanceUser);

            if (!isCloseToTowers && !isCloseToUser)
            {
                //Vector3 planeX0Y0 = MainController.ObjectModel.transform.GetChild(0).transform.position;
                //Vector3 planeX1Y0 = MainController.ObjectModel.transform.GetChild(1).transform.position;
                //Vector3 planeX0Y1 = MainController.ObjectModel.transform.GetChild(2).transform.position;
                //Vector2 projectionRatio = CoordinateEvaluator.NormalizeRayInput(HitObject, hit);
                //Vector3 ProjectionCoordinate = new Vector3(Mathf.Lerp(planeX0Y0.x, planeX1Y0.x, projectionRatio.x),
                //                                                      planeX0Y0.y + 1f, //a bit above the model desk
                //                                           Mathf.Lerp(planeX0Y0.z, planeX0Y1.z, projectionRatio.y));
                //RaycastHit projectionHit3;
                //Physics.Raycast(ProjectionCoordinate, Vector3.down, out projectionHit3, Mathf.Infinity);
                //Vector3 ModelHit = new Vector3(ProjectionCoordinate.x,
                //                               projectionHit3.point.y, ProjectionCoordinate.z);

                Vector2 projectionRatio = CoordinateEvaluator.NormalizeRayInput(HitObject, hit);
                Vector3 ModelHit = CoordinateEvaluator.NormalizeRayOutput(MainController.ObjectModel, projectionRatio);

                MainController.Generator.InstantiateGeometry(ModelHit);
            }
        }
        else if (HitObject.layer == LayerMask.NameToLayer(MainController.instantiatedLayerName))
        {
            MainController.Generator.DestroyGeometry(HitObject);           
        }
    }

    public void VisualizeGeometryLOS()
    {
        MainController.VisualizerReal.ComputeVisualization();
        MainController.VisualizerModel.ComputeVisualization();
        MainController.VisualizerModel.SetVisibilityMatrix(MainController.VisualizerReal.GetVisibilityMatrix());
        MainController.VisualizerReal.DrawVisualization(0.25f);
        MainController.VisualizerModel.DrawVisualization(0.001f);
    }
}
