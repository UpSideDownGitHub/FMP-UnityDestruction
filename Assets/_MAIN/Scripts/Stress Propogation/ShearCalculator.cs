﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityFracture
{
    /// <summary>
    /// Manages caclulating the shear strenghths currently being applied to the object
    /// </summary>
    public class ShearCalculator
    {
        public StressTensor stressTensor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShearCalculator"/> class.
        /// </summary>
        public ShearCalculator()
        {
            // calculate princible stress
            StressMaterial material = new StressMaterial(1f, 1f, 1f);
            stressTensor = new StressTensor(material);
            stressTensor.SetStress(0, 0, 1000f); // σ_xx
            stressTensor.SetStress(1, 1, -500f); // σ_yy
            stressTensor.SetStress(2, 2, 300f);  // σ_zz
            stressTensor.SetStress(0, 1, 200f);  // τ_xy (shear stress)
            stressTensor.SetStress(1, 2, 150f);  // τ_yz (shear stress)
            stressTensor.SetStress(0, 2, 50f);   // τ_xz (shear stress)
        }

        /// <summary>
        /// Updates the forces.
        /// </summary>
        /// <param name="forces">The forces.</param>
        public void UpdateForces(Vector3 forces)
        {
            stressTensor.UpdateStress(forces);
        }

        /// <summary>
        /// Calculates the shear.
        /// </summary>
        /// <returns></returns>
        public bool CalculateShear()
        {
            // Compute principal stresses
            float I1 = stressTensor.GetStress(0, 0) + stressTensor.GetStress(1, 1) + stressTensor.GetStress(2, 2);
            float I2 = 0.5f * (stressTensor.GetStress(0, 0) * stressTensor.GetStress(1, 1)
                               + stressTensor.GetStress(1, 1) * stressTensor.GetStress(2, 2)
                               + stressTensor.GetStress(2, 2) * stressTensor.GetStress(0, 0)
                               - stressTensor.GetStress(0, 1) * stressTensor.GetStress(0, 1)
                               - stressTensor.GetStress(1, 2) * stressTensor.GetStress(1, 2)
                               - stressTensor.GetStress(0, 2) * stressTensor.GetStress(0, 2));

            float sigma_max = I1 + Mathf.Sqrt(I1 * I1 - 3f * I2);
            float sigma_min = I1 - Mathf.Sqrt(I1 * I1 - 3f * I2);
            float shearThreshold = 0.1f * sigma_max; // 10% of max stress
            Debug.Log($"Shear Threshold: {shearThreshold}\n Shear: {Mathf.Abs(sigma_max - sigma_min)}");
            return Mathf.Abs(sigma_max - sigma_min) > shearThreshold; // true if should shear
        }
    }
}