using UnityEngine;

namespace ReubenMiller.Fracture
{

    public static class Math
    {
        /// <summary>
        /// Determines whether the given quad is convex.
        /// </summary>
        /// <param name="a1">The a1.</param>
        /// <param name="a2">The a2.</param>
        /// <param name="b1">The b1.</param>
        /// <param name="b2">The b2.</param>
        /// <returns>
        ///   <c>true</c> if the quad is conevec; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsQuadConvex(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            return LinesIntersectInternal(a1, a2, b1, b2, true);
        }

        /// <summary>
        /// determines weather the 2 given lines intersect
        /// </summary>
        /// <param name="a1">The a1.</param>
        /// <param name="a2">The a2.</param>
        /// <param name="b1">The b1.</param>
        /// <param name="b2">The b2.</param>
        /// <returns><c>true</c> if the lines intersect; otherwse, <c>false</c></returns>
        public static bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            return LinesIntersectInternal(a1, a2, b1, b2, false);
        }

        /// <summary>
        /// determines weather the given 2 lines intersect internallt
        /// </summary>
        /// <param name="a1">The a1.</param>
        /// <param name="a2">The a2.</param>
        /// <param name="b1">The b1.</param>
        /// <param name="b2">The b2.</param>
        /// <param name="includeSharedEndpoints">if set to <c>true</c> [include shared endpoints].</param>
        /// <returns></returns>
        private static bool LinesIntersectInternal(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, bool includeSharedEndpoints)
        {
            Vector2 a12 = new Vector2(a2.x - a1.x, a2.y - a1.y);
            Vector2 b12 = new Vector2(b2.x - b1.x, b2.y - b1.y);

            // if the lines are the same
            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
                return includeSharedEndpoints;
            // return true of the lines intersect
            else
            {
                float a1xb = (a1.x - b1.x) * b12.y - (a1.y - b1.y) * b12.x;
                float a2xb = (a2.x - b1.x) * b12.y - (a2.y - b1.y) * b12.x;
                float b1xa = (b1.x - a1.x) * a12.y - (b1.y - a1.y) * a12.x;
                float b2xa = (b2.x - a1.x) * a12.y - (b2.y - a1.y) * a12.x;
                return ((a1xb >= 0 && a2xb <= 0) || (a1xb <= 0 && a2xb >= 0)) &&
                       ((b1xa >= 0 && b2xa <= 0) || (b1xa <= 0 && b2xa >= 0));
            }
        }
        /// <summary>
        /// determines if a line intersects the plane
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="n">The n.</param>
        /// <param name="p0">The p0.</param>
        /// <param name="x">The x.</param>
        /// <param name="s">The s.</param>
        /// <returns><c>true</c> if line intersects plane; otherwise <c>false</c></returns>
        public static bool LinePlaneIntersection(Vector3 a,
                                                 Vector3 b,
                                                 Vector3 n,
                                                 Vector3 p0,
                                                 out Vector3 x,
                                                 out float s)
        {
            s = 0;
            x = Vector3.zero;

            // line cant be a point
            if (a == b)
                return false;
            // if no normal then not valid
            else if (n == Vector3.zero)
                return false;
            s = Vector3.Dot(p0 - a, n) / Vector3.Dot(b - a, n);
            // if the line and plane intersect
            if (s >= 0 && s <= 1)
            {
                x = a + (b - a) * s;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether point is on right side of line.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <returns>
        ///   <c>true</c> if point is on right side of line ; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPointOnRightSideOfLine(Vector2 a, Vector2 b, Vector2 c)
        {
            return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) <= 0;
        }

    }
}
