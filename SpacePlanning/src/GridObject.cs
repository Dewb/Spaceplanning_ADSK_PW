
using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using stuffer;
using System.Collections;
using System.Diagnostics;
using System.Linq;


namespace SpacePlanning
{
    /// <summary>
    /// Class to perform operations on the cell objects inside the site outline, inluding building a form and other similar uses.
    /// </summary>
    public class GridObject
    {

        // private variables
        private List<Cell> _cells = new List<Cell>();
        private List<Point2d> _siteOutline = new List<Point2d>();
        private List<Point2d> _siteBoundingBox = new List<Point2d>();
        private double _dimX;
        private double _dimY;

        private static int MINCELL = 400, MAXCELL = 800;

        //constructor
        internal GridObject(List<Point2d> siteOutline, List<Point2d> siteBoundingBox, double dimensionX, double dimensionY)
        {

            _siteOutline = siteOutline;
            _siteBoundingBox = siteBoundingBox;
            _dimX = dimensionX;
            _dimY = dimensionY;
        }


        #region - Public Methods
        // make cells inside a given building outline
        /// <summary>
        /// Builds the cells which are inside a given polygon2d outline.
        /// Returns list of cellls
        /// </summary>
        /// <param name="bbox">Point2d list representing a bounding box.</param>
        /// <param name="outlinePoints">Point2d list representing a polygon2d outline.</param>
        /// <param name="dimXX">X axis dimension of the cell object.</param>
        /// <param name="dimYY">Y axis dimension of the cell object.</param>
        /// <returns name="CellList">List of cell object inside the outline.</returns>
        /// <search>
        /// make cells inside outline, grid cells inside.
        /// </search>
        public static List<Cell> GridCellsInsideOutline(List<Point2d> bbox, List<Point2d> outlinePoints, double dimXX, double dimYY)
        {
            List<Point2d> pointsGrid = new List<Point2d>();
            List<Cell> cells = new List<Cell>();
            Range2d xyRange = PointUtility.FromPoint2dGetRange2D(bbox);

            double xDistance = xyRange.Xrange.Span;
            double yDistance = xyRange.Yrange.Span;

            int numPtsX = Convert.ToInt16(Math.Floor(xDistance / dimXX));
            int numPtsY = Convert.ToInt16(Math.Floor(yDistance / dimYY));

            double diffX = xDistance - (dimXX * numPtsX);
            double diffY = yDistance - (dimYY * numPtsY);

            double posX = bbox[0].X + diffX;
            double posY = bbox[0].Y + diffY;
            for (int i = 0; i < numPtsX; i++)
            {
                for (int j = 0; j < numPtsY; j++)
                {
                    //cellId calc
                    int cellId = (i * (numPtsY)) + j;
                    bool inside = GraphicsUtility.PointInsidePolygonTest(Polygon2d.ByPoints(outlinePoints), Point2d.ByCoordinates(posX, posY));
                    if (inside) {
                        pointsGrid.Add(new Point2d(posX, posY));
                        cells.Add(new Cell(new Point2d(posX, posY), dimXX, dimYY, cellId, true));
                    }
                    posY += dimYY;
                }
                posX += dimXX;
                posY = bbox[0].Y + diffY;
            }
            return cells;
        }

        //make cells as polygon2d's from point2d based on the indices provided
        /// <summary>
        /// Builds list of cell object based on a provided point 2d list and indices list.
        /// Returns list of polygon2d's representing the cells.
        /// </summary>
        /// <param name="pointsgrid">Point2d list to make the cells.</param>
        /// <param name="dimX">X axis dimension of the cell object.</param>
        /// <param name="dimY">y axis dimension of the cell object.</param>
        /// <param name="indexList">List of int, representing indicies to be used from the point2d list.</param>
        /// <returns name="PolyList">Polygon2d list representing cells.</returns>
        /// <search>
        /// make cell polys from point2d
        /// </search>
        public static List<Polygon2d> MakeCellPolysFromIndicesPoint2d(List<Point2d> pointsgrid, double dimX, double dimY, List<int> indexList = null)
        {
            List<Polygon2d> cellsPolyList = new List<Polygon2d>();
            List<Cell> cellList = new List<Cell>();
            if (indexList == null)
            {
                indexList = new List<int>();
                for (int i = 0; i < pointsgrid.Count; i++)
                {
                    indexList.Add(i);
                }
            }

            for (int i = 0; i < indexList.Count; i++)
            {
                List<Point2d> ptList = new List<Point2d>();

                double a = pointsgrid[indexList[i]].X - (dimX / 2);
                double b = pointsgrid[indexList[i]].Y - (dimY / 2);
                Point2d pt = Point2d.ByCoordinates(a, b);
                ptList.Add(pt);

                a = pointsgrid[indexList[i]].X - (dimX / 2);
                b = pointsgrid[indexList[i]].Y + (dimY / 2);
                pt = Point2d.ByCoordinates(a, b);
                ptList.Add(pt);

                a = pointsgrid[indexList[i]].X + (dimX / 2);
                b = pointsgrid[indexList[i]].Y + (dimY / 2);
                pt = Point2d.ByCoordinates(a, b);
                ptList.Add(pt);

                a = pointsgrid[indexList[i]].X + (dimX / 2);
                b = pointsgrid[indexList[i]].Y - (dimY / 2);
                pt = Point2d.ByCoordinates(a, b);
                ptList.Add(pt);


                Polygon2d pol = Polygon2d.ByPoints(ptList);
                cellsPolyList.Add(pol);
            }


            return cellsPolyList;
        }

        //make cells as polygon2d's from cell objects
        /// <summary>
        /// Makes polygon2d's to represent cell object
        /// </summary>
        /// <param name="cellList">List of cell objects.</param>
        /// <param name="indexList">List of int, representing indices of the cell objects to be used.</param>
        /// <param name="height">Z axis value of the polygon2d.</param>
        /// <returns name="PolyList">Polygon2d list representing cells.</returns>
        /// <search>
        /// make cell polys from cell objects
        /// </search>
        public static List<Polygon> MakeCellPolysFromCellObjects(List<Cell> cellList, List<int> indexList = null, double height  = 0)
        {
            if (cellList == null) return null;
            List<Polygon> cellsPolyList = new List<Polygon>();
            List<Point2d> pointsgrid = new List<Point2d>();
            for (int i = 0; i < cellList.Count; i++)
            {
                pointsgrid.Add(cellList[i].CenterPoint);
            }
            double dimX = cellList[0].DimX;
            double dimY = cellList[0].DimY;

            if (indexList.Count == 0)
            {
                indexList = new List<int>();
                for (int i = 0; i < pointsgrid.Count; i++)
                {
                    indexList.Add(i);
                }
            }

            for (int i = 0; i < indexList.Count; i++)
            {
                if(indexList[i] > -1)
                {

                List<Point> ptList = new List<Point>();

                double a = pointsgrid[indexList[i]].X - (dimX / 2);
                double b = pointsgrid[indexList[i]].Y - (dimY / 2);
                Point pt = Point.ByCoordinates(a, b,height);
                ptList.Add(pt);

                a = pointsgrid[indexList[i]].X - (dimX / 2);
                b = pointsgrid[indexList[i]].Y + (dimY / 2);
                pt = Point.ByCoordinates(a, b,height);
                ptList.Add(pt);

                a = pointsgrid[indexList[i]].X + (dimX / 2);
                b = pointsgrid[indexList[i]].Y + (dimY / 2);
                pt = Point.ByCoordinates(a, b,height);
                ptList.Add(pt);

                a = pointsgrid[indexList[i]].X + (dimX / 2);
                b = pointsgrid[indexList[i]].Y - (dimY / 2);
                pt = Point.ByCoordinates(a, b,height);
                ptList.Add(pt);

            

                Polygon pol = Polygon.ByPoints(ptList);
                cellsPolyList.Add(pol);

                }
            }

            return cellsPolyList;
        }

