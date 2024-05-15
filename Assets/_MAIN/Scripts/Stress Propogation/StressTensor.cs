using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFracture
{
    /// <summary>
    /// stress tensor to calcualte if object should break due to the stresses being applied
    /// </summary>
    public class StressTensor
    {
        private float[,] stresses;
        private StressMaterial material;

        /// <summary>
        /// Initializes a new instance of the <see cref="StressTensor"/> class.
        /// </summary>
        /// <param name="mat">The mat.</param>
        public StressTensor(StressMaterial mat)
        {
            this.material = mat;
            stresses = new float[3, 3];
        }
        /// <summary>
        /// Sets the stress.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="j">The j.</param>
        /// <param name="value">The value.</param>
        public void SetStress(int i, int j, float value)
        {
            stresses[i, j] = value;
        }
        /// <summary>
        /// Gets the stress.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="j">The j.</param>
        /// <returns></returns>
        public float GetStress(int i, int j)
        {
            return stresses[i, j];
        }
        /// <summary>
        /// Updates the stress.
        /// </summary>
        /// <param name="totalForce">The total force.</param>
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
