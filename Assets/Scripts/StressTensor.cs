using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReubenMiller.Fracture
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

        public void UpdateStress(Vector3[] forces)
        {
            if (forces.Length != 3)
                return;

            // Calculate total force acting on the material
            Vector3 totalForce = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                totalForce += forces[i];
            }

            // Calculate average normal stress
            float averageNormalStress = totalForce.magnitude / 3.0f;

            // Consider material properties (assuming isotropic material)
            float youngsModulus = material.youngsModulus;
            float poissonRatio = material.poissonRatio;

            // Update normal stresses
            stresses[0, 0] = averageNormalStress * (1 - poissonRatio) / (1 - 2 * poissonRatio); // σ_xx
            stresses[1, 1] = averageNormalStress * (1 - poissonRatio) / (1 - 2 * poissonRatio); // σ_yy
            stresses[2, 2] = averageNormalStress * (1 - poissonRatio) / (1 - 2 * poissonRatio); // σ_zz

            // Update shear stresses (assuming simple averaging, can be more complex)
            stresses[0, 1] = (forces[0].x * forces[1].y + forces[0].y * forces[1].x) / (2 * youngsModulus); // τ_xy
            stresses[0, 2] = (forces[0].x * forces[2].z + forces[0].z * forces[2].x) / (2 * youngsModulus); // τ_xz
            stresses[1, 2] = (forces[1].y * forces[2].z + forces[1].z * forces[2].y) / (2 * youngsModulus); // τ_yz

        }
    }
}
