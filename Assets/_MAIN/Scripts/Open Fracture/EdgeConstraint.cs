using UnityEngine;

namespace ReubenMiller.Fracture
{
    /*
     * The following code is a re-written version of:
     * Greenheck, D. (2024). OpenFracture [Source Code]. Available from: https://github.com/dgreenheck/OpenFracture [Accessed May 2024].
     *
     * Unless otherwise specified
    */
    /// <summary>
    /// Represents an edge constraint between two vertices in the triangulation
    /// </summary>
    public class EdgeConstraint
    {
        // Index of the first end point of the constraint
        public int v1;
        // Index of the second end point of the constraint
        public int v2;
        // Index of the triangle prior to the edge crossing (v1 -> v2)
        public int t1;
        // Index of the triangle after the edge crossing (v1 -> v2)
        public int t2;
        // Index of the edge on the t1 side
        public int t1Edge;

        /// <summary>
        /// Creates a new edge constraint with the given end points
        /// </summary>
        public EdgeConstraint(int v1, int v2)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.t1 = -1;
            this.t2 = -1;
        }

        /// <summary>
        /// Creates a new edge constraint and defines triangles on either side of the edge
        /// </summary>
        public EdgeConstraint(int v1, int v2, int triangle1, int triangle2, int edge1)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.t1 = triangle1;
            this.t2 = triangle2;
            this.t1Edge = edge1;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return new { v1, v2 }.GetHashCode() + new { v2, v1 }.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(EdgeConstraint lhs, EdgeConstraint rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(EdgeConstraint lhs, EdgeConstraint rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Edge: T{t1}->T{t2} (V{v1}->V{v2})";
        }
    }

}
