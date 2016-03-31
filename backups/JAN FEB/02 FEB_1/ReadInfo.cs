
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
//using Excel = Microsoft.Office.Interop.Excel;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
using System.IO;
using stuffer;

///////////////////////////////////////////////////////////////////
/// NOTE: This project requires references to the ProtoInterface
/// and ProtoGeometry DLLs. These are found in the Dynamo install
/// directory.
///////////////////////////////////////////////////////////////////

namespace SpacePlanning
{
    public class ReadInfo
    {
        // Two private variables for example purposes
        private double _a;
        private double _b;



        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        internal ReadInfo(double a, double b)
        {
            _a = a;
            _b = b;
        }

      

        /// <summary>
        /// An example of how to construct an object via a static method.
        /// This is needed as Dynamo lacks a "new" keyword to construct a 
        /// new object
        /// </summary>
        /// <param name="a">1st number. This will be stored in the Class.</param>
        /// <param name="b">2nd number. This will be stored in the Class</param>
        /// <returns>A newly-constructed ZeroTouchEssentials object</returns>
        public static ReadInfo ByTwoDoubles(double a, double b)
        {
            return new ReadInfo(a, b);
        }

        /// <summary>
        /// Example property returning the value _a inside the object
        /// </summary>
        public double A
        {
            get { return _a; }
        }


        //TO READ SAT FILE CONTAINING NURBS CURVE
        public static Autodesk.DesignScript.Geometry.Geometry[] readSAT(string path)
        {
            Autodesk.DesignScript.Geometry.Geometry[] nurbCurve =  Autodesk.DesignScript.Geometry.Geometry.ImportFromSAT(path);
            
            return nurbCurve;
           
        }

        //GET INPUT POINT LIST ( FROM SAT FILE ) AND MAKE A GRID OF SQUARES
        public static void makeSquaresFromPointList(Autodesk.DesignScript.Geometry.Point[] ptList)
        {
            double gridSize = 100;
            double flrHeight = 5;
            GridBasis gr = new GridBasis(gridSize, flrHeight);

            
                

        }

        //READ THE POINTS FROM THE INPUT NURBS CURVE
        public static void pointListOutline(string path)
        {
            Autodesk.DesignScript.Geometry.Geometry[] nurbCurve = Autodesk.DesignScript.Geometry.Geometry.ImportFromSAT(path);
        }

        //TO READ .CSV FILE AND ACCESS THE DATA
        public static List<List<string>> readCSV(string path)
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
                areaEachProgList.Add(values[4]);
                prefValProgList.Add(values[5]);
                progAdjList.Add(values[6]);



            }

            dataStack.Add(progIdList);
            dataStack.Add(programList);
            dataStack.Add(deptNameList);
            dataStack.Add(progQuantList);
            dataStack.Add(areaEachProgList);
            dataStack.Add(prefValProgList);
            dataStack.Add(progAdjList);

            return dataStack;


        }

        public static List<Point> makePointGrid(List<List<string>> dataStack)
        {

            int lenPointLists = dataStack[0].Count;
            int count = 0;
            double x= 0, y = 0;
            double addX = 10, addY = 10;

            List<Point> ptList = new List<Point>();

            for(int i = 0; i < lenPointLists; i++)
            {
                //ptList.Add(Point.ByCoordinates(0, 0));
            }


            for(int i =0; i<lenPointLists; i++)
            {
                count += 1;
                if (count > 5)
                {
                    count = 0;
                    x = 0;
                    y += addY;
                }

                ptList.Add(Point.ByCoordinates(x, y));
                x += addX;


            }

            return ptList;

        }


        public static List<Circle> makeBubbles(Point[] ptList, List<List<string>> dataStack, double scale, Boolean tag)
        {
            List<Circle> cirList = new List<Circle>();
            List<string> progAreaList = dataStack[3];
            List<string> progQuantList = dataStack[4];

            List<double> progTotalList = new List<double>();
            for(int i =0; i < progAreaList.Count; i++)
            {
                double totArea = 0;
                if (!tag)                   
                {
                   totArea = Convert.ToDouble(progAreaList[i]) * Convert.ToDouble(progQuantList[i]);
                }
                else
                {
                    totArea = Convert.ToDouble(progAreaList[i]);
                }
                
                progTotalList.Add(totArea);
            }

            double maxArea = progTotalList.Max();
            List<double> progNormalizedTotArea = new List<double>();
            for (int i = 0; i < progTotalList.Count; i++)
            {
                progNormalizedTotArea.Add(progTotalList[i]*scale/maxArea);
            }

            for (int i = 0; i < progNormalizedTotArea.Count; i++)
            {
                Circle cir = Circle.ByCenterPointRadius(ptList[i], progNormalizedTotArea[i]);
                cirList.Add(cir);
            }

            return cirList;

        }

        [MultiReturn(new[] { "add", "mult" })]
        public static Dictionary<string, object> ReturnMultiExample(double a, double b)
        {
            return new Dictionary<string, object>
            {
                { "add", (a + b) },
                { "mult", (a * b) }
            };
        }

        //MAKING ADJACENCY LIST AND LINE CONNECTION BETWEEN POINTS
        public static Dictionary<string, object> makeAdjacencyGraph(Point[] ptList, List<List<string>> dataStack)
        {
            List<int> idList = new List<int>();
            List<string> adjList = dataStack[6];


            List<List<int>> adjListPlace = new List<List<int>>();
            Random rnd1 = new Random();
            Random rnd2 = new Random();
            for (int i =0; i < adjList.Count; i++)
            {
                
                int randomNum = rnd1.Next(0, 7);                
                List<int> adjAdd = new List<int>();

                for (int j = 0; j < randomNum; j++)
                {
                    int num = rnd2.Next(0, 15);
                    adjAdd.Add(num);
                }
                adjListPlace.Add(adjAdd);
            }

            List<List<Line>> allLinList = new List<List<Line>>();
            for (int i =0; i < adjList.Count; i++)
            {
                List<int> adjAdd = new List<int>();
                adjAdd = adjListPlace[i];
                List<Line> linList = new List<Line>();
                for (int j = 0; j < adjAdd.Count; j++)
                {
                    if(ptList[i].X == ptList[j].X && ptList[i].Y == ptList[j].Y)
                    {
                        continue;
                    }
                    Line ln = Line.ByStartPointEndPoint(ptList[i], ptList[j]);
                    linList.Add(ln);
                }
                allLinList.Add(linList);              

            }


            return new Dictionary<string, object>
            {
                { "AdjacencyList", adjListPlace },
                { "ConnectionList", allLinList }
            };

        }

        public static void pushBubbles(Point[] ptList, List<List<string>> dataStack)
        {

        }

   

    


    }
}
