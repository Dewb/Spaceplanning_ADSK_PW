using stuffer;

///////////////////////////////////////////////////////////////////
/// NOTE: This project requires REFERENCEPOINTs to the ProtoInterface
/// and ProtoGeometry DLLs. These are found in the Dynamo install
/// directory.
///////////////////////////////////////////////////////////////////

namespace SpacePlanning
{
    public class Cell
    {

       
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

        public Point2d LeftDownCorner
        {
            get { return new Point2d(_centerPoint.X - _dimX/2, _centerPoint.Y - _dimY/2); }
        }

        public Point2d RightDownCorner
        {
            get { return new Point2d(_centerPoint.X + _dimX / 2, _centerPoint.Y - _dimY / 2); }
        }

        public Point2d LeftUpCorner
        {
            get { return new Point2d(_centerPoint.X - _dimX / 2, _centerPoint.Y + _dimY / 2); }
        }

        public Point2d RightUpCorner
        {
            get { return new Point2d(_centerPoint.X + _dimX / 2, _centerPoint.Y + _dimY / 2); }
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
