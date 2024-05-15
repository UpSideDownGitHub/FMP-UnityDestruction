using UnityEngine;

namespace UnityFracture
{
    /// <summary>
    /// class to destroy objects in a custom amount of time
    /// </summary>
    public class CustomDestroyed : MonoBehaviour
    {
        public float destroyTime;
        /// <summary>
        /// Destroy this object in [destroyTime]
        /// </summary>
        public void Start()
        {
            Destroy(gameObject, destroyTime);
        }
    }
}
