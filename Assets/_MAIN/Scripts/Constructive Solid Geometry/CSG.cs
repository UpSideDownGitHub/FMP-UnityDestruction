// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/

using System.Collections.Generic;
using UnityEngine;

namespace UnityFracture
{
    public static class CSG
    {
        public enum BooleanOp
        {
            Intersection,
            Union,
            Subtraction
        }

        public static float epsilon = 0.00001f;

        public static Model Perform(BooleanOp op, GameObject lhs, GameObject rhs)
        {
            switch (op)
            {
                case BooleanOp.Intersection:
                    return Intersect(lhs, rhs);
                case BooleanOp.Union:
                    return Union(lhs, rhs);
                case BooleanOp.Subtraction:
                    return Subtract(lhs, rhs);
                default:
                    return null;
            }
        }

        public static Model Intersect(GameObject lhs, GameObject rhs)
        {
            Model csg_model_a = new Model(lhs);
            Model csg_model_b = new Model(rhs);

            Node a = new Node(csg_model_a.ToPolygons());
            Node b = new Node(csg_model_b.ToPolygons());

            List<Polygon> polygons = Node.Intersect(a, b).AllPolygons();

            return new Model(polygons);
        }

        public static Model Union(GameObject lhs, GameObject rhs)
        {
            Model csg_model_a = new Model(lhs);
            Model csg_model_b = new Model(rhs);

            Node a = new Node(csg_model_a.ToPolygons());
            Node b = new Node(csg_model_b.ToPolygons());

            List<Polygon> polygons = Node.Union(a, b).AllPolygons();

            return new Model(polygons);
        }

        public static Model Subtract(GameObject lhs, GameObject rhs)
        {
            Model csg_model_a = new Model(lhs);
            Model csg_model_b = new Model(rhs);

            Node a = new Node(csg_model_a.ToPolygons());
            Node b = new Node(csg_model_b.ToPolygons());

            List<Polygon> polygons = Node.Subtract(a, b).AllPolygons();

            return new Model(polygons);
        }
    }
}
