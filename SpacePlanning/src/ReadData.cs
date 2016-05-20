
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using System.IO;
using stuffer;
using System.Diagnostics;
using System.Reflection;


namespace SpacePlanning
{
    public class ReadData
    {

        #region - Public Methods  

        //read embedded .csv file and make data stack
        //arrange depts on site, till all depts have not been satisfied
        /// <summary>
        /// It arranges the dept on the site based on input from program document
        /// Returns the dept polygon2d and dept  .
        /// </summary>
        /// <param name="DimX">x axis dimension of the grid</param>
        /// <param name="Dimy">y axis dimension of the grid</param>
        /// <param name="CirculationFactor">Multiplier to account for add on circulation area</param>
        /// <returns name="DataStackArray">Data Stack from the embedded .csv</param>
        /// <returns name="ProgramDataObject">Program Data Object containing information from the embedded .csv file</param>
        /// <returns name="DeptDataObject">Department Data Object containing information from the embedded .csv file</param>
        /// <search>
        /// make data stack, embedded data
        /// </search>
        public static List<DeptData> AutoMakeDataStack(double dimX, double dimY, double circulationFactor = 1)
        {

            List<string> progIdList = new List<string>();
            List<string> programList = new List<string>();
            List<string> deptNameList = new List<string>();
            List<string> progQuantList = new List<string>();
            List<string> areaEachProgList = new List<string>();
            List<string> prefValProgList = new List<string>();
            List<string> progAdjList = new List<string>();

            List<List<string>> dataStack = new List<List<string>>();
            List<ProgramData> programDataStack = new List<ProgramData>();

            int readCount = 0;
            //string[] csvText = Properties.Resources.PROGRAMCSV.Split('\n'); 
            Stream res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.ProgramDocument.csv");
            StreamReader reader = new StreamReader(res);
            string docInfo = reader.ReadToEnd();
            string[] csvText = docInfo.Split('\n');
            Trace.WriteLine(csvText);
            foreach (string s in csvText)
            {
                if (s.Length == 0) continue;              
                var values = s.Split(',');
                if (readCount == 0) { readCount += 1; continue; }
                progIdList.Add(values[0]);
                programList.Add(values[1]);
                deptNameList.Add(values[2]);
                progQuantList.Add(values[3]);
                prefValProgList.Add(values[5]);
                progAdjList.Add(values[6]);
                List<Cell> dummyCell = new List<Cell> { new Cell(Point2d.ByCoordinates(0, 0), 0, 0, true) };
                List<string> adjList = new List<string>();
                adjList.Add(values[6]);
                ProgramData progData = new ProgramData(Convert.ToInt16(values[0]), values[1], values[2], Convert.ToInt16(values[3]),
                    Convert.ToDouble(values[4]), Convert.ToInt16(values[6]), adjList, dummyCell, dimX, dimY); // prev multipled circulationfactor with unit area of prog
                programDataStack.Add(progData);
            }// end of for each statement

            List<string> deptNames = GetDeptNames(deptNameList);
            List<DeptData> deptDataStack = new List<DeptData>();

            for (int i = 0; i < deptNames.Count; i++)
            {
                List<ProgramData> progInDept = new List<ProgramData>();
                for (int j = 0; j < programDataStack.Count; j++)
                    if (deptNames[i] == programDataStack[j].DeptName) progInDept.Add(programDataStack[j]);
                List<ProgramData> programBasedOnQuanity = MakeProgramListBasedOnQuantity(progInDept);
                deptDataStack.Add(new DeptData(deptNames[i], programBasedOnQuanity, circulationFactor, dimX, dimY));
            }// end of for loop statement

            //sort the depts by high area
            deptDataStack = SortDeptData(deptDataStack);

            //added to compute area percentage for each dept
            double totalDeptArea = 0;
            for(int i = 0; i < deptDataStack.Count; i++) totalDeptArea += deptDataStack[i].DeptAreaNeeded;
            for (int i = 0; i < deptDataStack.Count; i++) deptDataStack[i].DeptAreaProportion = Math.Round((deptDataStack[i].DeptAreaNeeded / totalDeptArea), 3);

            return SortProgramsByPrefInDept(deptDataStack);
       

        }
        

        //read embedded .sat file and make building outline
        /// <summary>
        /// It arranges the dept on the site based on input from program document
        /// Returns the dept polygon2d and dept  .
        /// </summary>
        /// <returns name="NurbsGeometry">List of Nurbs curve representing the edges of the buildable area</param>
        /// <search>
        /// make site outline, building periphery
        /// </search>
        public static Geometry[] AutoMakeBuildingOutline()
        {
            Stream res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.ATORIGINDK.sat");
            string saveTo = Path.GetTempFileName();
            FileStream writeStream = new FileStream(saveTo, FileMode.Create, FileAccess.Write);

            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = res.Read(buffer, 0, Length);
            while (bytesRead > 0)
            {
                writeStream.Write(buffer, 0, bytesRead);
                bytesRead = res.Read(buffer, 0, Length);
            }
            writeStream.Close();
            return Geometry.ImportFromSAT(saveTo);
        }