        //make grahams scan algo based convex hull from an input list of points
        /// <summary>
        /// Makes convex hull for an input list of points based on Grahams scan algorithm.
        /// Returns list of point2d.
        /// </summary>
        /// <param name="pointList">List of point2d whose convex hull needs to be made.</param>
        /// <returns name="PointList">List of point2d representing the convex hull.</returns>
        /// <search>
        /// make convex hull based on grahams scan algorithm
        /// </search>
        internal static List<Point2d> ConvexHullFromPoint2dList(List<Point2d> pointList)
        {
            List<Point2d> convexHullPtList = new List<Point2d>();
            int sizePtList = pointList.Count;
            //get the lowest point in the list and place it on 0 index
            PointUtility.GetLowestPointForGrahamsScan(ref pointList, sizePtList);
            PointUtility.SortedPoint2dListForGrahamScan(ref pointList, sizePtList);
            Stack tempStack = new Stack();
            tempStack.Push(pointList[0]);
            tempStack.Push(pointList[1]);
            tempStack.Push(pointList[2]);
            for (int i = 3; i < sizePtList; i++)
            {
                while (ValidateObject.CheckPointOrder(PointUtility.BeforeTopPointForGrahamScan(ref tempStack),
                    (Point2d)tempStack.Peek(), pointList[i]) != 2 && tempStack.Count > 1) tempStack.Pop();
                tempStack.Push(pointList[i]);
            }
            while (tempStack.Count > 0)
            {
                Point2d ptTop = (Point2d)tempStack.Peek();
                convexHullPtList.Add(ptTop);
                tempStack.Pop();
            }
            return convexHullPtList;
        }

        //creates grid lines on an input polygon2d
        /// <summary>
        /// Builds grid lines on an input polygon2d, based on input offset distance and multiplier
        /// </summary>
        /// <param name="polyOutline">Polygon2d outline on whiche grid linese needs to be built.</param>
        /// <param name="dim">X and Y axis dimension of the grid line spacings.</param>
        /// <param name="scale">Multipler on the dimension to offset grid lines.</param>
        /// <returns name="GridXLines">List of line2d in X direction.</returns>
        /// <returns name="GridYLines">List of line2d in X direction.</returns>
        /// <search>
        /// grid lines
        /// </search>
        [MultiReturn(new[] { "GridXLines", "GridYLines"})]
        internal static Dictionary<string, object> CreateGridLines(Polygon2d polyOutline, double dim = 10, int scale = 1)
        {
            if (scale < 1) scale = 1;
            double eps = 25, extension = 300;
            double distanceX = scale * dim, distanceY = scale * dim;

            Dictionary<string, object> lowHighObj = PointUtility.ReturnHighestAndLowestPointofBBox(polyOutline);
            Point2d lowPt = (Point2d)lowHighObj["LowerPoint"];
            Point2d hipt = (Point2d)lowHighObj["HigherPoint"];
            List<Point2d> pointXList = new List<Point2d>(), pointYList = new List<Point2d>();
            double minY = lowPt.Y, minX = lowPt.X, maxY = hipt.Y, maxX = hipt.X;

            double currentXPos = minY + distanceY;
            while (currentXPos <= maxY - distanceY / 2)
            {
                pointXList.Add(new Point2d(minX, currentXPos));
                currentXPos += distanceY;
            }
            double currentYPos = minX + distanceX;
            while (currentYPos <= maxX - distanceX / 2)
            {
                pointYList.Add(new Point2d(currentYPos, minY));
                currentYPos += distanceX;
            }
            List<Line2d> gridXLines = new List<Line2d>(), gridYLines = new List<Line2d>();
            for (int i = 0; i < pointYList.Count; i++)
            {
                Line2d line = new Line2d(pointYList[i], new Point2d(pointYList[i].X, pointYList[i].Y + eps));
                line = LineUtility.ExtendLine(line, extension);
                gridXLines.Add(line);
            }
            for (int i = 0; i < pointXList.Count; i++)
            {
                Line2d line = new Line2d(pointXList[i], new Point2d(pointXList[i].X + eps, pointXList[i].Y));
                line = LineUtility.ExtendLine(line, extension);
                gridYLines.Add(line);
            }

            return new Dictionary<string, object>
            {
                { "GridXLines", (gridXLines) },
                { "GridYLines", (gridYLines) }
            };
        }

        //finds the border cells and builds the cell neighbor matrix
        /// <summary>
        /// Finds the border cells for the input polyoutline
        /// Builds the Cell NeighborMatrix
        /// </summary>
        /// <param name="polyOutline">Polygon2d as the border outline.</param>
        /// <param name="cellDim">Dimension of the cell object in X and Y direction.</param>
        /// <param name="iteration">Boolean item to activate two separate modes of calculation.</param>
        /// <returns name="OrthoSiteOutline">Polygon2d representing orthogonal poly outline.</returns>
        /// <returns name="BorderCellsFound">Cell objects at the border of the outline.</returns>  
        /// <returns name="CellNeighborMatrix">Cell NeighborMatrix object.</returns> 
        /// <returns name="SortedCells">Sorted cell objects.</returns> 
        /// <search>
        /// bordercells, cellneighbormatrix
        /// </search>
        [MultiReturn(new[] { "OrthoSiteOutline", "BorderCellsFound","CellNeighborMatrix", "SortedCells"})]
        public static Dictionary<string, object> BorderAndCellNeighborMatrix(Polygon2d polyOutline, double cellDim, int iteration = 100 )
        {
            double proportion = 0.75;
            if (!ValidateObject.CheckPoly(polyOutline)) return null;
            Dictionary<string, object> borderObject = new Dictionary<string, object>();
            Polygon2d borderPoly = new Polygon2d(null);
            List<Cell> sortedCells = new List<Cell>();
            List<List<int>> cellNeighborMatrix = new List<List<int>>(); 
            List<Polygon2d> cellsFound = new List<Polygon2d>();
            int minCells = MINCELL, maxCells = MAXCELL;
            double dimAdjusted = cellDim;
            double areaPoly = PolygonUtility.AreaPolygon(polyOutline), eps = 0.01;
            int numCells = (int)(areaPoly/(dimAdjusted*dimAdjusted));
            if (numCells < minCells) dimAdjusted = Math.Sqrt(areaPoly / minCells);
            if (numCells > maxCells) dimAdjusted = Math.Sqrt(areaPoly / maxCells);
            bool checkOut = false;
            int count = 0;
            while (!checkOut && count < iteration)
            {
                count += 1;
                List<Cell> cellsInside = CellsInsidePoly(polyOutline.Points, dimAdjusted);
                Dictionary<string, object> neighborObject = BuildCellNeighborMatrix(cellsInside);
                cellNeighborMatrix = (List<List<int>>)neighborObject["CellNeighborMatrix"];
                sortedCells = (List<Cell>)neighborObject["SortedCells"];               

                borderObject = CreateBorder(cellNeighborMatrix, cellsInside, true, true);
                borderPoly = (Polygon2d)borderObject["BorderPolyLine"];
                if (!ValidateObject.CheckPoly(borderPoly)) { dimAdjusted -= eps; continue; }
                borderPoly = new Polygon2d(borderPoly.Points);
                cellsFound = (List<Polygon2d>)borderObject["BorderCellsFound"];
                double areaBorder = PolygonUtility.AreaPolygon(borderPoly);
                if (!ValidateObject.CheckPolygonSelfIntersection(borderPoly) && areaBorder/ areaPoly > proportion) checkOut = true;
                else dimAdjusted -= eps;
                //Trace.WriteLine("Trying Border Poly again for : " + count);
                //Trace.WriteLine("Dimension Cell used is " + dimAdjusted);
            }// end of while  

           
            return new Dictionary<string, object>
            {
                { "OrthoSiteOutline", (borderPoly) },
                { "BorderCellsFound", (cellsFound) },
                { "CellNeighborMatrix", (cellNeighborMatrix) },
                { "SortedCells", (sortedCells) }
            };
        }


