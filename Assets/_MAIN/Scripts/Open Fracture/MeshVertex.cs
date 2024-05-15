using UnityEngine;
namespace UnityFracture
{
    /// <summary>
    /// Data structure containing position/normal/UV data for a single vertex
    /// </summary>
    public struct MeshVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        public MeshVertex(Vector3 position)
        {
            this.position = position;
            this.normal = Vector3.zero;
            this.uv = Vector2.zero;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MeshVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="normal">The normal.</param>
        /// <param name="uv">The uv.</param>
        public MeshVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
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
            if (!(obj is MeshVertex)) return false;

            return ((MeshVertex)obj).position.Equals(this.position);
        }
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(MeshVertex lhs, MeshVertex rhs)
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
        public static bool operator !=(MeshVertex lhs, MeshVertex rhs)
        {
            return !lhs.Equals(rhs);
        }
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.position.GetHashCode();
        }
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Position = {position}, Normal = {normal}, UV = {uv}";
        }
    }
}