        //read provided .csv file and make data stack
        /// <summary>
        /// It arranges the dept on the site based on input from program document
        /// Returns the dept polygon2d and dept  .
        /// </summary>
        /// <param name="DimX">x axis dimension of the grid</param>
        /// <param name="Dimy">y axis dimension of the grid</param>
        /// <param name="CirculationFactor">Multiplier to account for add on circulation area</param>
        /// <returns name="DataStackArray">Data Stack from the embedded .csv</param>
        /// <returns name="ProgramDataObject">Program Data Object containing information from the input .csv file</param>
        /// <returns name="DeptDataObject">Department Data Object containing information from the input .csv file</param>
        /// <search>
        /// make data stack, embedded data
        /// </search>
        [MultiReturn(new[] { "DataStackArray", "ProgramDataObject", "DeptNames", "DeptDataObject" })]
        public static Dictionary<string, IEnumerable<object>> MakeDataStack([ArbitraryDimensionArrayImport]string path, [ArbitraryDimensionArrayImport]double dimX, [ArbitraryDimensionArrayImport]double dimY, double factor = 1)
        {

            var reader = new StreamReader(File.OpenRead(@path));
            List<string> progIdList = new List<string>();
            List<string> programList = new List<string>();
            List<string> deptNameList = new List<string>();
            List<string> progQuantList = new List<string>();
            List<string> areaEachProgList = new List<string>();
            List<string> prefValProgList = new List<string>();
            List<string> progAdjList = new List<string>();
            List<List<string>> dataStack = new List<List<string>>();
            List<ProgramData> programDataStack = new List<ProgramData>();

            int readCount = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (readCount == 0)
                {
                    readCount += 1;
                    continue;
                }
                progIdList.Add(values[0]);
                programList.Add(values[1]);
                deptNameList.Add(values[2]);
                progQuantList.Add(values[3]);
                double areaValue = Convert.ToDouble(values[4]) * factor;
                areaEachProgList.Add(areaValue.ToString());
                prefValProgList.Add(values[5]);
                progAdjList.Add(values[6]);

                Cell dummyC = new Cell(Point2d.ByCoordinates(0, 0), 0, 0, true);
                List<Cell> dummyCell = new List<Cell>();
                dummyCell.Add(dummyC);
                List<string> adjList = new List<string>();
                adjList.Add(values[6]);
                ProgramData progData = new ProgramData(Convert.ToInt16(values[0]), values[1], values[2], Convert.ToInt16(values[3]),
                    Convert.ToDouble(values[4]), Convert.ToInt16(values[5]), adjList, dummyCell, dimX, dimY);
                programDataStack.Add(progData);
            }

            List<string> deptNames = GetDeptNames(deptNameList);
            List<DeptData> deptDataStack = new List<DeptData>();

            for (int i = 0; i < deptNames.Count; i++)
            {
                List<ProgramData> progInDept = new List<ProgramData>();
                for (int j = 0; j < programDataStack.Count; j++)
                {
                    if (deptNames[i] == programDataStack[j].DeptName) progInDept.Add(programDataStack[j]);
                }
                DeptData deptD = new DeptData(deptNames[i], progInDept, factor, dimX, dimY);
                deptDataStack.Add(deptD);
            }

            dataStack.Add(progIdList);
            dataStack.Add(programList);
            dataStack.Add(deptNameList);
            dataStack.Add(progQuantList);
            dataStack.Add(areaEachProgList);
            dataStack.Add(prefValProgList);
            dataStack.Add(progAdjList);

            //added to compute area percentage for each dept
            double totalDeptArea = 0;
            for (int i = 0; i < deptDataStack.Count; i++) totalDeptArea += deptDataStack[i].DeptAreaNeeded;
            for (int i = 0; i < deptDataStack.Count; i++) deptDataStack[i].DeptAreaProportion = Math.Round((deptDataStack[i].DeptAreaNeeded / totalDeptArea),3);


            return new Dictionary<string, IEnumerable<object>>
            {
                { "DataStackArray", (dataStack) },
                { "ProgramDataObject", (programDataStack) },
                { "DeptNames", (deptNames) },
                { "DeptDataObject", (deptDataStack) }
            };


        }

        //reads .sat file and returns geometry
        /// <summary>
        /// It arranges the dept on the site based on input from program document
        /// Returns the dept polygon2d and dept  .
        /// </summary>
        /// <param name="Path">Path of the input sat file as a string</param>
        /// <returns name="NurbsGeometry">List of Nurbs curve representing site outline</param>
        /// <search>
        /// make nurbs curve, site outline
        /// </search>
        public static Geometry[] ReadBuildingOutline(string path)
        {
            return Geometry.ImportFromSAT(path);
        }