        //makes orhtogonal form as polygon2d based on input ground coverage
        /// <summary>
        /// Builds the building outline form based on input site outline and ground coverage
        /// </summary>
        /// <param name="borderPoly">Orthogonal border polygon2d of the site outline</param>
        /// <param name="origSitePoly">Original polygon2d of the site outline</param>
        /// <param name="cellList">List of cell objects inside the site</param>
        /// <param name="groundCoverage">Expected ground coverage, value between 0.2 to 0.8</param>
        /// <param name="iteration">Number of times the node should iterate untill it retreives form satisfying ground coverage.</param>
        /// <returns name="BuildingOutline">Polygon2d representing orthogonal poly outline.</returns>
        /// <returns name="WholesomePolys">List of Polygon2d each wholesame having four sides.</returns>  
        /// <returns name="SiteArea">Area of the site outline.</returns> 
        /// <returns name="BuildingOutlineArea">Area of the building outline formed.</returns> 
        /// <returns name="GroundCoverAchieved">Ground coverage achieved, value between 0.2 to 0.8.</returns> 
        /// <returns name="SortedCells">Sorted cell objects.</returns> 
        /// <search>
        /// form maker, buildingoutline, orthogonal forms
        /// </search>
        [MultiReturn(new[] { "BuildingOutline","WholesomePolys", "SiteArea" , "BuildingOutlineArea", "GroundCoverAchieved", "SortedCells" })]
        internal static Dictionary<string, object> FormMakeInSite(Polygon2d borderPolyInp, Polygon2d origSitePoly, 
            List<Cell> cellList,double groundCoverage = 0.5, int iteration = 100, double notchDist = 10, bool randomToggle = false, bool notchToggle = true)
        {
            if (cellList == null) return null;
            if (!ValidateObject.CheckPoly(borderPolyInp)) return null;
            Polygon2d borderPoly = new Polygon2d(null);
            if(notchToggle)
            {
                Dictionary<string, object> notchObj = PolygonUtility.RemoveAllNotches(borderPolyInp, notchDist);
                borderPoly = (Polygon2d)notchObj["PolyNotchRemoved"];
            }
            bool blockPlaced = false;
            int count = 0;
            double areaSite = PolygonUtility.AreaPolygon(origSitePoly), eps = 0.05, areaPlaced = 0;
            if (groundCoverage < eps) groundCoverage = 2 * eps;
            if (groundCoverage > 0.8) groundCoverage = 0.8;
            double groundCoverLow = groundCoverage - eps, groundCoverHigh = groundCoverage + eps;
            Dictionary<string, object> wholeSomeData = new Dictionary<string, object>();
            Random ran = new Random();
            while (blockPlaced == false && count < iteration)
            {
                wholeSomeData = PolygonUtility.MakeWholesomeBlockInPoly(borderPoly, groundCoverage);
                List<Polygon2d> polysWhole = (List<Polygon2d>)wholeSomeData["WholesomePolys"];

                List<int> indicesList = BasicUtility.GenerateList(0, polysWhole.Count);
                indicesList = BasicUtility.RandomizeList(indicesList, ran);
                areaPlaced = 0;
                for (int i = 0; i < polysWhole.Count; i++)
                {
                    if (randomToggle) { areaPlaced += PolygonUtility.AreaPolygon(polysWhole[indicesList[i]]); }
                    else { areaPlaced += PolygonUtility.AreaPolygon(polysWhole[i]); }                 
                }
                if (areaPlaced < areaSite * groundCoverLow || areaPlaced > areaSite * groundCoverHigh) blockPlaced = false;
                else blockPlaced = true;
                count += 1;
            }
            List<Polygon2d> cleanWholesomePolyList = (List<Polygon2d>)wholeSomeData["WholesomePolys"];
            Dictionary<string, object> mergeObject = MergePoly(cleanWholesomePolyList, cellList);
            Polygon2d mergedPoly = (Polygon2d)mergeObject["MergedPoly"];
            List<Cell> sortedCells = (List<Cell>)mergeObject["SortedCells"];
            return new Dictionary<string, object>
            {
                { "BuildingOutline", (mergedPoly) },
                { "WholesomePolys", (cleanWholesomePolyList) },
                { "SiteArea", (areaSite) },
                { "BuildingOutlineArea", (areaPlaced) },
                { "GroundCoverAchieved", (areaPlaced/areaSite) },
                { "SortedCells", (sortedCells)}
            };
        }


        /// <summary>
        /// Iteratively builds the building outline as per the input site coverage.
        /// </summary>
        /// <param name="orthoSiteOutline">Orthogonal site outline Polygon2d.</param>
        /// <param name="cellListInp">List of cell objects on the site outline.</param>
        /// <param name="attractorPoints">List of Point2d representing points where building outline should not be placed.</param>
        /// <param name="weightList">List of double's representing weights of the attractor points.</param>
        /// <param name="siteCoverage">Expected site coverage.</param>
        /// <param name="iteration">Seed value representing design choice.</param>
        /// <param name="removeNotch">True or False toggling notch removal from the building outline.</param>
        /// <param name="minNotchDistance">Threshold distance below which , side of the building outline will be considered a notch and will be removed.</param>
        /// <param name="cellRefine">True or False toggle to allow addition of extra cell objects for precise building outline computation.</param>
        /// <param name="scanResolution">Integer value representing size of the box to scan possible cell objects to form the building outline.</param>
        /// <returns name = "BuildingOutline">Building outline as a Polygon2d.</returns>
        /// <search>
        /// form maker, buildingoutline, orthogonal forms
        /// </search>
        [MultiReturn(new[] { "BuildingOutline", "ExtraPoly", "SubdividedPolys", "SiteArea", "LeftOverArea", "BuildingOutlineArea", "GroundCoverAchieved", "SortedCells", "CellNeighborMatrix" })]
        public static Dictionary<string, object> FormBuildingIterator(Polygon2d orthoSiteOutline, List<Cell> cellListInp, List<Point2d> attractorPoints = default(List<Point2d>), List<double> weightList = default(List<double>),
     double siteCoverage = 0.5, int iteration = 100, bool removeNotch = false, double minNotchDistance = 10, bool cellRefine = false, int scanResolution = 0)
        {
            if (iteration < 1) iteration = 1;
            int count = 0, maxTry = 40;
            bool worked = false;
            double siteCoverAchieved = 0, scDifference = 0, scDifferenceBest = 10000;
            Dictionary<string, object> formBuildingOutlineObj = new Dictionary<string, object>();
            Dictionary<string, object> formBuildingOutlineObjBest = new Dictionary<string, object>();
            int dummy = 0;
            if (scanResolution == 0) dummy = (int)BasicUtility.RandomBetweenNumbers(new Random(iteration), 40, 3);
            else dummy = scanResolution;

            while (count < maxTry && !worked)
            {
                count += 1;
                Trace.WriteLine("||||||||||||||||||||||||||||||trying to get the form we want : " + count);
                formBuildingOutlineObj = FormBuildingOutlineTest(orthoSiteOutline, cellListInp, attractorPoints, weightList, siteCoverage, iteration, removeNotch, minNotchDistance, dummy, cellRefine);

                if (formBuildingOutlineObj == null)
                {
                    if (dummy < 1) dummy = (int)BasicUtility.RandomBetweenNumbers(new Random(iteration), 12, 3);
                    iteration += 1;
                    dummy -= 1;
                }
                else
                {
                    siteCoverAchieved = (double)formBuildingOutlineObj["GroundCoverAchieved"];
                    scDifference = Math.Abs(siteCoverAchieved - siteCoverage);
                    if (scDifference < 0.05) worked = true;
                    else
                    {
                        if (dummy < 1) dummy = (int)BasicUtility.RandomBetweenNumbers(new Random(iteration), 20, 7);
                        iteration += 1;
                        dummy -= 1;
                    }
                    if (scDifference < scDifferenceBest) { formBuildingOutlineObjBest = formBuildingOutlineObj; scDifferenceBest = scDifference; }
                }
                Trace.WriteLine("+++++++++++++++Difference in GC is : " + Math.Abs(siteCoverAchieved - siteCoverage));
            }// end of while loop
            //formBuildingOutlineObjBest["BuildingOutlineArea"] = count;
            return formBuildingOutlineObjBest;
        }




