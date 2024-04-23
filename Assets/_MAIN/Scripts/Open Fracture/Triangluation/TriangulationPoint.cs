using UnityEngine;

namespace UnityFracture
{
    public class TriangulationPoint : IBinSortable
    {
        public Vector2 coords;
        public int bin { get; set; }
        public int index = 0;

        public TriangulationPoint(int index, Vector2 coords)
        {
            this.index = index;
            this.coords = coords;
        }

        public override string ToString()
        {
            return $"{coords} -> {bin}";
        }
    }
}
