using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEngine;

namespace UnityFracture.Demo
{
    public class FadeDestroy : MonoBehaviour
    {
        public float destroyTime = 2f;
        private float _timeOfDestroy;
        private Vector3 _origScale;
        private float _startTime;

        public void Start()
        {
            _timeOfDestroy = Time.time + destroyTime;
            _origScale = transform.localScale;
            _startTime = Time.time;
        }

        public void Update()
        {
            float scaleFactor = 1 - ((Time.time - _startTime) / (_timeOfDestroy - _startTime));
            if (scaleFactor < 0.0f)
                Destroy(gameObject);
            else
                transform.localScale = new Vector3(_origScale.x * scaleFactor, _origScale.y * scaleFactor, _origScale.z * scaleFactor);
        }
    }
}
