using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using System;
using DSCore;
using Display;
using System.Linq;
using Math = System.Math;

namespace SpacePlanning
{
    /// <summary>
    /// Class to perform spatial analytis to score and appraise space plan layouts.
    /// </summary>
    public static class SpaceAnalysis
    {
        //const colors
        
        

        #region - Public Methods
        //provides information related to dept data
        /// <summary>
        /// Provides analytics on Department data after spaces has been assigned.
        /// </summary>
        /// <param name="deptData">Department data object.</param>
        /// <returns name="DepartmentNames">Name of the departments.</returns>
        /// <returns name="NumCellsTaken">Number of cells assgined to each department.</returns>
        /// <returns name="AreaSatisfied">If Department area is satisfied</returns>
        /// <search>
        /// department analytics
        /// </search>
        [MultiReturn(new[] { "DepartmentNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "ProgramsInDepts", "PolyAssignedDepts" })]
        public static Dictionary<string, object> AnalyticsDeptData(List<DeptData> deptData)
        {

            if (deptData == null) return null; 
            List<string> deptNameList = new List<string>();
            List<int> numCellsList = new List<int>();
            List<bool> areaSatisfiedList = new List<bool>();
            List<List<ProgramData>> progLists = new List<List<ProgramData>>();
            List<double> areaNeededList = new List<double>();
            List<double> areaProvidedList = new List<double>();
            List<List<Polygon2d>> polyAssignedList = new List<List<Polygon2d>>();
            for (int i = 0; i < deptData.Count; i++)
            {
                deptNameList.Add(deptData[i].DepartmentName);
                numCellsList.Add(deptData[i].NumCellsInDept);
                areaSatisfiedList.Add(deptData[i].IsAreaSatisfied);
                progLists.Add(deptData[i].ProgramsInDept);
                areaNeededList.Add(deptData[i].DeptAreaNeeded);
                areaProvidedList.Add(deptData[i].DeptAreaProvided);
                polyAssignedList.Add(deptData[i].PolyAssignedToDept);
            }
            return new Dictionary<string, object>
            {
                { "DepartmentNames", (deptNameList) },
                { "NumCellsTaken", (numCellsList) },
                { "AreaSatisfied", (areaSatisfiedList) },
                { "AreaNeeded", (areaNeededList) },
                { "AreaProvided", (areaProvidedList) },
                { "ProgramsInDepts", (progLists) },
                { "PolyAssignedDepts", (polyAssignedList) }
            };
        }
        //Provides information related to program data

        [MultiReturn(new[] { "DisplayGeomList" })]
        public static Dictionary<string, object> VisualizeCirculation(List<Polygon2d> deptCirculationPoly, List<Polygon2d> progCirculationPoly, double height =0)
        {
            List<Color> colorList = new List<Color>();
            colorList.Add(Color.ByARGB(255, 0, 255, 255)); // cyan
            colorList.Add(Color.ByARGB(255, 153, 255, 102)); // fluoro green

            if (!ValidateObject.CheckPolyList(deptCirculationPoly) || !ValidateObject.CheckPolyList(progCirculationPoly)) return null;
            
            List<List<Polygon2d>> polyProgsList = new List<List<Polygon2d>>();
            polyProgsList.Add(deptCirculationPoly);
            polyProgsList.Add(progCirculationPoly);
       
            List<List<Surface>> srfListAll = new List<List<Surface>>();
            List<List<Display.Display>> displayListAll = new List<List<Display.Display>>();
            for (int i = 0; i < polyProgsList.Count; i++)
            {
                List<Polygon2d> polyProgs = polyProgsList[i];
                Color col = colorList[i];
                List<Surface> srfList = new List<Surface>();
                List<Display.Display> displayList = new List<Display.Display>();
                for (int j = 0; j < polyProgs.Count; j++)
                {
                    
                    Polygon2d polyReduced = new Polygon2d(polyProgs[j].Points);
                    List<Point2d> ptList = polyReduced.Points;
                    List<Point> ptNewList = new List<Point>();
                    for (int k = 0; k < ptList.Count; k++) ptNewList.Add(Point.ByCoordinates(ptList[k].X, ptList[k].Y));
                    Surface srf;
                    try { srf = Surface.ByPerimeterPoints(ptNewList); }
                    catch { continue; }                    
                    Geometry gm = srf.Translate(0, 0, height);
                    Display.Display dis = Display.Display.ByGeometryColor(gm, col);
                    displayList.Add(dis);
                    //srfList.Add(srf);                    
                    srf.Dispose();                   
                    ptNewList.Clear();
                    
                }
                displayListAll.Add(displayList);
                //srfListAll.Add(srfList);
            }
            return new Dictionary<string, object>
            {
                { "DisplayGeomList", (displayListAll) }
            };
        }

        [MultiReturn(new[] { "DisplayGeomList" })]
        internal static Dictionary<string,object> ColorPrograms(List<DeptData> deptDataInp, double height = 0, int transparency = 255, int colorScheme = 0)
        {
            if (deptDataInp == null) return null;
            List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy
            if (transparency < 0 || transparency > 255) transparency = 255;
            // hard coded list of colors for 20 depts
            List<Color> colorList = ProvideColorList(transparency), colorListSelected = new List<Color>();
           

            List<int> indicesList = new List<int>();
            for (int i = 0; i < colorList.Count; i++) indicesList.Add(i);

            if (colorScheme == 0) colorListSelected = colorList;
            else
            {
                Random ran = new Random(colorScheme);
                List<int> indicesRandomList = BasicUtility.RandomizeList(indicesList, ran);
                for (int i = 0; i < colorList.Count; i++) { colorListSelected.Add(colorList[indicesRandomList[i]]); }
            }
           
            
            List<List<List<Polygon2d>>> polyProgsList = new List<List<List<Polygon2d>>>();
            for (int i = 0; i < deptData.Count; i++)
            {
                List<ProgramData> progsInDept = deptData[i].ProgramsInDept;
                List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
                for (int j = 0; j < progsInDept.Count; j++) polyList.Add(progsInDept[j].PolyAssignedToProg);
                polyProgsList.Add(polyList);
            }
            List<List<Surface>> srfListAll = new List<List<Surface>>();
            List<Display.Display> displayList = new List<Display.Display>();
            int index = 0;
            for (int i = 0; i < polyProgsList.Count; i++)
            {
                if (index > colorList.Count - 1) index = 0;
                Color col = colorListSelected[index];
                for (int j = 0; j < polyProgsList[i].Count; j++)
                {
                    if (i > 0)
                    {
                        if (index > colorList.Count - 1) index = 0;
                        col = colorListSelected[index];
                    }                   
                    for (int k = 0; k < polyProgsList[i][j].Count; k++)
                    {                        
                        Polygon2d polyReduced = new Polygon2d(polyProgsList[i][j][k].Points);
                        //polyReduced = PolygonUtility.PolyExtraEdgeRemove(polyReduced);
                        //polyReduced = PolygonUtility.CreateOrthoPoly(polyReduced);
                        List<Point2d> ptList = polyReduced.Points;
                        List<Point> ptNewList = new List<Point>();
                        for (int l = 0; l < ptList.Count; l++)
                        {
                            ptNewList.Add(Point.ByCoordinates(ptList[l].X, ptList[l].Y));
                        }

                        Surface srf;
                        try { srf = Surface.ByPerimeterPoints(ptNewList); }
                        catch { continue; }

                        Geometry gm = srf.Translate(0, 0, height);
                        Display.Display dis = Display.Display.ByGeometryColor(gm, col);
                        displayList.Add(dis);
                        //srfList.Add(srf);
                        srf.Dispose();
                        ptNewList.Clear();
                        index += 1;
                    }


                }
            }
            List<List<Display.Display>> displayListAll = new List<List<Display.Display>>();
            displayListAll.Add(displayList);
            return new Dictionary<string, object>
            {
                { "DisplayGeomList", (displayListAll) }
            };
        }

        internal static List<Color> ProvideColorList(int transparency = 255)
        {
            // hard coded list of colors for 20 depts
            List<Color> colorList = new List<Color>();
            colorList.Add(Color.ByARGB(transparency, 119, 179, 0)); // light green
            colorList.Add(Color.ByARGB(transparency, 255, 51, 204)); // bright pink
            colorList.Add(Color.ByARGB(transparency, 102, 102, 255)); // violetish blue
            colorList.Add(Color.ByARGB(transparency, 255, 195, 77)); // orangish yellow
            colorList.Add(Color.ByARGB(transparency, 204, 153, 255)); // violet blue
            colorList.Add(Color.ByARGB(transparency, 51, 51, 204)); // darker blue
            colorList.Add(Color.ByARGB(transparency, 0, 128, 0)); // darker green
            colorList.Add(Color.ByARGB(transparency, 98, 98, 98)); // grey dark
            colorList.Add(Color.ByARGB(transparency, 204, 255, 102)); // light green
            colorList.Add(Color.ByARGB(transparency, 255, 51, 153)); // reddish pink
            colorList.Add(Color.ByARGB(transparency, 0, 102, 153)); // teal blue
            colorList.Add(Color.ByARGB(transparency, 153, 0, 204)); // purple

            colorList.Add(Color.ByARGB(transparency, 255, 195, 155));
            colorList.Add(Color.ByARGB(transparency, 204, 153, 255));
            colorList.Add(Color.ByARGB(transparency, 25, 155, 204));
            colorList.Add(Color.ByARGB(transparency, 150, 128, 0));
            colorList.Add(Color.ByARGB(transparency, 98, 98, 98));
            colorList.Add(Color.ByARGB(transparency, 14, 255, 102));

            colorList.Add(Color.ByARGB(transparency, 119, 179, 0));
            colorList.Add(Color.ByARGB(transparency, 100, 225, 15));
            colorList.Add(Color.ByARGB(transparency, 102, 102, 255));
            colorList.Add(Color.ByARGB(transparency, 15, 195, 77));
            colorList.Add(Color.ByARGB(transparency, 204, 153, 255));

            colorList.Add(Color.ByARGB(transparency, 255, 195, 5));
            colorList.Add(Color.ByARGB(transparency, 204, 4, 255));
            colorList.Add(Color.ByARGB(transparency, 155, 155, 204));
            colorList.Add(Color.ByARGB(transparency, 5, 179, 0));
            colorList.Add(Color.ByARGB(transparency, 255, 225, 15));

            colorList.Add(Color.ByARGB(transparency, 119, 179, 0));
            colorList.Add(Color.ByARGB(transparency, 255, 55, 15));
            colorList.Add(Color.ByARGB(transparency, 102, 102, 255));
            colorList.Add(Color.ByARGB(transparency, 15, 195, 77));
            colorList.Add(Color.ByARGB(transparency, 204, 153, 255));

            colorList.Add(Color.ByARGB(transparency, 55, 195, 5));
            colorList.Add(Color.ByARGB(transparency, 204, 4, 255));
            colorList.Add(Color.ByARGB(transparency, 25, 155, 100));
            colorList.Add(Color.ByARGB(transparency, 5, 55, 0));
            colorList.Add(Color.ByARGB(transparency, 255, 225, 15));
            return colorList;
        }


        //Provides information related to program data
        [MultiReturn(new[] { "DisplayGeomList" })]
        internal static Dictionary<string, object> VisualizeDeptProgramsIn2D(List<DeptData> deptDataInp, double height = 0, int transparency = 255, int colorScheme = 0, bool colorProgramSeparate = false)
        {


            List<DeptData> deptData = deptDataInp;
            deptDataInp = deptData.Select(x => new DeptData(x)).ToList(); // example of deep copy
            if (colorProgramSeparate) return ColorPrograms(deptDataInp, height, transparency, colorScheme);

            if (transparency < 0 || transparency > 255) transparency = 255;

            List<int> indicesList = new List<int>();
            List<Color> colorList = ProvideColorList(transparency), colorListSelected = new List<Color>();

            for (int i = 0; i < colorList.Count; i++) indicesList.Add(i);

            if (colorScheme == 0) colorListSelected = colorList;
            else
            {
                Random ran = new Random(colorScheme);
                List<int> indicesRandomList = BasicUtility.RandomizeList(indicesList, ran);
                for (int i = 0; i < colorList.Count; i++) { colorListSelected.Add(colorList[indicesRandomList[i]]); }
            }
            
            if (deptDataInp == null) return null;
            //List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy
            List<List<Polygon2d>> polyProgsList = new List<List<Polygon2d>>();
            for (int i = 0; i < deptData.Count; i++)
            {
                List<ProgramData> progsInDept = deptData[i].ProgramsInDept;
                List<Polygon2d> polyList = new List<Polygon2d>();
                for (int j = 0; j < progsInDept.Count; j++) polyList.AddRange(progsInDept[j].PolyAssignedToProg);
                polyProgsList.Add(polyList);
            }
            List<List<Surface>> srfListAll = new List<List<Surface>>();
            List<List<Display.Display>> displayListAll = new List<List<Display.Display>>();
            for (int i = 0; i < polyProgsList.Count; i++)
            {
                List<Polygon2d> polyProgs = polyProgsList[i];

                int index = i;
                if (index > colorList.Count) index = 0;
                Color col = colorListSelected[index];
                List<Surface> srfList = new List<Surface>();
                List<Display.Display> displayList = new List<Display.Display>();
                for (int j = 0; j < polyProgs.Count; j++)
                {
                    Polygon2d polyReduced = new Polygon2d(polyProgs[j].Points);
                    //polyReduced = PolygonUtility.PolyExtraEdgeRemove(polyReduced);
                    //polyReduced = PolygonUtility.CreateOrthoPoly(polyReduced);
                    List<Point2d> ptList = polyReduced.Points;
                    List<Point> ptNewList = new List<Point>();
                    for (int k = 0; k < ptList.Count; k++) ptNewList.Add(Point.ByCoordinates(ptList[k].X, ptList[k].Y));
                    Surface srf;
                    try { srf = Surface.ByPerimeterPoints(ptNewList); }
                    catch { continue; }

                    Geometry gm = srf.Translate(0, 0, height);
                    Display.Display dis = Display.Display.ByGeometryColor(gm, col);
                    displayList.Add(dis);
                    //srfList.Add(srf);
                    srf.Dispose();
                    ptNewList.Clear();
                }
                displayListAll.Add(displayList);
                //srfListAll.Add(srfList);

            }

            return new Dictionary<string, object>
            {
                { "DisplayGeomList", (displayListAll) }
            };

        }

        [MultiReturn(new[] { "DisplayGeomList" })]
        internal static Dictionary<string, object> VisualizeDeptProgramsIn3D(List<DeptData> deptDataInp, double height = 0, int transparency = 255, int colorScheme = 0, bool colorProgramSeparate = false)
        {
            List<DeptData> deptData = deptDataInp;
            deptDataInp = deptData.Select(x => new DeptData(x)).ToList(); // example of deep copy
            List<List<Display.Display>> displayListAll = new List<List<Display.Display>>();
            List<double> floorHeightList = deptDataInp[0].FloorHeightList;
            for (int i = 0; i < floorHeightList.Count; i++)
            {
                transparency -= (i+1) * 10;
                Dictionary<string, object> displayObj = VisualizeDeptProgramsIn2D(deptDataInp, floorHeightList[i], transparency, colorScheme, colorProgramSeparate);
                if(displayObj != null) displayListAll.AddRange((List<List<Display.Display>>)displayObj["DisplayGeomList"]); 
            }
            return new Dictionary<string, object>
            {
                { "DisplayGeomList", (displayListAll) }
            };
        }

        //Provides information related to program data
        [MultiReturn(new[] { "DisplayGeomList" })]
        public static Dictionary<string, object> VisualizeDeptPrograms(List<DeptData> deptDataInp, double height = 0, int transparency = 255, int colorScheme = 0, bool colorProgramSeparate = false)
        {
            List<DeptData> deptData = deptDataInp;
            deptDataInp = deptData.Select(x => new DeptData(x)).ToList(); // example of deep copy
            if (deptDataInp[0].Mode3D) { return VisualizeDeptProgramsIn3D(deptDataInp, height, transparency, colorScheme, colorProgramSeparate); }
            else { return VisualizeDeptProgramsIn2D(deptDataInp, height, transparency, colorScheme, colorProgramSeparate); }           
        }

        //Provides information related to program data
        /// <summary>
        /// Provides analytics on Program data after spaces has been assigned.
        /// </summary>
        /// <param name="deptData">List of Department Data Object</param>
        /// <param name="height">Z height of the geometry returned</param>
        /// <returns name="progPolygons">Polygons representing programs.</returns>
        /// <returns name="progPolyOrigin">Centroid of the polygons representing programs.</returns>
        /// <returns name="progNameAsText">Name of the programs.</returns>
        /// <search>
        /// visualize program polgons, program polylines
        /// </search>
        [MultiReturn(new[] { "progPolygons", "progPolyOrigin", "progNameAsText" })]
        public static Dictionary<string, object> VisualizeProgramPolyLinesAndOrigin(List<DeptData> deptData, double height =0, double heightPolylines = 0, bool fullProgramNames = true)
        {
            if (deptData == null) return null;
            List<List<List<Polygon>>> polyDeptListMega = new List<List<List<Polygon>>>();
            List<List<List<Point>>> ptDeptListMega = new List<List<List<Point>>>();
            List<List<string>> nameDeptListMega = new List<List<string>>();
            for (int i = 0; i < deptData.Count; i++)
            {
                List<ProgramData> progInDept = deptData[i].ProgramsInDept;
                List<List<Polygon>> polyDeptList = new List<List<Polygon>>();
                List<List<Point>> ptDeptList = new List<List<Point>>();
                List<string> nameDeptList = new List<string>();
                for (int j = 0; j < progInDept.Count; j++)
                {
                    List<Polygon2d> polyProg = progInDept[j].PolyAssignedToProg;
                    List<Point> ptCenterList = new List<Point>();
                    List<Polygon> polyList = new List<Polygon>();
                   
                    for (int k = 0; k < polyProg.Count; k++)
                    {
                        if (!ValidateObject.CheckPoly(polyProg[k])) continue;
                        Point2d center2d = PolygonUtility.CentroidOfPoly(polyProg[k]);
                        Point center = Point.ByCoordinates(center2d.X, center2d.Y,height+1);
                        ptCenterList.Add(center);
                        double ht = heightPolylines;
                        if (i == 0) ht = height;
                        Polygon poly = DynamoGeometry.PolygonByPolygon2d(polyProg[k], ht);
                        polyList.Add(poly);
                    }
                    polyDeptList.Add(polyList);
                    ptDeptList.Add(ptCenterList);
                    if (fullProgramNames) nameDeptList.Add(progInDept[j].ProgramName);
                    else
                    {
                        if (progInDept[j].ProgramName.IndexOf("#") != -1)
                        {
                            nameDeptList.Add(progInDept[j].ProgramNameShort + " ##");
                        }
                        else
                        {
                            nameDeptList.Add(progInDept[j].ProgramNameShort);
                        }
                       
                    }
                }
                polyDeptListMega.Add(polyDeptList);
                ptDeptListMega.Add(ptDeptList);
                nameDeptListMega.Add(nameDeptList);
            }            
            return new Dictionary<string, object>
            {
                { "progPolygons", (polyDeptListMega) },
                { "progPolyOrigin", (ptDeptListMega) },
                { "progNameAsText", (nameDeptListMega) }
            };
        }


        //Provides information related to program data
        /// <summary>
        /// Provides analytics on Program data after spaces has been assigned.
        /// </summary>
        /// <param name="progData">Program data object</param>
        /// <returns name="ProgramNames">Name of the programs.</returns>
        /// <returns name="NumCellsTaken">Number of cells assgined to each program.</returns>
        /// <returns name="AreaSatisfied">Area of the program is satisfied or not</returns>
        /// <search>
        /// program analytics
        /// </search>
        [MultiReturn(new[] { "ProgramNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "Quantity", "PolyAssignedProgs" })]
        public static Dictionary<string, object> AnalyticsProgramData(List<ProgramData> progData)
        {
            if (progData == null) return null;

            List<string> progNameList = new List<string>();
            List<int> numCellsList = new List<int>();
            List<bool> areaSatisfiedList = new List<bool>();
            List<double> areaNeededList = new List<double>();
            List<double> areaProvidedList = new List<double>();
            List<double> quantList = new List<double>();
            List<List<Polygon2d>> polyProgList = new List<List<Polygon2d>>();
            for (int i = 0; i < progData.Count; i++)
            {
                progNameList.Add(progData[i].ProgramName);
                numCellsList.Add(progData[i].NumberofCellsAdded);
                areaSatisfiedList.Add(progData[i].IsAreaSatisfied);
                areaNeededList.Add(progData[i].ProgAreaNeeded);
                areaProvidedList.Add(progData[i].ProgAreaProvided);
                quantList.Add(progData[i].Quantity);
                polyProgList.Add(progData[i].PolyAssignedToProg);
            }
            return new Dictionary<string, object>
            {
                { "ProgramNames", (progNameList) },
                { "NumCellsTaken", (numCellsList) },
                { "AreaSatisfied", (areaSatisfiedList) },
                { "AreaNeeded", (areaNeededList) },
                { "AreaProvided", (areaProvidedList) },
                { "Quantity", (quantList) },
                { "PolyAssignedProgs", (polyProgList) }
            };
        }

        //Visualizes the space plan scores and displays as text
        /// <summary>
        /// Visualizes space plan scores on the viewport.
        /// </summary>
        /// <param name="totalScore">Total score of the space plan layout.</param>
        /// <param name="programFitScore">Program fitness score of the space plan layout.</param>
        /// <param name="extViewScore">External view score of the space plan layout.</param>
        /// <param name="travelDistScore">Travel distance score of the space plan layout.</param>
        /// <param name="percKPUScore">Percentage KPU score of the space plan layout.</param>
        /// <param name="totalKPURoomsAdded">Total KPU rooms addded to the space plan layout.</param>
        /// <param name="x">X coordinate of the visualization.</param>
        /// <param name="y">Y coordinate of the visualization.</param>
        /// <param name="spacingX">Spacing in the direction of X axis.</param>
        /// <param name="spacingY">Spacing in the direction of Y axis.</param>
        /// <returns name="TextToWrite">String to visualize.</returns>
        /// <returns name="Points">Point at visualiation.</returns>        
        [MultiReturn(new[] { "TextToWrite", "Points", "SiteBoundingBox", "ScoreBox", "TextScale" })]
        public static Dictionary<string, object> SpacePlanFitnessVisualize(double totalScore, double programFitScore,
            double extViewScore, double travelDistScore, double percKPUScore, double totalKPURoomsAdded, Polygon2d insetSiteOutLine = null)
        {
            List<string> textList = new List<string>();
            List<Point> ptList = new List<Point>();
            int num = 6, pad = 8, dist = 10;
            double textScale = dist * 0.7;
            
            Polygon2d bBox = new Polygon2d(ReadData.FromPointsGetBoundingPoly(insetSiteOutLine.Points));
            Point2d origin = PolygonUtility.GetLowestAndHighestPointFromPoly(bBox)[0]; // get lowest pt of bounding poly
            origin = Point2d.ByCoordinates(origin.X - dist, origin.Y - dist);
            double xDim = origin.X + pad, yDim = origin.Y - pad;
            Polygon boundingPoly = DynamoGeometry.PolygonByPolygon2d(bBox,0);
            Surface srfPoly = Surface.ByPatch(boundingPoly);
            double area1 = srfPoly.Area;
            Curve siteBoundingCurve= boundingPoly.Offset(dist);
            Surface srfCurve = Surface.ByPatch(siteBoundingCurve);
            double area2 = srfCurve.Area;
            if(area2<area1) siteBoundingCurve = boundingPoly.Offset(-1*dist);
            srfPoly.Dispose(); srfCurve.Dispose(); boundingPoly.Dispose();

            totalScore = Math.Round(totalScore, 2);
            programFitScore = Math.Round(programFitScore, 4)*100;
            extViewScore = Math.Round(extViewScore, 4)*100;
            travelDistScore = Math.Round(travelDistScore, 4)*100;
            percKPUScore = Math.Round(percKPUScore, 4)*100;
            double spaceY = 10, spaceX = 85;

            // 6 text elements, each lets say takes 10 pt space height
            double heightBox = num * spaceY;
            double widthBox = 2.25 * heightBox;

            Point textBoxCenter = Point.ByCoordinates(origin.X + (widthBox / 2), origin.Y - (heightBox / 2));
            CoordinateSystem cs = CoordinateSystem.ByOrigin(textBoxCenter);
            Rectangle textBox = Rectangle.ByWidthLength(cs, widthBox, heightBox);

            //pts for texts or keys
            for (int i = 0; i < num; i++)
            {
                ptList.Add(Point.ByCoordinates(xDim, yDim));
                yDim -= spaceY;
                //if (i == 0) 
                //else yDim += spacingY;
            }
            //pts for scores or values
            xDim = origin.X + pad + spaceX; yDim = origin.Y - pad;
            for (int i=0;i< num; i++)
            {
                ptList.Add(Point.ByCoordinates(xDim, yDim));
                yDim -= spaceY;
                //if (i == 0) yDim += spacingY + extra;
                //else yDim += spacingY;
            }
            textList.Add("Total Design Score");
            textList.Add("Program Fitted Score");
            textList.Add("External View Score");
            textList.Add("Travel Distance Score");
            textList.Add("KPU Proportion Score");
            textList.Add("Number of KPU Rooms");
            textList.Add(totalScore.ToString());
            textList.Add(programFitScore.ToString());
            textList.Add(extViewScore.ToString());
            textList.Add(travelDistScore.ToString());
            textList.Add(percKPUScore.ToString());
            textList.Add(totalKPURoomsAdded.ToString());
            return new Dictionary<string, object>
            {
                { "TextToWrite", (textList) },
                { "Points", (ptList) },
                { "SiteBoundingBox", (siteBoundingCurve) },
                { "ScoreBox" , (textBox) },
                { "TextScale", (textScale) }
            };
        }

        //scores the space plan layout. currently there are four individual scores and total score is the summation of them.
        /// <summary>
        /// Scores the space plan layout based on four key metrics, program fitness score, external view score, travel distance score, percentage of key planning units score.
        /// </summary>
        /// <param name="deptData">Department data object.</param>
        /// <param name="cellList">List of cell objects for the building outline.</param>
        /// <param name="siteArea">Area of the site.</param>
        /// <param name="programFitWeight">User assigned weight for program fitness score.</param>
        /// <param name="extViewWeight">User assigned weight for external view score.</param>
        /// <param name="traveDistWeight">User assigned weight for travel distance score.</param>
        /// <param name="percKPUWeight">User assigned weight for percentage of key planning units score.</param>
        /// <returns name="TotalScore">Total score of the space plan layout.</returns>
        /// <returns name="ProgramFitScore">Program fitness score of the space plan layout.</returns>
        /// <returns name="ExtViewKPUScore">External view score of the space plan layout.</returns>
        /// <returns name="TravelDistanceScore">Travel distance score of the space plan layout.</returns>
        /// <returns name="PercentageKPUScore">Percentage KPU score of the space plan layout.</returns>
        /// <returns name="TotalKPURoomsAdded">Totol number of KPU rooms added.</returns>
        /// <search>
        /// space plane scoring, space plan metrics
        /// </search>
        [MultiReturn(new[] { "TotalScore", "ProgramFitScore", "ExtViewKPUScore", "TravelDistanceScore", "PercentageKPUScore", "TotalKPURoomsAdded" })]
        public static Dictionary<string, object> SpacePlanFitnessTest(List<DeptData> deptData, List<Cell> cellList, double siteArea = 0, double programFitWeight = 0.6, double extViewWeight = 1, double traveDistWeight = 0.8,
            double percKPUWeight = 0.70)
        {
            if (deptData == null) return null;
            if (cellList == null) return null;
            List<DeptData> deptDataInp = deptData;
            deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy
            DeptData inPatientDeptData = deptData[0];
            List<double> testData = new List<double>();
            List<double> inPatientData = new List<double>();
            int totalPatientRoomCount = 0;
            double areaInpatientRooms = 0, percInpatientFromSite = 0, dim = cellList[0].DimX;
            List<Polygon2d> primaryProgPoly = inPatientDeptData.PolyAssignedToDept;
            List<ProgramData> primaryProg = inPatientDeptData.ProgramsInDept;

            List<Polygon2d> polyprog = new List<Polygon2d>();

            for (int j = 0; j < primaryProg.Count; j++) polyprog.AddRange(primaryProg[j].PolyAssignedToProg);

            totalPatientRoomCount = polyprog.Count;
            for (int i = 0; i < polyprog.Count; i++) { areaInpatientRooms += PolygonUtility.AreaPolygon(polyprog[i]); }


            percInpatientFromSite = areaInpatientRooms / (2 * siteArea);
            testData.Add(totalPatientRoomCount);
            testData.Add(areaInpatientRooms);
            testData.Add(percInpatientFromSite);
            inPatientData.Add(inPatientDeptData.AreaPercentageAchieved);
            inPatientData.Add(inPatientDeptData.DeptAreaNeeded);
            inPatientData.Add(inPatientDeptData.DeptAreaProvided);


            Dictionary<string, object> cellNeighborObj = GridObject.BuildCellNeighborMatrix(cellList);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)cellNeighborObj["CellNeighborMatrix"];
            List<Cell> sortedCells = (List<Cell>)cellNeighborObj["SortedCells"];
            List<int> borderCellIndices = GridObject.GetCornerAndEdgeCellId(cellNeighborMatrix);
            List<Point2d> borderPts = new List<Point2d>();
            List<Point2d> polyCenterList = new List<Point2d>();
            for (int i = 0; i < borderCellIndices.Count; i++)
            {
                borderPts.Add(sortedCells[borderCellIndices[i]].CenterPoint);
            }

            List<bool> getsExternalWall = new List<bool>();
            Point2d buildingCenter = PointUtility.CentroidInPointLists(borderPts);
            List<Polygon2d> polyFlatList = polyprog;//primaryProgPoly; //PolygonUtility.FlattenPolygon2dList(primaryProgPoly);
            double dimPoly = 0, numTrues = 0, travelDistancePatientRms = 0, arbLargeValue = 10000;
            for (int i = 0; i < polyFlatList.Count; i++)
            {
                bool check = false;
                Point2d cenPoly = PointUtility.CentroidInPointLists(polyFlatList[i].Points);
                polyCenterList.Add(cenPoly);
                List<double> spanList = PolygonUtility.GetSpansXYFromPolygon2d(polyFlatList[i].Points);
                if (spanList[0] > spanList[1]) dimPoly = spanList[0];
                else dimPoly = spanList[1];
                double dimAdd = dim + dimPoly;

                travelDistancePatientRms += PointUtility.DistanceBetweenPoints(buildingCenter, cenPoly);

                for (int j = 0; j < borderPts.Count; j++)
                {
                    double distToCell = PointUtility.DistanceBetweenPoints(borderPts[j], cenPoly);
                    if (distToCell <= dimAdd) { check = true; numTrues += 1; break; }
                }
                getsExternalWall.Add(check);
            }
            //double programFitWeight = 0.6, extViewWeight = 1, traveDistWeight = 0.8, percKPUWeight = 0.70;
            double programFitScore = 1, extViewScore = 1, travelDistScore = 1, percKPUScore = 1;


            for (int i = 0; i < deptData.Count; i++) programFitScore += deptData[i].DeptAreaProportionAchieved;
            programFitScore = programFitScore / deptData.Count;
            extViewScore = numTrues / polyFlatList.Count;
            travelDistScore = travelDistancePatientRms / arbLargeValue;
            percKPUScore = inPatientDeptData.DeptAreaProportionAchieved;
            double totalScore = Math.Round(((programFitWeight * programFitScore + extViewWeight * extViewScore +
                                traveDistWeight * travelDistScore + percKPUWeight * percKPUScore) * 40), 2);

            return new Dictionary<string, object>
            {
                { "TotalScore", (totalScore) },
                { "ProgramFitScore", (programFitWeight * programFitScore) },
                { "ExtViewKPUScore", (extViewWeight * extViewScore) },
                { "TravelDistanceScore", (traveDistWeight * travelDistScore) },
                { "PercentageKPUScore", (percKPUWeight * percKPUScore) },
                { "TotalKPURoomsAdded", (totalPatientRoomCount) }
            };
        }


        //scores the space plan layout. currently there are four individual scores and total score is the summation of them.
        /// <summary>
        /// Exports Cell Data in excel format.
        /// </summary>
        /// <param name="deptData">List of Department data object.</param>
        /// <param name="cellList">List of Cell Object.</param>
        /// <param name="cellNeighborMatrix">List of list of integers representing cell neighboring matrix.</param>    
        /// <returns name="CellData">Export of cell data in excel format</returns> 
        /// <search>
        /// export data, cell data
        /// </search>
        public static List<List<string>> ExportCellData(List<DeptData> deptDataInp, List<Cell> cellList, List<List<int>> cellNeighborMatrix)
        {
            

            if (deptDataInp == null) return null;
            if (cellList == null) return null;
            List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy

            List<List<string>> dataToWriteList = new List<List<string>>();
            List<string> cellStrings = new List<string>();
            cellStrings.Add("CELL ID");
            cellStrings.Add("CELL TYPE");
            cellStrings.Add("PRORGAM ASSIGNED");
            cellStrings.Add("DEPT ASSIGNED");
            cellStrings.Add("CELL AVAILABILITY");
            dataToWriteList.Add(cellStrings);

            List<Polygon2d> polyList = new List<Polygon2d>();
            List<string> progrNameList = new List<string>(), deptNameList = new List<string>();
            for (int i = 0; i < deptData.Count; i++)
            {
                if (deptData[i].ProgramsInDept == null) continue;
                for (int j = 0; j < deptData[i].ProgramsInDept.Count; j++)
                {
                    polyList.AddRange(deptData[i].ProgramsInDept[j].PolyAssignedToProg);
                    for(int k=0;k< deptData[i].ProgramsInDept[j].PolyAssignedToProg.Count; k++)
                    {
                        progrNameList.Add(deptData[i].ProgramsInDept[j].ProgramName);
                        deptNameList.Add(deptData[i].DepartmentName);
                    }
                    
                    
                }
            }

            List<Polygon2d> polyListinCell = new List<Polygon2d>();
            List<string> progrNameListinCell = new List<string>(), deptNameListinCell = new List<string>();
            List<string> cellIdList = new List<string>(), cellTypeList = new List<string>(), cellAvailList = new List<string>();
            for (int i = 0; i < cellList.Count; i++)
            {
                bool cellAssigned = false;
                List<int> neighborCells = cellNeighborMatrix[i];
                neighborCells.RemoveAll(s => s == -1);
                for (int j =0;j< polyList.Count; j++)
                {
                    if (GraphicsUtility.PointInsidePolygonTest(polyList[j], cellList[i].LeftDownCorner))
                    {
                        progrNameListinCell.Add(progrNameList[j]); // error
                        deptNameListinCell.Add(deptNameList[j]);
                        cellIdList.Add(i.ToString());
                        if (neighborCells.Count == 2) cellTypeList.Add("CornerCell");
                        else if (neighborCells.Count == 3) cellTypeList.Add("EdgeCell");
                        else  cellTypeList.Add("CoreCell");
                        cellList[i].CellAvailable = false;
                        cellAvailList.Add("False");
                        cellAssigned = true;
                        break;
                    }            
                }
                //cell not inside any prorgam
                if (!cellAssigned)
                {
                    progrNameListinCell.Add("Not Assigned");
                    deptNameListinCell.Add("Not Assigned");
                    cellIdList.Add(i.ToString());
                    if (neighborCells.Count == 2) cellTypeList.Add("CornerCell");
                    else if (neighborCells.Count == 3) cellTypeList.Add("EdgeCell");
                    else cellTypeList.Add("CoreCell");
                    cellList[i].CellAvailable = true;
                    cellAvailList.Add("True");
                }
            
            }

            for(int i = 0; i < cellList.Count; i++)
            {
                cellStrings = new List<string>();
                cellStrings.Add(cellIdList[i]);
                cellStrings.Add(cellTypeList[i]);
                cellStrings.Add(progrNameListinCell[i]);
                cellStrings.Add(deptNameListinCell[i]);
                cellStrings.Add(cellAvailList[i]);
                dataToWriteList.Add(cellStrings);
            }
            return dataToWriteList;
        }



        //exports data to excel
        /// <summary>
        /// Exports Program Data in excel format.
        /// </summary>
        /// <param name="deptData"> List of Department Data object.</param>
        /// <returns name="ProgramDataExport">Export of program data in excel format.</returns>
        /// <search>
        /// export data, program data
        /// </search>
        public static List<List<string>> ExportDepartmentProgramData(List<DeptData> deptDataInp)
        {
            if (deptDataInp == null) return null;
            List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy

            List<List<string>> dataAll = new List<List<string>>();
            List<string> progStrings = new List<string>();
            progStrings.Add("DEPT NAME");
            progStrings.Add("PROG NAME");
            progStrings.Add("PROG AREA NEEDED");
            progStrings.Add("PROG AREA PROVIDED");
            progStrings.Add("PROG NUM POLYS ASSIGNED");
            progStrings.Add("PROG POLY LENGTH");
            progStrings.Add("PROG POLY WIDTH");
            dataAll.Add(progStrings);
            List<ProgramData> progData = new List<ProgramData>();
            for(int i = 0; i < deptData.Count; i++)
            {
                List<List<string>> dataOut = EachDeptProgramDataExport(deptData[i].ProgramsInDept);
                dataAll.AddRange(dataOut);
            }
            return dataAll;
        }


        //exports data to excel
        /// <summary>
        /// Exports Program Data in excel format.
        /// </summary>
        /// <param name="progDataList"> List of program data object.</param>
        /// <returns name="ProgramData">Export of program data in excel format.</returns>
        /// <search>
        /// export data, program data
        /// </search>
        internal static List<List<string>>EachDeptProgramDataExport(List<ProgramData> progDataListInp)
        {
            if (progDataListInp == null) return null;
            List<ProgramData> progDataList = progDataListInp.Select(x => new ProgramData(x)).ToList(); // example of deep copy

            List<List<string>> dataToWriteList = new List<List<string>>();
            List<string> progStrings = new List<string>();
            progStrings.Add("DEPT NAME");
            progStrings.Add("PROG NAME");
            progStrings.Add("PROG AREA NEEDED");
            progStrings.Add("PROG AREA PROVIDED");
            progStrings.Add("PROG NUM POLYS ASSIGNED");
            progStrings.Add("PROG POLY LENGTH");
            progStrings.Add("PROG POLY WIDTH");
            //dataToWriteList.Add(progStrings);
           
            for (int i = 0; i < progDataList.Count; i++)
            {
               
                if (progDataList[i] == null) continue;
                progStrings = new List<string>();
                progStrings.Add(progDataList[i].DeptName.ToString());
                progStrings.Add(progDataList[i].ProgramName.ToString());
                progStrings.Add(progDataList[i].ProgAreaNeeded.ToString());
                progStrings.Add(progDataList[i].ProgAreaProvided.ToString());
                if(progDataList[i].PolyAssignedToProg == null || progDataList[i].PolyAssignedToProg.Count == 0)
                {
                    progStrings.Add(0.ToString());
                    progStrings.Add("Invalid Length");
                    progStrings.Add("Invalid Width");
                }
                else
                {
                    progStrings.Add(progDataList[i].PolyAssignedToProg.Count.ToString());
                    if(progDataList[i].PolyAssignedToProg.Count == 1)
                    {
                        List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(progDataList[i].PolyAssignedToProg[0].Points);
                        progStrings.Add(spans[0].ToString());
                        progStrings.Add(spans[1].ToString());
                    }
                    else
                    {
                        progStrings.Add("Many Poly");
                        progStrings.Add("Many Poly");
                    }
                }
                
                dataToWriteList.Add(progStrings);
            }
            progDataList.Clear();
            return dataToWriteList;
        }

        #endregion

    }
}
