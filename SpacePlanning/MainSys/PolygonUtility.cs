using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;


namespace SpacePlanning
{
    class PolygonUtility
    {

        //checker function - can be discarded later
        internal static List<Point2d> CheckLowest_HighestPoint(Polygon2d poly)
        {
            List<Point2d> returnPts = new List<Point2d>();
            List<Point2d> ptList = poly.Points;
            int highPtInd = GraphicsUtility.ReturnHighestPointFromListNew(ptList);
            int lowPtInd = GraphicsUtility.ReturnLowestPointFromListNew(ptList);
            returnPts.Add(ptList[lowPtInd]);
            returnPts.Add(ptList[highPtInd]);

            return returnPts;

        }


        // not using now can be discarded
        internal static List<Point2d> organizePointToMakePoly(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {
            List<Point2d> sortedPoint = new List<Point2d>();
            List<Point2d> unsortedPt = new List<Point2d>();
            // make two unsorted point lists
            for (int i = 0; i < pIndex.Count; i++)
            {
                unsortedPt.Add(poly[pIndex[i]]);
            }
            unsortedPt.AddRange(intersectedPoints);
            //compute lowest and highest pts
            Point2d lowPt = unsortedPt[GraphicsUtility.ReturnLowestPointFromListNew(unsortedPt)];
            Point2d hiPt = unsortedPt[GraphicsUtility.ReturnHighestPointFromListNew(unsortedPt)];
            //form a line2d between them
            Line2d lineHiLo = new Line2d(lowPt, hiPt);

            //make left and right points based on the line
            List<Point2d> ptOnA = new List<Point2d>();
            List<Point2d> ptOnB = new List<Point2d>();
            for (int i = 0; i < unsortedPt.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(lineHiLo, unsortedPt[i]);
                if (check)
                {
                    //pIndexA.Add(i);
                    ptOnA.Add(unsortedPt[i]);
                }
                else
                {
                    //pIndexB.Add(i);
                    ptOnB.Add(unsortedPt[i]);
                }
            }

            //sort ptOnA and ptOnB individually
            List<Point2d> SortedPtA = GraphicsUtility.SortPointsByDistanceFromPoint(ptOnA,
                         lowPt);
            List<Point2d> SortedPtB = GraphicsUtility.SortPointsByDistanceFromPoint(ptOnB,
                         lowPt);
            SortedPtB.Reverse();
            //add the sorted ptOnA and ptOnB
            sortedPoint.AddRange(SortedPtA);
            sortedPoint.AddRange(SortedPtB);
            return sortedPoint;
        }

        //can be discarded cleans duplicate points and returns updated list
        internal static List<Point2d> CleanDuplicatePoint2d(List<Point2d> ptListUnclean)
        {
            List<Point2d> cleanList = new List<Point2d>();
            List<double> dummyList = new List<double>();
            List<bool> isDuplicate = new List<bool>();
            double eps = 1;
            bool duplicate = false;



            for (int i = 0; i < ptListUnclean.Count; i++)
            {
                duplicate = false;
                double count = 0;
                for (int j = 0; j < ptListUnclean.Count; j++)
                {

                    if (j == i)
                    {
                        continue;
                    }

                    if (ptListUnclean[i].X - eps < ptListUnclean[j].X && ptListUnclean[j].X < ptListUnclean[i].X + eps)
                    {
                        if (ptListUnclean[i].Y - eps < ptListUnclean[j].Y && ptListUnclean[j].Y < ptListUnclean[i].Y + eps)
                        {
                            count += 1;
                        }
                    }

                    if (count > 1)
                    {
                        duplicate = true;

                        //continue;
                    }


                }

                dummyList.Add(count);



            }
            for (int i = 0; i < ptListUnclean.Count; i++)
            {
                //Trace.WriteLine("count here is : " + dummyList[i]);
                if (dummyList[i] < 2)
                {
                    cleanList.Add(ptListUnclean[i]);
                }

            }
            return cleanList;
        }


        //cleans duplicate points and returns updated list
        internal static List<Point2d> CleanDuplicatePoint2dNew(List<Point2d> ptListUnclean)
        {
            List<Point2d> cleanList = new List<Point2d>();
            List<double> exprList = new List<double>();
            double a = 45, b = 65;
            for (int i = 0; i < ptListUnclean.Count; i++)
            {
                double expr = a * ptListUnclean[i].X + b * ptListUnclean[i].Y;
                exprList.Add(expr);
            }

            var dups = exprList.GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

            List<double> distinct = exprList.Distinct().ToList();
            for (int i = 0; i < distinct.Count; i++)
            {
                double dis = distinct[i];
                for (int j = 0; j < exprList.Count; j++)
                {
                    if (dis == exprList[j])
                    {
                        cleanList.Add(ptListUnclean[j]);
                        break;
                    }
                }
            }
            return cleanList;

        }
        //trying new way to sort points clockwise
        internal static List<Point2d> DoSortClockwise(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {
            if (intersectedPoints == null || intersectedPoints.Count == 0)
            {
                return null;

            }
            if (intersectedPoints.Count > 2)
            {

                //Trace.WriteLine("Wow found  " + intersectedPoints.Count + " intersection points!!!!!!!!!!!!!!!");
                List<Point2d> cleanedPtList = CleanDuplicatePoint2dNew(intersectedPoints);
                //Trace.WriteLine("After Cleaning found  " + cleanedPtList.Count + " intersection points!!!!!!!!!!!!!!!");
                //intersectedPoints = GraphicsUtility.CleanDuplicatePoint2d(intersectedPoints);
                //return null;

            }
            /*

            if (intersectedPoints.Count < 2)
            {
                Trace.WriteLine("Returning null as less than 1 points");
                //return null;
            }
            List<Point2d> pointList = new List<Point2d>();
            List<Point2d> mergedPoints = new List<Point2d>();
            
            for (int i = 0; i < pIndex.Count; i++)
            {
                mergedPoints.Add(poly[pIndex[i]]);
            }
            reference = GraphicsUtility.CentroidInPointLists(mergedPoints);
            mergedPoints.AddRange(intersectedPoints);

            //mergedPoints.Sort((a, b) => GraphicsUtility.Angle(a, reference).CompareTo(GraphicsUtility.Angle(b, reference)));
            //mergedPoints.Sort(new Comparison<Point2d>(GraphicsUtility.SortCornersClockwise));
            //return mergedPoints;
            */
            return makePolyPointsStraight(poly, intersectedPoints, pIndex);
        }

        internal static List<Point2d> makePolyPointsStraight(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {

            if (intersectedPoints.Count < 2)
            {
                return null;
            }

            //List<Point2d> intersectedPoints = GraphicsUtility.PointUniqueChecker(intersectedPointsUnclean);
            //Trace.WriteLine("Intersected Points Length are : " + intersectedPoints.Count);
            //bool isUnique = intersectedPoints.Distinct().Count() == intersectedPoints.Count();
            //Trace.WriteLine("Intersected Point List is unique ?   " + isUnique);
            List<Point2d> pt = new List<Point2d>();
            bool added = false;

            int a = 0;
            int b = 1;
            if (intersectedPoints.Count > 2)
            {
                //Trace.WriteLine("Intersected pnts are more than one " + intersectedPoints.Count);
                //a = 0;
                //b = intersectedPoints.Count - 1;
            }

            //Trace.WriteLine("Intersected Points Length are : " + intersectedPoints.Count);
            //Trace.WriteLine("a and b are : " + a + "  ,  " + b );
            //Trace.WriteLine("Index Point length is :  " + pIndex.Count);
            for (int i = 0; i < pIndex.Count - 1; i++)
            {
                pt.Add(poly[pIndex[i]]);
                //enter only when indices difference are more than one and intersected points are not added yet
                if (Math.Abs(pIndex[i] - pIndex[i + 1]) > 1 && added == false)
                {
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                        poly[pIndex[i]]);
                    pt.Add(intersNewList[a]);
                    pt.Add(intersNewList[b]);
                    //Trace.WriteLine("Added Intersect Before for PtA");
                    added = true;
                }

                if (i == (pIndex.Count - 2) && added == false)
                {
                    pt.Add(poly[pIndex[i + 1]]);
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                             poly[pIndex[i + 1]]);
                    pt.Add(intersNewList[a]);
                    pt.Add(intersNewList[b]);
                    added = true;
                }

                else if (i == (pIndex.Count - 2) && added == true)
                {
                    pt.Add(poly[pIndex[i + 1]]);
                }
            }
            //Trace.WriteLine("Point Returned Length : " + pt.Count);
            //Trace.WriteLine("I++++++++++++++++++++++++++++++++++++++++++");
            return pt;
        }



    }
}
