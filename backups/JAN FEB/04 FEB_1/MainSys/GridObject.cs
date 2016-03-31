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
    public class GridObject
    {

        // private variables
        private List<Cell> _cells = new List<Cell>();
        private List<Point2d> _siteOutline = new List<Point2d>();
        private List<Point2d> _siteBoundingBox = new List<Point2d>();
        private double _dimX;
        private double _dimY;



        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        internal GridObject(List<Point2d> siteOutline, List<Point2d> siteBoundingBox, double dimensionX, double dimensionY)
        {
           
            _siteOutline = siteOutline;
            _siteBoundingBox = siteBoundingBox;
            _dimX = dimensionX;
            _dimY = dimensionY;
        }

        /// <summary>
        /// Takes the input Sat file geometry and converts to a PolyLine Object
        /// Can Also work with a sequential list of points
        /// </summary>
        /// <param name="lines">list of lines 2d store in class.</param>
        /// <returns>A newly-constructed ZeroTouchEssentials object</returns>
        public static GridObject BySiteOutline(List<Cell> cellList, List<Point2d> siteOutline, List<Point2d> siteBoundingBox, double dimensionX, double dimensionY)
        {
            return new GridObject(siteOutline,siteBoundingBox,dimensionX,dimensionY);
        }

        //private functions
        internal void addSome()
        {

        }

        //public functions


        //MAKE POINT2D LIST OF POINTS INSIDE THE BOUNDING BOX - points are stored clockwise
        public static List<Point2d> GridPointsFromBBox(List<Point2d> bbox, double dimXX, double dimYY)
        {
            // index 0 stores min point 2 stores the max point
            List<Point2d> pointsGrid = new List<Point2d>();
            Range2d xyRange = ReadData.FromPoint2dGetRange2D(bbox);

            double xDistance = xyRange.Xrange.Span;
            double yDistance = xyRange.Yrange.Span;

            int numPtsX = Convert.ToInt16(Math.Floor(xDistance / dimXX));
            int numPtsY = Convert.ToInt16(Math.Floor(xDistance / dimXX));
            double posX =   bbox[0].X;
            double posY =   bbox[0].Y;
            for (int i = 0; i < numPtsX; i++)
            {
                
                for(int j = 0; j < numPtsY; j++)
                {
                    pointsGrid.Add(new Point2d(posX, posY));
                    posY += dimYY;
                }

                posX += dimXX;
                posY = bbox[0].Y;
            }

            
            return pointsGrid;

        }







    }
}
