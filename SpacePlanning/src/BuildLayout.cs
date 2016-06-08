using System;
using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using System.Linq;

namespace SpacePlanning
{
    /// <summary>
    /// Builds department and programs polygons based on input contextual data.
    /// </summary>
    public static class BuildLayout
    {
        
        internal static double SPACING = 10; //higher value makes code faster, 6, 10 was good too
        internal static double SPACING2 = 10;
        internal static Random RANGENERATE = new Random();
        internal static double RECURSE = 0;
        internal static Point2d REFERENCEPOINT = new Point2d(0,0);
        internal static int MAXCOUNT = 20, MAXROUND = 50;

        #region - Public Methods



        //arranges depts on site and updates dept data object
        /// <summary>
        /// Arranges dept on site by assigning polygon2d's to each dept in the Dept Data object.
        /// Returns Dept polygon2d's, Left Over polygon2d's, Circulation polygon2d's and Updated Dept Data object.
        /// </summary>
        /// <param name="deptData">List of DeptData object.</param>
        /// <param name="buildingOutline">Building outline polygon2d geometry.</param>
        /// <param name="primaryDeptDepth">Depth in feet of the main department.</param>
        /// <param name="acceptableWidth">Acceptable width in meters while allocating area and polygon2d to each dept on site.</param>
        /// <param name="minNotchDistance">Minimum distance below which an edge will be considered as a removable notch.</param>
        /// <param name="circulationFreq">Value to consider while checking frequency of cirulation computation polygon2d.</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time its value is changed.</param>
        /// <param name="randomToggle">Boolean toggle to turn on or off randomness to assign departments. Default is off.</param>
        /// <returns name="UpdatedDeptData">Updated Dept Data object</returns>
        /// <returns name="LeftOverPolys">Polygon2d's not assigned to any department.</returns>
        /// <returns name="CirculationPolys">Polygon2d's needed to compute circulation networks.</returns>
        /// <returns name="OtherDeptMainPoly">Polygon2d for all other departments except for the primary department.</returns>
        /// <search>
        /// DeptData object, department arrangement on site
        /// </search>
        [MultiReturn(new[] { "UpdatedDeptData","LeftOverPolys", "CirculationPolys","OtherDeptMainPoly"})]
        public static Dictionary<string, object> PlaceDepartments(List<DeptData> deptData, Polygon2d buildingOutline,  double primaryDeptDepth, 
            double acceptableWidth, double minNotchDistance = 20, double circulationFreq = 8, int recompute = 1, bool randomToggle = false)
        {           
            Dictionary<string, object> deptArrangement = new Dictionary<string, object>();
            double count = 0,eps = minNotchDistance;            
            Random rand = new Random();
            bool deptPlaced = false;
            while(deptPlaced == false && count < MAXCOUNT)
            {
                Trace.WriteLine("Lets arrange dept again : " + count);
                deptArrangement = DeptPlacer(deptData, buildingOutline, primaryDeptDepth, acceptableWidth, minNotchDistance, circulationFreq, recompute, randomToggle);
                if(deptArrangement != null)
                {
                    List<DeptData> deptDataUpdated =(List<DeptData>) deptArrangement["UpdatedDeptData"];
                    List<List<Polygon2d>> deptAllPolys = new List<List<Polygon2d>>();
                    for(int i = 0; i < deptDataUpdated.Count; i++) deptAllPolys.Add(deptDataUpdated[i].PolyAssignedToDept);
                    List<Polygon2d> deptPolysTogether = new List<Polygon2d>();
                    for (int i = 0; i < deptAllPolys.Count; i++) deptPolysTogether.AddRange(deptAllPolys[i]);
                    if(deptAllPolys.Count>0) Trace.WriteLine("dept arrangement not null, lets check further");
                    for (int i = 0; i < deptAllPolys.Count; i++)
                    {                       
                        List<Polygon2d> eachDeptPoly = deptAllPolys[i];
                        if (ValidateObject.CheckPolyList(eachDeptPoly)) deptPlaced = true;
                        else { deptPlaced = false; Trace.WriteLine("dept arrangement bad polys, rejected"); break; }
                        bool orthoResult = ValidateObject.CheckPolygon2dListOrtho(deptPolysTogether, eps);
                        Trace.WriteLine("The poly formed is : " + orthoResult);
                        if (orthoResult) deptPlaced = true;
                        else { deptPlaced = false; Trace.WriteLine("dept arrangement non orthogonal, rejected"); break; }
                    }               
                }
                else
                {
                    deptPlaced = false;
                    Trace.WriteLine("DeptPlacer returned null, rejected for: " + count);
                }
                count += 1;
                Trace.WriteLine("+++++++++++++++++++++++++++++++++");
            }// end of while loop
            return deptArrangement;
        }


