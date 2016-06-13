
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
                    bool inside = GraphicsUtility.PointInsidePolygonTest(Polygon2d.ByPoints(outlinePoints), Point2d.ByCoordinates(posX, posY));
                    if (inside) {
                        pointsGrid.Add(new Point2d(posX, posY));
                        cells.Add(new Cell(new Point2d(posX, posY), dimXX, dimYY, true));
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
        public static List<Point2d> ConvexHullFromPoint2dList(List<Point2d> pointList)
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
        public static Dictionary<string, object> CreateGridLines(Polygon2d polyOutline, double dim = 10, int scale = 1)
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
        /// <param name="dim">Dimension of the cell object in X and Y direction.</param>
        /// <param name="tag">Boolean item to activate two separate modes of calculation.</param>
        /// <returns name="BorderPolyLine">Polygon2d representing orthogonal poly outline.</returns>
        /// <returns name="BorderCellsFound">Cell objects at the border of the outline.</returns>  
        /// <returns name="CellNeighborMatrix">Cell NeighborMatrix object.</returns> 
        /// <returns name="SortedCells">Sorted cell objects.</returns> 
        /// <search>
        /// bordercells, cellneighbormatrix
        /// </search>
        [MultiReturn(new[] { "BorderPolyLine", "BorderCellsFound","CellNeighborMatrix", "SortedCells"})]
        public static Dictionary<string, object> BorderAndCellNeighborMatrix(Polygon2d polyOutline, double dim,int iterationCount = 100, double proportion = 0.75)
        {
            if (!ValidateObject.CheckPoly(polyOutline)) return null;
            Dictionary<string, object> borderObject = new Dictionary<string, object>();
            Polygon2d borderPoly = new Polygon2d(null);
            List<Cell> sortedCells = new List<Cell>();
            List<List<int>> cellNeighborMatrix = new List<List<int>>(); 
            List<Polygon2d> cellsFound = new List<Polygon2d>();
            double dimAdjusted = dim, areaPoly = PolygonUtility.AreaPolygon(polyOutline), eps = 0.01;
            bool checkOut = false;
            int count = 0;
            while (!checkOut && count < iterationCount)
            {
                count += 1;
                List<Cell> cellsInside = CellsInsidePoly(polyOutline.Points, dimAdjusted);
                Dictionary<string, object> neighborObject = FormsCellNeighborMatrix(cellsInside);
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
                { "BorderPolyLine", (borderPoly) },
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
        public static Dictionary<string, object> FormMakeInSite(Polygon2d borderPoly, Polygon2d origSitePoly, 
            List<Cell> cellList,double groundCoverage = 0.5, int iteration = 100, bool randomToggle = false)
        {
            if (cellList == null) return null;
            if (!ValidateObject.CheckPoly(borderPoly)) return null;
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
        [MultiReturn(new[] { "BuildingOutline", "SiteArea", "BuildingOutlineArea", "GroundCoverAchieved", "SortedCells" })]
        public static Dictionary<string, object> MakeBuildingOutline(Polygon2d origSitePoly,
            List<Cell> cellList, double groundCoverage = 0.5, int iteration = 100)
        {
            if (cellList == null) return null;
            if (!ValidateObject.CheckPoly(origSitePoly)) return null;
            double eps = 0.05, fac = 0.95;          

            if (groundCoverage < eps) groundCoverage = 2 * eps;
            if (groundCoverage > 0.8) groundCoverage = 0.8;
            //double groundCoverLow = groundCoverage - eps, groundCoverHigh = groundCoverage + eps;
            double areaSite = PolygonUtility.AreaPolygon(origSitePoly),  areaPlaced = 0;
            double areaBuilding = groundCoverage * areaSite;
            Dictionary<string, object> sortCellObj = SortCellList(cellList);
            cellList = (List<Cell>)sortCellObj["SortedCells"];
            Random ran = new Random();
            List<Cell> selectedCells = new List<Cell>();
            for(int i = 0; i < cellList.Count; i++)
            {
                selectedCells.Add(cellList[i]);
                areaPlaced += cellList[i].CellArea;
                if (areaPlaced > fac * areaBuilding) break;
            }

            Dictionary<string, object> cellNeighborMatrixObject = FormsCellNeighborMatrix(selectedCells);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)cellNeighborMatrixObject["CellNeighborMatrix"];
            Dictionary<string, object> borderObject = CreateBorder(cellNeighborMatrix, selectedCells, true, true);
            Polygon2d borderPoly = (Polygon2d)borderObject["BorderPolyLine"];


            return new Dictionary<string, object>
            {
                { "BuildingOutline", (borderPoly) },
                { "SiteArea", (areaSite) },
                { "BuildingOutlineArea", (areaPlaced) },
                { "GroundCoverAchieved", (areaPlaced/areaSite) },
                { "SortedCells", (selectedCells)}
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
        [MultiReturn(new[] { "BuildingOutline", "SiteArea", "BuildingOutlineArea", "GroundCoverAchieved", "SortedCells" })]
        public static Dictionary<string, object> MakeBuildingOutline2(Polygon2d origSitePoly,
            List<Cell> cellList, List<List<int>> cellNeighborMatrixInp, double groundCoverage = 0.5, int iteration = 100)
        {
            if (cellList == null) return null;
            if (!ValidateObject.CheckPoly(origSitePoly)) return null;
            double eps = 0.05, fac = 0.95;

            if (groundCoverage < eps) groundCoverage = 2 * eps;
            if (groundCoverage > 0.8) groundCoverage = 0.8;
            //double groundCoverLow = groundCoverage - eps, groundCoverHigh = groundCoverage + eps;
            double areaSite = PolygonUtility.AreaPolygon(origSitePoly), areaPlaced = 0;
            double areaBuilding = groundCoverage * areaSite;
            Dictionary<string, object> sortCellObj = SortCellList(cellList);
            cellList = (List<Cell>)sortCellObj["SortedCells"];
            Random ran = new Random();
            List<Cell> selectedCells = new List<Cell>();
            int index = (int)BasicUtility.RandomBetweenNumbers(new Random(), cellList.Count, 0), count =0, count2=0;
            Trace.WriteLine("+++++++++++++++++++++++++++++++++++++++");
            Trace.WriteLine("First Index : " + index);
            Stack<int> cellsToIndex = new Stack<int>();
            while (areaPlaced < fac * areaBuilding)
            {
                int preIndex = index;
                count += 1;
                Trace.WriteLine("Iterating for : " + count + " Index selected is  : " + index);
                bool indexUpdate = false;
                List<Cell> cellsReview = new List<Cell>();
                count += 1;
                Cell cellitem = cellList[index];
                if (cellitem.CellAvailable)
                {
                    cellList[index].CellAvailable = false;
                    cellsReview.Add(cellitem);
                    areaPlaced += cellitem.CellArea;
                }
                List<int> neighborCells = cellNeighborMatrixInp[index];
                neighborCells.RemoveAll(s => s==-1);
                for(int i = 0; i < neighborCells.Count; i++)
                {
                    if (cellList[neighborCells[i]].CellAvailable)
                    {
                        if(!indexUpdate) index = neighborCells[i];
                        else { cellsToIndex.Push(index); }
                        indexUpdate = true;                      
                        cellsReview.Add(cellList[neighborCells[i]]);
                        cellList[neighborCells[i]].CellAvailable = false;
                        areaPlaced += cellList[neighborCells[i]].CellArea;
                    }
                }       
                if(preIndex == index)
                {
                    count2 += 1;
                    if (cellsToIndex.Count > 0) index = cellsToIndex.Pop();
                    else { break; }
                    Trace.WriteLine("Index did not update : " + count2);
                }
                else
                {
                    selectedCells.AddRange(cellsReview);
                }
                Trace.WriteLine("Cells to index on stack is : " + cellsToIndex.Count);
                //if (count2 > 10) break;    

            }

            Trace.WriteLine("+++++++++++++++++++++++++++++++++++++++");
            Trace.WriteLine("AreaNeeded : " + areaBuilding);
            Trace.WriteLine("AreaPlaced : " + areaPlaced);
            Dictionary<string, object> cellNeighborMatrixObject = FormsCellNeighborMatrix(selectedCells);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)cellNeighborMatrixObject["CellNeighborMatrix"];
            Dictionary<string, object> borderObject = CreateBorder(cellNeighborMatrix, selectedCells, true, true);
            Polygon2d borderPoly = (Polygon2d)borderObject["BorderPolyLine"];


            return new Dictionary<string, object>
            {
                { "BuildingOutline", (borderPoly) },
                { "SiteArea", (areaSite) },
                { "BuildingOutlineArea", (areaPlaced) },
                { "GroundCoverAchieved", (areaPlaced/areaSite) },
                { "SortedCells", (selectedCells)}
            };
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
            for(int i = 0; i < cellIdLists.Count; i++)
            {
                cellSelectedList.Add(cellLists[cellIdLists[i]]);
            }
            return cellSelectedList;
        }
        #endregion


        #region - Private Methods
        //sorts a list of cells based on a equation
        [MultiReturn(new[] { "SortedCells", "SortedCellIndices", "XYEqualtionList" })]
        public static Dictionary<string, object> SortCellList(List<Cell> cellLists)
        {
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
        public static Dictionary<string, object> FormsCellNeighborMatrix(List<Cell> cellLists)
        {
            double dimX = cellLists[0].DimX, dimY = cellLists[0].DimY;
            List<List<int>> cellNeighborMatrix = new List<List<int>>();
            List<Cell> cellListCopy = cellLists.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY)).ToList(); // example of deep copy
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
        [MultiReturn(new[] { "BorderPolyLine", "BorderCellsFound", "BorderPolyPoints" })]
        public static Dictionary<string, object> CreateBorder(List<List<int>> cellNeighborMatrix, List<Cell> cellListInp, bool tag = true, bool goReverse = false)
        {
            if (cellListInp == null || cellListInp.Count == 0) return null;

            List<Cell> cellList = cellListInp.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY)).ToList(); // example of deep copy
            int minValue = 25 ;
            //get the id of the lowest left cell centroid from all the boundary cells
            List<Point2d> cenPtBorderCells = new List<Point2d>();
            List<Point2d> borderPolyPoints = new List<Point2d>();
            List<Point2d> borderPolyThroughCenter = new List<Point2d>();
            for (int i = 0; i < cellList.Count; i++) cenPtBorderCells.Add(cellList[i].CenterPoint);
            int lowestCellId = PointUtility.LowestPointFromList(cenPtBorderCells);
            Cell currentCell = cellList[lowestCellId];
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
            for (int i = 0; i < cellList.Count; i++) isBorderCellList.Add(false);
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
                    cellList[cellNeighborMatrix[currentIndex][0]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][0]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][0]; // right
                    possibleCellFound = true;
                    if (!tag)
                    {
                        currentCell = cellList[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.RightDownCorner;
                    }

                }
                else if (cellNeighborMatrix[currentIndex][1] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][1]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][1]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][1]; // up
                    possibleCellFound = true;
                    if (!tag)
                    {
                        currentCell = cellList[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.RightUpCorner;
                    }
                }
                else if (cellNeighborMatrix[currentIndex][2] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][2]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][2]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][2]; // left
                    possibleCellFound = true;
                    if (!tag)
                    {
                        currentCell = cellList[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.LeftUpCorner;
                    }
                }
                else if (cellNeighborMatrix[currentIndex][3] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][3]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][3]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][3]; // down
                    possibleCellFound = true;
                    if (!tag)
                    {
                        currentCell = cellList[currentIndex];
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
                        currentCell = cellList[currentIndex];
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
                            currentCell = cellList[currentIndex];
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
                        currentCell = cellList[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.LeftDownCorner;
                    }
                    currentCellCenter = currentCell.CenterPoint;
                    num += 1;
                 
                }
            }// end of while loop
            //List<Point> ptList = DynamoGeometry.pointFromPoint2dList(borderPolyThroughCenter);
            List<Polygon2d> cellFound = MakeCellPolysFromIndicesPoint2d(borderPolyThroughCenter, cellList[0].DimX, cellList[0].DimY, null);
            Polygon2d borderPoly = PolygonUtility.CreateOrthoPoly(new Polygon2d(borderPolyPoints));
            return new Dictionary<string, object>
            {
                { "BorderPolyLine", (borderPoly) },
                { "BorderCellsFound", (cellFound) },
                { "BorderPolyPoints", (borderPolyPoints) }
            };
        }

        //get only corner and edge cells
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
            List<Cell> cellList = cellListInput.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY)).ToList(); // example of deep copy
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
            Dictionary<string, object> cellNeighborData = FormsCellNeighborMatrix(cellsInsideList);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)cellNeighborData["CellNeighborMatrix"];//---
            List<Cell> sortedCells = (List<Cell>)cellNeighborData["SortedCells"];



            Dictionary<string, object> cellNeighborData2 = FormsCellNeighborMatrix(sortedCells);
            List<List<int>> cellNeighborMatrix2 = (List<List<int>>)cellNeighborData2["CellNeighborMatrix"];//---
            Dictionary<string, object> borderObject = CreateBorder(cellNeighborMatrix2, sortedCells);
            Polygon2d mergePoly = (Polygon2d)borderObject["BorderPolyLine"];
            return new Dictionary<string, object>
            {
                { "MergedPoly", (mergePoly) },
                { "SortedCells", ( sortedCells) }
            };
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
