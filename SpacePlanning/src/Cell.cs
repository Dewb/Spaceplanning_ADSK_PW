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

        private  enum _cellType{
            CORNER,
            EDGE,
            PAD,
            CORE        
        };



        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        public Cell(Point2d centerPt, double dimensionX, double dimensionY, bool cellAvail = true)
        {
            _centerPoint = centerPt;
            _dimX = dimensionX;
            _dimY = dimensionY;
            _cellAvailable = cellAvail;
        }

        public Cell(Cell other)
        {
            _centerPoint = other.CenterPoint;
            _dimX = other.DimX;
            _dimY = other.DimY;
            _cellAvailable = other.CellAvailable;
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
