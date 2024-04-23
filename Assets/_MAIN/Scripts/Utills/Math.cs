using UnityEngine;

namespace UnityFracture
{

    public static class Math
    {
        public static bool IsQuadConvex(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            return LinesIntersectInternal(a1, a2, b1, b2, true);
        }

        public static bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            return LinesIntersectInternal(a1, a2, b1, b2, false);
        }

        private static bool LinesIntersectInternal(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, bool includeSharedEndpoints)
        {
            Vector2 a12 = new Vector2(a2.x - a1.x, a2.y - a1.y);
            Vector2 b12 = new Vector2(b2.x - b1.x, b2.y - b1.y);

            if (a1 == b1 || a1 == b2 || a2 == b1 || a2 == b2)
            {
                return includeSharedEndpoints;
            }
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
        public static bool LinePlaneIntersection(Vector3 a,
                                                 Vector3 b,
                                                 Vector3 n,
                                                 Vector3 p0,
                                                 out Vector3 x,
                                                 out float s)
        {
            s = 0;
            x = Vector3.zero;

            if (a == b)
            {
                return false;
            }
            else if (n == Vector3.zero)
            {
                return false;
            }
            s = Vector3.Dot(p0 - a, n) / Vector3.Dot(b - a, n);
            if (s >= 0 && s <= 1)
            {
                x = a + (b - a) * s;
                return true;
            }
            return false;
        }

        public static bool IsPointOnRightSideOfLine(Vector2 a, Vector2 b, Vector2 c)
        {
            return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) <= 0;
        }

    }
}
