using UnityEngine;
using UnityFracture.Demo;

namespace UnityFracture
{
    /// <summary>
    /// This is a demo class for how to implement area based destruction,
    /// in this case the objects collider must be set to trigger, then when the object
    /// enters other objects it will activate the function destroying the object -
    /// if it is a destructible
    /// </summary>
    public class AreaActivation : MonoBehaviour
    {
        public string destructibleTag;
        private bool _spawnEffect = false;
        private GameObject _effect;

        /// <summary>
        /// Sets the effect.
        /// </summary>
        /// <param name="spawn">if set to <c>true</c> [spawn].</param>
        /// <param name="effect">The effect.</param>
        public void SetEffect(bool spawn, GameObject effect)
        {
            _spawnEffect = spawn;
            _effect = effect;
        }

        /// <summary>
        /// Called when [trigger enter].
        /// and will destroy the object, if it is a destructible and spawn effect is needed
        /// </summary>
        /// <param name="other">The collied objet.</param>
        public void OnTriggerEnter(Collider other)
        {
            // if the object is a destrutible then destroy is by calling "FractureThis"
            if (other.CompareTag(destructibleTag))
            {
                other.gameObject.GetComponent<RuntimeFracture>().FractureThis();
                if (_spawnEffect)
                    Instantiate(_effect, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}