        [MultiReturn(new[] { "BuildingOutline", "ExtraPoly", "SubdividedPolys", "SiteArea", "LeftOverArea", "BuildingOutlineArea", "GroundCoverAchieved", "SortedCells", "CellNeighborMatrix" })]
        public static Dictionary<string, object> FormBuildingOutlineTest(Polygon2d orthoSiteOutline, List<Cell> cellListInp, List<Point2d> attractorPoints = default(List<Point2d>), 
            List<double>weightList = default(List<double>), double groundCoverage = 0.5, int iteration = 100, 
            bool removeNotch = false, double minNotchDistance  = 10,int dummy=100, bool cellRefine = false)
        {
            if (iteration < 1) iteration = 0;
            bool randomAllow = true, tag = true;
            if (cellListInp == null) return null;
            if (!ValidateObject.CheckPoly(orthoSiteOutline)) return null;
            List<Cell> cellList = cellListInp.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY,x.CellID)).ToList(); // example of deep copy 
            if(attractorPoints != null && weightList!= null) cellList = RemoveCellsBasedOnAttractors(cellList, attractorPoints, weightList);        
            double eps = 0.05, fac = 0.95, whole = 1, prop = 0;
            int number = (int)BasicUtility.RandomBetweenNumbers(new Random(iteration), 2, 6);
            number = dummy;
            int count = 0, dir = 0, prevDir = 0, countInner =0, countCircleMode = 0, countExtremeMode = 0, extremePointIndex = 0;

            if (groundCoverage < eps) groundCoverage = 2 * eps;
            if (groundCoverage > 0.8) groundCoverage = 0.8;
            double areaSite = PolygonUtility.AreaPolygon(orthoSiteOutline), areaPlaced = 0;
            double areaBuilding = groundCoverage * areaSite, areaLeft = 10000;
            List<double> areaPartsList = new List<double>();
            for (int i = 0; i < number; i++)
            {
                if (i == number - 1)
                {
                    prop = whole;
                    areaPartsList.Add(areaBuilding * prop);
                }
                else
                {
                    prop = whole / (2 * (i + 1));
                    areaPartsList.Add(areaBuilding * prop);
                    whole -= prop;
                }
            }// end of for loop
            List<Cell> selectedCells = new List<Cell>();
            List<List<Cell>> cellsGrouped = new List<List<Cell>>();
            List<Polygon2d> polySquares = new List<Polygon2d>(), polyExtra = new List<Polygon2d>();
            List<Point2d> ptSquares = new List<Point2d>();
            Polygon2d currentPoly = new Polygon2d(orthoSiteOutline.Points);
            Point2d center = PolygonUtility.CentroidOfPoly(orthoSiteOutline);
            if (randomAllow)
            {
                int side = 0;                
                double value = BasicUtility.RandomBetweenNumbers(new Random(iteration), 1, 0);
                if (value > 0.5) side = 1;
                else side = 0;
                Dictionary<string, object> quadrantObj = PolygonUtility.GetPointOnOneQuadrantTest(orthoSiteOutline, iteration,side);
                center = (Point2d)quadrantObj["RandomPoint"];
                if(value > 0.65) center = PolygonUtility.PlaceRandomPointInsidePoly(orthoSiteOutline, iteration);
            }

            Point2d topRight = new Point2d(0, 0), topLeft = topRight, bottomRight = topRight, bottomLeft = topRight;
            Queue<Polygon2d> polySqrStack = new Queue<Polygon2d>(), polySqrStackCopy = new Queue<Polygon2d>();
            dir = 0; // 0- right, 1 - up, 2 - left, 3 - down, 4 - down
     
            count = 0;
            bool found = false, deQueueMode = false, circleMode = false, sqrFinishedModed = false;
            dir = 0;
            double dist = Math.Sqrt(areaBuilding / dummy);
            //center = PolygonUtility.FindPointOnPolySide(currentPoly, dir, dist / 2);
            Trace.WriteLine("++++++++++++++++++++++++++");
            while (areaLeft > areaBuilding/20 && countInner < 10 && countExtremeMode < 10) 
            {               
                prevDir = dir;
                count += 1;
                Trace.WriteLine("lets place square again  ========================== : " + count);
                if (!deQueueMode)
                {
                    dir = 0;
                    currentPoly = PolygonUtility.SquareByCenter(center, dist);
                    polySqrStack.Enqueue(currentPoly);
                    polySqrStackCopy.Enqueue(currentPoly);
                    polySquares.Add(currentPoly);
                    ptSquares.Add(center);
                    center = PolygonUtility.FindPointOnPolySide(currentPoly, dir, dist / 2);
                }
                else
                {
                    //deQueue Mode On
                    countCircleMode += 1;
                    circleMode = true;
                    if (dir == 0) dir = 1;
                    else if (dir == 1) dir = 2;
                    else if (dir == 2) dir = 3;
                    else if (dir == 3) dir = 0;
                    if (countCircleMode > 4) { circleMode = false; countCircleMode = 0; }
                    center = PolygonUtility.FindPointOnPolySide(currentPoly, dir, dist / 2);
                    currentPoly = PolygonUtility.SquareByCenter(center, dist);
                    Trace.WriteLine("After popped , Direction set is now :  " + dir);
                    polySqrStack.Enqueue(currentPoly);
                    polySqrStackCopy.Enqueue(currentPoly);
                    //popped = false;
                }

                found = false;
                List<Cell> cellsFoundThisRound = new List<Cell>();
                for (int j = 0; j < cellList.Count; j++)
                {
                    if (cellList[j].CellAvailable)
                    {
                        if (GraphicsUtility.PointInsidePolygonTest(currentPoly, cellList[j].CenterPoint) &&
                            GraphicsUtility.PointInsidePolygonTest(orthoSiteOutline, cellList[j].CenterPoint))
                        {
                            //Trace.WriteLine("Cell Found, count = " + count);
                            found = true;
                            cellList[j].CellAvailable = false;
                            selectedCells.Add(cellList[j]);
                            cellsFoundThisRound.Add(cellList[j]);
                        }
                    }                    

                }// end of for loop
                if (!found) { Trace.WriteLine("No Cell Found, Area Still left = " + areaLeft); deQueueMode = true; }
                else Trace.WriteLine("Cell Found, count = " + count + "!! Area left: " + areaLeft);
                if (deQueueMode && !circleMode)
                {
                    if (polySqrStack.Count > 0) { currentPoly = polySqrStack.Dequeue(); Trace.WriteLine("After Popped , Stack has : " + polySqrStack.Count); polySquares.Add(currentPoly); }
                    else { Trace.WriteLine("Breaking , When count is : " + count); break; }
                }
                double areaPrevLeft = areaLeft;
                areaPlaced = AreaFromCells(selectedCells);
                areaLeft = areaBuilding - areaPlaced;

                if (areaLeft == areaPrevLeft)
                {
                    if (polySqrStack.Count > 0)
                    {
                        Trace.WriteLine("No change in area@@@@@@@@@@@@@@@@@@@@@@@@  " + countInner);
                        countInner += 1;
                        currentPoly = polySqrStack.Dequeue();
                        polySquares.Add(currentPoly);
                    }
                    else
                    {
                        
                        Trace.WriteLine("Poly Count in Queue@@@@@@@@@@@@@@@@@@@@@@@@  " + polySqrStack.Count);
                        // do this when all modes tried
                        //sqrFinishedModed = true;
                        dist = Math.Sqrt(areaLeft);
                        Dictionary<string, object> extremePtObj = PolygonUtility.GetExtremePointsFromCells(selectedCells);
                        if(extremePtObj != null)
                        {
                            topRight = (Point2d)extremePtObj["TopRightPoint"];
                            topLeft = (Point2d)extremePtObj["TopLeftPoint"];
                            bottomRight = (Point2d)extremePtObj["BottomRightPoint"];
                            bottomLeft = (Point2d)extremePtObj["BottomLeftPoint"];
                            if (extremePointIndex == 0) { center = bottomLeft; extremePointIndex += 1; }
                            else if (extremePointIndex == 1) { center = bottomRight; extremePointIndex += 1; }
                            else if (extremePointIndex == 2) { center = topLeft; extremePointIndex += 1; }
                            else { center = topRight; extremePointIndex += 1; }
                            if (extremePointIndex > 3) extremePointIndex = 0;
                            currentPoly = PolygonUtility.SquareByCenter(center, dist);
                            polySquares.Add(currentPoly);
                            polyExtra.Add(currentPoly);
                            polySqrStack.Enqueue(currentPoly);
                        }                                       
                        countExtremeMode += 1;
                    }                    
                }
                else countInner = 0;
                if(cellsFoundThisRound.Count>0) cellsGrouped.Add(cellsFoundThisRound);
            }// end of while loop


