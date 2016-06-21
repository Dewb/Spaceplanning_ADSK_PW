
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
    /// <summary>
    /// A static class to read contextual data and build the data stack about site outline and program document.
    /// </summary>
    public static class ReadData
    {
        ///////////////////////////////////////////////////////////////////
        /// NOTE: This project requires REFERENCEPOINTs to the ProtoInterface
        /// and ProtoGeometry DLLs. These are found in the Dynamo install
        /// directory.
        ///////////////////////////////////////////////////////////////////

        #region - Public Methods  
        //read embedded .csv file and make data stack
        /// <summary>
        /// Builds the data stack from the embedded program document.
        /// Returns the Dept Data object
        /// </summary>
        /// <param name="dimX">x axis dimension of the grid.</param>
        /// <param name="dimY">y axis dimension of the grid.</param>
        /// <param name="circulationFactor">Multiplier to account for add on circulation area.</param>
        /// <returns name="DeptData">List of department data object from the provided program document.</returns>
        /// <search>
        /// make data stack, dept data object, program data object
        /// </search>
        public static List<DeptData> AutoMakeDataStack(double dimX, double dimY, double circulationFactor = 1, int caseStudy = 0, [ArbitraryDimensionArrayImport]string programDocumentPath = null)
        {
            StreamReader reader;
            List<string> progIdList = new List<string>();
            List<string> programList = new List<string>();
            List<string> deptNameList = new List<string>();
            List<string> progQuantList = new List<string>();
            List<string> areaEachProgList = new List<string>();
            List<string> prefValProgList = new List<string>();
            List<string> progAdjList = new List<string>();

            List<List<string>> dataStack = new List<List<string>>();
            List<ProgramData> programDataStack = new List<ProgramData>();
            Stream res;
            if (programDocumentPath == "")
            {               
                //string[] csvText = Properties.Resources.PROGRAMCSV.Split('\n'); 
                if (caseStudy == 1) res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.MayoProgram_1.csv");
                else if (caseStudy == 2) res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.OtherProgram.csv");
                else if (caseStudy == 3) res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.MayoProgram_1.csv");
                else if (caseStudy == 4) res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.OtherProgram.csv");
                else res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.ProgramDocument.csv");

                reader = new StreamReader(res);
            }
            else reader = new StreamReader(File.OpenRead(@programDocumentPath));
            int readCount = 0;
            
       
            //StreamReader reader = new StreamReader(res);
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
            for (int i = 0; i < deptDataStack.Count; i++) deptDataStack[i].DeptAreaProportionNeeded = Math.Round((deptDataStack[i].DeptAreaNeeded / totalDeptArea), 3);

            return SortProgramsByPrefInDept(deptDataStack);
       

        }


        //read embedded .sat file and make site outline
        /// <summary>
        /// Forms site outline from the embedded .sat file.
        /// Returns list of nurbs curve geometry.
        /// </summary>
        /// <returns name="NurbsGeometry">List of nurbs curve representing the site outline</returns>
        /// <search>
        /// make site outline, site geometry.
        /// </search>
        public static Geometry[] AutoMakeSiteOutline(int caseStudy =0)
        {
            Stream res;
            if(caseStudy == 1) res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.siteMayo.sat");
            else if (caseStudy == 2) res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.otherSite.sat");
            else if (caseStudy == 3) res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.site3.sat");
            else if (caseStudy == 4) res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.site4.sat");
            else res = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.ATORIGINDK.sat");
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

        //read user input .csv file and make data stack
        /// <summary>
        /// Builds the data stack from the embedded program document.
        /// Returns the Dept Data object
        /// </summary>
        /// <param name="path">Path of the input program document</param>
        /// <param name="dimX">X axis dimension of the cell grid</param>
        /// <param name="dimY">Y axis dimension of the cell grid</param>
        /// <param name="circulationFactor">Multiplier to accomodate circulation spaces with the program</param>
        /// <returns name="DataStackArray">Data Stack from the embedded .csv</returns>
        /// <returns name="ProgramDataObject">Program Data Object containing information from the input .csv file</returns>
        /// <returns name="DeptDataObject">Department Data Object containing information from the input .csv file</returns>
        /// <search>
        /// make data stack, embedded data
        /// </search>
        public static List<DeptData> MakeDataStack([ArbitraryDimensionArrayImport]string path, [ArbitraryDimensionArrayImport]double dimX, [ArbitraryDimensionArrayImport]double dimY, double circulationFactor = 1)
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

                if (readCount == 0) { readCount += 1; continue; }
                progIdList.Add(values[0]);
                programList.Add(values[1]);
                deptNameList.Add(values[2]);
                progQuantList.Add(values[3]);
                double areaValue = Convert.ToDouble(values[4]) * circulationFactor;
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
                for (int j = 0; j < programDataStack.Count; j++) if (deptNames[i] == programDataStack[j].DeptName) progInDept.Add(programDataStack[j]);
                deptDataStack.Add(new DeptData(deptNames[i], progInDept, circulationFactor, dimX, dimY));
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
            for (int i = 0; i < deptDataStack.Count; i++) deptDataStack[i].DeptAreaProportionNeeded = Math.Round((deptDataStack[i].DeptAreaNeeded / totalDeptArea),3);
            return SortDeptData(deptDataStack);
        }








        //reads .sat file and returns nurbs geometry
        /// <summary>
        /// Reads the site outline from the input path to a .sat file
        /// Returns list of nurbs curve as geometry representing site outline.
        /// </summary>
        /// <param name="path">Path of the input .sat file</param>
        /// <returns name="nurbsGeometry">List of nurbs curve representing site outline</returns>
        /// <search>
        /// nurbs curve, site outline
        /// </search>
        public static Geometry[] MakeSiteOutline(string path)
        {
            return Geometry.ImportFromSAT(path);
        }

        //get point information from outline
        /// <summary>
        /// Retrieves and orders list of point2d geometry from the site outline. 
        /// Returns ordered point2d list geometry.
        /// </summary>
        /// <param name="nurbList">List of nurbs geometry</param>
        /// <returns name="pointList">List of point2d representing site outline</returns>
        /// <search>
        /// get points of site outline
        /// </search>
        public static List<Point2d> FromSiteOutlineGetPoints(List<NurbsCurve> nurbList)
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


        //builds the bounding box for a closed polygon2d
        /// <summary>
        /// Builds a bounding box polygon2d from input point2d representing site outline.
        /// Returns list of point2d, representing site outline.
        /// </summary>
        /// <param name="pointList">List of nurbs geometry</param>
        /// <returns name="listPoint2d">List of point2d representing site outline</returns>
        /// <search>
        /// get points of site outline
        /// </search>
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
                    double key = progItems[j].ProgPreferenceVal + eps;
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
