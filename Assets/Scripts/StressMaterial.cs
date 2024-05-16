using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReubenMiller.Fracture
{
    public class StressMaterial
    {
        public float youngsModulus;
        public float poissonRatio;
        public float dampingCoefficient;

        public StressMaterial(float young, float poisson, float damping)
        {
            this.youngsModulus = young;
            this.poissonRatio = poisson;
            this.dampingCoefficient = damping;
        }
    }
}