using UnityEngine;

namespace UnityFracture
{
    public class DestructionController : MonoBehaviour
    {
        public void OptmizeChildren()
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                transform.GetChild(i).GetComponent<MeshFilter>().sharedMesh.Optimize();
            }
        }
    }
}
