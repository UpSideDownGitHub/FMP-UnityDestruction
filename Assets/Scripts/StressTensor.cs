using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressTensor
{
    private float[,] stresses;
    private StressMaterial material;

    public StressTensor(StressMaterial mat)
    {
        this.material = mat;
        stresses = new float[3, 3];
    }

    public void SetStress(int i, int j, float value)
    {
        stresses[i, j] = value;
    }

    public float GetStress(int i, int j)
    {
        return stresses[i, j];
    }

    /*
     *   NEED TO MAKE THIS UPDATE BASED ON THE OTHER OBJECT IN THE SCENE
     *   SO IF AN OBJECT I NOT BEING HELD BY ANYTHING THEN IT WILL CALCUALTED,
     *   AS BEING HEAVIER AND IF ENOUGH FORCE WILL BREAK OFF
     *   
     *   FOR THIS I WILL NEED CODE THAT WILL CALUCLATE THE NEIGHTBOURS OF THE OBJECT
     *   BUT SHOULD BE ABLE TO USE SOME OF THE CODE FROM THE ORIGNAL THING FOR THE 
     *   BLAST IMPLEMENTATION AS THAT CALUCLATES THE NEIGHTBOURS TO APPLY SPRING
     *   JOINTS TO THEM
    */
    public void UpdateStress(Vector3[] forces, float dt)
    {
        // TO BE IMPLEMENTED
    }
}
