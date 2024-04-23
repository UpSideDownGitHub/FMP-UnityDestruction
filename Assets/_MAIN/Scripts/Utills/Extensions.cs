using UnityEngine;

namespace UnityFracture
{
    [System.Flags]
    public enum VertexAttributes
    {
        Position = 0x1,
        Texture0 = 0x2,
        Texture1 = 0x4,
        Lightmap = 0x4,
        Texture2 = 0x8,
        Texture3 = 0x10,
        Color = 0x20,
        Normal = 0x40,
        Tangent = 0x80,
        All = 0xFF
    };

    public static class Extensions
    {
        public static bool IsAbovePlane(this Vector3 p, Vector3 n, Vector3 o)
        {
            return (n.x * (p.x - o.x) + n.y * (p.y - o.y) + n.z * (p.z - o.z)) >= 0;
        }
    }
}
