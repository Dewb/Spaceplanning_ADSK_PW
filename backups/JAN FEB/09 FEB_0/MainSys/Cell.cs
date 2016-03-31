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
        private bool _cellAvailable;



        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        public Cell(Point2d centerPt, double dimensionX, double dimensionY, bool cellAvail = true)
        {
            _centerPoint = centerPt;
            _dimX = dimensionX;
            _dimY = dimensionY;
            _cellAvailable = cellAvail;
        }

        public bool CellAvailable
        {
            get { return _cellAvailable;}
            set { _cellAvailable = value; }
        }

        public Point2d CenterPoint
        {
            get { return _centerPoint; }
        }

        public double DimX
        {
            get { return _dimX; }
        }

        public double DimY
        {
            get { return _dimY; }
        }



    }
}
