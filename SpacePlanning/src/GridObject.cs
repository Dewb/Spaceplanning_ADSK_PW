
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
    public class GridObject
    {

        // private variables
        private List<Cell> _cells = new List<Cell>();
        private List<Point2d> _siteOutline = new List<Point2d>();
        private List<Point2d> _siteBoundingBox = new List<Point2d>();
        private double _dimX;
        private double _dimY;
        #region - Private Methods
        //constructor
        internal GridObject(List<Point2d> siteOutline, List<Point2d> siteBoundingBox, double dimensionX, double dimensionY)
        {

            _siteOutline = siteOutline;
            _siteBoundingBox = siteBoundingBox;
            _dimX = dimensionX;
            _dimY = dimensionY;
        }
        

        //make list of point2d from a bounding box as cell centers
        internal static List<Point2d> GridPointsFromBBoxNew(List<Point2d> bbox, double dimXX, double dimYY, double a, double b)
        {
            List<Point2d> pointsGrid = new List<Point2d>();
            Range2d xyRange = GraphicsUtility.FromPoint2dGetRange2D(bbox);
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

        #region - Public Methods
        // make point2d list which are inside the bounding box
        public static List<Cell> GridCellsInsideOutline(List<Point2d> bbox, List<Point2d> outlinePoints, double dimXX, double dimYY)
        {
            List<Point2d> pointsGrid = new List<Point2d>();
            List<Cell> cells = new List<Cell>();
            Range2d xyRange = GraphicsUtility.FromPoint2dGetRange2D(bbox);

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
                    bool inside = GraphicsUtility.PointInsidePolygonTest(outlinePoints, Point2d.ByCoordinates(posX, posY));
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


        //make cells on the grids poit
        public static List<Polygon2d> MakeCellsFromGridPoints(List<Point> pointsgrid, double dimX, double dimY)
        {
            List<Point2d> pt2dList = new List<Point2d>();
            for (int i = 0; i < pointsgrid.Count; i++)
            {
                pt2dList.Add(Point2d.ByCoordinates(pointsgrid[i].X, pointsgrid[i].Y));
            }

            return MakeCellsFromGridPoints2d(pt2dList, dimX, dimY);

        }


        //make cells on the grids from point2d
        public static List<Polygon2d> MakeCellsFromGridPoints2d(List<Point2d> point2dgrid, double dimX, double dimY)
        {

            List<Polygon2d> cellsPolyList = new List<Polygon2d>();
            List<Cell> cellList = new List<Cell>();
            for (int i = 0; i < point2dgrid.Count; i++)
            {
                List<Point2d> ptList = new List<Point2d>();

                double a = point2dgrid[i].X - (dimX / 2);
                double b = point2dgrid[i].Y - (dimY / 2);
                Point2d pt = Point2d.ByCoordinates(a, b);
                ptList.Add(pt);

                a = point2dgrid[i].X - (dimX / 2);
                b = point2dgrid[i].Y + (dimY / 2);
                pt = Point2d.ByCoordinates(a, b);
                ptList.Add(pt);

                a = point2dgrid[i].X + (dimX / 2);
                b = point2dgrid[i].Y + (dimY / 2);
                pt = Point2d.ByCoordinates(a, b);
                ptList.Add(pt);

                a = point2dgrid[i].X + (dimX / 2);
                b = point2dgrid[i].Y - (dimY / 2);
                pt = Point2d.ByCoordinates(a, b);
                ptList.Add(pt);

                Polygon2d pol = Polygon2d.ByPoints(ptList);
                cellsPolyList.Add(pol);
            }
            return cellsPolyList;
        }


        //make cells on the grids from point2d from indices
        public static List<Polygon> MakeCellsFromIndicesPoint2d(List<Point2d> pointsgrid, double dimX, double dimY, List<int> indexList)
        {
            List<Polygon> cellsPolyList = new List<Polygon>();
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

                List<Point> ptList = new List<Point>();

                double a = pointsgrid[indexList[i]].X - (dimX / 2);
                double b = pointsgrid[indexList[i]].Y - (dimY / 2);
                Point pt = Point.ByCoordinates(a, b);
                ptList.Add(pt);

                a = pointsgrid[indexList[i]].X - (dimX / 2);
                b = pointsgrid[indexList[i]].Y + (dimY / 2);
                pt = Point.ByCoordinates(a, b);
                ptList.Add(pt);

                a = pointsgrid[indexList[i]].X + (dimX / 2);
                b = pointsgrid[indexList[i]].Y + (dimY / 2);
                pt = Point.ByCoordinates(a, b);
                ptList.Add(pt);

                a = pointsgrid[indexList[i]].X + (dimX / 2);
                b = pointsgrid[indexList[i]].Y - (dimY / 2);
                pt = Point.ByCoordinates(a, b);
                ptList.Add(pt);


                Polygon pol = Polygon.ByPoints(ptList);
                cellsPolyList.Add(pol);
            }


            return cellsPolyList;
        }

        //make cells on the grids from cell objects
        public static List<Polygon> MakeCellsFromCellObjects(List<Cell> cellList, List<int> indexList = null, double height  = 0)
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
        public static List<Point2d> ConvexHullFromPoint2D(List<Point2d> ptList)
        {
            List<Point2d> convexHullPtList = new List<Point2d>();
            int sizePtList = ptList.Count;
            //get the lowest point in the list and place it on 0 index
            GraphicsUtility.FindLowestPointFromList(ref ptList, sizePtList);
            GraphicsUtility.SortedPoint2dList(ref ptList, sizePtList);
            Stack tempStack = new Stack();
            tempStack.Push(ptList[0]);
            tempStack.Push(ptList[1]);
            tempStack.Push(ptList[2]);
            for (int i = 3; i < sizePtList; i++)
            {
                while (GraphicsUtility.CheckPointOrder(GraphicsUtility.BeforeTopPoint(ref tempStack),
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


        //find points inside polygons
        internal static List<Point> PointsOnlyInsidePolygon(List<Point2d> polyPts, List<Point2d> testPointsList)
        {
            List<Point> ptList = new List<Point>();
            for (int i = 0; i < testPointsList.Count; i++)
            {
                bool inside = GraphicsUtility.PointInsidePolygonTest(polyPts, testPointsList[i]);
                if (inside)
                {
                    ptList.Add(Point.ByCoordinates(testPointsList[i].X, testPointsList[i].Y));
                }
            }
            return ptList;
        }

        //get only corner and edge cells
        public static List<int> GetCornerAndEdgeCellId( List<List<int>> cellNeighborMatrix){
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
                if (cellNeighborMatrix[i].Count <= 3)  cellIdList.Add(i);   
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

        //gets the cell and forms grid line based on distance provided
        [MultiReturn(new[] { "LowerPoint", "HigherPoint", "GridXLines", "GridYLines", "PointXList", "PointYList" })]
        public static Dictionary<string, object> CreateGridLines(Polygon2d poly, double dim = 10, int mul = 1)
        {
            if (mul < 1) mul = 1;
            double eps = 25, extension = 300;
            double distanceX = mul * dim, distanceY = mul * dim;          

            Dictionary<string, object> lowHighObj = GraphicsUtility.ReturnHighestAndLowestPointofBBox(poly);
            Point2d lowPt = (Point2d)lowHighObj["LowerPoint"];
            Point2d hipt = (Point2d)lowHighObj["HigherPoint"];
            List<Point2d> pointXList = new List<Point2d>(), pointYList = new List<Point2d>();
            double minY = lowPt.Y, minX = lowPt.X, maxY = hipt.Y, maxX = hipt.X;

            double currentXPos = minY + distanceY;
            while(currentXPos <= maxY - distanceY/2)
            {
                pointXList.Add(new Point2d(minX, currentXPos));
                currentXPos += distanceY;
            }
            double currentYPos = minX + distanceX;
            while (currentYPos <= maxX - distanceX/2)
            {
                pointYList.Add(new Point2d(currentYPos,minY));
                currentYPos += distanceX;
            }
            List<Line2d> gridXLines = new List<Line2d>(), gridYLines = new List<Line2d>();
            for(int i=0; i < pointYList.Count; i++)
            {
                Line2d line = new Line2d(pointYList[i], new Point2d(pointYList[i].X, pointYList[i].Y + eps));
                line = LineUtility.ExtendLine(line, extension);
                gridXLines.Add(line);
            }
            for (int i = 0; i < pointXList.Count; i++)
            {
                Line2d line = new Line2d(pointXList[i], new Point2d(pointXList[i].X+ eps, pointXList[i].Y));
                line = LineUtility.ExtendLine(line, extension);
                gridYLines.Add(line);
            }

            return new Dictionary<string, object>
            {
                { "LowerPoint", (lowPt) },
                { "HigherPoint", (hipt) },
                { "GridXLines", (gridXLines) },
                { "GridYLines", (gridYLines) },
                { "PointXList", (pointXList) },
                { "PointYList", (pointYList) }
            };
        }


        //get the cells and make an orthogonal outline poly - not using now, but works best
        [MultiReturn(new[] { "BorderPolyLine", "BorderCellsFound" })]
        internal static Dictionary<string,object> CreateBorder(List<List<int>> cellNeighborMatrix, List<Cell> cellList, bool tag = true)
        {
            if (cellList == null || cellList.Count == 0) return null;
            //get the id of the lowest left cell centroid from all the boundary cells
            List<Point2d> cenPtBorderCells = new List<Point2d>();
            List<Point2d> borderPolyPoints = new List<Point2d>();
            List<Point2d> borderPolyThroughCenter = new List<Point2d>();
            for (int i = 0; i < cellList.Count; i++) cenPtBorderCells.Add(cellList[i].CenterPoint);
            int lowestCellId = GraphicsUtility.ReturnLowestPointFromListNew(cenPtBorderCells);
            Cell currentCell = cellList[lowestCellId];
            Point2d currentCellPoint = currentCell.LeftDownCorner;
            Point2d currentCellCenter = currentCell.CenterPoint;
            int currentIndex = lowestCellId, num = 0;
            List<List<int>> cellNeighborMatrixCopy = new List<List<int>>();
            for (int i = 0; i < cellNeighborMatrix.Count; i++)
            {
                List<int> idsList = new List<int>();
                for(int j=0;j<cellNeighborMatrix[i].Count; j++)
                {
                    idsList.Add(cellNeighborMatrix[i][j]);
                }
                cellNeighborMatrixCopy.Add(idsList);
            }
            List<int> borderCellIdList = GetCornerAndEdgeCellId(cellNeighborMatrixCopy);
            List<bool> isBorderCellList = new List<bool>();
            for (int i = 0; i < cellList.Count; i++)  isBorderCellList.Add(false);
            for (int i = 0; i < borderCellIdList.Count; i++) isBorderCellList[borderCellIdList[i]] = true;

            //order of neighbors : right , up , left , down
            while (num < borderCellIdList.Count)
            {
                borderPolyPoints.Add(currentCellPoint);
                borderPolyThroughCenter.Add(currentCellCenter);
                if (cellNeighborMatrix[currentIndex][0] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][0]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][0]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][0]; // right
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
                    if (!tag)
                    {
                        currentCell = cellList[currentIndex];
                        currentCell.CellAvailable = false;
                        currentCellPoint = currentCell.LeftDownCorner;
                    }
                }
                if (tag)
                {
                    currentCell = cellList[currentIndex];
                    currentCell.CellAvailable = false;
                    currentCellPoint = currentCell.LeftDownCorner;
                }

                currentCellCenter = currentCell.CenterPoint;
                //Trace.WriteLine( + num + "  iteration +++++++++++++++++++++++++++++++++++++");
                num += 1;
            }

            //return new Polygon2d(borderPolyPoints);
            List<Point> ptList = DynamoGeometry.pointFromPoint2dList(borderPolyThroughCenter);
            List<Polygon2d> cellFound = MakeCellsFromGridPoints(ptList, cellList[0].DimX, cellList[0].DimY);
            Polygon2d borderPoly = PolygonUtility.EditToMakeOrthoPoly(new Polygon2d(borderPolyPoints));
            return new Dictionary<string, object>
            {
                { "BorderPolyLine", (borderPoly) },
                { "BorderCellsFound", (cellFound) }
            };
        }


        //get the cells and make an orthogonal outline poly - not using now, but works best
        [MultiReturn(new[] { "BorderPolyLine", "BorderCellsFound","CellNeighborMatrix", "SortedCells"})]
        public static Dictionary<string, object> BorderAndCellNeighborMatrix(Polygon2d poly, double dim, bool tag = true)
        {
            if (!PolygonUtility.CheckPoly(poly)) return null;
            List<Cell> cellsInside = CellsInsidePoly(poly.Points, dim);
            Dictionary<string, object> neighborObject = FormsCellNeighborMatrix(cellsInside);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)neighborObject["CellNeighborMatrix"];
            List<Cell> sortedCells = (List<Cell>)neighborObject["SortedCells"];
            Dictionary<string, object> borderObject = CreateBorder(cellNeighborMatrix, cellsInside, true);
            Polygon2d borderPoly = (Polygon2d)borderObject["BorderPolyLine"];
            List<Polygon2d> cellsFound = (List<Polygon2d>)borderObject["BorderCellsFound"];
            return new Dictionary<string, object>
            {
                { "BorderPolyLine", (borderPoly) },
                { "BorderCellsFound", (cellsFound) },
                { "CellNeighborMatrix", (cellNeighborMatrix) },
                { "SortedCells", (sortedCells) }
            };
        }
        

        //make wholesome polys inside till it meets certain area cover
        [MultiReturn(new[] { "BuildingOutline","WholesomePolys", "SiteArea" , "BuildingOutlineArea", "GroundCoverAchieved", "SortedCells" })]
        public static Dictionary<string, object> FormMakerInSite(Polygon2d borderPoly, Polygon2d origSitePoly, 
            List<Cell> cellList,double groundCoverage = 0.5)
        {
            if (cellList == null) return null;
            if (!PolygonUtility.CheckPoly(borderPoly)) return null;
            bool blockPlaced = false;
            int count = 0, maxTry = 100;
            double areaSite = PolygonUtility.AreaCheckPolygon(origSitePoly), eps = 0.05, areaPlaced = 0;
            if (groundCoverage < eps) groundCoverage = 2 * eps;
            if (groundCoverage > 0.8) groundCoverage = 0.8;
            double groundCoverLow = groundCoverage - eps, groundCoverHigh = groundCoverage + eps;
            Dictionary<string, object> wholeSomeData = new Dictionary<string, object>();
            while (blockPlaced == false && count < maxTry)
            {
                wholeSomeData = PolygonUtility.MakeWholesomeBlockInPoly(borderPoly, groundCoverage);
                List<Polygon2d> polysWhole = (List<Polygon2d>)wholeSomeData["WholesomePolys"];
                areaPlaced = 0;
                for (int i = 0; i < polysWhole.Count; i++) areaPlaced += PolygonUtility.AreaCheckPolygon(polysWhole[i]);
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

        //get list of poly and merge them to make one big poly
        [MultiReturn(new[] { "MergedPoly", "SortedCells" })]
        public static Dictionary<string, object> MergePoly(List<Polygon2d> polyList, List<Cell> cellListInput)
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
            //needs fix , currenly making cell neighbor matrix twice
            //Dictionary<string, object> sortCellObject = SortCellList(cellsInsideList);
            //List<Cell> sortedCells = (List<Cell>)sortCellObject["SortedCells"];
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

        //make cells inside polly
        public static List<Cell> CellsInsidePoly(List<Point2d> outlinePoints, double dim)
        {
            List<Point2d> bboxPoints = ReadData.FromPointsGetBoundingPoly(outlinePoints);
            return GridCellsInsideOutline(bboxPoints, outlinePoints, dim, dim);                     
        }

        //gets cells bt indices in a list
        public static List<Cell> CellsByIndex(List<Cell> cellLists, List<int> cellIdLists)
        {
            List<Cell> cellSelectedList = new List<Cell>();
            for(int i = 0; i < cellIdLists.Count; i++)
            {
                cellSelectedList.Add(cellLists[cellIdLists[i]]);
            }
            return cellSelectedList;
        }

      
        //finds any id issues in the cellNeighborMatrix
        internal static List<bool> FindProblemsInCellNeighbors(List<List<int>> cellNeighborMatrix)
        {
            List<bool> isErrorList = new List<bool>();
            for(int i = 0; i < cellNeighborMatrix.Count; i++)
            {
                bool error = false;
                for(int j = 0; j < cellNeighborMatrix[i].Count; j++)
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
            return 1000*Math.Round(((A * x) + (B * y)),3);
        }
        
        //sorts a list of cells based on a equation
        [MultiReturn(new[] { "SortedCells", "SortedCellIndices", "XYEqualtionList" })]
        public static Dictionary<string, object> SortCellList(List<Cell> cellLists)
        {
            List<Cell> newCellLists = new List<Cell>();
            for (int i = 0; i < cellLists.Count; i++) newCellLists.Add(new Cell(cellLists[i]));
            List<Point2d> cellCenterPtLists = new List<Point2d>();
            for (int i = 0; i < newCellLists.Count; i++) cellCenterPtLists.Add(newCellLists[i].CenterPoint);

            List<double> XYEquationList = new List<double>();
           // double[] XYEquationList = new double[cellCenterPtLists.Count];
            int[] UnsortedIndices = new int[cellCenterPtLists.Count];
            double[] XCordCenterPt = new double[cellCenterPtLists.Count];
            double[] YCordCenterPt = new double[cellCenterPtLists.Count];

            for (int i = 0; i < cellCenterPtLists.Count; i++)
            {
                UnsortedIndices[i] = i;
                XCordCenterPt[i] = cellCenterPtLists[i].X; YCordCenterPt[i] = cellCenterPtLists[i].Y;
                //XYEquationList[i] = EquationforXYLocation(cellCenterPtLists[i].X, cellCenterPtLists[i].Y);
                XYEquationList.Add(EquationforXYLocation(cellCenterPtLists[i].X, cellCenterPtLists[i].Y));
            }
            List<int> SortedIndicesX = new List<int>(), SortedIndicesY = new List<int>(), SortedXYEquationIndices = new List<int>();
            //SortedXYEquationIndices = BasicUtility.Quicksort(XYEquationList, UnsortedIndices, 0, UnsortedIndices.Length - 1);
            SortedXYEquationIndices = BasicUtility.Quicksort(XYEquationList);
            List<double> XYEquationLists = new List<double>();
            for (int k = 0; k < cellCenterPtLists.Count; k++) XYEquationLists.Add(XYEquationList[k]);
            List<Cell> sortedCells = new List<Cell>();
            for(int i = 0; i < SortedXYEquationIndices.Count; i++) sortedCells.Add(newCellLists[SortedXYEquationIndices[i]]);

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
                Cell currentCell = newCellLists[i];
                List<Point2d> neighborPoints = new List<Point2d>();
                List<int> neighborCellIndex = new List<int>();
                Point2d currentCenterPt = cellCenterPtLists[i];                
                Point2d down    = new Point2d(currentCenterPt.X, currentCenterPt.Y - dimY);
                Point2d left    = new Point2d(currentCenterPt.X - dimX, currentCenterPt.Y);
                Point2d up      = new Point2d(currentCenterPt.X, currentCenterPt.Y + dimY);
                Point2d right   = new Point2d(currentCenterPt.X + dimX, currentCenterPt.Y);

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
                if (rightCellIndex > -1)    { neighborCellIndex.Add(rightCellIndex);} else { neighborCellIndex.Add(-1); };
                if (upCellIndex > -1)       { neighborCellIndex.Add(upCellIndex);   } else { neighborCellIndex.Add(-1); }; 
                if (leftCellIndex > -1)     { neighborCellIndex.Add(leftCellIndex); } else { neighborCellIndex.Add(-1); };
                if (downCellIndex > -1)     { neighborCellIndex.Add(downCellIndex); } else { neighborCellIndex.Add(-1); };

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
        #endregion
        
    }
}
