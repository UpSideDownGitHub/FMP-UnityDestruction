using UnityEngine;

namespace UnityFracture
{
    /// <summary>
    /// This data structure is used to represent a point during triangulation.
    /// </summary>
    public class TriangulationPoint : IBinSortable
    {
        // 2D coordinates of the point on the triangulation plane
        public Vector2 coords;
        // Bin used for sorting points in grid
        public int bin { get; set; }
        // Original index prior to sorting
        public int index = 0;

        /// <summary>
        /// Instantiates a new triangulation point
        /// </summary>
        /// <param name="index">The index of the point in the original point list</param>
        /// <param name="coords">The 2D coordinates of the point in the triangulation plane</param>
        public TriangulationPoint(int index, Vector2 coords)
        {
            this.index = index;
            this.coords = coords;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{coords} -> {bin}";
        }
    }
}
