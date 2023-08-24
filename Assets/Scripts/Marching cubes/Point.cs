using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Point
{
    public Vector3 pointPosition;
    public bool pointOn;

    public Point(Vector3 position)
    {
        pointPosition = position;

        pointOn = false;
    }
    public Point(Vector3 position, bool value)
    {
        pointPosition = position;
        pointOn = value;
    }
    public void SetPointOn()
    {
        this.pointOn = true;
    }


}