        //get point information from outline
        /// <summary>
        /// It arranges the dept on the site based on input from program document
        /// Returns the dept polygon2d and dept  .
        /// </summary>
        /// <param name="NurbsList">List fo Nurbs Geometry</param>
        /// <returns name="DataStackArray">Data Stack from the embedded .csv</param>
        /// <returns name="PointList">Point List representing site outline</param>
        /// <search>
        /// get points of site outline
        /// </search>
        public static List<Point2d> FromOutlineGetPoints(List<NurbsCurve> nurbList)
        {
            List<Point2d> pointList = new List<Point2d>();
            for (int i = 0; i < nurbList.Count; i++)
            {
                Point2d pointPerCurve = Point2d.ByCoordinates(nurbList[i].StartPoint.X, nurbList[i].StartPoint.Y);
                pointList.Add(pointPerCurve);
                if (i == nurbList.Count) pointList.Add(Point2d.ByCoordinates(nurbList[i].EndPoint.X, nurbList[i].EndPoint.Y));
            }
            return pointList;
        }


        //gets the bounding box for a closed polygon2d
        public static List<Point2d> FromPointsGetBoundingPoly(List<Point2d> pointList)
        {
            if (pointList == null) return null;
            if (pointList.Count == 0) return null;
            List<Point2d> pointCoordList = new List<Point2d>();
            List<double> xCordList = new List<double>();
            List<double> yCordList = new List<double>();
            double xMax = 0, xMin = 0, yMax = 0, yMin = 0;
            for (int i = 0; i < pointList.Count; i++)
            {
                xCordList.Add(pointList[i].X);
                yCordList.Add(pointList[i].Y);
            }

            xMax = xCordList.Max();
            yMax = yCordList.Max();

            xMin = xCordList.Min();
            yMin = yCordList.Min();

            pointCoordList.Add(Point2d.ByCoordinates(xMin, yMin));
            pointCoordList.Add(Point2d.ByCoordinates(xMin, yMax));
            pointCoordList.Add(Point2d.ByCoordinates(xMax, yMax));
            pointCoordList.Add(Point2d.ByCoordinates(xMax, yMin));
            return pointCoordList;
        }
        #endregion

        #region-Private Methods
        //get program list quantity and make new list based on it
        internal static List<ProgramData> MakeProgramListBasedOnQuantity(List<ProgramData> progData)
        {
            List<ProgramData> progQuantityBasedList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
                for (int j = 0; j < progData[i].Quantity; j++) progQuantityBasedList.Add(progData[i]);
            return progQuantityBasedList;
        }
        
        //returns the dept names
        internal static List<string> GetDeptNames(List<string> deptNameList)
        {
            List<string> uniqueItemsList = deptNameList.Distinct().ToList();
            return uniqueItemsList;
        }


        //sorts a program data inside dept data based on pREFERENCEPOINT 
        internal static List<DeptData> SortProgramsByPrefInDept(List<DeptData> deptDataInp)
        {
            if (deptDataInp == null) return null;
            List<DeptData> deptData = new List<DeptData>();
            for (int i = 0; i < deptDataInp.Count; i++) deptData.Add(new DeptData(deptDataInp[i]));
            double eps = 01, inc = 0.01;
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData deptItem = deptData[i];
                List<ProgramData> sortedProgramData = new List<ProgramData>();
                List<ProgramData> progItems = deptItem.ProgramsInDept;
                SortedDictionary<double, ProgramData> sortedPrograms = new SortedDictionary<double, ProgramData>();
                List<double> keys = new List<double>();
                for (int j = 0; j < progItems.Count; j++)
                {
                    double key = progItems[j].ProgPrefValue + eps;
                    sortedPrograms.Add(key, progItems[j]);
                    eps += inc;
                }
                eps = 0;
                foreach (KeyValuePair<double, ProgramData> p in sortedPrograms) sortedProgramData.Add(p.Value);
                sortedProgramData.Reverse();
                deptItem.ProgramsInDept = sortedProgramData;
            }
            List<DeptData> newDept = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++) newDept.Add(new DeptData(deptData[i]));
            return newDept;
        }

        //sorts a deptdata based on area 
        internal static List<DeptData> SortDeptData(List<DeptData> deptData)
        {
            SortedDictionary<double, DeptData> sortedD = new SortedDictionary<double, DeptData>();
            for (int i = 0; i < deptData.Count; i++) sortedD.Add(deptData[i].DeptAreaNeeded, deptData[i]);

            List<DeptData> sortedDepartmentData = new List<DeptData>();
            foreach (KeyValuePair<double, DeptData> p in sortedD) sortedDepartmentData.Add(p.Value);
            sortedDepartmentData.Reverse();
            return sortedDepartmentData;
        }

        #endregion


    }
}
