//class Interactible is an extended behavioral struct (object data, behavior) assigned to interactible objects
//  method SetPreset() assigns the model of the presets
//  method SetTeleportationPoint() is a Vector3 coordinate
//
//usage: assign this class to an interactible object as a component from a master class
//       when this object is interacted with, the master class checks for Interactible component and decides action


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InteractiblePreset
{
    bool isDeletable;
    bool isHighlightable;
    bool isTeleportableToCoordinateAbsolute;
    bool isTeleportableToCoordinateRelative;
    bool isTeleportableToPoint;
    Vector3 TeleportationPoint;

    public InteractiblePreset (bool isDel, bool isHigh, bool isTCA, bool isTCR, bool isTCP, Vector3 tp)
    {
        this.isDeletable = isDel;
        this.isHighlightable = isHigh;
        this.isTeleportableToCoordinateAbsolute = isTCA;
        this.isTeleportableToCoordinateRelative = isTCR;
        this.isTeleportableToPoint = isTCP;
        this.TeleportationPoint = tp;
    }
}

public class Interactible : MonoBehaviour
{
    public bool isDeletable { get; private set; }
    public bool isHighlightable { get; private set; }
    public bool isTeleportableToCoordinateAbsolute { get; private set; }
    public bool isTeleportableToCoordinateRelative { get; private set; }
    public bool isTeleportableToPoint { get; private set; }
    public Vector3 TeleportationPoint { get; private set; }

    [HideInInspector]
    public enum Preset { Undefined, TerrainModel, TerrainReal, RadioTower };
    private Preset myPreset = Preset.Undefined;

    public void SetPreset(Preset preset)
    {
        switch (preset)
        {
            case Preset.TerrainModel:
                isDeletable = false;
                isHighlightable = false;
                isTeleportableToCoordinateAbsolute = false;
                isTeleportableToCoordinateRelative = true;
                isTeleportableToPoint = false;
                myPreset = Preset.TerrainModel;
                break;
            case Preset.TerrainReal:
                isDeletable = false;
                isHighlightable = false;
                isTeleportableToCoordinateAbsolute = true;
                isTeleportableToCoordinateRelative = false;
                isTeleportableToPoint = false;
                myPreset = Preset.TerrainReal;
                break;
            case Preset.RadioTower:
                isDeletable = true;
                isHighlightable = true;
                isTeleportableToCoordinateAbsolute = false;
                isTeleportableToCoordinateRelative = false;
                isTeleportableToPoint = true;
                //RadioTower lookout (near its top origin)
                TeleportationPoint = new Vector3(this.transform.position.x + 1.5f,
                                                 this.transform.position.y - 4.5f,
                                                 this.transform.position.z); 
                myPreset = Preset.RadioTower;
                break;
            default:
                isDeletable = false;
                isHighlightable = false;
                isTeleportableToCoordinateAbsolute = false;
                isTeleportableToCoordinateRelative = false;
                isTeleportableToPoint = false;
                TeleportationPoint = Vector3.zero;
                myPreset = Preset.Undefined;
                break;
        }
    }

    public InteractiblePreset GetPreset()
    {
        InteractiblePreset preset = new InteractiblePreset(this.isDeletable,this.isHighlightable,
                                                           this.isTeleportableToCoordinateAbsolute,
                                                           this.isTeleportableToCoordinateRelative,
                                                           this.isTeleportableToPoint, this.TeleportationPoint);
        return preset;
    }

    public void ResetValue()
    {
        this.SetPreset(Preset.Undefined);
    }

    public void SetTeleportationPoint(Vector3 point)
    {
        TeleportationPoint = point;
    }
}
