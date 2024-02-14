using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public enum Angle
{
    UP,
    FORWARD
}

public class MouseClickedCut : MonoBehaviour
{
    public Angle angle;
    public MeshCutter meshCutter;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                GameObject victim = hit.collider.gameObject;
                if (victim.tag != "Safe")
                {
                    if (angle == Angle.UP)
                        meshCutter.Cut(victim, hit.point, Vector3.up);
                    else if (angle == Angle.FORWARD)
                        meshCutter.Cut(victim, hit.point, Vector3.forward);
                }
            }
        }
    }
}