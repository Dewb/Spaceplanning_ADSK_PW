using stuffer;
namespace SpacePlanning
{
    /// <summary>
    /// Cell object, can be represented as a polygon2d ( square or rectangle ).
    /// </summary>
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

        #region - Private Constructor
        internal Cell(Point2d centerPt, double dimensionX, double dimensionY, bool cellAvail = true)
        {
            _centerPoint = centerPt;
            _dimX = dimensionX;
            _dimY = dimensionY;
            _cellAvailable = cellAvail;
        }

        internal Cell(Cell other)
        {
            _centerPoint = other.CenterPoint;
            _dimX = other.DimX;
            _dimY = other.DimY;
            _cellAvailable = other.CellAvailable;
        }
        #endregion

        #region - Public properties    

        /// <summary>
        /// Returns if the cell is available to be assigned to any program or department.
        /// </summary>
        public bool CellAvailable
        {
            get { return _cellAvailable;}
            set { _cellAvailable = value; }
        }

        /// <summary>
        /// Returns a point2d representing the center point of a cell object.
        /// </summary>
        public Point2d CenterPoint
        {
            get { return _centerPoint; }
        }

        /// <summary>
        /// Returns a point2d representing the left bottom corner point of a cell object.
        /// </summary>
        public Point2d LeftDownCorner
        {
            get { return new Point2d(_centerPoint.X - _dimX/2, _centerPoint.Y - _dimY/2); }
        }

        /// <summary>
        /// Returns a point2d representing the right bottom corner point of a cell object.
        /// </summary>
        public Point2d RightDownCorner
        {
            get { return new Point2d(_centerPoint.X + _dimX / 2, _centerPoint.Y - _dimY / 2); }
        }

        /// <summary>
        /// Returns a point2d representing the left top corner point of a cell object.
        /// </summary>
        public Point2d LeftUpCorner
        {
            get { return new Point2d(_centerPoint.X - _dimX / 2, _centerPoint.Y + _dimY / 2); }
        }

        /// <summary>
        /// Returns a point2d representing the right top corner point of a cell object.
        /// </summary>
        public Point2d RightUpCorner
        {
            get { return new Point2d(_centerPoint.X + _dimX / 2, _centerPoint.Y + _dimY / 2); }
        }

        /// <summary>
        /// X axis dimension value of a cell object
        /// </summary>
        public double DimX
        {
            get { return _dimX; }
        }

        /// <summary>
        /// Y axis dimension value of a cell object
        /// </summary>
        public double DimY
        {
            get { return _dimY; }
        }
        #endregion




    }
}
