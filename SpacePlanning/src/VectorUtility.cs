
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
        //returns a point after addition of a vector to a point
        internal static Point2d VectorAddToPoint(Point2d pt, Vector2d vec, double scale =1)
        {
            double x = pt.X + vec.X * scale;
            double y = pt.Y + vec.Y * scale;
            return new Point2d(x, y);
        }


    }
}
