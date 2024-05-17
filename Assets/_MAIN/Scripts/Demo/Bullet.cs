using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ReubenMiller.Fracture.Demo
{
    public class Bullet : MonoBehaviour
    {
        public bool spawnEffect;
        public GameObject effect;

        public void OnDestroy()
        {
            if (spawnEffect)
                Instantiate(effect, transform.position, Quaternion.identity);
        }
    }
}
