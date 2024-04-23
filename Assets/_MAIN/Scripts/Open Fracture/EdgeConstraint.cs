using UnityEngine;

namespace UnityFracture
{
    public class EdgeConstraint
    {
        public int v1;
        public int v2;
        public int t1;
        public int t2;
        public int t1Edge;

        public EdgeConstraint(int v1, int v2)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.t1 = -1;
            this.t2 = -1;
        }

        public EdgeConstraint(int v1, int v2, int triangle1, int triangle2, int edge1)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.t1 = triangle1;
            this.t2 = triangle2;
            this.t1Edge = edge1;
        }

        public override bool Equals(object obj)
        {
            if (obj is EdgeConstraint)
            {
                var other = (EdgeConstraint)obj;
                return (this.v1 == other.v1 && this.v2 == other.v2) ||
                       (this.v1 == other.v2 && this.v2 == other.v1);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return new { v1, v2 }.GetHashCode() + new { v2, v1 }.GetHashCode();
        }

        public static bool operator ==(EdgeConstraint lhs, EdgeConstraint rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(EdgeConstraint lhs, EdgeConstraint rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override string ToString()
        {
            return $"Edge: T{t1}->T{t2} (V{v1}->V{v2})";
        }
    }

}
