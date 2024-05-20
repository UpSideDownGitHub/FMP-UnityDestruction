namespace ReubenMiller.Fracture
{
    /*
     * The following code is a re-written version of:
     * Greenheck, D. (2024). OpenFracture [Source Code]. Available from: https://github.com/dgreenheck/OpenFracture [Accessed May 2024].
     *
     * Unless otherwise specified
    */

    /// <summary>
    /// Data structure that holds triangulation adjacency data for a quad
    /// </summary>
    public struct Quad
    {
        //               q3        
        //      *---------*---------*
        //       \       / \       /
        //        \ t2L /   \ t2R /
        //         \   /     \   /
        //          \ /   t2  \ /
        //        q1 *---------* q2 
        //          / \   t1  / \    
        //         /   \     /   \     
        //        / t1L \   / t1R \   
        //       /       \ /       \  
        //      *---------*---------*
        //               q4      

        // The indices of the quad vertices
        public int q1, q2, q3, q4;
        // The triangles that make up the quad
        public int t1, t2;
        // Triangle adjacency data
        public int t1L, t1R, t2L, t2R;

        /// <summary>
        /// Initializes a new instance of the <see cref="Quad"/> struct.
        /// </summary>
        /// <param name="q1">The q1.</param>
        /// <param name="q2">The q2.</param>
        /// <param name="q3">The q3.</param>
        /// <param name="q4">The q4.</param>
        /// <param name="t1">The t1.</param>
        /// <param name="t2">The t2.</param>
        /// <param name="t1L">The t1 l.</param>
        /// <param name="t1R">The t1 r.</param>
        /// <param name="t2L">The t2 l.</param>
        /// <param name="t2R">The t2 r.</param>
        public Quad(int q1, int q2, int q3, int q4, int t1, int t2, int t1L, int t1R, int t2L, int t2R)
        {
            this.q1 = q1;
            this.q2 = q2;
            this.q3 = q3;
            this.q4 = q4;
            this.t1 = t1;
            this.t2 = t2;
            this.t1L = t1L;
            this.t1R = t1R;
            this.t2L = t2L;
            this.t2R = t2R;
        }
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"T{t1}/T{t2} (V{q1},V{q2},V{q3},V{q4})";
        }
    }
}
