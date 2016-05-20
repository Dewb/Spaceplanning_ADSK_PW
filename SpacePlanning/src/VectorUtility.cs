
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
using System.IO;
using stuffer;
using System.Collections;
using System.Diagnostics;
namespace SpacePlanning
{
    public class VectorUtility

    {
        #region - Public Methods

        //returns a point after addition of a vector to a point
        public static Point2d VectorAddToPoint(Point2d pt, Vector2d vec, double scale = 1)
        {
            double x = pt.X + vec.X * scale;
            double y = pt.Y + vec.Y * scale;
            return new Point2d(x, y);
        }

        //returns the angle between two vector3d
        public static double AngleBetweenVec3d(Vector3d vecA, Vector3d vecB, bool tag = true)
        {
            double dotProd = vecA.Dot(vecB);
            double magA = vecA.Length, magB = vecB.Length;
            double cosThetha = dotProd / (magA * magB);
            if (tag) return Math.Acos(cosThetha) * 180 / Math.PI;
            else return Math.Acos(cosThetha);
        }

        //returns angle between two vectors
        //for 'returndegrees' enter true for an answer in degrees, false for radians
        internal static double AngleBetweenVec2d(Vector2d u, Vector2d v, bool returndegrees)
        {
            double numerator = u.X * v.X + u.Y * v.Y;
            double u2 = u.X * u.X + u.Y * u.Y;
            double v2 = v.X * v.X + v.Y * v.Y;
            double denominator = 0;
            denominator = Math.Sqrt(u2 * v2);
            double rtnval = Math.Acos(numerator / denominator);
            if (returndegrees) rtnval *= 360.0 / (2 * Math.PI);
            return rtnval;
        }


        #endregion


    }
}
