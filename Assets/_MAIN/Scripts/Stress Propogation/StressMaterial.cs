using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityFracture
{
    /// <summary>
    /// Data structure to hold information about the current material
    /// </summary>
    public class StressMaterial
    {
        public float youngsModulus;
        public float poissonRatio;
        public float dampingCoefficient;

        /// <summary>
        /// Initializes a new instance of the <see cref="StressMaterial"/> class.
        /// </summary>
        /// <param name="young">The young.</param>
        /// <param name="poisson">The poisson.</param>
        /// <param name="damping">The damping.</param>
        public StressMaterial(float young, float poisson, float damping)
        {
            this.youngsModulus = young;
            this.poissonRatio = poisson;
            this.dampingCoefficient = damping;
        }
    }
}