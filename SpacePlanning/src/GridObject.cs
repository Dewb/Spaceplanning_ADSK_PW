using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using stuffer;
using System.Collections;
using System.Diagnostics;
using System.Linq;

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


        // make point2d list which are inside the bounding box
        [MultiReturn(new[] { "PointsInsideOutline", "CellsFromPoints" })]
        public static Dictionary<string, object> GridPointsInsideOutline(List<Point2d> bbox, List<Point2d> outlinePoints, double dimXX, double dimYY)
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
            return new Dictionary<string, object>
            {
                { "PointsInsideOutline", (pointsGrid) },
                { "CellsFromPoints", (cells) }
            };

        }

        // make point2d list which are inside the bounding box
        public static Dictionary<string, object> GridPointsInsideOutlineSingleOut(List<Point2d> bbox, List<Point2d> outlinePoints, double dimXX, double dimYY)
        {
            return GridPointsInsideOutline(bbox, outlinePoints, dimXX, dimYY);
        }

        //make cells on the grids
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
        public static List<Polygon> MakeCellsFromCellObjects(List<Cell> cellList, List<int> indexList = default(List<int>))
        {
            List<Polygon> cellsPolyList = new List<Polygon>();
            List<Point2d> pointsgrid = new List<Point2d>();
            for (int i = 0; i < cellList.Count; i++)
            {
                pointsgrid.Add(cellList[i].CenterPoint);
            }
            double dimX = cellList[0].DimX;
            double dimY = cellList[0].DimY;

            if (indexList == default(List<int>))
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
            for(int i = 0; i < cellNeighborMatrix.Count; i++)
            {
                cellNeighborMatrix[i].Remove(-1);
                if (cellNeighborMatrix[i].Count <= 3) { cellIdList.Add(i); cornerCellList.Add(true); }
                else cornerCellList.Add(false);        
            }

            for (int i = 0; i < cellNeighborMatrix.Count; i++)
            {
                int cornerCount = 0;
                for (int j = 0; j < cellNeighborMatrix[i].Count; j++) if (cellNeighborMatrix[i][j] > -1) { if (cornerCellList[cellNeighborMatrix[i][j]]) cornerCount += 1; }
                if (cornerCount > 1 && cornerCount < 3) cellIdList.Add(i);
            }
            return cellIdList;
            }

        //get border cells and make outline border poly -  not working
        public static Polygon2d MakeBorderPoly(List<List<int>> cellNeighborMatrix, List<int> borderCellIdList, List<Cell> cellList)
        {
           
            //get the id of the lowest left cell centroid from all the boundary cells
            List<Point2d> cenPtBorderCells = new List<Point2d>();
            List<Point2d> borderPolyPoints = new List<Point2d>();
            for (int i = 0; i < borderCellIdList.Count; i++) cenPtBorderCells.Add(cellList[borderCellIdList[i]].CenterPoint);    
            int lowestCellId = GraphicsUtility.ReturnLowestPointFromListNew(cenPtBorderCells);
            Cell currentCell = cellList[borderCellIdList[lowestCellId]];
            Point2d currentCellPoint = currentCell.CenterPoint;
            int currentIndex = lowestCellId;
            int num = 0;
            //order of neighbors : right , up , left , down
            while (num < borderCellIdList.Count)
            {
                borderPolyPoints.Add(currentCellPoint);
                if (cellNeighborMatrix[borderCellIdList[currentIndex]][0] > -1)
                {
                    currentIndex = cellNeighborMatrix[borderCellIdList[currentIndex]][0];
                }
                else if(cellNeighborMatrix[borderCellIdList[currentIndex]][1] > -1)
                {
                    currentIndex = cellNeighborMatrix[borderCellIdList[currentIndex]][1];
                }
                else if (cellNeighborMatrix[borderCellIdList[currentIndex]][2] > -1)
                {
                    currentIndex = cellNeighborMatrix[borderCellIdList[currentIndex]][2];
                }
                else if (cellNeighborMatrix[borderCellIdList[currentIndex]][3] > -1)
                {
                    currentIndex = cellNeighborMatrix[borderCellIdList[currentIndex]][3];
                }
                currentCell = cellList[borderCellIdList[currentIndex]];
                currentCellPoint = currentCell.CenterPoint;
                num += 1;
            }
            
            return new Polygon2d(borderPolyPoints);
            //return lowestCellId;
        }


        //get the cells and make an orthogonal outline poly
        public static Polygon2d MakeOrthoBorderOutline(List<List<int>> cellNeighborMatrix, List<Cell> cellList)
        {
            if (cellList == null || cellList.Count == 0) return null;
            //get the id of the lowest left cell centroid from all the boundary cells
            List<Point2d> cenPtBorderCells = new List<Point2d>();
            List<Point2d> borderPolyPoints = new List<Point2d>();
            for (int i = 0; i < cellList.Count; i++) cenPtBorderCells.Add(cellList[i].CenterPoint);
            int lowestCellId = GraphicsUtility.ReturnLowestPointFromListNew(cenPtBorderCells);
            Cell currentCell = cellList[lowestCellId];
            Point2d currentCellPoint = currentCell.LeftDownCorner;
            int currentIndex = lowestCellId;
            bool downMode = false;
            int num = 0;
            //order of neighbors : right , up , left , down
            while (num < 3000)
            {
                borderPolyPoints.Add(currentCellPoint);
                if (cellNeighborMatrix[currentIndex][0] > -1 && 
                    cellList[cellNeighborMatrix[currentIndex][0]].CellAvailable && downMode == false)
                {
                    currentIndex = cellNeighborMatrix[currentIndex][0]; // right
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.RightDownCorner;
                }
                else if (cellNeighborMatrix[currentIndex][1] > -1 && 
                    cellList[cellNeighborMatrix[currentIndex][1]].CellAvailable && downMode == false)
                {
                    currentIndex = cellNeighborMatrix[currentIndex][1]; // up
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.RightUpCorner;
                }
                else if (cellNeighborMatrix[currentIndex][2] > -1 && 
                    cellList[cellNeighborMatrix[currentIndex][2]].CellAvailable)
                {
                    currentIndex = cellNeighborMatrix[currentIndex][2]; // left
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.LeftUpCorner;
                }
                else if (cellNeighborMatrix[currentIndex][3] > -1 && 
                    cellList[cellNeighborMatrix[currentIndex][3]].CellAvailable)
                {
                    currentIndex = cellNeighborMatrix[currentIndex][3]; // down
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.LeftDownCorner;
                    downMode = true;
                }
                currentCell = cellList[currentIndex];
                currentCell.CellAvailable = false;
                currentCellPoint = currentCell.LeftDownCorner;
                num += 1;
            }

            return new Polygon2d(borderPolyPoints);
        }

        //get the cells and make an orthogonal outline poly
        public static Polygon2d CreateBorder(List<List<int>> cellNeighborMatrix, List<Cell> cellList)
        {
            if (cellList == null || cellList.Count == 0) return null;
            //get the id of the lowest left cell centroid from all the boundary cells
            List<Point2d> cenPtBorderCells = new List<Point2d>();
            List<Point2d> borderPolyPoints = new List<Point2d>();
            for (int i = 0; i < cellList.Count; i++) cenPtBorderCells.Add(cellList[i].CenterPoint);
            int lowestCellId = GraphicsUtility.ReturnLowestPointFromListNew(cenPtBorderCells);
            Cell currentCell = cellList[lowestCellId];
            Point2d currentCellPoint = currentCell.LeftDownCorner;
            int currentIndex = lowestCellId;
            bool downMode = false;
            int num = 0;
            // List<List<int>> cellNeighborMatrixCopy = cellNeighborMatrix.Select(x =>x ).ToList();
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
            while (num < 3000)
            {
                borderPolyPoints.Add(currentCellPoint);
                if (cellNeighborMatrix[currentIndex][0] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][0]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][0]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][0]; // right
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.RightDownCorner;
                }
                else if (cellNeighborMatrix[currentIndex][1] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][1]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][1]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][1]; // up
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.RightUpCorner;
                }
                else if (cellNeighborMatrix[currentIndex][2] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][2]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][2]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][2]; // left
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.LeftUpCorner;
                }
                else if (cellNeighborMatrix[currentIndex][3] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][3]].CellAvailable && isBorderCellList[cellNeighborMatrix[currentIndex][3]])
                {
                    currentIndex = cellNeighborMatrix[currentIndex][3]; // down
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.LeftDownCorner;
                }
                currentCell = cellList[currentIndex];
                currentCell.CellAvailable = false;
                currentCellPoint = currentCell.LeftDownCorner;
                num += 1;
            }

            return new Polygon2d(borderPolyPoints);
        }

        //get the cells and make an orthogonal outline poly
        public static Dictionary<string,object> CreateBorderOutline(List<Cell> cellList, List<int> borderCellIdList)
        {
            if (cellList == null || cellList.Count == 0) return null;
            //get the id of the lowest left cell centroid from all the boundary cells
            List<Point2d> cenPtBorderCells = new List<Point2d>();
            List<Point2d> borderPolyPoints = new List<Point2d>();
            List <Cell> borderCellLists = new List<Cell>();
            for (int i = 0; i < borderCellIdList.Count; i++)
            {
                cenPtBorderCells.Add(cellList[borderCellIdList[i]].CenterPoint);
                borderCellLists.Add(cellList[borderCellIdList[i]]);
            }
            int lowestCellId = GraphicsUtility.ReturnLowestPointFromListNew(cenPtBorderCells);
            Cell currentCell = cellList[lowestCellId];
            Point2d currentCellPoint = currentCell.LeftDownCorner; 
            int currentIndex = lowestCellId;
            bool downMode = false;
            int num = 0;
            Dictionary<string,object> cellInformation = FormsCellNeighborMatrix(borderCellLists);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)cellInformation["CellNeighborMatrix"];
            //order of neighbors : right , up , left , down
            while (num < 50)
            {
                borderPolyPoints.Add(currentCellPoint);
                if (cellNeighborMatrix[currentIndex][0] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][0]].CellAvailable)
                {
                    currentIndex = cellNeighborMatrix[currentIndex][0]; // right
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.RightDownCorner;
                }
                else if (cellNeighborMatrix[currentIndex][1] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][1]].CellAvailable)
                {
                    currentIndex = cellNeighborMatrix[currentIndex][1]; // up
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.RightUpCorner;
                }
                else if (cellNeighborMatrix[currentIndex][2] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][2]].CellAvailable)
                {
                    currentIndex = cellNeighborMatrix[currentIndex][2]; // left
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.LeftUpCorner;
                }
                else if (cellNeighborMatrix[currentIndex][3] > -1 &&
                    cellList[cellNeighborMatrix[currentIndex][3]].CellAvailable)
                {
                    currentIndex = cellNeighborMatrix[currentIndex][3]; // down
                    //currentCell = cellList[currentIndex];
                    //currentCell.CellAvailable = false;
                    //currentCellPoint = currentCell.LeftDownCorner;
                  
                }
                currentCell = cellList[currentIndex];
                currentCell.CellAvailable = false;
                currentCellPoint = currentCell.LeftDownCorner;
                num += 1;
            }

            return new Dictionary<string, object>
            {
                { "BorderOutline", (new Polygon2d(borderPolyPoints)) },
                { "LowestCellId", ( lowestCellId) },
                { "CellNeighborMatrixForBorderCells" , (cellNeighborMatrix) }
            };
        }
    



        //get list of poly and merge them to make one big poly
        public static Polygon2d MergePoly(List<Polygon2d> polyList, List<Cell> cellListInput)
        {
            List<Cell> cellList = cellListInput.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY)).ToList(); // example of deep copy
            List<Cell> cellsInsideList = new List<Cell>();
            for(int i = 0; i < polyList.Count; i++)
            {
                for(int j = 0; j < cellList.Count; j++)
                {
                    if (GraphicsUtility.PointInsidePolygonTest(polyList[i], cellList[j].CenterPoint))
                        cellsInsideList.Add(cellList[j]);
                }
            }
            Dictionary<string, object> cellNeighborData = FormsCellNeighborMatrix(cellsInsideList);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)cellNeighborData["CellNeighborMatrix"];
            string foo = "";
            return MakeOrthoBorderOutline(cellNeighborMatrix, cellsInsideList);           
          
        }


        //get list of poly and merge them to make one big poly
        public static Dictionary<string,object> MergePolyDict(List<Polygon2d> polyList, List<Cell> cellListInput)
        {
            List<Cell> cellList = cellListInput.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY)).ToList(); // example of deep copy
            List<Cell> cellsInsideList = new List<Cell>();
            for (int i = 0; i < polyList.Count; i++)
            {
                for (int j = 0; j < cellList.Count; j++)
                {
                    if (GraphicsUtility.PointInsidePolygonTest(polyList[i], cellList[j].CenterPoint))
                        cellsInsideList.Add(cellList[j]);
                }
            }
            Dictionary<string, object> cellNeighborData = FormsCellNeighborMatrix(cellsInsideList);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)cellNeighborData["CellNeighborMatrix"];
            string foo = "";
            Polygon2d mergePoly = MakeOrthoBorderOutline(cellNeighborMatrix, cellsInsideList);
            return new Dictionary<string, object>
            {
                { "MergePoly", (mergePoly) },
                { "CellLists", ( cellsInsideList) }
            };
        }

        //make cells inside polly
        public static List<Cell> CellsInsidePoly(List<Point2d> outlinePoints, double dim)
        {
            List<Point2d> bboxPoints = ReadData.FromPointsGetBoundingPoly(outlinePoints);
            Dictionary<string, object> cellInformation = GridPointsInsideOutline(bboxPoints, outlinePoints, dim, dim);
            return(List<Cell>)cellInformation["CellsFromPoints"];            
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

        //make cells and then make ortho border
        public static Dictionary<string,object> MakeOrthogonalCornersPoly(List<Point2d> outlinePoints,double dim)
        {
            List<Point2d> bboxPoints = ReadData.FromPointsGetBoundingPoly(outlinePoints);
            Dictionary<string,object> cellInformation = GridPointsInsideOutline(bboxPoints, outlinePoints, dim, dim);
            List<Cell> cellList1 = (List<Cell>)cellInformation["CellsFromPoints"];
            List<Cell> cellList2 = cellList1.Select(x => new Cell(x.CenterPoint,x.DimX,x.DimY)).ToList(); // example of deep copy
            List<Cell> cellList3 = cellList1.Select(x => new Cell(x.CenterPoint, x.DimX, x.DimY)).ToList(); // example of deep copy
            List<Point2d> pointsInsideOutline = (List<Point2d>)cellInformation["PointsInsideOutline"];
            Dictionary<string, object> cellNeighborData = FormsCellNeighborMatrix(cellList1);
            List<List<int>> cellNeighborMatrix = (List <List<int>>)cellNeighborData["CellNeighborMatrix"];
            Polygon2d orthoBorder =  MakeOrthoBorderOutline(cellNeighborMatrix, cellList2);
            return new Dictionary<string, object>
            {
                { "OrthoBorderOutline", (orthoBorder) },
                { "CellLists", ( cellList3) }
            };
        }


        //make cell neighbor matrix
        [MultiReturn(new[] { "CellNeighborMatrix", "XYEqualtionList" })]
        public static Dictionary<string, object> FormsCellNeighborMatrix(List<Cell> cellLists, int tag = 1)
        {
            List<List<int>> cellNeighborMatrix = new List<List<int>>();
            List<Cell> newCellLists = new List<Cell>();
            for (int i = 0; i < cellLists.Count; i++)
            {
                Cell cellItem = new Cell(cellLists[i]);
                newCellLists.Add(cellItem);
            }
            //cellLists.Clear();
            List<Point2d> cellCenterPtLists = new List<Point2d>();
            for (int i = 0; i < newCellLists.Count; i++)
            {
                cellCenterPtLists.Add(newCellLists[i].CenterPoint);
            }

            double[] XYEquationList = new double[cellCenterPtLists.Count];
            
            double A = 100.0;
            double B = 1.0;
            int[] UnsortedIndices = new int[cellCenterPtLists.Count];
            double[] XCordCenterPt = new double[cellCenterPtLists.Count];
            double[] YCordCenterPt = new double[cellCenterPtLists.Count];
           
            for (int i = 0; i < cellCenterPtLists.Count; i++)
            {

                UnsortedIndices[i] = i;
                XCordCenterPt[i] = cellCenterPtLists[i].X;
                YCordCenterPt[i] = cellCenterPtLists[i].Y;
                double value = (A * cellCenterPtLists[i].X) + (B * cellCenterPtLists[i].Y);
                XYEquationList[i] = value;
           
            }
            List<int> SortedIndicesX = new List<int>();
            List<int> SortedIndicesY = new List<int>();
            List<int> SortedXYEquationIndices = new List<int>();
            SortedXYEquationIndices = BasicUtility.Quicksort(XYEquationList, UnsortedIndices, 0, UnsortedIndices.Length - 1);
            double dimX = newCellLists[0].DimX;
            double dimY = newCellLists[0].DimY;
            List<List<Point2d>> cellNeighborPoint2d = new List<List<Point2d>>();
            
            List<double> SortedXYEquationValues = new List<double>();
            for (int i = 0; i < SortedXYEquationIndices.Count; i++)
            {
                SortedXYEquationValues.Add(XYEquationList[SortedXYEquationIndices[i]]);
            }

            List<double> XYEquationLists = new List<double>();
            for (int k = 0; k < cellCenterPtLists.Count; k++)
            {
                XYEquationLists.Add(XYEquationList[k]);
            }
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
                double downValue = (A * currentCenterPt.X) + (B * (currentCenterPt.Y - dimY));               
                int downCellIndex = BasicUtility.BinarySearchDouble(XYEquationLists, downValue);
                //find index of left cell
                double leftValue = (A * (currentCenterPt.X-dimX)) + (B * (currentCenterPt.Y));
                int leftCellIndex = BasicUtility.BinarySearchDouble(XYEquationLists, leftValue);
                //find index of up cell
                double upValue = (A * (currentCenterPt.X)) + (B * (currentCenterPt.Y+dimY));
                int upCellIndex = BasicUtility.BinarySearchDouble(XYEquationLists, upValue);
                //find index of up cell
                double rightValue = (A * (currentCenterPt.X+dimX)) + (B * (currentCenterPt.Y));
                int rightCellIndex = BasicUtility.BinarySearchDouble(XYEquationLists, rightValue);


                //RULD - right, up, left, down | adding -1 means the cell does not have any neighbor at that spot
                if (rightCellIndex > -1)    { neighborCellIndex.Add(rightCellIndex);} else { neighborCellIndex.Add(-1); };
                if (upCellIndex > -1)       { neighborCellIndex.Add(upCellIndex);   } else { neighborCellIndex.Add(-1); }; 
                if (leftCellIndex > -1)     { neighborCellIndex.Add(leftCellIndex); } else { neighborCellIndex.Add(-1); };
                if (downCellIndex > -1)     { neighborCellIndex.Add(downCellIndex); } else { neighborCellIndex.Add(-1); };



                neighborPoints.Add(right);
                neighborPoints.Add(up);
                neighborPoints.Add(left);
                neighborPoints.Add(down);     
                cellNeighborPoint2d.Add(neighborPoints);
                cellNeighborMatrix.Add(neighborCellIndex);
            }
          
            return new Dictionary<string, object>
            {
                { "CellNeighborMatrix", (cellNeighborMatrix) },
                { "XYEqualtionList", ( XYEquationLists) }
            };
        }


        // test binary search
        public static List<int> TestBinarySearch(List<int> inp, int key)
        {
            List<int> indices = new List<int>();
            List<int> inpList = new List<int>();
            for(int i = 0; i < inp.Count; i++)
            {
                inpList.Add(inp[i]);
            }
            int value = 0;
            int prevValue = 10000000;
            int m = 1;
            while (value != -1) {
                value = BasicUtility.BinarySearch(inpList, key);                
                if (value > -1)
                {
                    inpList.RemoveAt(value);
                    if (value >= prevValue)
                    {
                        indices.Add(value + 1);
                        m += 1;
                    }
                    else indices.Add(value);
                }
                prevValue = value;

            }// end of while loop
            return indices;
        }
 

        // test quick sort algorithm
        public static List<int> TestQuickSort(double[] main = null, int[] index = null, int tag=1)
        {
            int left = 0;
            int right = index.Length - 1;
            int[] newIndex = new int[index.Length];
            for(int i = 0; i < index.Length; i++)
            {
                newIndex[i] = index[i];
            }

            return BasicUtility.Quicksort(main, newIndex, left, right); 
        }





    }
}
