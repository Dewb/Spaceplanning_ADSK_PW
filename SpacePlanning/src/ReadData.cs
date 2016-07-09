
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using System.IO;
using stuffer;
using System.Diagnostics;
using System.Reflection;
//using Dynamo.Translation;




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
        public static List<DeptData> MakeDataStack(double circulationFactor = 1, int caseStudy = 0, string programDocumentPath = "", bool stackingOptionsDept = false, bool stackingOptionsProg =false, int designSeed = 0)
        {
            double dim = 5;
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
                if (caseStudy == 1) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.MayoProgram_1.csv");
                else if (caseStudy == 2) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.OtherProgram.csv");
                else if (caseStudy == 3) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.ProgramDocument_Reg.csv");
                else if (caseStudy == 4) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.OtherProgram.csv");
                else if (caseStudy == 5) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.MULTIDEPT.csv");
                else res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.ProgramDocument.csv");

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
                progAdjList.Add(values[8]);
                List<Cell> dummyCell = new List<Cell> { new Cell(Point2d.ByCoordinates(0, 0), 0, 0,0, true) };
                //List<string> adjList = new List<string>();
                //adjList.Add(values[8]);
                ProgramData progData = new ProgramData(Convert.ToInt32(values[0]), values[1], values[2], Convert.ToInt32(Convert.ToDouble(values[3])),
                    Convert.ToDouble(values[4]), Convert.ToInt32(values[6]), progAdjList, dummyCell, dim, dim,  Convert.ToString(values[7]), stackingOptionsProg); // prev multipled circulationfactor with unit area of prog
                programDataStack.Add(progData);
            }// end of for each statement

            List<string> deptNames = GetDeptNames(deptNameList);
            List<DeptData> deptDataStack = new List<DeptData>();
            Dictionary<string, object> progAdjWeightObj = FindPreferredProgs(circulationFactor = 1, caseStudy = 0, programDocumentPath,stackingOptionsProg);
            List<int> adjWeightList = (List<int>)progAdjWeightObj["ProgAdjWeightList"];
            for (int i = 0; i < deptNames.Count; i++)
            {
                List<ProgramData> progInDept = new List<ProgramData>();
                for (int j = 0; j < programDataStack.Count; j++)
                    if (deptNames[i] == programDataStack[j].DeptName)
                    {
                        programDataStack[j].AdjacencyWeight = adjWeightList[j];
                        progInDept.Add(programDataStack[j]);
                    }
                List<ProgramData> programBasedOnQuanity = MakeProgramListBasedOnQuantity(progInDept);
                DeptData dept = new DeptData(deptNames[i], programBasedOnQuanity, circulationFactor, dim, dim, stackingOptionsDept);
                deptDataStack.Add(dept);
            }// end of for loop statement
            Dictionary<string, object> programDocObj = FindPreferredDepts(circulationFactor, caseStudy, programDocumentPath,stackingOptionsDept);
            List<string> preferredDept = (List<string>)programDocObj["MostFrequentDeptSorted"];
            //sort the depts by high area
            deptDataStack = SortDeptData(deptDataStack, preferredDept);

            //added to compute area percentage for each dept
            double totalDeptArea = 0;
            for(int i = 0; i < deptDataStack.Count; i++) totalDeptArea += deptDataStack[i].DeptAreaNeeded;
            for (int i = 0; i < deptDataStack.Count; i++) deptDataStack[i].DeptAreaProportionNeeded = Math.Round((deptDataStack[i].DeptAreaNeeded / totalDeptArea), 3);

            return SortProgramsByPrefInDept(deptDataStack, stackingOptionsProg, designSeed);    

        }





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
        [MultiReturn(new[] { "ProgIdList", "ProgramList","DeptNameList", "ProgQuantList","AreaEachProgList",
            "ProgPrefValList","ProgAdjList", "DeptTopoList", "DeptTopoAdjacency" , "EachDeptAdjDeptList",
            "DeptTopListTotal", "DeptNamesUnique", "MostFrequentDept", "MostFrequentDeptSorted"})]
        internal static Dictionary<string, object> FindPreferredDepts(double circulationFactor = 1, int caseStudy = 0, string programDocumentPath = "", bool stackingOptionsProg = false)
        {
            double dim = 5;
            StreamReader reader;
            List<string> progIdList = new List<string>();
            List<string> programList = new List<string>();
            List<string> deptNameList = new List<string>();
            List<string> progQuantList = new List<string>();
            List<string> areaEachProgList = new List<string>();
            List<string> prefValProgList = new List<string>();
            List<string> progAdjList = new List<string>();
            List<string> progTypeList = new List<string>();
            List<List<string>> dataStack = new List<List<string>>();
            List<ProgramData> programDataStack = new List<ProgramData>();
            Stream res;
            if (programDocumentPath == "")
            {
                //string[] csvText = Properties.Resources.PROGRAMCSV.Split('\n'); 
                if (caseStudy == 1) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.MayoProgram_1.csv");
                else if (caseStudy == 2) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.OtherProgram.csv");
                else if (caseStudy == 3) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.ProgramDocument_Reg.csv");
                else if (caseStudy == 4) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.OtherProgram.csv");
                else if (caseStudy == 5) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.MULTIDEPT.csv");
                else res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.ProgramDocument.csv");

                reader = new StreamReader(res);
            }
            else reader = new StreamReader(File.OpenRead(@programDocumentPath));
            int readCount = 0;


            //StreamReader reader = new StreamReader(res);
            string docInfo = reader.ReadToEnd();
            string[] csvText = docInfo.Split('\n');
            //Trace.WriteLine(csvText);
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
                progTypeList.Add(values[7]);
                progAdjList.Add(values[8]);
                List<Cell> dummyCell = new List<Cell> { new Cell(Point2d.ByCoordinates(0, 0), 0, 0, 0, true) };
              
                ProgramData progData = new ProgramData(Convert.ToInt32(values[0]), values[1], values[2], Convert.ToInt32(Convert.ToDouble(values[3])),
                    Convert.ToDouble(values[4]), Convert.ToInt32(values[6]), progAdjList, dummyCell, dim, dim, values[7], stackingOptionsProg); // prev multipled circulationfactor with unit area of prog
                programDataStack.Add(progData);
            }// end of for each statement
            List<List<string>> deptTopList = MakeDeptTopology(progAdjList);
            List<List<string>> deptNameAdjacencyList = new List<List<string>>();
            string kpuDeptName = "";
            int kpuIndex = 0;
            for (int i = 0; i < deptTopList.Count; i++)
            {
                if (progTypeList[i].IndexOf(BuildLayout.KPU.ToLower()) != -1 ||
                   progTypeList[i].IndexOf(BuildLayout.KPU.ToUpper()) != -1) { kpuDeptName = deptNameList[i];  break; }
            }

                for (int i = 0; i < deptTopList.Count; i++)
            {
                bool kpuFound = false;
                List<string> deptNameAdjacency = new List<string>();             
                for (int j = 0; j < deptTopList[i].Count; j++)
                {
                    string depName = deptNameList[Convert.ToInt16(deptTopList[i][j])];
                    deptNameAdjacency.Add(depName);
                }
                deptNameAdjacencyList.Add(deptNameAdjacency);
            }// end of for loop


            List<string> deptNames = GetDeptNames(deptNameList);
            for (int i = 0; i < deptNames.Count; i++) if (deptNames[i] == kpuDeptName) { kpuIndex = i; break; }
  

            List<List<string>> NumberOfDeptNames = new List<List<string>>();
            List<List<string>> NumberOfDeptTop = new List<List<string>>();
            for (int i = 0; i < deptNames.Count; i++)
            {
                List<string> numDeptnames = new List<string>();
                List<string> numDeptTop = new List<string>();
                for (int j = 0; j < deptNameList.Count; j++)
                    if (deptNames[i] == deptNameList[j]) { numDeptnames.AddRange(deptNameAdjacencyList[j]); numDeptTop.AddRange(deptTopList[j]); }
                NumberOfDeptNames.Add(numDeptnames);
                NumberOfDeptTop.Add(numDeptTop);
            }// end of for loop statement


            for (int i = 0; i < NumberOfDeptNames.Count; i++)
            {
                NumberOfDeptNames[i].RemoveAll(x => x == deptNames[i]);
                NumberOfDeptNames[i].RemoveAll(x => x == kpuDeptName);
                if (i == kpuIndex) NumberOfDeptNames[i].Clear();
            }

            List<string> mostFreq = new List<string>();
            for (int i = 0; i < NumberOfDeptNames.Count; i++)
            {
                var most = "";
                if(NumberOfDeptNames[i].Count == 0) most = "";
                else
                {
                    most = (from item in NumberOfDeptNames[i]
                            group item by item into g
                            orderby g.Count() descending
                            select g.Key).First();
                }               
                mostFreq.Add(most);               
            }

            var frequency = mostFreq.GroupBy(x => x).OrderByDescending(x => x.Count()).ToList();
            List<string> depImpList = new List<string>();
            for(int i = 0; i < frequency.Count(); i++) depImpList.AddRange(frequency[i]);

            depImpList = depImpList.Distinct().ToList();
            for (int i = 0; i < depImpList.Count(); i++) depImpList.Remove("");
            return new Dictionary<string, object>
            {
                 { "ProgIdList", (progIdList) },
                 { "ProgramList", (programList) },
                 { "DeptNameList", (deptNameList) },
                 { "ProgQuantList", (progQuantList) },
                 { "AreaEachProgList", (areaEachProgList) },
                 { "ProgPrefValList", (prefValProgList) },
                 { "ProgAdjList", (progAdjList) },
                 { "DeptTopoList", (deptTopList) },
                 { "DeptTopoAdjacency", (deptNameAdjacencyList) },
                 { "EachDeptAdjDeptList", (NumberOfDeptNames) },
                 { "DeptTopListTotal", (NumberOfDeptTop) },
                 { "DeptNamesUnique", (deptNames) },
                 { "MostFrequentDept", (mostFreq) },
                 { "MostFrequentDeptSorted", (depImpList) }

            };
        }
        

        //read embedded .csv file and make data stack
 
        [MultiReturn(new[] { "ProgIdList", "ProgramList","DeptNameList", "ProgQuantList","AreaEachProgList",
            "ProgPrefValList","ProgAdjList", "ProgAdjWeightList"})]
        internal static Dictionary<string, object> FindPreferredProgs(double circulationFactor = 1, int caseStudy = 0, string programDocumentPath = "", bool stackingOptionsProg = false)
        {
            double dim = 5;
            StreamReader reader;
            List<string> progIdList = new List<string>();
            List<string> programList = new List<string>();
            List<string> deptNameList = new List<string>();
            List<string> progQuantList = new List<string>();
            List<string> areaEachProgList = new List<string>();
            List<string> prefValProgList = new List<string>();
            List<string> progAdjList = new List<string>();
            List<string> progTypeList = new List<string>();
            List<List<string>> dataStack = new List<List<string>>();
            List<ProgramData> programDataStack = new List<ProgramData>();
            Stream res;
            if (programDocumentPath == "")
            {
                //string[] csvText = Properties.Resources.PROGRAMCSV.Split('\n'); 
                if (caseStudy == 1) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.MayoProgram_1.csv");
                else if (caseStudy == 2) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.OtherProgram.csv");
                else if (caseStudy == 3) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.ProgramDocument_Reg.csv");
                else if (caseStudy == 4) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.OtherProgram.csv");
                else if (caseStudy == 5) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.MULTIDEPT.csv");
                else res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.ProgramDocument.csv");

                reader = new StreamReader(res);
            }
            else reader = new StreamReader(File.OpenRead(@programDocumentPath));
            int readCount = 0;


            //StreamReader reader = new StreamReader(res);
            string docInfo = reader.ReadToEnd();
            string[] csvText = docInfo.Split('\n');
            //Trace.WriteLine(csvText);
            foreach (string s in csvText)
            {
                if (s.Length == 0) continue;
                var values = s.Split(',');
                if (readCount == 0) { readCount += 1; continue; }
                progIdList.Add(values[0]);
                programList.Add(values[1]);
                deptNameList.Add(values[2]);
                progQuantList.Add(values[3]);
                prefValProgList.Add(values[6]);
                progTypeList.Add(values[7]);
                progAdjList.Add(values[8]);
                List<Cell> dummyCell = new List<Cell> { new Cell(Point2d.ByCoordinates(0, 0), 0, 0, 0, true) };

                ProgramData progData = new ProgramData(Convert.ToInt32(values[0]), values[1], values[2], Convert.ToInt32(Convert.ToDouble(values[3])),
                    Convert.ToDouble(values[4]), Convert.ToInt32(values[6]), progAdjList, dummyCell, dim, dim, values[7], stackingOptionsProg); // prev multipled circulationfactor with unit area of prog
                programDataStack.Add(progData);
            }// end of for each statement

            List<string> progAdjId = new List<string>();
            for (int i = 0; i < progIdList.Count; i++)
            {
                string adjacency = progAdjList[i];
                List<string> adjList = adjacency.Split('.').ToList();
                progAdjId.AddRange(adjList);
            }
            List<string> strList = new List<string>();
            List<int> numIdList = new List<int>();
            for (int i = 0; i < progAdjId.Count; i++)
            {
                strList.Add(progAdjId[i]);
                int value = Int32.Parse(progAdjId[i]);
                numIdList.Add(value);
                progAdjId[i] = value.ToString();
            }
            List<int> adjWeightList = new List<int>();
            for (int i=0;i< progIdList.Count; i++)
            {
                int count = 0;
                for(int j = 0; j < progAdjId.Count; j++) if (i == numIdList[j]) count += 1;
                adjWeightList.Add(count);
            }

            return new Dictionary<string, object>
            {
                 { "ProgIdList", (progIdList) },
                 { "ProgramList", (programList) },
                 { "DeptNameList", (deptNameList) },
                 { "ProgQuantList", (progQuantList) },
                 { "AreaEachProgList", (areaEachProgList) },
                 { "ProgPrefValList", (prefValProgList) },
                 { "ProgAdjList", (progAdjList) },
                 { "ProgAdjWeightList" ,(adjWeightList) }
            };
        }






        internal static string FindDeptForProgId(List<string> deptName, int id = 5)
        {
            return deptName[id];
        }


        internal static List<List<string>> MakeDeptTopology(List<string> adjList)
        {
            List<List<string>> stringNumber = new List<List<string>>();
            foreach (string s in adjList) stringNumber.Add(s.Split('.').ToList());
            return stringNumber;
        }


        //read embedded .sat file and make site outline || 0 = .sat file, 1 = .dwg file
        /// <summary>
        /// Forms site outline from the embedded .sat file.
        /// Returns list of nurbs curve geometry.
        /// </summary>
        /// <returns name="GeomList">List of nurbs curve representing the site outline</returns>
        /// <search>
        /// make site outline, site geometry.
        /// </search>
        internal static Geometry[] GetSiteOutline(int caseStudy =0, string siteOutlinePath = "")
        {
            string path;
            if (siteOutlinePath == "")
            {
                Stream res;
                if (caseStudy == 1) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.siteMayo.sat");
                else if (caseStudy == 2) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.otherSite.sat");
                else if (caseStudy == 3) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.site3.sat");
                else if (caseStudy == 4) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.site4.sat");
                else res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.site1.sat"); //"SpacePlanning.src.Asset.ATORIGINDK.sat"
                path = Path.GetTempFileName();
                FileStream writeStream = new FileStream(path, FileMode.Create, FileAccess.Write);

                int Length = 256;
                Byte[] buffer = new Byte[Length];
                int bytesRead = res.Read(buffer, 0, Length);
                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = res.Read(buffer, 0, Length);
                }
                writeStream.Close();
                return Geometry.ImportFromSAT(path);
                //return ConvertSiteOutlineToPolygon2d(Geometry.ImportFromSAT(path).ToList());
            }
            else path = siteOutlinePath;

            if(path.IndexOf(".sat") != -1)
            {
                return Geometry.ImportFromSAT(path);
                //return ConvertSiteOutlineToPolygon2d(Geometry.ImportFromSAT(path).ToList());
            }
            else if (path.IndexOf(".dwg") != -1)
            {
                return null;
                /*
                FileLoader file = FileLoader.FromPath(path);
                ImportedObject[] impObj = file.GetImportedObjects().ToArray();
                return ImportedObject.ConvertToGeometries(impObj).ToArray();
                //return ConvertSiteOutlineToPolygon2d(ImportedObject.ConvertToGeometries(impObj).ToList());
                */
            }
            else return null;
        }
        //read embedded .sat file and make site outline || 0 = .sat file, 1 = .dwg file
        /// <summary>
        /// Forms site outline from the embedded .sat file.
        /// Returns list of nurbs curve geometry.
        /// </summary>
        /// <returns name="geomLIst">List of nurbs curve representing the site outline</returns>
        /// <search>
        /// make site outline, site geometry, site.
        /// </search>
        public static Geometry[] GetSiteOutlineFromSat(int caseStudy = 0, string siteOutlinePath = "")
        {
            string path;
            if (siteOutlinePath == "")
            {
                Stream res;
                if (caseStudy == 1) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.siteMayo.sat");
                else if (caseStudy == 2) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.otherSite.sat");
                else if (caseStudy == 3) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.site3.sat");
                else if (caseStudy == 4) res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.site4.sat");
                else res = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SpacePlanning.src.Asset.site1.sat"); //"SpacePlanning.src.Asset.ATORIGINDK.sat"
                path = Path.GetTempFileName();
                FileStream writeStream = new FileStream(path, FileMode.Create, FileAccess.Write);

                int Length = 256;
                Byte[] buffer = new Byte[Length];
                int bytesRead = res.Read(buffer, 0, Length);
                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = res.Read(buffer, 0, Length);
                }
                writeStream.Close();
                return Geometry.ImportFromSAT(path);
                //return ConvertSiteOutlineToPolygon2d(Geometry.ImportFromSAT(path).ToList());
            }
            else path = siteOutlinePath;

            if (path.IndexOf(".sat") != -1)
            {
                return Geometry.ImportFromSAT(path);
                //return ConvertSiteOutlineToPolygon2d(Geometry.ImportFromSAT(path).ToList());
            }
            else return null;
        }


        //get point information from outline
        /// <summary>
        /// Retrieves and orders list of point2d geometry from the site outline. 
        /// Returns ordered point2d list geometry.
        /// </summary>
        /// <param name="geomList">List of nurbs geometry</param>
        /// <param name="inset">Integer to inset the given site outline.</param>
        /// <returns name="InsetSiteOutline">Site outline inset for form building computation.</returns>
        /// <search>
        /// get points of site outline
        /// </search>
        [MultiReturn(new[] { "InsetSiteOutline" })]
        public static Dictionary<string,object> ConvertSiteOutlineToPolygon2d(List<Geometry> geomList, double inset = 5)// Geometry
        {
            string type = geomList[0].GetType().ToString();
            List<Point2d> pointList = new List<Point2d>();
            //Trace.WriteLine("list found is + " + type);
            List<NurbsCurve> nurbList = new List<NurbsCurve>();
            if (type.IndexOf("Line") != -1)
            {
                List<Line> lineList = new List<Line>();
                for (int i = 0; i < geomList.Count; i++) lineList.Add((Line)geomList[i]);
                for (int i = 0; i < lineList.Count; i++)
                {
                    Point2d pointPerCurve = Point2d.ByCoordinates(lineList[i].StartPoint.X, lineList[i].StartPoint.Y);
                    pointList.Add(pointPerCurve);
                    if (i == lineList.Count) pointList.Add(Point2d.ByCoordinates(lineList[i].EndPoint.X, lineList[i].EndPoint.Y));
                }
            }
            else if(type.IndexOf("NurbsCurve") != -1)
            {
                
                nurbList = new List<NurbsCurve>();
                for (int i = 0; i < geomList.Count; i++) { nurbList.Add((NurbsCurve)geomList[i]); }
                /*
                for (int i = 0; i < nurbList.Count; i++)
                {
                    Point2d pointPerCurve = Point2d.ByCoordinates(nurbList[i].StartPoint.X, nurbList[i].StartPoint.Y);
                    pointList.Add(pointPerCurve);
                    if (i == nurbList.Count) pointList.Add(Point2d.ByCoordinates(nurbList[i].EndPoint.X, nurbList[i].EndPoint.Y));
                }
                */
            }
            else return null;

            PolyCurve pCrv = PolyCurve.ByJoinedCurves((Curve[])nurbList.ToArray());


            Curve cInsetA = pCrv.Offset(inset);
            Curve cInsetB = pCrv.Offset(inset*-1);
            Curve cInset;
            Surface srfA = Surface.ByPatch(cInsetA), srfB = Surface.ByPatch(cInsetB);
            if (srfA.Area > srfB.Area) cInset = cInsetB;
            else cInset = cInsetA;

            srfA.Dispose(); srfB.Dispose();
            List<Curve> curvList = new List<Curve>();
            geomList = cInset.Explode().ToList();
            for (int i = 0; i < geomList.Count; i++) { curvList.Add((Curve)geomList[i]); }
            for (int i = 0; i < curvList.Count; i++)
            {
                Point2d pointPerCurve = Point2d.ByCoordinates(curvList[i].StartPoint.X, curvList[i].StartPoint.Y);
                pointList.Add(pointPerCurve);
                if (i == curvList.Count) pointList.Add(Point2d.ByCoordinates(curvList[i].EndPoint.X, curvList[i].EndPoint.Y));
            }
            //return new Polygon2d(pointList);
            return new Dictionary<string, object>
            {
                { "InsetSiteOutline", (new Polygon2d(pointList)) }
            };
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
        internal static List<Point2d> FromPointsGetBoundingPoly(List<Point2d> pointList)
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

        internal static Polygon2d GetBoundingBoxfromLines(List<Line2d> lineList)
        {
            if (lineList == null) return null;
            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < lineList.Count; i++) ptList.Add(LineUtility.LineMidPoint(lineList[i]));
            return new Polygon2d(FromPointsGetBoundingPoly(ptList));
        }
        #endregion

        #region-Private Methods
        //get program list quantity and make new list based on it
        internal static List<ProgramData> MakeProgramListBasedOnQuantity(List<ProgramData> progData)
        {
            //List<ProgramData> progQuantityBasedList = new List<ProgramData>();
            List<ProgramData> progQuantityBasedList = progData.Select(x => new ProgramData(x)).ToList(); // example of deep copy

            for (int i = 0; i < progData.Count; i++)
                for (int j = 0; j < progData[i].Quantity-1; j++) progQuantityBasedList.Add(progData[i]);
            List<ProgramData> progReturn = progQuantityBasedList.Select(x => new ProgramData(x)).ToList();
            return progReturn;
        }
        
        //returns the dept names
        internal static List<string> GetDeptNames(List<string> deptNameList)
        {
            List<string> uniqueItemsList = deptNameList.Distinct().ToList();
            return uniqueItemsList;
        }

        //sorts a program data inside dept data based on PREFERENCEPOINT 
        internal static List<DeptData> SortProgramsByPrefInDept(List<DeptData> deptDataInp, bool stackingOptions = false, int designSeed = 0)
        {
            double weight = 100;
            if (deptDataInp == null) return null;
            List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy

            //for (int i = 0; i < deptDataInp.Count; i++) deptData.Add(new DeptData(deptDataInp[i]));
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
                    double key = progItems[j].ProgPreferenceVal + eps + weight * progItems[j].AdjacencyWeight;
                    //double key = progItems[j].ProgPreferenceVal + eps;
                    try { sortedPrograms.Add(key, progItems[j]); }
                    catch { Random rand = new Random(j);  key += rand.NextDouble(); }
                    progItems[j].ProgramCombinedAdjWeight = key;
                    eps += inc;
                }
                eps = 0;
                foreach (KeyValuePair<double, ProgramData> p in sortedPrograms) sortedProgramData.Add(p.Value);
                sortedProgramData.Reverse();

                if (stackingOptions) sortedProgramData = RandomizeProgramList(sortedProgramData,designSeed);
                deptItem.ProgramsInDept = sortedProgramData;
            }
            List<DeptData> newDept = deptData.Select(x => new DeptData(x)).ToList(); // example of deep copy
            return newDept;
        }

        //randomize program data
        internal static List<ProgramData> RandomizeProgramList(List<ProgramData> progList, int designSeed = 0)
        {
            if (progList == null) return null;
            List<ProgramData> progListNew = progList.Select(x => new ProgramData(x)).ToList();
            List<int> indices = new List<int>();
            for (int i = 0; i < progList.Count; i++) indices.Add(i);
            List<int> indicesRandom = BasicUtility.RandomizeList(indices, new Random(designSeed));
            List<ProgramData> progListOut = new List<ProgramData>();
            for (int i = 0; i < progListNew.Count; i++) progListOut.Add(progListNew[indicesRandom[i]]);
            return progListOut;
        }



        //sorts a deptdata based on area 
        internal static List<DeptData> SortDeptData(List<DeptData> deptDataInp, List<string> preferredDept)
        {

            List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy
            SortedDictionary<double, DeptData> sortedD = new SortedDictionary<double, DeptData>();
            List<double> areaList = new List<double>(), weightList = new List<double>();
            List<string> deptFound = new List<string>();
            //Queue<string> preferredDeptQueue = new Queue<string>();
            for (int i = 0; i < preferredDept.Count; i++) weightList.Add(10000000 - (i + 1) * 1000);
           
           
                
            for (int i = 0; i < deptData.Count; i++)
            {
                bool match = false;
                for (int j = 0; j < preferredDept.Count; j++)
                {
                    if (preferredDept[j] == deptData[i].DepartmentName)
                    {
                        areaList.Add(weightList[j]);
                        match = true;
                        deptFound.Add(preferredDept[j]);
                        deptData[i].DeptAdjacencyWeight = weightList[j];
                        break;
                    }
                }
                if (!match) { areaList.Add(0); deptFound.Add(""); deptData[i].DeptAdjacencyWeight = areaList[i]; }
            }// end of forloop
               
            for (int i = 0; i < deptData.Count; i++)
            {   
                double surpluss = 0;
                double eps = i * BasicUtility.RandomBetweenNumbers(new Random(i),50,10);
               
                    if (deptData[i].DepartmentType.IndexOf(BuildLayout.KPU.ToLower()) != -1 || deptData[i].DepartmentType.IndexOf(BuildLayout.KPU.ToUpper()) != -1)
                        surpluss = 1000000000 + eps + areaList[i];
                    else
                        surpluss = areaList[i];
               

                double area = 0.25 * deptData[i].DeptAreaNeeded + surpluss;
                deptData[i].DeptAdjacencyWeight = area;
                sortedD.Add(area, deptData[i]);
            }

            List<DeptData> sortedDepartmentData = new List<DeptData>();
            foreach (KeyValuePair<double, DeptData> p in sortedD) sortedDepartmentData.Add(p.Value);
            sortedDepartmentData.Reverse();
            return sortedDepartmentData;
        }


       

        #endregion


    }
}
