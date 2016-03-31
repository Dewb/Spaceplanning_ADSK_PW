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
        public static GridObject BySiteOutline(List<Point2d> siteOutline, List<Point2d> siteBoundingBox, double dimensionX, double dimensionY)
        {
            return new GridObject(siteOutline,siteBoundingBox,dimensionX,dimensionY);
        }

        //private functions
        internal void addSome()
        {

        }

        //public functions

        
        // can be deleted later        /////////////////////////
        //MAKE POINT2D LIST OF POINTS INSIDE THE BOUNDING BOX - points are stored clockwise
        internal static List<Point2d> GridPointsFromBBoxNew(List<Point2d> bbox, double dimXX, double dimYY,double a,double b)
        {
            // index 0 stores min point 2 stores the max point
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

        // 0         /////////////////////////
        //MAKE POINT2D LIST OF POINTS INSIDE THE BOUNDING BOX - points are stored clockwise
        [MultiReturn(new[] { "PointsInsideOutline", "CellsFromPoints" })]
        public static Dictionary<string, object> GridPointsInsideOutline(List<Point2d> bbox, List<Point2d> outlinePoints, double dimXX, double dimYY)
        {
            // index 0 stores min point 2 stores the max point
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


            //return pointsGrid;
            return new Dictionary<string, object>
            {
                { "PointsInsideOutline", (pointsGrid) },
                { "CellsFromPoints", (cells) }
            };

        }


        // 1     could be deleted       //////////////////////////////
        internal static List<Point> PointInsidePolygon(List<Point> pointGrid, List<Point> pointOnPoly)
        {
            List<Point> pointInsideList = new List<Point>();

            Polygon pol = Polygon.ByPoints(pointOnPoly);

            for (int i = 0; i < pointGrid.Count; i++)
            {
                if (pol.ContainmentTest(pointGrid[i]))
                {
                    pointInsideList.Add(pointGrid[i]);
                }
            }

            pol.Dispose();

            return pointInsideList;
        }



        //2A        //////////////////////////
        public static List<List<Polygon>> MakeCellsFromCellIndex()
        {
            List<List<Polygon>> polygonLists = new List<List<Polygon>>();

            return polygonLists;
        }



        // 2        //////////////////////////
        //MAKE CELLS ON THE GRIDS
        public static List<Polygon> MakeCellsFromGridPoints(List<Point> pointsgrid,double dimX,double dimY)
        {
            List<Polygon> cellsPolyList = new List<Polygon>();
            List<Cell> cellList = new List<Cell>();
            for (int i = 0; i < pointsgrid.Count; i++)
            {
                // Cell cell = Cell.ByCenterPoint(pointsgrid[i], dimX, dimY);
                //cellList.Add(cell);

                List<Point> ptList = new List<Point>();

                double a = pointsgrid[i].X - (dimX / 2);
                double b = pointsgrid[i].Y - (dimY / 2);
                Point pt = Point.ByCoordinates(a, b);
                ptList.Add(pt);

                a = pointsgrid[i].X - (dimX / 2);
                b = pointsgrid[i].Y + (dimY / 2);
                pt = Point.ByCoordinates(a, b);
                ptList.Add(pt);

                a = pointsgrid[i].X + (dimX / 2);
                b = pointsgrid[i].Y + (dimY / 2);
                pt = Point.ByCoordinates(a, b);
                ptList.Add(pt);

                a = pointsgrid[i].X + (dimX / 2);
                b = pointsgrid[i].Y - (dimY / 2);
                pt = Point.ByCoordinates(a, b);
                ptList.Add(pt);


                Polygon pol = Polygon.ByPoints(ptList);
                cellsPolyList.Add(pol);
            }


            return cellsPolyList;
        }


        //MAKE GRAHAMS SCAN CONVEX HULL FROM AN INPUT LIST OF POINTS
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

        

        //3    could ve deleted     ////////////////////
        //CODE TO SPIT POINTS WHICH PASS THE CONTAINMENT TEST OF A POLYGON
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


        //4  /////////////////////////
        //CODE TO FIND NEIGHBORS OF EACH CELL
        public static List<List<int>> MakeCellNeighborMatrix(List<Cell> cellLists, int tag =1)
        {
            List<List<int>> cellNeighborMatrix = new List<List<int>>();

            //0 make new cell Lists
            List<Cell> newCellLists = new List<Cell>();
            for (int i = 0; i < cellLists.Count; i++)
            {
                Cell cellItem = new Cell(cellLists[i]);
                newCellLists.Add(cellItem);
            }
            cellLists.Clear();

            //1 get the center points from cellLists
            List<Point2d> cellCenterPtLists = new List<Point2d>();
            for (int i = 0; i < newCellLists.Count; i++)
            {
                cellCenterPtLists.Add(newCellLists[i].CenterPoint);
            }

            //2 get the lowest point cell index, with least x value and least y value
            int LowestPointIndex = GraphicsUtility.ReturnLowestPointFromList(cellCenterPtLists);

            //2b - create indices 
            //List<int> UnsortedIndices = new List<int>();
            int[] UnsortedIndices = new int[cellCenterPtLists.Count];
            double[] XCordCenterPt = new double[cellCenterPtLists.Count];
            double[] YCordCenterPt = new double[cellCenterPtLists.Count];
            for (int i = 0; i < cellCenterPtLists.Count; i++)
            {
                
                UnsortedIndices[i] = i;
                XCordCenterPt[i] = cellCenterPtLists[i].X;
                YCordCenterPt[i] = cellCenterPtLists[i].Y;
            }

            //3xy sort cellIndices based on X coord and based on Y coord
            List<int> SortedIndicesX = new List<int>();
            List<int> SortedIndicesY = new List<int>();
            SortedIndicesX = BasicUtility.quicksort(XCordCenterPt, UnsortedIndices, 0, UnsortedIndices.Length - 1);
            SortedIndicesY = BasicUtility.quicksort(YCordCenterPt, UnsortedIndices, 0, UnsortedIndices.Length - 1);

            for(int i = 0; i < cellCenterPtLists.Count; i++)
            {
                //Trace.WriteLine("Sorted X Cell Indices are : " + SortedIndicesX[i]);
                //Trace.WriteLine("Sorted Y Cell Indices are : " + SortedIndicesY[i]);
            }
            
            

            //4 iterate through all the cells and find closest neighbors ( by Binary Search )


            //5 return the cellNeighborMatrix and also term each cells Type(corner,edge, core)
            return cellNeighborMatrix;
        }

        // can be deleted later on
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
                
                Trace.WriteLine("InpList Count is : " + inpList.Count);
                for (int i = 0; i < inpList.Count; i++)
                {
                    Trace.WriteLine("InpList Items are : " + inpList[i]);
                }
               
                value = BasicUtility.BinarySearch(inpList, key);
                Trace.WriteLine("Prev Value Obtained was : " + prevValue);
                Trace.WriteLine("Value Obtained is : " + value);
                if (value > -1)
                {
                   
                    inpList.RemoveAt(value);
                    if(value >= prevValue)
                    {
                        indices.Add(value + m);
                        m += 1;

                    }
                    else
                    {
                        indices.Add(value);
                        
                    }
                   

                }


                prevValue = value;
                Trace.WriteLine("-----------------------------------");

            }// end of while loop
            return indices;
        }

        // can be deleted later on
        public static List<int> TestQuickSort(double[] main = null, int[] index = null, int tag=1)
        {
            int left = 0;
            int right = index.Length - 1;
            //index = new int[5] { 0, 1, 2, 3, 4 };
            //main = new float[5] { 10, 21, 2, 8, 1 };
            int[] newIndex = new int[index.Length];

            for(int i = 0; i < index.Length; i++)
            {
                newIndex[i] = index[i];
            }


            List<int> modifiedIndices = BasicUtility.quicksort(main, newIndex, left,right);

            return modifiedIndices;
        }





    }
}
