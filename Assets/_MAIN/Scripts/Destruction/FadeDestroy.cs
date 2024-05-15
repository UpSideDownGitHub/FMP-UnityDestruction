using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEngine;

namespace UnityFracture.Demo
{
    /// <summary>
    /// this script will destroy an object in a given time, but before destroying the object
    /// it will make it shrink to make it look like it faded away
    /// </summary>
    public class FadeDestroy : MonoBehaviour
    {
        public float destroyTime = 2f;
        private float _timeOfDestroy;
        private Vector3 _origScale;
        private float _startTime;

        /// <summary>
        /// initilise all of the start varaiables so the object gets destroyed at the correct time
        /// </summary>
        public void Start()
        {
            _timeOfDestroy = Time.time + destroyTime;
            _origScale = transform.localScale;
            _startTime = Time.time;
        }

        /// <summary>
        /// will make the object shrink untill it is small enough at which point it will be destroyed
        /// </summary>
        public void Update()
        {
            // shirnk the object
            float scaleFactor = 1 - ((Time.time - _startTime) / (_timeOfDestroy - _startTime));
            // if the object is small enough then destroy if not then update the scale of the object
            if (scaleFactor < 0.0f)
                Destroy(gameObject);
            else
                transform.localScale = new Vector3(_origScale.x * scaleFactor, _origScale.y * scaleFactor, _origScale.z * scaleFactor);
        }
    }
}
