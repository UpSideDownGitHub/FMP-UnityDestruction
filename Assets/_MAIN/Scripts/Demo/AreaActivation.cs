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

        /// <summary>
        /// Called when [trigger enter].
        /// and will destroy the object, if it is a destructible
        /// </summary>
        /// <param name="other">The collied objet.</param>
        public void OnTriggerEnter(Collider other)
        {
            // if the object is a destrutible then destroy is by calling "FractureThis"
            if (other.CompareTag(destructibleTag))
            {
                other.gameObject.GetComponent<RuntimeFracture>().FractureThis();
                Destroy(gameObject);
            }
        }
    }
}