        //arranges program elements inside primary dept unit and updates program data object
        /// <summary>
        /// Assigns program elements inside the primary department polygon2d.
        /// </summary>
        /// <param name="deptPoly">Polygon2d's of primary department which needs program arrangement inside.</param>
        /// <param name="progData">Program Data object</param>
        /// <param name="primaryProgramWidth">Width of the primary program element in  department.</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time it's value is changed.</param>
        /// <returns name="PolyAfterSplit">Polygon2d's obtained after assigning programs inside the department.</returns>
        /// <returns name="UpdatedProgramData">Updated program data object.</returns>
        /// <returns name="ProgramsAddedCount">Number of program units added.</returns>
        [MultiReturn(new[] { "PolyAfterSplit", "UpdatedProgramData", "ProgramsAddedCount" })]
        public static Dictionary<string, object> PlacePrimaryPrograms(List<Polygon2d> deptPoly, List<ProgramData> progData, double primaryProgramWidth, int recompute = 1, int iterCount = 100)
        {
            
            if (!ValidateObject.CheckPolyList(deptPoly)) return null;
            if (progData == null || progData.Count == 0) return null;
            int roomCount = 0;
            List<Polygon2d> polyList = new List<Polygon2d>();
            List<Point2d> pointsList = new List<Point2d>();
            Queue<ProgramData> programDataRetrieved = new Queue<ProgramData>();
            List<ProgramData> progDataAddedList = new List<ProgramData>();
            ProgramData copyProgData = new ProgramData(progData[0]);

            for (int i = 0; i < progData.Count; i++) programDataRetrieved.Enqueue(progData[i]);
            for (int i = 0; i < deptPoly.Count; i++)
            {
                Polygon2d poly = deptPoly[i];
                if (!ValidateObject.CheckPoly(poly)) continue;
                int lineId = 0, count = 0;
                if (poly.Lines[0].Length > poly.Lines[1].Length) lineId = 1;
                else lineId = 0;
                
                List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);
                double setSpan = 1000000000000, fac = 2.1;
                if (spans[0] > spans[1]) setSpan = spans[0];
                else setSpan = spans[1];
                Polygon2d currentPoly = poly;
                Polygon2d polyAfterSplitting = new Polygon2d(null), leftOverPoly = new Polygon2d(null);
                ProgramData progItem = new ProgramData(progData[0]);
                Point2d centerPt = PolygonUtility.CentroidOfPoly(currentPoly);
                while (setSpan > primaryProgramWidth && count< iterCount) 
                {
                    double dist = 0;
                    if (setSpan < fac * primaryProgramWidth) dist = setSpan;
                    else dist = primaryProgramWidth;

                    /*
                    if (currentPoly.Lines[0].Length > currentPoly.Lines[1].Length) lineId = 1;
                    else lineId = 0;
                    int lineIdPlusTwo = lineId + 2;
                    Point2d midPt1 = LineUtility.LineMidPoint(currentPoly.Lines[lineId]);
                    Point2d midPt2 = LineUtility.LineMidPoint(currentPoly.Lines[lineIdPlusTwo]);
                    double dist1 = PointUtility.DistanceBetweenPoints(midPt1, centerPt);
                    double dist2 = PointUtility.DistanceBetweenPoints(midPt2, centerPt);
                    if (dist2 < dist1) lineId = lineIdPlusTwo;
                    */
                    Dictionary<string, object> splitReturn = SplitObject.SplitByOffsetFromLine(currentPoly,lineId, dist);
                    polyAfterSplitting = (Polygon2d)splitReturn["PolyAfterSplit"];
                    leftOverPoly = (Polygon2d)splitReturn["LeftOverPoly"];
                    if (ValidateObject.CheckPoly(leftOverPoly))
                    {
                        progItem = programDataRetrieved.Dequeue();
                        currentPoly = leftOverPoly;
                        Point2d centerPolySplit = PolygonUtility.CentroidOfPoly(polyAfterSplitting);
                        if(GraphicsUtility.PointInsidePolygonTest(poly,centerPolySplit)) polyList.Add(polyAfterSplitting);
                        progItem.AreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting);
                        setSpan -= dist;
                        progDataAddedList.Add(progItem);
                        count += 1;
                        //Trace.WriteLine("leftover poly all fine : " + count);
                    }
                    else
                    {
                        //Trace.WriteLine("leftover poly not valid found : " + count);
                        break;
                    }
                    if (programDataRetrieved.Count == 0) programDataRetrieved.Enqueue(copyProgData);
                }// end of while                 
                progItem = programDataRetrieved.Dequeue();
                polyList.Add(leftOverPoly);
                progItem.AreaProvided = PolygonUtility.AreaPolygon(leftOverPoly); 
                progDataAddedList.Add(progItem);
                if (programDataRetrieved.Count == 0) programDataRetrieved.Enqueue(copyProgData);
                
            }// end of for loop