            List<Cell> preSelectedCellsCopy = selectedCells.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY,x.CellID)).ToList(); // example of deep copy
            if (cellRefine)
            {
                List<Cell> cellRefinedList = new List<Cell>();
                for (int i = 0; i < selectedCells.Count; i++) cellRefinedList.AddRange(CellsAddinCell(selectedCells[i]));
                selectedCells = cellRefinedList;
            }

            polySquares.AddRange(polySqrStack);
            Polygon2d borderPoly = new Polygon2d(null), offsetBorder = new Polygon2d(null);
            List<Polygon2d> borders = new List<Polygon2d>();
            //make the first borderPoly
            int numCells = selectedCells.Count;
            List<Cell> cellInsideBorderPoly = new List<Cell>();
            List<Cell> selectedCellsCopy = selectedCells.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY,x.CellID)).ToList(); // example of deep copy
            selectedCellsCopy = SetCellAvailability(selectedCellsCopy);
            Dictionary<string, object> sortCellObj = SortCellList(selectedCellsCopy);
            if (sortCellObj == null) return null;
            selectedCellsCopy = (List<Cell>)sortCellObj["SortedCells"];
            Dictionary<string, object> cellNeighborMatrixObject = BuildCellNeighborMatrix(selectedCellsCopy);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)cellNeighborMatrixObject["CellNeighborMatrix"];
            Dictionary<string, object> borderObject = CreateBorder(cellNeighborMatrix, selectedCellsCopy, true, true);
            if (borderObject != null)
            {
                borderPoly = (Polygon2d)borderObject["BorderPolyLine"];
                offsetBorder = PolygonUtility.OffsetPoly(borderPoly, selectedCells[0].DimX / 2);
                if (removeNotch)
                {
                    Dictionary<string, object> notchObj = PolygonUtility.RemoveAllNotches(borderPoly, minNotchDistance);
                    if (notchObj != null) borderPoly = (Polygon2d)notchObj["PolyNotchRemoved"];
                }
                borders.Add(borderPoly);
            }

            Polygon2d polyOrig = borderPoly;
            for (int i = 0; i < selectedCells.Count; i++)
                if (!GraphicsUtility.PointInsidePolygonTest(offsetBorder, selectedCells[i].LeftDownCorner)) cellInsideBorderPoly.Add(selectedCells[i]);
            
            int countMultiple = 0, maxTry = 30;
            //implement a while loop such that if there is any cells left which are not inside a poly you run border poly again and again
            while (cellInsideBorderPoly.Count > 0 && countMultiple < maxTry)
            {
                countMultiple += 1;
                selectedCells = cellInsideBorderPoly;

                selectedCellsCopy = selectedCells.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY, x.CellID)).ToList(); // example of deep copy
                selectedCellsCopy = SetCellAvailability(selectedCellsCopy);
                sortCellObj = SortCellList(selectedCellsCopy);
                if (sortCellObj == null) return null;
                selectedCellsCopy = (List<Cell>)sortCellObj["SortedCells"];
                cellNeighborMatrixObject = BuildCellNeighborMatrix(selectedCellsCopy);
                cellNeighborMatrix = (List<List<int>>)cellNeighborMatrixObject["CellNeighborMatrix"];
                borderObject = CreateBorder(cellNeighborMatrix, selectedCellsCopy, true, true);
                if (borderObject != null)
                {
                    borderPoly = (Polygon2d)borderObject["BorderPolyLine"];
                    //if (PolygonUtility.AreaPolygon(borderPoly) < areaBuilding * 0.2) continue;
                    offsetBorder = PolygonUtility.OffsetPoly(borderPoly, selectedCells[0].DimX / 2);
                    if (removeNotch)
                    {
                        Dictionary<string, object> notchObj = PolygonUtility.RemoveAllNotches(borderPoly, minNotchDistance);
                        if (notchObj != null) borderPoly = (Polygon2d)notchObj["PolyNotchRemoved"];

                    }
                    borders.Add(borderPoly);
                }                
                cellInsideBorderPoly.Clear();
                for (int i = 0; i < selectedCells.Count; i++)
                    if (!GraphicsUtility.PointInsidePolygonTest(offsetBorder, selectedCells[i].LeftDownCorner)) cellInsideBorderPoly.Add(selectedCells[i]);
            }// end while loop

            double areaBorder = 0;
            for (int i = 0; i < borders.Count; i++) areaBorder += PolygonUtility.AreaPolygon(borders[i]);
            double groundCovAchieved = areaBorder / areaSite;
            return new Dictionary<string, object>
            {
               
                { "BuildingOutline", (borders) },//borders
                { "ExtraPoly", (polyExtra) },
                { "SubdividedPolys", (polySquares) },
                { "SiteArea", (ptSquares) },
                { "LeftOverArea", (areaLeft) },
                { "BuildingOutlineArea", (cellsGrouped) },
                { "GroundCoverAchieved", (areaPlaced/areaSite) },//areaPlaced/areaSite
                { "SortedCells", (preSelectedCellsCopy)},
                { "CellNeighborMatrix", (cellNeighborMatrix) }
            };
        }



        //bounding box from a group of cells
        internal static Polygon2d FromCellsGetBoundingPoly(List<Cell> cellList)
        {
            if (cellList == null || cellList.Count == 0) return null;
            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < cellList.Count; i++) ptList.Add(cellList[i].CenterPoint);
            Polygon2d poly =  new Polygon2d(ReadData.FromPointsGetBoundingPoly(ptList));
            return PolygonUtility.OffsetPoly(poly, cellList[0].DimX / 2);
        }

 
        //checks if cells are withing the range of given attractor points, with respective weight values.
        internal static List<Cell> RemoveCellsBasedOnAttractors( List<Cell> cellListInp,List<Point2d> attractorPointList, List<double> weightList)
        {
            if (cellListInp == null || attractorPointList == null || weightList == null) return null;
            List<Cell> cellList = cellListInp.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY,x.CellID)).ToList(); // example of deep copy           
            List<Cell> selectedCells = new List<Cell>();
            List<Polygon2d> circleList = new List<Polygon2d>();
            for(int i = 0; i < attractorPointList.Count; i++) circleList.Add(PolygonUtility.CircleByRadius(attractorPointList[i], weightList[i]));
            for (int i = 0; i < cellList.Count; i++)
            {
                bool inside = false;
                for(int j = 0; j < circleList.Count; j++)
                {
                    Polygon2d currentPoly = circleList[j];
                    if (GraphicsUtility.PointInsidePolygonTest(currentPoly, cellList[i].CenterPoint)) { inside = true; break; }            
                }
                if (!inside) selectedCells.Add(cellList[i]);

            }// end of for loop
            return selectedCells;
        }

       
        //make cells inside polgon2d
        /// <summary>
        /// Builds cell objects inside a given polygon2d
        /// </summary>
        /// <param name="outlinePoints">Polygon2d outline input as a list of point2d list.</param>
        /// <param name="dim">Dimension of the cell object in X and Y axis.</param>
        /// <returns name="Cell Objects">List of cell objects.</returns> 
        /// <search>
        /// cell objects, cells inside polygon2d
        /// </search>
        public static List<Cell> CellsInsidePoly(List<Point2d> outlinePoints, double dim)
        {
            List<Point2d> bboxPoints = ReadData.FromPointsGetBoundingPoly(outlinePoints);
            return GridCellsInsideOutline(bboxPoints, outlinePoints, dim, dim);                     
        }

        //gets cells by indices in a list
        /// <summary>
        /// Gets the cell objects whose indices are provided
        /// </summary>
        /// <param name="cellLists">List of cell objects.</param>
        /// <param name="cellIdLists">List of int, indices of the cell objects to be obtained.</param>
        /// <returns name="CellList">List of cell objects.</returns> 
        /// <search>
        /// cells by indices, cell objects
        /// </search>
        public static List<Cell> CellsByIndex(List<Cell> cellLists, List<int> cellIdLists)
        {
            List<Cell> cellSelectedList = new List<Cell>();
            for(int i = 0; i < cellIdLists.Count; i++) cellSelectedList.Add(cellLists[cellIdLists[i]]); 
            return cellSelectedList;         
        }
        #endregion


        #region - Private Methods
        //sorts a list of cells based on a equation
        [MultiReturn(new[] { "SortedCells", "SortedCellIndices", "XYEqualtionList" })]
        internal static Dictionary<string, object> SortCellList(List<Cell> cellLists)
        {
            if (cellLists == null || cellLists.Count < 2) return null;
            List<Cell> newCellLists = new List<Cell>();
            for (int i = 0; i < cellLists.Count; i++) newCellLists.Add(new Cell(cellLists[i]));
            List<Point2d> cellCenterPtLists = new List<Point2d>();
            for (int i = 0; i < newCellLists.Count; i++) cellCenterPtLists.Add(newCellLists[i].CenterPoint);

            List<double> XYEquationList = new List<double>();
            int[] UnsortedIndices = new int[cellCenterPtLists.Count];
            double[] XCordCenterPt = new double[cellCenterPtLists.Count];
            double[] YCordCenterPt = new double[cellCenterPtLists.Count];

            for (int i = 0; i < cellCenterPtLists.Count; i++) XYEquationList.Add(EquationforXYLocation(cellCenterPtLists[i].X, cellCenterPtLists[i].Y));
            List<int> SortedXYEquationIndices = new List<int>();
            SortedXYEquationIndices = BasicUtility.Quicksort(XYEquationList);
            List<double> XYEquationLists = new List<double>();
            for (int k = 0; k < cellCenterPtLists.Count; k++) XYEquationLists.Add(XYEquationList[k]);
            List<Cell> sortedCells = new List<Cell>();
            for (int i = 0; i < SortedXYEquationIndices.Count; i++) sortedCells.Add(newCellLists[SortedXYEquationIndices[i]]);

            //fix cell ids
            for (int i = 0; i < sortedCells.Count; i++) sortedCells[i].CellID = i;

            return new Dictionary<string, object>
            {
                { "SortedCells", (sortedCells) },
                { "CellCenterPoints" , (cellCenterPtLists) },
                { "SortedCellIndices", (SortedXYEquationIndices) },
                { "XYEqualtionList", ( XYEquationLists) }
            };
        }

        //make cell neighbor matrix
        [MultiReturn(new[] { "CellNeighborMatrix", "XYEqualtionList", "SortedCells" })]
        internal static Dictionary<string, object> BuildCellNeighborMatrix(List<Cell> cellLists)
        {
            double dimX = cellLists[0].DimX, dimY = cellLists[0].DimY;
            List<List<int>> cellNeighborMatrix = new List<List<int>>();
            List<Cell> cellListCopy = cellLists.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY,x.CellID)).ToList(); // example of deep copy
            Dictionary<string, object> sortedObject = SortCellList(cellListCopy);
            List<Cell> newCellLists = (List<Cell>)sortedObject["SortedCells"];
            List<Point2d> cellCenterPtLists = (List<Point2d>)sortedObject["CellCenterPoints"];
            List<double> XYEquationLists = (List<double>)sortedObject["XYEqualtionList"];



            for (int i = 0; i < cellCenterPtLists.Count; i++)
            {
                List<Point2d> neighborPoints = new List<Point2d>();
                List<int> neighborCellIndex = new List<int>();
                Point2d currentCenterPt = cellCenterPtLists[i];
                Point2d down = new Point2d(currentCenterPt.X, currentCenterPt.Y - dimY);
                Point2d left = new Point2d(currentCenterPt.X - dimX, currentCenterPt.Y);
                Point2d up = new Point2d(currentCenterPt.X, currentCenterPt.Y + dimY);
                Point2d right = new Point2d(currentCenterPt.X + dimX, currentCenterPt.Y);

                //find index of down cell
                double downValue = EquationforXYLocation(currentCenterPt.X, (currentCenterPt.Y - dimY));
                int downCellIndex = BasicUtility.BinarySearch(XYEquationLists, downValue);
                //if (downCellIndex == i) downCellIndex = -1;
                //find index of left cell
                double leftValue = EquationforXYLocation((currentCenterPt.X - dimX), currentCenterPt.Y);
                int leftCellIndex = BasicUtility.BinarySearch(XYEquationLists, leftValue);
                // if (leftCellIndex == i) leftCellIndex = -1;
                //find index of up cell
                double upValue = EquationforXYLocation(currentCenterPt.X, (currentCenterPt.Y + dimY));
                int upCellIndex = BasicUtility.BinarySearch(XYEquationLists, upValue);
                //if (upCellIndex == i) upCellIndex = -1;

                //find index of up cell
                double rightValue = EquationforXYLocation((currentCenterPt.X + dimX), currentCenterPt.Y);
                int rightCellIndex = BasicUtility.BinarySearch(XYEquationLists, rightValue);
                //if (rightCellIndex == i) rightCellIndex = -1;


                //RULD - right, up, left, down | adding -1 means the cell does not have any neighbor at that spot
                if (rightCellIndex > -1) { neighborCellIndex.Add(rightCellIndex); } else { neighborCellIndex.Add(-1); };
                if (upCellIndex > -1) { neighborCellIndex.Add(upCellIndex); } else { neighborCellIndex.Add(-1); };
                if (leftCellIndex > -1) { neighborCellIndex.Add(leftCellIndex); } else { neighborCellIndex.Add(-1); };
                if (downCellIndex > -1) { neighborCellIndex.Add(downCellIndex); } else { neighborCellIndex.Add(-1); };

                neighborPoints.Add(right);
                neighborPoints.Add(up);
                neighborPoints.Add(left);
                neighborPoints.Add(down);
                cellNeighborMatrix.Add(neighborCellIndex);
            }

            return new Dictionary<string, object>
            {
                { "CellNeighborMatrix", (cellNeighborMatrix) },
                { "XYEqualtionList", ( XYEquationLists) },
                { "SortedCells" , (newCellLists) }
            };
        }

        //get the cells and make an orthogonal outline poly - not using now, but works best
        /// <summary>
        /// Creates the orthogonal border Polygon2d for a list of cell objects.
        /// </summary>
        /// <param name="cellNeighborMatrix"> Cell neighbor matrix represented as a list of list of integers.</param>
        /// <param name="cellList">List of cell objects.</param>
        /// <param name="tag">Bool value to allow same cell corner Point2d to computer border Polygon2d. Recommended value is true.</param>
        /// <param name="goReverse">Bool value to enable reverse tracking cells to find best fit border Polygon2d.Recommended value is true.</param>
        /// <returns name ="BorderPolygon">Border polygon as a Polygon2d.</returns>
        [MultiReturn(new[] { "BorderPolygon", "BorderCells" })]
        public static Dictionary<string, object> CreateBorder(List<List<int>> cellNeighborMatrix, List<Cell> cellList, bool tag = true, bool goReverse = true)
        {
            if (cellList == null || cellList.Count == 0) return null;

            List<Cell> cellListNew = cellList.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY,x.CellID)).ToList(); // example of deep copy
            int minValue = 25 ;
            //get the id of the lowest left cell centroid from all the boundary cells
            List<Point2d> cenPtBorderCells = new List<Point2d>();
            List<Point2d> borderPolyPoints = new List<Point2d>();
            List<Point2d> borderPolyThroughCenter = new List<Point2d>();
            for (int i = 0; i < cellListNew.Count; i++) cenPtBorderCells.Add(cellListNew[i].CenterPoint);
            int lowestCellId = PointUtility.LowestPointFromList(cenPtBorderCells);
            Cell currentCell = cellListNew[lowestCellId];
            Point2d currentCellPoint = currentCell.LeftDownCorner;
            Point2d currentCellCenter = currentCell.CenterPoint;
            int currentIndex = lowestCellId, num = 0, reverseCount =0;
            List<List<int>> cellNeighborMatrixCopy = new List<List<int>>();
            for (int i = 0; i < cellNeighborMatrix.Count; i++)
            {
                List<int> idsList = new List<int>();
                for (int j = 0; j < cellNeighborMatrix[i].Count; j++) idsList.Add(cellNeighborMatrix[i][j]);
                cellNeighborMatrixCopy.Add(idsList);
            }
            List<int> borderCellIdList = GetCornerAndEdgeCellId(cellNeighborMatrixCopy);
            List<bool> isBorderCellList = new List<bool>();
            for (int i = 0; i < cellListNew.Count; i++) isBorderCellList.Add(false);
            for (int i = 0; i < borderCellIdList.Count; i++) isBorderCellList[borderCellIdList[i]] = true;

            Stack<int> visitedCellIndices = new Stack<int>();
            bool possibleCellFound = false, checking = false;
            visitedCellIndices.Push(currentIndex);
            //order of neighbors : right , up , left , down
            while (num < borderCellIdList.Count)
            {
                //Trace.WriteLine("Starting while loop : " + num);
                possibleCellFound = false;
                if(!checking)
                {
                    borderPolyPoints.Add(currentCellPoint);
                    borderPolyThroughCenter.Add(currentCellCenter);
                }             
              
                if (cellNeighborMatrix[currentIndex][0] > -1 &&
                    cellListNew[cellNeighborMatrix[currentIndex][0]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][0]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][0]; // right
                    possibleCellFound = true;
                    if (!tag)
                    {
                        currentCell = cellListNew[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.RightDownCorner;
                    }

                }
                else if (cellNeighborMatrix[currentIndex][1] > -1 &&
                    cellListNew[cellNeighborMatrix[currentIndex][1]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][1]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][1]; // up
                    possibleCellFound = true;
                    if (!tag)
                    {
                        currentCell = cellListNew[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.RightUpCorner;
                    }
                }
                else if (cellNeighborMatrix[currentIndex][2] > -1 &&
                    cellListNew[cellNeighborMatrix[currentIndex][2]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][2]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][2]; // left
                    possibleCellFound = true;
                    if (!tag)
                    {
                        currentCell = cellListNew[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.LeftUpCorner;
                    }
                }
                else if (cellNeighborMatrix[currentIndex][3] > -1 &&
                    cellListNew[cellNeighborMatrix[currentIndex][3]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][3]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][3]; // down
                    possibleCellFound = true;
                    if (!tag)
                    {
                        currentCell = cellListNew[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.LeftDownCorner;
                    }
                }

                if (goReverse)
                {
                    //Trace.WriteLine("Reverse Mode On +++");
                    //do the following when finding reverse Cell id is set as true
                    if (tag && possibleCellFound)
                    {
                        visitedCellIndices.Push(currentIndex);
                        currentCell = cellListNew[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.LeftDownCorner;
                        currentCellCenter = currentCell.CenterPoint;
                        num += 1;
                        checking = false;
                        reverseCount = 0;
                        //Trace.WriteLine("Cell id found after checking");
                    }
                    else
                    {                       
                        if (visitedCellIndices.Count > 0 && reverseCount < minValue)
                        {
                            reverseCount += 1;
                            currentIndex = visitedCellIndices.Pop();
                            currentCell = cellListNew[currentIndex];
                            currentCell.CellAvailable = false;
                            currentCellPoint = currentCell.LeftDownCorner;
                            currentCellCenter = currentCell.CenterPoint;
                            checking = true;
                            //Trace.WriteLine("Reversing , " + reverseCount + "|| Going back to prev cell, cell id was : " + currentIndex);
                        }
                        else
                        {
                            num += 1;
                            //Trace.WriteLine("No need reversing, lets move on");
                        }

                    }// end of goReverse is true, if else loop
                }
                else
                {
                    //Trace.WriteLine("Reverse Mode Off //////////");
                    //do the following when finding reverse Cell id is set as false             
                    if (tag)
                    {
                        currentCell = cellListNew[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.LeftDownCorner;
                    }
                    currentCellCenter = currentCell.CenterPoint;
                    num += 1;
                 
                }
            }// end of while loop
            //List<Point> ptList = DynamoGeometry.pointFromPoint2dList(borderPolyThroughCenter);
            List<Polygon2d> cellFound = MakeCellPolysFromIndicesPoint2d(borderPolyThroughCenter, cellListNew[0].DimX, cellListNew[0].DimY, null);
            Polygon2d borderPoly = PolygonUtility.CreateOrthoPoly(new Polygon2d(borderPolyPoints));
            return new Dictionary<string, object>
            {
                { "BorderPolygon", (borderPoly) },
                { "BorderCells", (cellFound) }
            };
        }

        //get only corner and edge cells
        /// <summary>
        /// Returns the cell indices of corner and edge cells from the input cell neighbor matrix.
        /// </summary>
        /// <param name="cellNeighborMatrix">Cell Neighbor Matrix represented as list of list of integers.</param>
        /// <returns name = "cellIndices">List of integers representing corner and edge cell indices.</returns>
        /// <search>
        /// border cells, corner cells, cell ids
        /// </search>
        public static List<int> GetCornerAndEdgeCellId(List<List<int>> cellNeighborMatrix)
        {
            List<int> cellIdList = new List<int>();
            List<bool> cornerCellList = new List<bool>();

            List<List<int>> cellNeighborMatrixCopy = new List<List<int>>();
            for (int i = 0; i < cellNeighborMatrix.Count; i++)
            {
                List<int> idsList = new List<int>();
                for (int j = 0; j < cellNeighborMatrix[i].Count; j++) idsList.Add(cellNeighborMatrix[i][j]);
                cellNeighborMatrixCopy.Add(idsList);
            }
            for (int i = 0; i < cellNeighborMatrix.Count; i++)
            {
                cellNeighborMatrix[i].Remove(-1);
                if (cellNeighborMatrix[i].Count <= 3) cellIdList.Add(i);
            }

            for (int i = 0; i < cellNeighborMatrixCopy.Count; i++)
            {
                bool cellSelected = false;
                if (cellNeighborMatrixCopy[i][0] > -1 && cellNeighborMatrixCopy[i][1] > -1
                    && cellNeighborMatrixCopy[i][2] > -1 && cellNeighborMatrixCopy[i][3] > -1)
                {
                    //1-RU
                    if (cellNeighborMatrixCopy[cellNeighborMatrixCopy[i][0]][1] == -1) cellSelected = true;
                    //2-UL
                    else if (cellNeighborMatrixCopy[cellNeighborMatrixCopy[i][1]][2] == -1) cellSelected = true;
                    //3-LD
                    else if (cellNeighborMatrixCopy[cellNeighborMatrixCopy[i][2]][3] == -1) cellSelected = true;
                    //4-DR
                    else if (cellNeighborMatrixCopy[cellNeighborMatrixCopy[i][3]][0] == -1) cellSelected = true;
                    if (cellSelected) cellIdList.Add(i);
                }
            }
            return cellIdList;
        }

        //get list of poly and merge them to make one big poly
        [MultiReturn(new[] { "MergedPoly", "SortedCells" })]
        internal static Dictionary<string, object> MergePoly(List<Polygon2d> polyList, List<Cell> cellListInput)
        {
            List<Cell> cellList = cellListInput.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY,x.CellID)).ToList(); // example of deep copy
            List<Cell> cellsInsideList = new List<Cell>();
            for (int i = 0; i < polyList.Count; i++)
            {
                for (int j = 0; j < cellList.Count; j++)
                {
                    if (GraphicsUtility.PointInsidePolygonTest(polyList[i], cellList[j].CenterPoint) && cellList[j].CellAvailable)
                    {
                        cellsInsideList.Add(cellList[j]);
                        cellList[j].CellAvailable = false;
                    }
                }
            }
            Dictionary<string, object> cellNeighborData = BuildCellNeighborMatrix(cellsInsideList);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)cellNeighborData["CellNeighborMatrix"];//---
            List<Cell> sortedCells = (List<Cell>)cellNeighborData["SortedCells"];



            Dictionary<string, object> cellNeighborData2 = BuildCellNeighborMatrix(sortedCells);
            List<List<int>> cellNeighborMatrix2 = (List<List<int>>)cellNeighborData2["CellNeighborMatrix"];//---
            Dictionary<string, object> borderObject = CreateBorder(cellNeighborMatrix2, sortedCells);
            Polygon2d mergePoly = (Polygon2d)borderObject["BorderPolyLine"];
            return new Dictionary<string, object>
            {
                { "MergedPoly", (mergePoly) },
                { "SortedCells", ( sortedCells) }
            };
        }

        //adds more cells inside a cell , 0 =  4 cell, 1 = 16 cells
        internal static List<Cell> CellsAddinCell(Cell cell)
        {
            if (cell == null) return null;
            double dimX = 0, dimY =0, num = 2;
            List<Cell> cellAddedList = new List<Cell>();
            Point2d centerPt = new Point2d(0, 0);

            dimX = cell.DimX* 0.25; dimY = cell.DimY * 0.25;
            centerPt = new Point2d(cell.LeftDownCorner.X + dimX, cell.LeftDownCorner.Y + dimY);
            cellAddedList.Add(new Cell(centerPt, cell.DimX/ num, cell.DimY/ num, cell.CellID, cell.CellAvailable));

            dimX = cell.DimX * 0.75; dimY = cell.DimY * 0.25;
            centerPt = new Point2d(cell.LeftDownCorner.X + dimX, cell.LeftDownCorner.Y + dimY);
            cellAddedList.Add(new Cell(centerPt, cell.DimX / num, cell.DimY / num, cell.CellID,cell.CellAvailable));

            dimX = cell.DimX * 0.75; dimY = cell.DimY * 0.75;
            centerPt = new Point2d(cell.LeftDownCorner.X + dimX, cell.LeftDownCorner.Y + dimY);
            cellAddedList.Add(new Cell(centerPt, cell.DimX / num, cell.DimY / num, cell.CellID, cell.CellAvailable));

            dimX = cell.DimX * 0.25; dimY = cell.DimY * 0.75;
            centerPt = new Point2d(cell.LeftDownCorner.X + dimX, cell.LeftDownCorner.Y + dimY);
            cellAddedList.Add(new Cell(centerPt, cell.DimX / num, cell.DimY / num, cell.CellID, cell.CellAvailable));
            return cellAddedList;

        }

        //finds any id issues in the cellNeighborMatrix
        internal static List<bool> FindProblemsInCellNeighbors(List<List<int>> cellNeighborMatrix)
        {
            List<bool> isErrorList = new List<bool>();
            for (int i = 0; i < cellNeighborMatrix.Count; i++)
            {
                bool error = false;
                for (int j = 0; j < cellNeighborMatrix[i].Count; j++)
                {
                    if (i == cellNeighborMatrix[i][j]) error = true;
                }
                isErrorList.Add(error);
            }
            return isErrorList;
        }

        //returns the value of equation for cell neighbor matrix
        internal static double EquationforXYLocation(double x, double y)
        {
            double A = 100, B = 1;
            //return 1000 * Math.Round((A * x * x * x + B * y * y * y + (A - B) * x + (B - A) * y),3);
            //return 1000*Math.Round(((A * x + B * y)), 3); 
            return 1000 * Math.Round(((A * x) + (B * y)), 3);
        }

        //make grahams scan algo based convex hull from an input list of points
        internal static List<Point2d> ConvexHullFromPoint2d(List<Point2d> ptList)
        {
            List<Point2d> convexHullPtList = new List<Point2d>();
            int sizePtList = ptList.Count;
            //get the lowest point in the list and place it on 0 index
            PointUtility.GetLowestPointForGrahamsScan(ref ptList, sizePtList);
            PointUtility.SortedPoint2dListForGrahamScan(ref ptList, sizePtList);
            Stack tempStack = new Stack();
            tempStack.Push(ptList[0]);
            tempStack.Push(ptList[1]);
            tempStack.Push(ptList[2]);
            for (int i = 3; i < sizePtList; i++)
            {
                while (ValidateObject.CheckPointOrder(PointUtility.BeforeTopPointForGrahamScan(ref tempStack),
                    (Point2d)tempStack.Peek(), ptList[i]) != 2 && tempStack.Count > 1)
                {
                    tempStack.Pop();
                }
                tempStack.Push(ptList[i]);
            }

            while (tempStack.Count > 0)
            {
                Point2d ptTop = (Point2d)tempStack.Peek();
                convexHullPtList.Add(ptTop);
                tempStack.Pop();
            }
            return convexHullPtList;
        }
        //make convex hulls from cell objects

        [MultiReturn(new[] { "PolyCurves", "PointLists" })]
        internal static Dictionary<string, object> MakeConvexHullsFromCells(List<List<Cell>> cellProgramsList)
        {
            List<Polygon> polyList = new List<Polygon>();
            List<List<Point2d>> ptAllnew = new List<List<Point2d>>();
            for (int i = 0; i < cellProgramsList.Count; i++)
            {
                List<Cell> cellList = cellProgramsList[i];
                List<Point2d> tempPts = new List<Point2d>();
                List<Point2d> convexHullPts = new List<Point2d>();
                if (cellList.Count > 0) for (int j = 0; j < cellList.Count; j++) tempPts.Add(cellList[j].CenterPoint);
                if (tempPts.Count > 2)
                {
                    ptAllnew.Add(tempPts);
                    convexHullPts = GridObject.ConvexHullFromPoint2d(tempPts);
                    List<Point> ptAll = DynamoGeometry.pointFromPoint2dList(convexHullPts);
                    if (ptAll.Count > 2) polyList.Add(Polygon.ByPoints(ptAll));
                    else polyList.Add(null);
                }
                else polyList.Add(null);
            }

            return new Dictionary<string, object>
            {
                { "PolyCurves", (polyList) },
                { "PointLists", (ptAllnew) }
            };


        }

        //finds the area of a list of cell
        /// <summary>
        /// Computes total area of the input cells.
        /// </summary>
        /// <param name="cellList">List of cell objects.</param>
        /// <returns name="Area">Total area covered by the cell.</returns>
        /// <search>
        /// cell areas, area from cells
        /// </search>
        public static double AreaFromCells(List<Cell> cellList)
        {
            if (cellList == null) return -1;
            double area = 0;
            for(int i = 0; i < cellList.Count; i++) area += cellList[i].CellArea; 
            return area;
        }

        //finds the area of a list of cell
        internal static List<Cell> SetCellAvailability(List<Cell> cellList, bool value = true)
        {
            if (cellList == null) return null;
            List<Cell> cellListCopy = cellList.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY,x.CellID)).ToList(); // example of deep copy
            for (int i = 0; i < cellListCopy.Count; i++) cellListCopy[i].CellAvailable = value;
            return cellListCopy;
        }


        //make list of point2d from a bounding box as cell centers
        internal static List<Point2d> GridPointsFromBBoxNew(List<Point2d> bbox, double dimXX, double dimYY, double a, double b)
        {
            List<Point2d> pointsGrid = new List<Point2d>();
            Range2d xyRange = PointUtility.FromPoint2dGetRange2D(bbox);
            double xDistance = xyRange.Xrange.Span;
            double yDistance = xyRange.Yrange.Span;

            int numPtsX = Convert.ToInt16(Math.Floor(xDistance / dimXX));
            int numPtsY = Convert.ToInt16(Math.Floor(yDistance / dimYY));

            double diffX = xDistance - (dimXX * numPtsX);
            double diffY = yDistance - (dimYY * numPtsY);

            double posX = bbox[0].X + diffX * a;
            double posY = bbox[0].Y + diffY * b;
            for (int i = 0; i < numPtsX; i++)
            {
                for (int j = 0; j < numPtsY; j++)
                {
                    pointsGrid.Add(new Point2d(posX, posY));
                    posY += dimYY;
                }
                posX += dimXX;
                posY = bbox[0].Y + diffY * b;
            }
            return pointsGrid;

        }
        #endregion

    
    }
}
