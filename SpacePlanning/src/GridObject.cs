using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using stuffer;
using System.Collections;
using System.Diagnostics;

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
        internal static List<Point2d> GridPointsFromBBoxNew(List<Point2d> bbox, double dimXX, double dimYY,double a,double b)
        {
            List<Point2d> pointsGrid = new List<Point2d>();
            Range2d xyRange = ReadData.FromPoint2dGetRange2D(bbox);
            double xDistance = xyRange.Xrange.Span;
            double yDistance = xyRange.Yrange.Span;

            int numPtsX = Convert.ToInt16(Math.Floor(xDistance / dimXX));
            int numPtsY = Convert.ToInt16(Math.Floor(yDistance / dimYY));

            double diffX = xDistance - (dimXX * numPtsX);
            double diffY = yDistance - (dimYY * numPtsY);

            double posX =   bbox[0].X + diffX*a;
            double posY =   bbox[0].Y + diffY*b;
            for (int i = 0; i < numPtsX; i++)
            {
                for(int j = 0; j < numPtsY; j++)
                {
                    pointsGrid.Add(new Point2d(posX, posY));
                    posY += dimYY;
                }
                posX += dimXX;
                posY = bbox[0].Y + diffY*b;
            }
            return pointsGrid;

        }
        
        
        // make point2d list which are inside the bounding box
        [MultiReturn(new[] { "PointsInsideOutline", "CellsFromPoints" })]
        public static Dictionary<string, object> GridPointsInsideOutline(List<Point2d> bbox, List<Point2d> outlinePoints, double dimXX, double dimYY)
        {
            List<Point2d> pointsGrid = new List<Point2d>();
            List<Cell> cells = new List<Cell>();
            Range2d xyRange = ReadData.FromPoint2dGetRange2D(bbox);

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
        public static List<Polygon2d> MakeCellsFromGridPoints(List<Point> pointsgrid,double dimX,double dimY)
        {
            List<Point2d> pt2dList = new List<Point2d>();
            for(int i = 0; i < pointsgrid.Count; i++)
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
                Point2d pt = Point2d.ByCoordinates(a,b);
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
            if ( indexList == null)
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
            for(int i=3; i < sizePtList; i++)
            {
                while( GraphicsUtility.CheckPointOrder(GraphicsUtility.BeforeTopPoint(ref tempStack),
                    (Point2d)tempStack.Peek(),ptList[i]) != 2 && tempStack.Count > 1)
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
            for(int i = 0; i < testPointsList.Count; i++)
            {
               bool inside = GraphicsUtility.PointInsidePolygonTest(polyPts, testPointsList[i]);
                if (inside)
                {
                    ptList.Add(Point.ByCoordinates(testPointsList[i].X, testPointsList[i].Y));
                }
            }
            return ptList;
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
            cellLists.Clear();
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
            SortedXYEquationIndices = BasicUtility.quicksort(XYEquationList, UnsortedIndices, 0, UnsortedIndices.Length - 1);
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


                if (downCellIndex > -1) { neighborCellIndex.Add(downCellIndex); };
                if (leftCellIndex > -1) { neighborCellIndex.Add(leftCellIndex); };
                if (upCellIndex > -1)   { neighborCellIndex.Add(upCellIndex);   };
                if (rightCellIndex > -1){ neighborCellIndex.Add(rightCellIndex);};

                neighborPoints.Add(down);
                neighborPoints.Add(left);
                neighborPoints.Add(up);
                neighborPoints.Add(right);
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

            return BasicUtility.quicksort(main, newIndex, left, right); 
        }





    }
}
