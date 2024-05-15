using UnityEngine;

namespace UnityFracture
{
    /// <summary>
    /// contains a function that will allow for the optimisation of the children
    /// of an object
    /// </summary>
    public class DestructionController : MonoBehaviour
    {
        /// <summary>
        /// Optmizes the children of the current object.
        /// </summary>
        public void OptmizeChildren()
        {
            // loop through all of the children and optimize the mesh attached
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                transform.GetChild(i).GetComponent<MeshFilter>().sharedMesh.Optimize();
            }
        }
    }
}