            roomCount = progDataAddedList.Count;
            List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progDataAddedList.Count; i++) //progData.Count
            {
                ProgramData progItem = progDataAddedList[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count) progNew.PolyAssignedToProg = new List<Polygon2d> { polyList[i] };
                else progNew.PolyAssignedToProg = null;
                UpdatedProgramDataList.Add(progNew);
            }
            List<Polygon2d> cleanPolyList = ValidateObject.CheckAndCleanPolygon2dList(polyList);    
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (cleanPolyList) },
                { "UpdatedProgramData",(UpdatedProgramDataList) },
                { "ProgramsAddedCount" , (roomCount) }
            };
        }
        
        
        
        //arranges program elements inside primary dept unit and updates program data object
        /// <summary>
        /// Assigns program elements inside the primary department polygon2d.
        /// </summary>
        /// <param name="deptPoly">Polygon2d's of primary department which needs program arrangement inside.</param>
        /// <param name="progData">Program Data object</param>
        /// <param name="primaryProgramWidth">Width of the primary program element in  department.</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time it's value is changed.</param>
        /// <returns name="PolyAfterSplit">Polygon2d's obtained after assigning programs inside the department.</returns>
        /// <returns name="UpdatedProgramData">Updated program data object.</returns>
        /// <returns name="ProgramsAddedCount">Number of program units added.</returns>
        [MultiReturn(new[] { "UpdatedProgramData", "ProgramsAddedCount" })]
        public static Dictionary<string, object> PlaceInpatientPrograms(List<Polygon2d> deptPoly, List<ProgramData> progData, double primaryProgramWidth, int recompute = 1, int space = 10)
        {

            if (!ValidateObject.CheckPolyList(deptPoly)) return null;
            if (progData == null || progData.Count == 0) return null;
            int roomCount = 0;
            List<Polygon2d> polyList = new List<Polygon2d>();
            List<Point2d> pointsList = new List<Point2d>();
            Queue<ProgramData> programDataRetrieved = new Queue<ProgramData>();
            List<ProgramData> progDataAddedList = new List<ProgramData>();
            ProgramData copyProgData = new ProgramData(progData[0]);

            for (int i = 0; i < progData.Count; i++) programDataRetrieved.Enqueue(progData[i]);
            for (int i = 0; i < deptPoly.Count; i++)
            {
                Polygon2d poly = deptPoly[i];
                if (!ValidateObject.CheckPoly(poly)) continue;
                int dir = 0, count = 0;
             
                List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);
                double setSpan = 1000000000000, fac = 1.1;
                if (spans[0] > spans[1]) { setSpan = spans[0]; dir = 1; } // poly is horizontal, dir should be 1
                else { setSpan = spans[1]; dir = 0; }// poly is vertical, dir should be 0
                Polygon2d currentPoly = poly;
                List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
                ProgramData progItem = new ProgramData(progData[0]);
                Point2d centerPt = PolygonUtility.CentroidOfPoly(currentPoly);
                while (setSpan > primaryProgramWidth && count < recompute)
                {
                    Trace.WriteLine("Keep going : " + count);
                    double dist = 0;
                    if (setSpan < fac * primaryProgramWidth)
                    {
                        progItem = programDataRetrieved.Dequeue();                        
                        progItem.AreaProvided = PolygonUtility.AreaPolygon(currentPoly);
                        polyList.Add(currentPoly);
                        progDataAddedList.Add(progItem);
                        count += 1;
                        break;
                    }
                    else dist = primaryProgramWidth;
                    Dictionary<string, object> splitReturn = SplitObject.SplitByDistanceFromPoint(currentPoly, dist, dir);
                    polyAfterSplitting = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    if (ValidateObject.CheckPolyList(polyAfterSplitting))
                    {
                     
                        progItem = programDataRetrieved.Dequeue();
                        polyAfterSplitting = PolygonUtility.SortPolygonList(polyAfterSplitting);
                        progItem.AreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting[0]);
                        polyList.Add(polyAfterSplitting[0]);
                        currentPoly = polyAfterSplitting[1];
                        setSpan -= dist;
                        progDataAddedList.Add(progItem);
                        count += 1;
                    }
                    else
                    {
                        Random ran = new Random();
                        double eps = BasicUtility.RandomBetweenNumbers(ran, 2, -2);
                        dist = primaryProgramWidth + eps;
                        Dictionary<string, object> splitReturn2 = SplitObject.SplitByDistanceFromPoint(currentPoly, dist, dir);
                        polyAfterSplitting = (List<Polygon2d>)splitReturn2["PolyAfterSplit"];
                        if (ValidateObject.CheckPolyList(polyAfterSplitting))
                        {

                            progItem = programDataRetrieved.Dequeue();
                            polyAfterSplitting = PolygonUtility.SortPolygonList(polyAfterSplitting);
                            progItem.AreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting[0]);
                            polyList.Add(polyAfterSplitting[0]);
                            currentPoly = polyAfterSplitting[1];
                            setSpan -= dist;
                            progDataAddedList.Add(progItem);
                            
                        }
                        count += 1;
                    }
                    if (programDataRetrieved.Count == 0) programDataRetrieved.Enqueue(copyProgData);
                }// end of while
                //add the last left over poly for each dept poly
                if (polyAfterSplitting.Count > 0)
                {
                    polyList.Add(polyAfterSplitting[1]);
                    progItem = programDataRetrieved.Dequeue();
                    progItem.AreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting[1]);
                    progDataAddedList.Add(progItem);
                    count += 1;
                }                                              

            }// end of for loop
          
            roomCount = progDataAddedList.Count;
            List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progDataAddedList.Count; i++) //progData.Count
            {
                ProgramData progItem = progDataAddedList[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count) progNew.PolyAssignedToProg = new List<Polygon2d> { polyList[i] };
                else progNew.PolyAssignedToProg = null;
                UpdatedProgramDataList.Add(progNew);
            }
            List<Polygon2d> cleanPolyList = ValidateObject.CheckAndCleanPolygon2dList(polyList);
            return new Dictionary<string, object>
            {
                { "UpdatedProgramData",(UpdatedProgramDataList) },
                { "ProgramsAddedCount" , (roomCount) }
            };
        }



        //arranges program elements inside primary dept unit and updates program data object
        /// <summary>
        /// Assigns program elements inside the primary department polygon2d.
        /// </summary>
        /// <param name="deptPoly">Polygon2d's of primary department which needs program arrangement inside.</param>
        /// <param name="progData">Program Data object</param>
        /// <param name="primaryProgramWidth">Width of the primary program element in  department.</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time it's value is changed.</param>
        /// <returns name="PolyAfterSplit">Polygon2d's obtained after assigning programs inside the department.</returns>
        /// <returns name="UpdatedProgramData">Updated program data object.</returns>
        /// <returns name="ProgramsAddedCount">Number of program units added.</returns>
        [MultiReturn(new[] { "UpdatedProgramData", "ProgramsAddedCount" })]
        public static Dictionary<string, object> PlaceInpatientPrograms2(List<Polygon2d> deptPoly, List<ProgramData> progData, double primaryProgramWidth, int recompute = 1, int space = 10)
        {

            if (!ValidateObject.CheckPolyList(deptPoly)) return null;
            if (progData == null || progData.Count == 0) return null;
            int roomCount = 0;
            List<Polygon2d> polyList = new List<Polygon2d>();
            List<Point2d> pointsList = new List<Point2d>();
            Queue<ProgramData> programDataRetrieved = new Queue<ProgramData>();
            List<ProgramData> progDataAddedList = new List<ProgramData>();
            ProgramData copyProgData = new ProgramData(progData[0]);

            for (int i = 0; i < progData.Count; i++) programDataRetrieved.Enqueue(progData[i]);
            for (int i = 0; i < deptPoly.Count; i++)
            {
                Polygon2d poly = deptPoly[i];
                if (!ValidateObject.CheckPoly(poly)) continue;
                int dir = 0, count = 0;

                List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);
                double setSpan = 1000000000000, fac = 1.1;
                if (spans[0] > spans[1]) { setSpan = spans[0]; dir = 1; } // poly is horizontal, dir should be 1
                else { setSpan = spans[1]; dir = 0; }// poly is vertical, dir should be 0
                Polygon2d currentPoly = poly;
                List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
                ProgramData progItem = new ProgramData(progData[0]);
                Point2d centerPt = PolygonUtility.CentroidOfPoly(currentPoly);
                while (setSpan > primaryProgramWidth && count < recompute)
                {
                    Trace.WriteLine("Keep going : " + count);
                    double dist = 0;
                    if (setSpan < fac * primaryProgramWidth)
                    {
                        progItem = programDataRetrieved.Dequeue();
                        progItem.AreaProvided = PolygonUtility.AreaPolygon(currentPoly);
                        polyList.Add(currentPoly);
                        progDataAddedList.Add(progItem);
                        count += 1;
                        break;
                    }
                    else dist = primaryProgramWidth;
                    int lineId = 0;
                    if (currentPoly.Lines[0].Length > currentPoly.Lines[1].Length) lineId = 1;
                    else lineId = 0;
                    Dictionary<string, object> splitReturn = SplitObject.SplitByOffsetFromLine(currentPoly, lineId, dist, 10);
                    if(splitReturn != null)
                    {
                        polyAfterSplitting.Clear();
                        Polygon2d polyA = (Polygon2d)splitReturn["PolyAfterSplit"];
                        Polygon2d polyB = (Polygon2d)splitReturn["LeftOverPoly"];
                        polyAfterSplitting.Add(polyA); polyAfterSplitting.Add(polyB);
                        progItem = programDataRetrieved.Dequeue();
                        //polyAfterSplitting = PolygonUtility.SortPolygonList(polyAfterSplitting);
                        progItem.AreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting[0]);
                        polyList.Add(polyAfterSplitting[0]);
                        currentPoly = polyAfterSplitting[1];
                        setSpan -= dist;
                        progDataAddedList.Add(progItem);
                        count += 1;
                    }
                   
                    /*
                    //Dictionary<string, object> splitReturn = SplitObject.SplitByDistanceFromPoint(currentPoly, dist, dir);
                    //polyAfterSplitting = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    if (ValidateObject.CheckPolyList(polyAfterSplitting))
                    {

                        progItem = programDataRetrieved.Dequeue();
                        polyAfterSplitting = PolygonUtility.SortPolygonList(polyAfterSplitting);
                        progItem.AreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting[0]);
                        polyList.Add(polyAfterSplitting[0]);
                        currentPoly = polyAfterSplitting[1];
                        setSpan -= dist;
                        progDataAddedList.Add(progItem);
                        count += 1;
                    }
                    else
                    {
                        Random ran = new Random();
                        double eps = BasicUtility.RandomBetweenNumbers(ran, 2, -2);
                        dist = primaryProgramWidth + eps;
                        Dictionary<string, object> splitReturn2 = SplitObject.SplitByDistanceFromPoint(currentPoly, dist, dir);
                        polyAfterSplitting = (List<Polygon2d>)splitReturn2["PolyAfterSplit"];
                        if (ValidateObject.CheckPolyList(polyAfterSplitting))
                        {

                            progItem = programDataRetrieved.Dequeue();
                            polyAfterSplitting = PolygonUtility.SortPolygonList(polyAfterSplitting);
                            progItem.AreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting[0]);
                            polyList.Add(polyAfterSplitting[0]);
                            currentPoly = polyAfterSplitting[1];
                            setSpan -= dist;
                            progDataAddedList.Add(progItem);

                        }
                        count += 1;
                    }
                    */
                    if (programDataRetrieved.Count == 0) programDataRetrieved.Enqueue(copyProgData);
                }// end of while
                //add the last left over poly for each dept poly
                if (polyAfterSplitting.Count > 0)
                {
                    polyList.Add(polyAfterSplitting[1]);
                    progItem = programDataRetrieved.Dequeue();
                    progItem.AreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting[1]);
                    progDataAddedList.Add(progItem);
                    count += 1;
                }

            }// end of for loop

            roomCount = progDataAddedList.Count;
            List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progDataAddedList.Count; i++) //progData.Count
            {
                ProgramData progItem = progDataAddedList[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count) progNew.PolyAssignedToProg = new List<Polygon2d> { polyList[i] };
                else progNew.PolyAssignedToProg = null;
                UpdatedProgramDataList.Add(progNew);
            }
            List<Polygon2d> cleanPolyList = ValidateObject.CheckAndCleanPolygon2dList(polyList);
            return new Dictionary<string, object>
            {
                { "UpdatedProgramData",(UpdatedProgramDataList) },
                { "ProgramsAddedCount" , (roomCount) }
            };
        }














        //arranges program elements inside secondary dept units and updates program data object
        /// <summary>
        /// Assigns program elements inside the secondary department polygon2d.
        /// </summary>
        /// <param name="deptPoly">Polygon2d's of secondary department which needs program arrangement inside.</param>
        /// <param name="progData">Program Data object.</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time its value is changed.</param>
        /// <returns></returns>
        [MultiReturn(new[] { "PolyAfterSplit", "UpdatedProgramData" })]
        public static Dictionary<string, object> PlaceSecondaryPrograms(List<Polygon2d> deptPoly, List<ProgramData> progData, int recompute = 0)
        {
            if (!ValidateObject.CheckPolyList(deptPoly)) return null;
            if (progData == null || progData.Count == 0) return null;
            Random ran = new Random();
            List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
            List<Polygon2d> polyCoverList = new List<Polygon2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();
            Stack<Polygon2d> polygonAvailable = new Stack<Polygon2d>();
            
            for (int j = 0; j < deptPoly.Count; j++) { polygonAvailable.Push(deptPoly[j]); }
            double areaAssigned = 0, eps = 50;
            int count = 0, maxTry = 100;
            for(int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                progItem.PolyAssignedToProg = new List<Polygon2d>();
                double areaNeeded = progItem.AreaNeeded;
                while (areaAssigned < areaNeeded && polygonAvailable.Count > 0 && count < maxTry)
                {
                    Polygon2d currentPoly = polygonAvailable.Pop();
                    double areaPoly = PolygonUtility.AreaPolygon(currentPoly);
                    int compareArea = BasicUtility.CheckWithinRange(areaNeeded, areaPoly, eps);
                    if (compareArea == 1) // current poly area is more
                    {
                        Dictionary<string,object> splitObj = SplitObject.SplitByRatio(currentPoly, 0.5);
                        List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitObj["PolyAfterSplit"];
                        for (int j = 0; j < polyAfterSplit.Count; j++) polygonAvailable.Push(polyAfterSplit[j]);
                        count += 1;
                        continue;
                    }
                    progItem.PolyAssignedToProg.Add(currentPoly);
                    areaAssigned += areaPoly;                
                    count += 1;
                }// end of while
                polyList.Add(progItem.PolyAssignedToProg);
                progItem.AreaProvided = areaAssigned;
                count = 0;
                areaAssigned = 0;
            }
            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++) newProgDataList.Add(new ProgramData(progData[i]));
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "UpdatedProgramData",(newProgDataList) }
            };
        }



        //arranges program elements inside secondary dept units and updates program data object
        /// <summary>
        /// Assigns program elements inside the secondary department polygon2d.
        /// </summary>
        /// <param name="deptData">List of Department Data Objects.</param>
        /// <param name="primaryProgramWidth">Width of the program poly in the primary department</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time its value is changed.</param>
        /// <returns></returns>
        [MultiReturn(new[] { "UpdatedDeptData" })]
        public static Dictionary<string, object> PlacePrograms(List<DeptData> deptData, double primaryProgramWidth = 30, int recompute = 0)
        {
            if (deptData == null) return null;
            List<List<Polygon2d>> polyPorgsAdded = new List<List<Polygon2d>>();
            List<ProgramData> progDataNew = new List<ProgramData>();
            for(int i = 0; i < deptData.Count; i++)
            {
                if (i == 0)
                {
                    
                    Dictionary<string, object> placedPrimaryProg = PlaceInpatientPrograms(deptData[i].PolyAssignedToDept, deptData[i].ProgramsInDept, primaryProgramWidth, recompute);
                    //Dictionary<string, object> placedPrimaryProg = PlacePrimaryPrograms(deptData[i].PolyAssignedToDept, deptData[i].ProgramsInDept, primaryProgramWidth, recompute);
                    deptData[i].ProgramsInDept = (List<ProgramData>)placedPrimaryProg["UpdatedProgramData"];
                }
                else
                {
                    Dictionary<string, object> placedSecondaryProg = PlaceSecondaryPrograms(deptData[i].PolyAssignedToDept, deptData[i].ProgramsInDept, recompute);
                    deptData[i].ProgramsInDept = (List<ProgramData>)placedSecondaryProg["UpdatedProgramData"];
                }
              
            }
            List<DeptData> newDeptData = new List<DeptData>();
            for(int i = 0; i < deptData.Count; i++) newDeptData.Add(new DeptData(deptData[i]));
            return new Dictionary<string, object>
            {
                { "UpdatedDeptData",(newDeptData) }
            };
        }



        #endregion





        #region - Private Methods  
        //blocks are assigned based on ratio of split, used for assigning other depts
        [MultiReturn(new[] { "DeptPoly", "LeftOverPoly", "AllPolys", "AreaAdded", "AllNodes" })]
        internal static Dictionary<string, object> AssignBlocksBasedOnRatio(double areaFactor, double areaAvailable, List<Polygon2d> polyList, double acceptableWidth = 10, double ratio = 0.5)
        {
            //if (!ValidateObject.CheckPolyList(polyList)) return null;           
            //for (int i = 0; i < polyList.Count; i++) areaAvailable += PolygonUtility.AreaPolygon(polyList[i]);
            Queue<Polygon2d> polyAvailable = new Queue<Polygon2d>();
            List<Polygon2d> polysToDept = new List<Polygon2d>(), leftOverPoly = new List<Polygon2d>();
            for (int i = 0; i < polyList.Count; i++) polyAvailable.Enqueue(polyList[i]);
            double deptAreaTarget = areaFactor * areaAvailable, areaAssigned = 0;
            //deptAreaTarget = areaFactor;
            //double deptAreaTarget = deptItem.DeptAreaNeeded,areaAssigned = 0;
            while (areaAssigned < deptAreaTarget && polyAvailable.Count > 0)
            {
                Polygon2d currentPoly = polyAvailable.Dequeue();
                areaAssigned += PolygonUtility.AreaPolygon(currentPoly);
                polysToDept.Add(currentPoly);
            }
            return new Dictionary<string, object>
            {
                { "DeptPoly", (polysToDept) },
                { "LeftOverPoly", (polyAvailable.ToList()) },
                { "AllPolys", (polyList)},
                { "AreaAdded", (areaAssigned) },
                { "AllNodes", (null)}
            };
        }


        //blocks are assigne based on offset distance, used for inpatient blocks
        [MultiReturn(new[] { "PolyAfterSplit", "LeftOverPoly", "AreaAssignedToBlock", "FalseLines", "LineOptions", "PointAdded" })]
        internal static Dictionary<string, object> AssignBlocksBasedOnDistance(Polygon2d poly, double distance = 16, double area = 0, double thresDistance = 10, double recompute = 5)
        {

            if (!ValidateObject.CheckPoly(poly)) return null;
            if (distance < 1) return null;
            Trace.WriteLine("assginblocks by distance in process");
            bool externalInclude = false;
            if (recompute > 5) externalInclude = true;
            int count = 0, maxTry = 100;
            poly = new Polygon2d(poly.Points);
            if (area == 0) area = 0.8 * PolygonUtility.AreaPolygon(poly);
            Stack<Polygon2d> polyLeftList = new Stack<Polygon2d>();
            double areaAdded = 0, minLength = 200;
            polyLeftList.Push(poly);
            Point2d pointAdd = new Point2d(0, 0);
            List<Polygon2d> blockPolyList = new List<Polygon2d>();
            List<Polygon2d> leftoverPolyList = new List<Polygon2d>();
            List<Line2d> falseLines = new List<Line2d>();
            List<Line2d> lineOptions = new List<Line2d>();
            bool error = false;
            while (polyLeftList.Count > 0 && areaAdded < area) //count<recompute count < maxTry
            {
                error = false;
                Polygon2d currentPoly = polyLeftList.Pop();
                Polygon2d tempPoly = new Polygon2d(currentPoly.Points, 0);
                Dictionary<string, object> splitObject = CreateBlocksByLines(currentPoly, poly, distance, thresDistance, externalInclude);
                if (splitObject == null) { count += 1; Trace.WriteLine("Split errored"); continue; }
                Polygon2d blockPoly = (Polygon2d)splitObject["PolyAfterSplit"];
                Polygon2d leftPoly = (Polygon2d)splitObject["LeftOverPoly"];
                lineOptions = (List<Line2d>)splitObject["LineOptions"];
                Dictionary<string, object> addPtObj = LayoutUtility.AddPointToFitPoly(leftPoly, poly, distance, thresDistance, recompute);
                leftPoly = (Polygon2d)addPtObj["PolyAddedPts"];
                falseLines = (List<Line2d>)addPtObj["FalseLineList"];
                pointAdd = (Point2d)addPtObj["PointAdded"];
                areaAdded += PolygonUtility.AreaPolygon(blockPoly);
                polyLeftList.Push(leftPoly);
                blockPolyList.Add(blockPoly);
                count += 1;
                if (lineOptions.Count == 0) error = true;
                else
                {
                    for (int i = 0; i < lineOptions.Count; i++)
                    {
                        if (lineOptions[i].Length > thresDistance) { error = false; break; }
                        else error = true;
                    }
                }
                if (error) break;
                //Trace.WriteLine("still inside while loop at assgineblocksbydistance");
            }// end of while loop


            /*
            //added to allow one more poly
            bool spaceAvailable = false;
            for (int i = 0; i < lineOptions.Count; i++) { if (lineOptions[i].Length > minLength) spaceAvailable = true; break; }

            if (spaceAvailable && polyLeftList.Count > 0)
            {
                Polygon2d currentPoly = polyLeftList.Pop();
                Polygon2d tempPoly = new Polygon2d(currentPoly.Points, 0);
                Dictionary<string, object> splitObject = CreateBlocksByLines(currentPoly, poly, distance, thresDistance, externalIncude);
                Trace.WriteLine("Well found that space is available");
                if (splitObject != null)
                {
                    Polygon2d blockPoly = (Polygon2d)splitObject["PolyAfterSplit"];
                    Polygon2d leftPoly = (Polygon2d)splitObject["LeftOverPoly"];
                    lineOptions = (List<Line2d>)splitObject["LineOptions"];
                    Dictionary<string, object> addPtObj = LayoutUtility.AddPointToFitPoly(leftPoly, poly, distance, thresDistance, recompute);
                    leftPoly = (Polygon2d)addPtObj["PolyAddedPts"];
                    falseLines = (List<Line2d>)addPtObj["FalseLineList"];
                    pointAdd = (Point2d)addPtObj["PointAdded"];
                    areaAdded += PolygonUtility.AreaPolygon(blockPoly);
                    polyLeftList.Push(leftPoly);
                    blockPolyList.Add(blockPoly);
                    count += 1;
                    if (lineOptions.Count == 0) error = true;
                    else
                    {
                        for (int i = 0; i < lineOptions.Count; i++)
                        {
                            if (lineOptions[i].Length > thresDistance) { error = false; break; }
                            else error = true;
                        }
                    }
                    Trace.WriteLine("Succesfully assigned one extra");
                } // end of if loop
            }
            */


            leftoverPolyList.AddRange(polyLeftList);
            blockPolyList = PolygonUtility.CleanPolygonList(blockPolyList);
            leftoverPolyList = PolygonUtility.CleanPolygonList(leftoverPolyList);
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (blockPolyList) },
                { "LeftOverPoly", (leftoverPolyList) },
                { "AreaAssignedToBlock", (areaAdded)},
                { "FalseLines", (falseLines) },
                { "LineOptions", (lineOptions) },
                { "PointAdded" , (pointAdd)}
            };
        }


        //makes a space data tree from dept data
        [MultiReturn(new[] { "SpaceTree", "NodeList" })]
        internal static Dictionary<string, object> CreateSpaceTreeFromDeptData(Node root, List<Node> nodeList,
            Point origin, double spaceX, double spaceY, double radius, bool symettry = true)
        {
            SpaceDataTree tree = new SpaceDataTree(root, origin, spaceX, spaceY);
            Node current = root;
            Node nodeAdditionResult = null;
            for (int i = 0; i < nodeList.Count; i++)
            {
                if (current.NodeType == NodeType.Space) current = current.ParentNode;
                nodeAdditionResult = tree.AddNewNodeSide(current, nodeList[i]);
                if (nodeAdditionResult == current) break;
                else if (nodeAdditionResult != current && nodeAdditionResult != null) current = nodeAdditionResult;
                else current = nodeList[i];
            }
            return new Dictionary<string, object>
            {
                { "SpaceTree", (tree) },
                { "NodeList", (nodeList) }
            };
        }

    
        //splits a polygon based on offset direction
        [MultiReturn(new[] { "PolyAfterSplit", "LeftOverPoly", "LineOptions", "SortedLengths" })]
        internal static Dictionary<string, object> CreateBlocksByLines(Polygon2d polyOutline, Polygon2d containerPoly, double distance = 10, double minDist = 20,bool tag = true)
        {
            if (!ValidateObject.CheckPoly(polyOutline)) return null;
            Polygon2d poly = new Polygon2d(polyOutline.Points,0);
            List<double> lineLength = new List<double>();
            List<Line2d> lineOptions = new List<Line2d>();
            Dictionary<string, object> checkLineOffsetObject = ValidateObject.CheckLinesOffsetInPoly(poly, containerPoly, distance, tag);
            List<bool> offsetAble = (List<bool>)checkLineOffsetObject["Offsetables"];
            for (int i = 0; i < poly.Points.Count; i++)
            {
                if (offsetAble[i] == true) { lineLength.Add(poly.Lines[i].Length); }
                else lineLength.Add(0);
            }       
            List<int> sortedIndices = BasicUtility.Quicksort(lineLength);
            if (sortedIndices != null) sortedIndices.Reverse();
            for (int i = 0; i < poly.Points.Count; i++) if (lineLength[i] > 0 && i != sortedIndices[0]) { lineOptions.Add(poly.Lines[i]); }
            Dictionary<string, object> splitObj = SplitObject.SplitByOffsetFromLine(poly, sortedIndices[0], distance, minDist);
            Polygon2d polyBlock = (Polygon2d)splitObj["PolyAfterSplit"];
            Polygon2d leftPoly = (Polygon2d)splitObj["LeftOverPoly"];
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyBlock) },
                { "LeftOverPoly", (leftPoly) },
                { "LineOptions" , (lineOptions) },
                { "SortedLengths", (sortedIndices) }           
            };

        }
           
        //dept assignment new way
        [MultiReturn(new[] { "UpdatedDeptData", "LeftOverPolys","CirculationPolys", "OtherDeptMainPoly" })]
        internal static Dictionary<string, object> DeptPlacer(List<DeptData> deptData, Polygon2d poly, double offset, 
            double acceptableWidth = 20,double minNotchDist = 20, double circulationFreq = 10, double recompute = 5, bool tag = false)
        {
            if (deptData == null) //|| !ValidateObject.CheckPoly(poly)
            {
                Trace.WriteLine("Null found poly or deptdata");
                return null;
            }

            //Dictionary<string, object> notchObj = ValidateObject.CheckPolyNotches(poly, minNotchDist);
            //poly = (Polygon2d)notchObj["PolyReduced"];
            Trace.WriteLine("Dept placer is good to go");
            List<double> AllDeptAreaAdded = new List<double>();
            List<List<Polygon2d>> AllDeptPolys = new List<List<Polygon2d>>();
            List<Polygon2d> leftOverPoly = new List<Polygon2d>(), polyCirculation = new List<Polygon2d>();//changed from stack
            List<Polygon2d> otherDeptPoly = new List<Polygon2d>();
            List<Polygon2d> subDividedPoly = new List<Polygon2d>();
            int count = 0, maxTry = 20;
            double totalDeptProp = 0, areaAvailable = 0, ratio = 0.6, eps = 2;
            for (int i = 0; i < deptData.Count; i++) if (i > 0) totalDeptProp += deptData[i].DeptAreaProportionNeeded;

            for (int i = 0; i < deptData.Count; i++)
            {            
                double areaAssigned = 0;
                DeptData deptItem = deptData[i];
                if (i == 0) // inpatient dept
                {
                    //double areaNeeded = deptItem.DeptAreaNeeded;
                    double areaNeeded = deptItem.DeptAreaProportionNeeded * PolygonUtility.AreaPolygon(poly);
                    areaNeeded = 100000;
                    Trace.WriteLine("placing inpatients");
                    Dictionary<string, object> inpatientObject = AssignBlocksBasedOnDistance(poly, offset, areaNeeded, 20, 30);
                    if (inpatientObject == null) return null;
                    List<Polygon2d> inpatienBlocks = (List<Polygon2d>)inpatientObject["PolyAfterSplit"];
                    List<Polygon2d> leftOverBlocks = (List<Polygon2d>)inpatientObject["LeftOverPoly"];
                    areaAssigned = (double)inpatientObject["AreaAssignedToBlock"];
                    AllDeptPolys.Add(inpatienBlocks);
                    AllDeptAreaAdded.Add(areaAssigned);
                    for (int j = 0; j < leftOverBlocks.Count; j++)
                    {
                        otherDeptPoly.Add(new Polygon2d(leftOverBlocks[j].Points));
                        leftOverPoly.Add(leftOverBlocks[j]);
                    }              
                }
                if (i == 1)
                {
                    List<List<Polygon2d>> polySubDivs = new List<List<Polygon2d>>();
                    if (!tag) polySubDivs = SplitObject.SplitRecursivelyToSubdividePoly(leftOverPoly, acceptableWidth, circulationFreq, ratio);
                    else polySubDivs = SplitObject.SplitRecursivelyRandomizedToSubdividePoly(leftOverPoly, acceptableWidth, circulationFreq, ratio, eps, recompute);
                    bool checkPoly1 = ValidateObject.CheckPolygon2dListOrtho(polySubDivs[0], 0.5);
                    bool checkPoly2 = ValidateObject.CheckPolygon2dListOrtho(polySubDivs[1], 0.5);
                    while (polySubDivs == null || polySubDivs.Count == 0 || !checkPoly1 || !checkPoly2 && count < maxTry)
                    {                 
                        ratio -= 0.01;
                        if (ratio < 0) ratio = 0.6; break;                        
                        Trace.WriteLine("Ratio problem faced , ratio reduced to : " + ratio);
                        polySubDivs = SplitObject.SplitRecursivelyToSubdividePoly(leftOverPoly, acceptableWidth, circulationFreq, ratio);
                        count += 1;
                    }
                    leftOverPoly = polySubDivs[0];
                    polyCirculation = polySubDivs[1];
                    for (int j = 0; j < leftOverPoly.Count; j++) areaAvailable += PolygonUtility.AreaPolygon(leftOverPoly[j]);
                    if (leftOverPoly == null) break;
                }

                if( i > 0 ) // other depts
                {
                    double areaFactor = deptItem.DeptAreaProportionNeeded / totalDeptProp;
                    Dictionary<string, object> assignedByRatioObj = AssignBlocksBasedOnRatio(areaFactor,areaAvailable, leftOverPoly,acceptableWidth,0.5);
                    List<Polygon2d> everyDeptPoly = (List<Polygon2d>)assignedByRatioObj["DeptPoly"];
                    leftOverPoly = (List<Polygon2d>)assignedByRatioObj["LeftOverPoly"];                 
                    areaAssigned = (double)assignedByRatioObj["AreaAdded"];
                    List<Node> AllNodesList = (List<Node>)assignedByRatioObj["AllNodes"];
                    AllDeptAreaAdded.Add(areaAssigned);
                    AllDeptPolys.Add(everyDeptPoly);
                }
            }
            //clean dept polys based on their fitness
            for(int i = 0; i < AllDeptPolys.Count; i++) AllDeptPolys[i] = ValidateObject.CheckAndCleanPolygon2dList(AllDeptPolys[i]);

            //update dept data based on polys assigned
            List<DeptData> UpdatedDeptData = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData newDeptData = new DeptData(deptData[i]);
                newDeptData.AreaProvided = AllDeptAreaAdded[i];
                newDeptData.PolyAssignedToDept = AllDeptPolys[i];
                UpdatedDeptData.Add(newDeptData);
            }

            //added to compute area percentage for each dept
            double totalDeptArea = 0;
            for (int i = 0; i < UpdatedDeptData.Count; i++) totalDeptArea += UpdatedDeptData[i].DeptAreaNeeded;
            for (int i = 0; i < UpdatedDeptData.Count; i++) UpdatedDeptData[i].DeptAreaProportionAchieved = Math.Round((UpdatedDeptData[i].AreaProvided / totalDeptArea), 3);

            if (leftOverPoly.Count == 0) leftOverPoly = null;

            return new Dictionary<string, object>
            {
                { "UpdatedDeptData", (UpdatedDeptData) },
                { "LeftOverPolys", (leftOverPoly) },
                { "CirculationPolys", (polyCirculation) },
                { "OtherDeptMainPoly", (otherDeptPoly) }
            };
        }

      
 
        #endregion

    }
}
