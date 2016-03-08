using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
using System.IO;
using stuffer;

///////////////////////////////////////////////////////////////////
/// NOTE: This project requires references to the ProtoInterface
/// and ProtoGeometry DLLs. These are found in the Dynamo install
/// directory.
///////////////////////////////////////////////////////////////////

namespace SpacePlanning
{
    public class Cell
    {

        // Two private variables for example purposes
        private Point2d _centerPoint;
        private double _dimX;
        private double _dimY;



        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        internal Cell(Point2d centerPt, double dimensionX, double dimensionY)
        {
            _centerPoint = centerPt;
            _dimX = dimensionX;
            _dimY = dimensionY;
        }

        /// <summary>
        /// Takes the input Sat file geometry and converts to a PolyLine Object
        /// Can Also work with a sequential list of points
        /// </summary>
        /// <param name="lines">list of lines 2d store in class.</param>
        /// <returns>A newly-constructed ZeroTouchEssentials object</returns>
        public static Cell ByCenterPoint(Point2d centerPt, double dimensionX, double dimensionY)
        {
            return new Cell(centerPt,dimensionX,dimensionY);
        }


    }
}
