using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;

namespace SpacePlanning
{
    public class SplitObject
    {



        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint" })]
        public static Dictionary<string, object> RecursiveSplitPoly(Polygon2d poly, double ratioA = 0.5, int recompute = 1)
        {

            /*PSUEDO CODE:
            stack polylist = new polylist
            polylist.push(input poly)
            while(polyList != empty )
            {

                poly = polyList[i]
                splittedpoly = splitpolyintowtwocheck(poly, ratio, extents, dir)
                if (splipoly[0].area > thresharea)
                polylist.addrange(splitpoly)
                i += 1
                count += 1

            }
                
            */
            double ratio = 0.5;

            List<Polygon2d> polyList = new List<Polygon2d>();
            List<Point2d> pointsList = new List<Point2d>();
            List<double> areaList = new List<double>();
            Stack<Polygon2d> polyRetrieved = new Stack<Polygon2d>();
            polyRetrieved.Push(poly);
            int count = 0;
            //int thresh = 10;
            //double areaThreshold = GraphicsUtility.AreaPolygon2d(poly.Points)/10;
            double areaThreshold = 1000;
            int dir = 1;
            List<Polygon2d> polyAfterSplit = null;
            Dictionary<string, object> splitReturn = null;
            Polygon2d currentPoly;
            Random rand = new Random();
            double maximum = 0.9;
            double minimum = 0.3;
            while (polyRetrieved.Count > 0)
            {
                //double mul = rand.NextDouble() * (maximum - minimum) + minimum;
                //ratio *= mul;
                ratio = rand.NextDouble() * (maximum - minimum) + minimum;
                currentPoly = polyRetrieved.Pop();
                try
                {
                    splitReturn = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                    polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                }
                catch (Exception)
                {
                    //toggle dir between 0 and 1
                    dir = BasicUtility.toggleInputInt(dir);
                    splitReturn = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                    if (splitReturn == null)
                    {
                        //Trace.WriteLine("Could Not Split due to Aspect Ration Problem : Sorry");
                        continue;
                    }
                    polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    //throw;
                }
                List<List<Point2d>> pointsOnPoly = (List<List<Point2d>>)splitReturn["EachPolyPoint"];
                double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                if (area1 > areaThreshold)
                {
                    polyRetrieved.Push(polyAfterSplit[0]);
                    polyList.Add(polyAfterSplit[0]);
                    pointsList.AddRange(pointsOnPoly[0]);
                    areaList.Add(area1);

                }
                if (area2 > areaThreshold)
                {
                    polyRetrieved.Push(polyAfterSplit[1]);
                    polyList.Add(polyAfterSplit[1]);
                    pointsList.AddRange(pointsOnPoly[1]);
                    areaList.Add(area2);

                }
                //pointsList.AddRange(pointsOnPoly);
                //polyList.AddRange(polyAfterSplit);
                //pointsList.AddRange(pointsOnPoly);
                //areaList.Add(area1);
                //areaList.Add(area2);


                //toggle dir between 0 and 1
                dir = BasicUtility.toggleInputInt(dir);
                count += 1;
            }

            //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) }
            };


        }

        internal static List<Polygon2d> BasicSplitWrapper(Polygon2d currentPoly, double ratio, int dir)
        {

            Dictionary<string, object> splitReturned = null;
            List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
            try
            {
                splitReturned = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
            }
            catch (Exception)
            {
                //toggle dir between 0 and 1
                dir = BasicUtility.toggleInputInt(dir);
                splitReturned = BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                if (splitReturned == null)
                {
                    //Trace.WriteLine("Split Polys did not work");
                    return null;
                }
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
                //throw;
            }

            return polyAfterSplitting;
        }


