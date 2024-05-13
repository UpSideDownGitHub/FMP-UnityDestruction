using UnityEngine;

namespace UnityFracture
{
    public class CustomDestroyed : MonoBehaviour
    {
        public float destroyTime;
        public void Start()
        {
            Destroy(gameObject, destroyTime);
        }
    }
}
