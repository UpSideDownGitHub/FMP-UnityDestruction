using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFracture
{
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

        public void UpdateStress(Vector3 totalForce)
        {
            // Calculate average normal stress
            float averageNormalStress = totalForce.magnitude / 3.0f;

            // Consider material properties (assuming isotropic material)
            float youngsModulus = material.youngsModulus;
            float poissonRatio = material.poissonRatio;

            // Update normal stresses
            stresses[0, 0] = averageNormalStress * (1 - poissonRatio) / (1 - 2 * poissonRatio); // σ_xx
            stresses[1, 1] = averageNormalStress * (1 - poissonRatio) / (1 - 2 * poissonRatio); // σ_yy
            stresses[2, 2] = averageNormalStress * (1 - poissonRatio) / (1 - 2 * poissonRatio); // σ_zz
        }
    }
}