        internal static List<Polygon2d> EdgeSplitWrapper(Polygon2d currentPoly, double distance, int dir)
        {

            Dictionary<string, object> splitReturned = null;
            List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
            try
            {
                splitReturned = SplitByDistance(currentPoly, distance, dir);
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
            }
            catch (Exception)
            {
                //toggle dir between 0 and 1
                dir = BasicUtility.toggleInputInt(dir);
                splitReturned = SplitByDistance(currentPoly, distance, dir);
                if (splitReturned == null)
                {
                    //Trace.WriteLine("Split Polys did not work");
                    return null;
                }
                polyAfterSplitting = (List<Polygon2d>)splitReturned["PolyAfterSplit"];

                if(polyAfterSplitting[0] == null && polyAfterSplitting[1] == null)
                {
                    return null;
                }
                //throw;
            }

            

            return polyAfterSplitting;
        }



        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "DepartmentNames" })]
        public static Dictionary<string, object> DeptSplitRefined(Polygon2d poly, List<DeptData> deptData, int recompute = 1)
        {
            

            List<List<Polygon2d>> AllDeptPolys = new List<List<Polygon2d>>();
            List<string> AllDepartmentNames = new List<string>();
            Stack<Polygon2d> leftOverPoly = new Stack<Polygon2d>();


            SortedDictionary<double, DeptData> sortedD = new SortedDictionary<double, DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                double area = deptData[i].AreaEachDept();
                DeptData deptD = deptData[i];
                sortedD.Add(area, deptD);

            }



            List<DeptData> sortedDepartmentData = new List<DeptData>();

            foreach (KeyValuePair<double, DeptData> p in sortedD)
            {
                DeptData deptItem = p.Value;
                sortedDepartmentData.Add(deptItem);
            }

            //SORT THE DEPT BASED ON THE AREA
            sortedDepartmentData.Reverse();


            for (int i = 0; i < sortedD.Count; i++)
            {
                //foreach (KeyValuePair<double, DeptData> p in sortedD)
                //{

                //p.Key,p.Value
                //SortedDictionary<double,DeptData>.Enumerator sortedEnumerator = sortedD.GetEnumerator();
                //sortedEnumerator.Current;
                DeptData deptItem = sortedDepartmentData[i];
                // DeptData deptItem = p.Value;
                double areaDeptNeeds = deptItem.DeptAreaNeeded;
                double areaAddedToDept = 0;
                double areaLeftOverToAdd = areaDeptNeeds - areaAddedToDept;
                double areaCurrentPoly = 0;
                double perc = 0.2;
                double limit = areaDeptNeeds * perc;

                Polygon2d currentPolyObj = poly;
                List<Polygon2d> everyDeptPoly = new List<Polygon2d>();
                List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
                int dir = 0;
               
                areaCurrentPoly = Polygon2d.AreaCheckPolygon(currentPolyObj);
                leftOverPoly.Push(poly);



               
                while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0)
                {
                    currentPolyObj = leftOverPoly.Pop();
                    areaCurrentPoly = Polygon2d.AreaCheckPolygon(currentPolyObj);
                    dir = BasicUtility.toggleInputInt(dir);
                    //Trace.WriteLine("Area left over is : " + areaLeftOverToAdd);
                    if(areaLeftOverToAdd > areaCurrentPoly)
                    {
                        everyDeptPoly.Add(currentPolyObj);
                        areaLeftOverToAdd = areaDeptNeeds - areaCurrentPoly;
                        //Trace.WriteLine("Area left over after assigning when area is greater than current : " + areaLeftOverToAdd);

                    }
                    else
                    {
                        Dictionary<string, object> splitReturn = RecursiveSplitByAreaUse(currentPolyObj, areaLeftOverToAdd,dir);
                        polyAfterSplitting = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                        everyDeptPoly.Add(polyAfterSplitting[0]);
                        leftOverPoly.Push(polyAfterSplitting[1]);
                        //Trace.WriteLine("Area left over after assigning when area is lesser than current : " + areaLeftOverToAdd);

                    }
                   

                   
                    //Trace.WriteLine("\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\");

                } // end of while loop

                AllDeptPolys.Add(everyDeptPoly);
                AllDepartmentNames.Add(deptItem.DepartmentName);

            }// end of for loop


  


            List<Polygon2d> AllLeftOverPolys = new List<Polygon2d>();

            AllLeftOverPolys.AddRange(leftOverPoly);

            //Trace.WriteLine("Dept Splitting Done ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            //return polyList;
            return new Dictionary<string, object>
            {
                { "DeptPolys", (AllDeptPolys) },
                { "LeftOverPolys", (AllLeftOverPolys) },
                { "DepartmentNames", (AllDepartmentNames) }
            };


        }


        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "DepartmentNames" })]
        public static Dictionary<string, object> DeptSplitMake(Polygon2d poly, List<DeptData> deptData, int recompute = 1)
        {

            double limit   = 300;
            double lowArea = 4000;
            

            List<List<Polygon2d>> AllDeptPolys = new List<List<Polygon2d>>();
            List<string> AllDepartmentNames = new List<string>();
            Stack<Polygon2d> leftOverPoly = new Stack<Polygon2d>();

            
            SortedDictionary<double, DeptData> sortedD = new SortedDictionary<double, DeptData>();
            for ( int i=0; i < deptData.Count; i++ )
                {
                    double area = deptData[i].AreaEachDept();
                    DeptData deptD = deptData[i];
                    sortedD.Add(area, deptD);

                }



            List<DeptData> sortedDepartmentData = new List<DeptData>();

            foreach (KeyValuePair<double, DeptData> p in sortedD)
            {
                DeptData deptItem = p.Value;
                sortedDepartmentData.Add(deptItem);
            }

            //SORT THE DEPT BASED ON THE AREA
            sortedDepartmentData.Reverse();


                for (int i = 0; i < sortedD.Count; i++)
                {
                //foreach (KeyValuePair<double, DeptData> p in sortedD)
                //{
                
                //p.Key,p.Value
                //SortedDictionary<double,DeptData>.Enumerator sortedEnumerator = sortedD.GetEnumerator();
                //sortedEnumerator.Current;
                DeptData deptItem = sortedDepartmentData[i];
                // DeptData deptItem = p.Value;
                double areaDeptNeeds = deptItem.DeptAreaNeeded;
                double areaAddedToDept = 0;
                double areaLeftOverToAdd = areaDeptNeeds - areaAddedToDept;
                double areaCurrentPoly = 0;
                double proportion = 0.5;
                int direction = 0;

                Polygon2d currentPolyObj = poly;
                List<Polygon2d> everyDeptPoly = new List<Polygon2d>();
                List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
                int iter = 0;



                do
                {
                    //toggle direction
                    direction = BasicUtility.toggleInputInt(direction);

                    //make split of current poly
                    //splitReturned = SplitPolyIntoTwoCheckNew(currentPolyObj, proportion, direction);
                    polyAfterSplitting = BasicSplitWrapper(currentPolyObj, proportion, direction);

                    //check which poly assigned to dept and which poly goes to leftOverPoly
                    double areaPoly1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                    double areaPoly2 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[1].Points);

                    double diff1 = areaPoly1 - areaDeptNeeds;
                    double diff2 = areaPoly2 - areaDeptNeeds;

                    if (diff1 < diff2)
                    {
                        currentPolyObj = polyAfterSplitting[0];
                        leftOverPoly.Push(polyAfterSplitting[1]);
                        areaCurrentPoly = areaPoly1;
                    }
                    else
                    {
                        currentPolyObj = polyAfterSplitting[1];
                        leftOverPoly.Push(polyAfterSplitting[0]);
                        areaCurrentPoly = areaPoly2;
                    }

                    iter += 1;

                    //area of polyAssigned to dept is LESS THAN areaLeftOverToAdd - CASE 1-------------------------------------
                    if (areaCurrentPoly < areaLeftOverToAdd)
                    {
                        everyDeptPoly.Add(currentPolyObj);
                        currentPolyObj = leftOverPoly.Pop();
                        areaAddedToDept += areaCurrentPoly;
                        areaLeftOverToAdd = areaDeptNeeds - areaAddedToDept;
                        areaCurrentPoly = GraphicsUtility.AreaPolygon2d(currentPolyObj.Points);
                    }


                    //                      -----OR------

                    //area of polyAssigned to dept is MORE THAN areaLeftOverToAdd - CASE 2--------------------------------------
                    else
                    {
                        //while()

                    }


                    if (areaCurrentPoly < lowArea)
                    {
                        break;
                    }
                } while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0);

                AllDeptPolys.Add(everyDeptPoly);
                AllDepartmentNames.Add(deptItem.DepartmentName);
            }
            

            List<Polygon2d> AllLeftOverPolys = new List<Polygon2d>();

            AllLeftOverPolys.AddRange(leftOverPoly);

            //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            //return polyList;
            return new Dictionary<string, object>
            {
                { "DeptPolys", (AllDeptPolys) },
                { "LeftOverPolys", (AllLeftOverPolys) },
                { "DepartmentNames", (AllDepartmentNames) }
            };


        }


        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, BASED ON A DIRECTION AND RATIO
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        public static Dictionary<string, object> BasicSplitPolyIntoTwo(Polygon2d polyOutline, double ratio = 0.5, int dir = 0)
        {
            double extents = 5000;
            double spacing = 0.5;
            double minimumLength = 10;
            // dir = 0 : horizontal split line
            // dir = 1 : vertical split line
           
            List<Point2d> polyOrig = polyOutline.Points;
            double eps = 0.1;
            //CHECKS
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //List<Point2d> poly = GraphicsUtility.AddPointsInBetween(polyOrig, 5);
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            // compute bounding box ( set of four points ) for the poly
            // find x Range, find y Range
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            //compute centroid
            Point2d polyCenter = Polygon2d.CentroidFromPoly(poly);
            //check aspect ratio
            double aspectRatio = 0;



            // check if width or length is enough to make split
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength)
            {
                return null;
            }


            //should check direction of split ( flip dir value )
            if (horizontalSpan > verticalSpan)
            {
                dir = 1;
                aspectRatio = horizontalSpan / verticalSpan;
            }
            else
            {
                dir = 0;
                aspectRatio = verticalSpan / horizontalSpan;
            }

            if (aspectRatio > 2)
            {
                //return null;
            }



            // adjust ratio
            if (ratio < 0.15)
            {
                ratio = ratio + eps;
            }
            if (ratio > 0.85)
            {
                ratio = ratio - eps;
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Line2d splitLine = new Line2d(polyCenter, extents, dir);


            //compute vertical or horizontal line via centroid
            double basic = 0.5;
            double shift = ratio - basic;

            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, shift * verticalSpan);
            }
            else
            {
                splitLine.move(shift * horizontalSpan, 0);
            }


            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            ////////////////////////////////////////////////////////////////////////////////////////////
            
            

            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                    //ptA.Add(poly[i]);
                }
                else
                {
                    pIndexB.Add(i);
                    //ptB.Add(poly[i]);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            List<Point2d> sortedA = makePolyPointsStraight(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = makePolyPointsStraight(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            polyA = new Polygon2d(twoSets[0], 0);
            polyB = new Polygon2d(twoSets[1], 0);


            List<Polygon2d> splittedPoly = new List<Polygon2d>();

            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);
            
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (twoSets) }
            };

        }







        //internal function for Recursive Split By Area to work
        internal static double DistanceEditBasedOnRatio(double distance, double areaPoly, double areaFound, double area, double setSpan,double areaDifference)
        {
            double distanceNew = 0;
            /*
            if (areaDifference < 0)
            {
                // need to split less - decrease distance
                double ratio = Math.Abs(areaDifference) / areaPoly;
                distance -= ratio * setSpan;
                Trace.WriteLine("Reducing Distance by : " + ratio * setSpan);
                //double ratio = areaFound / area;
                //distance -= distance*Math.Sqrt(ratio);


            }
            else
            {
                //need to split more -  increase distance
                double ratio = Math.Abs(areaDifference) / areaPoly;
                distance += ratio * setSpan;
                Trace.WriteLine("Increading Distance by : " + ratio * setSpan);
                //double ratio = areaFound / area;
                //distance += distance * Math.Sqrt(ratio);
            }
            */

            distanceNew = distance * (area / areaFound);
            //Trace.WriteLine("Ratio multiplied to distance is : " + (area / areaFound));
            //distanceNew = distance;
            return distanceNew;
        }

        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint" })]
        public static Dictionary<string, object> RecursiveSplitByArea(Polygon2d poly, double area, int dir, int recompute = 1)
        {

            /*PSUEDO CODE:
            get poly's vertical and horizontal span
            based on that get the direction of split
            get polys area and compare with given area ( if area bigger than polyarea then return null )
            based on the proportion calc distance
            bigpoly 
                split that into two
                save distance
                check area of both
                get the smaller poly and compare with asked area
                if area more increase distance by that much
                if area less decrease distance by that much
                split the bigger poly again
                repeat                
            */



            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly.Points);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);
            double minimumLength = 200;
            double perc = 0.2;
            //set limit of 10%
            double limit = area*perc;

            // increase required area by 10%
            //area += area * perc/4;

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X; 
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            double setSpan = 1000000000000;
            //int dir = 0;
            if (horizontalSpan > verticalSpan)
            {
                //dir = 1;
                setSpan = horizontalSpan;

            }
            else
            {
                //dir = 0;
                setSpan = verticalSpan;

            }
            double prop = 0;
            double areaPoly = GraphicsUtility.AreaPolygon2d(poly.Points);
            double areaDifference = 200000;
            if (areaPoly < area)
            {
                return null;
            }
            else
            {
                prop = area / areaPoly;
            }

            List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
            double distance = prop * setSpan;
            Polygon2d currentPoly = poly;
            int count = 0;
            //Trace.WriteLine("Initial Distance set is : " + distance);
            //Trace.WriteLine("Set Span found is : " + setSpan);
            //Trace.WriteLine("Limit accepted is : " + limit);
            while (Math.Abs(areaDifference) > limit && count < 300)
            {
                if (currentPoly.Points == null || distance > setSpan)
                {
                    //Trace.WriteLine("Breaking This---------------------------------");
                    break;
                }

                polyAfterSplitting = EdgeSplitWrapper(currentPoly, distance, dir);
                double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                

                areaDifference = area - area1;
                distance = DistanceEditBasedOnRatio(distance, areaPoly, area1,area, setSpan, areaDifference);
                //Trace.WriteLine("Updated Distance for 1 is : " + distance);
                //Trace.WriteLine("Area Difference found for 1 is : " + areaDifference);
                

                if (areaDifference < 0)
                {
                    //Trace.WriteLine("Reducing Distance");
                }






                //reduce number of points
                //currentPoly = new Polygon2d(currentPoly.Points);
                areaList.Add(distance);
                //Trace.WriteLine("Distance Now is : " + distance);
                //Trace.WriteLine("Iteration is : " + count);
                //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                count += 1;
            }

            polyList.AddRange(polyAfterSplitting);
            pointsList = null;
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) }
            };
        }






        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint" })]
        public static Dictionary<string, object> RecursiveSplitByAreaUse(Polygon2d poly, double area,int dir, int recompute = 1)
        {

            /*PSUEDO CODE:
            get poly's vertical and horizontal span
            based on that get the direction of split
            get polys area and compare with given area ( if area bigger than polyarea then return null )
            based on the proportion calc distance
            bigpoly 
                split that into two
                save distance
                check area of both
                get the smaller poly and compare with asked area
                if area more increase distance by that much
                if area less decrease distance by that much
                split the bigger poly again
                repeat                
            */



            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly.Points);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);
            double perc = 0.1;
            double limit = area*0.1;

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            double setSpan = 1000000000000;
            //int dir = 0;
            if (horizontalSpan > verticalSpan)
            {
                //dir = 1;
                setSpan = horizontalSpan;

            }
            else
            {
                //dir = 0;
                setSpan = verticalSpan;

            }
            double prop = 0;
            double areaPoly = GraphicsUtility.AreaPolygon2d(poly.Points);
            double areaDifference = 200000;
            if (areaPoly < area)
            {
                return null;
            }
            else
            {
                prop = area / areaPoly;
            }

            List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
            double distance = prop * setSpan;
            Polygon2d currentPoly = poly;
            int count = 0;

            while (Math.Abs(areaDifference) > limit && count < 300)
            {
                
                polyAfterSplitting = EdgeSplitWrapper(currentPoly, distance, dir);
                double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                
                areaDifference = area - area1;
                distance = DistanceEditBasedOnRatio(distance, areaPoly, area1, area, setSpan, areaDifference);
                Trace.WriteLine("Updated Distance for 1 is : " + distance);
                Trace.WriteLine("Area Difference found for 1 is : " + areaDifference);
                
                if (currentPoly.Points == null || distance > setSpan)
                {
                    //Trace.WriteLine("Breaking This");
                    break;
                }



                //reduce number of points
                //currentPoly = new Polygon2d(currentPoly.Points);
                //Trace.WriteLine("Iteration is : " + count);
                Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                count += 1;
            }

            polyList.AddRange(polyAfterSplitting);
            pointsList = null;
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) }
            };
        }






        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint" })]
        public static Dictionary<string, object> RecursiveSplitOneDirByDistance(Polygon2d poly, double distance, int recompute = 1)
        {

            /*PSUEDO CODE:
            get poly's vertical and horizontal span
            based on that get the direction of split
            bigpoly 
                split that into two
                push the smaller one in a list
                take the bigger one 
                make it big poly
                repeat
                
            */



            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly.Points);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);
            double minimumLength = 200;

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            double setSpan = 1000000000000;
            int dir = 0;
            if (horizontalSpan > verticalSpan)
            {
                dir = 1;
                setSpan = horizontalSpan;
        
            }
            else
            {
                dir = 0;
                setSpan = verticalSpan;
             
            }


            Polygon2d currentPoly = poly;
            int count = 0;

           
            
            while (setSpan > 0)
            {
                
                List<Polygon2d> polyAfterSplitting = EdgeSplitWrapper(currentPoly, distance, dir);
                double selectedArea = 0;
                double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[1].Points);
                if (area1 > area2)
                {
                    currentPoly = polyAfterSplitting[0];
                    if (polyAfterSplitting[1] == null)
                    {
                       break;
                    }
                    polyList.Add(polyAfterSplitting[1]);
                    areaList.Add(area2);
                    selectedArea = area2;
                    

                }
                else
                {
                    currentPoly = polyAfterSplitting[1];
                    polyList.Add(polyAfterSplitting[0]);
                    areaList.Add(area1);
                    selectedArea = area1;
                   
                }


                if (currentPoly.Points == null)
                {
                    Trace.WriteLine("Breaking This");
                    break;
                }


               
                //reduce number of points
                //currentPoly = new Polygon2d(currentPoly.Points);
               
                setSpan -= distance;
                count += 1;
            }

            pointsList = null;
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) }
            };
        }








        internal static List<double> PolySpanCheck(Polygon2d poly)
        {
            List<double> spanList = new List<double>();
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly.Points);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;

            //place longer span first
            if (horizontalSpan > verticalSpan)
            {
                spanList.Add(horizontalSpan);
                spanList.Add(verticalSpan);
            }
            else
            {
                spanList.Add(verticalSpan);
                spanList.Add(horizontalSpan);                
            }
            
            return spanList;
        }



        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, BASED ON A DIRECTION AND DISTANCE
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        internal static Dictionary<string, object> SplitByDistance(Polygon2d polyOutline, double distance = 10, int dir = 0)
        {
            double extents = 5000;
            int spacing = 5;
            double minimumLength = 10;
            double minValue = 10;
            bool horizontalSplit = false;
            bool verticalSplit = true;
            // dir = 0 : horizontal split line
            // dir = 1 : vertical split line

            List<Point2d> polyOrig = polyOutline.Points;
            double eps = 0.1;
            //CHECKS
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //List<Point2d> poly = GraphicsUtility.AddPointsInBetween(polyOrig, 5);
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            // compute bounding box ( set of four points ) for the poly
            // find x Range, find y Range
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            //compute centroid
            //Point2d polyCenter = Polygon2d.CentroidFromPoly(poly);

            //compute lowest point
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);
            Point2d lowPt = poly[lowInd];
            //check aspect ratio
            double aspectRatio = 0;



            // check if width or length is enough to make split
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength)
            {
                //return null;
            }


            //should check direction of split ( flip dir value )
            if (horizontalSpan > verticalSpan)
            {
                //dir = 1;
                aspectRatio = horizontalSpan / verticalSpan;
            }
            else
            {
                //dir = 0;
                aspectRatio = verticalSpan / horizontalSpan;
            }

            if (aspectRatio > 2)
            {
                //return null;
            }

            //set split style
            if (dir == 0)
            {
                horizontalSplit = true;
            }
            else
            {
                verticalSplit = true;
            }



            // adjust distance if less than some value
            if (distance < minValue)
            {
                //distance = minValue;
            }
            // adjust distance if more than total length of split possible
            if (verticalSplit)
            {
                if (distance > verticalSpan)
                {
                    //distance = verticalSpan - minValue; //CHANGED
                }
            }

            if (horizontalSplit)
            {
                if (distance > horizontalSpan)
                {
                    //distance = horizontalSpan - minValue; //CHANGED
                }
            }


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Line2d splitLine = new Line2d(lowPt, extents, dir);
            //compute vertical or horizontal line via centroid



            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, distance);
            }
            else
            {
                splitLine.move(distance, 0);
            }



            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            ////////////////////////////////////////////////////////////////////////////////////////////



            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                    //ptA.Add(poly[i]);
                }
                else
                {
                    pIndexB.Add(i);
                    //ptB.Add(poly[i]);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            List<Point2d> sortedA = makePolyPointsStraight(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = makePolyPointsStraight(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            polyA = new Polygon2d(twoSets[0], 0);
            polyB = new Polygon2d(twoSets[1], 0);


            List<Polygon2d> splittedPoly = new List<Polygon2d>();

            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);



            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (twoSets) }
            };

        }


        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, BASED ON A DIRECTION AND RATIO
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        public static Dictionary<string, object> SplitFromEdgePolyIntoTwo(Polygon2d polyOutline, double distance = 10, int dir = 0)
        {
            double extents = 5000;
            int spacing = 5;
            double minimumLength = 10;
            double minValue = 10;
            bool horizontalSplit = false;
            bool verticalSplit = true;
            // dir = 0 : horizontal split line
            // dir = 1 : vertical split line

            List<Point2d> polyOrig = polyOutline.Points;
            double eps = 0.1;
            //CHECKS
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //List<Point2d> poly = GraphicsUtility.AddPointsInBetween(polyOrig, 5);
            List<Point2d> poly = Polygon2d.SmoothPolygon(polyOrig, spacing);
            // compute bounding box ( set of four points ) for the poly
            // find x Range, find y Range
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            //compute centroid
            //Point2d polyCenter = Polygon2d.CentroidFromPoly(poly);

            //compute lowest point
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);
            Point2d lowPt = poly[lowInd];
            //check aspect ratio
            double aspectRatio = 0;



            // check if width or length is enough to make split
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength)
            {
                return null;
            }


            //should check direction of split ( flip dir value )
            if (horizontalSpan > verticalSpan)
            {
                //dir = 1;
                aspectRatio = horizontalSpan / verticalSpan;
            }
            else
            {
                //dir = 0;
                aspectRatio = verticalSpan / horizontalSpan;
            }

            if (aspectRatio > 2)
            {
                //return null;
            }

            //set split style
            if(dir == 0)
            {
                horizontalSplit = true;
            }
            else
            {
                verticalSplit = true;
            }


            
            // adjust distance if less than some value
            if (distance < minValue)
            {
                distance = minValue;
            }
            // adjust distance if more than total length of split possible
            if (verticalSplit)
            {
                if(distance > verticalSpan)
                {
                    distance = verticalSpan - minValue;
                }
            }

            if (horizontalSplit)
            {
                if (distance > horizontalSpan)
                {
                    distance = horizontalSpan - minValue;
                }
            }


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Line2d splitLine = new Line2d(lowPt, extents, dir);
            //compute vertical or horizontal line via centroid

         

            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, distance);
            }
            else
            {
                splitLine.move(distance, 0);
            }



            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            ////////////////////////////////////////////////////////////////////////////////////////////



            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                    //ptA.Add(poly[i]);
                }
                else
                {
                    pIndexB.Add(i);
                    //ptB.Add(poly[i]);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            List<Point2d> sortedA = makePolyPointsStraight(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = makePolyPointsStraight(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            polyA = new Polygon2d(twoSets[0], 0);
            polyB = new Polygon2d(twoSets[1], 0);


            List<Polygon2d> splittedPoly = new List<Polygon2d>();

            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);



            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (twoSets) }
            };

        }



        //checker function - can be discarded ater
        public static List<Point2d> CheckLowest_HighestPoint(Polygon2d poly)
        {
            List<Point2d> returnPts = new List<Point2d>();
            List<Point2d> ptList = poly.Points;
            int highPtInd = GraphicsUtility.ReturnHighestPointFromListNew(ptList);
            int  lowPtInd = GraphicsUtility.ReturnLowestPointFromListNew(ptList);
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


        internal static List<Point2d> makePolyPointsStraight(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {

            if (intersectedPoints.Count < 1)
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
                a = 0;
                b = intersectedPoints.Count - 1;
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
                    pt.Add(intersNewList[b]);
                    pt.Add(intersNewList[a]);
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

        // not using now can be discarded
        internal static List<List<Point2d>> sequencePointListsOld(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndexA, List<int> pIndexB)
        {
            List<List<Point2d>> twoSetsPoints = new List<List<Point2d>>();
            List<Point2d> ptA = new List<Point2d>();
            List<Point2d> ptB = new List<Point2d>();
            //Trace.WriteLine("Number of Points on the ptA : " + pIndexA.Count);
            //Trace.WriteLine("Number of Points on the ptB : " + pIndexB.Count);
            bool added = false;
            for (int i = 0; i < pIndexA.Count - 1; i++)
            {
                ptA.Add(poly[pIndexA[i]]);
                if (Math.Abs(pIndexA[i] - pIndexA[i + 1]) > 1 && added == false)
                {
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                        poly[pIndexA[i]]);
                    ptA.Add(intersNewList[1]);
                    ptA.Add(intersNewList[0]);
                    //Trace.WriteLine("Added Intersect Before for PtA");
                    added = true;
                }

                if (i == (pIndexA.Count - 2) && added == false)
                {
                    ptA.Add(poly[pIndexA[i + 1]]);
                    //Trace.WriteLine("Second Time Added Intersect Before for PtA");
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                             ptA[ptA.Count - 1]);
                    ptA.Add(intersNewList[1]);
                    ptA.Add(intersNewList[0]);
                    added = true;

                }
                else if (i == (pIndexA.Count - 2) && added == true)
                {
                    ptA.Add(poly[pIndexA[i + 1]]);
                }
            }
            /*
            if (added == false)
            {
                //Trace.WriteLine("Second Time Added Intersect Before for PtA");
                List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                         ptA[ptA.Count - 1]);
                ptA.Add(intersNewList[1]);
                ptA.Add(intersNewList[0]);
                added = true;
            }
            */
            ////////////////////////////////////////////////////////////////////

            bool added2 = false;
            for (int i = 0; i < pIndexB.Count - 1; i++)
            {
                ptB.Add(poly[pIndexB[i]]);
                if (Math.Abs(pIndexB[i] - pIndexB[i + 1]) > 1 && added2 == false)
                {
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                           poly[pIndexB[i]]);
                    ptB.Add(intersNewList[0]);
                    ptB.Add(intersNewList[1]);
                    added2 = true;
                    //Trace.WriteLine("Added Intersect Before for PtB");
                }

                if (i == (pIndexB.Count - 2))
                {
                    ptB.Add(poly[pIndexB[i + 1]]);
                }
            }

            if (added2 == false)
            {
                //Trace.WriteLine("Second Time Added Intersect Before for PtB");
                List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                        ptB[ptB.Count - 1]);
                ptB.Add(intersNewList[0]);
                ptB.Add(intersNewList[1]);
                added2 = true;
            }

            twoSetsPoints.Add(ptA);
            twoSetsPoints.Add(ptB);
            return twoSetsPoints;

        }







    }
}
