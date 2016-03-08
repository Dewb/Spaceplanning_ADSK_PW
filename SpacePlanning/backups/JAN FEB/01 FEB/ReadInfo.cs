
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


        //TO READ .CSV FILE AND ACCESS THE DATA
        public static List<List<string>> readCSV(string path)
        {
            
            var reader = new StreamReader(File.OpenRead(@path));

            List<string> programList = new List<string>();
            List<string> deptNameList = new List<string>();
            List<string> progQuantList = new List<string>();
            List<string> areaEachProgList = new List<string>();
            List<string> prefValProgList = new List<string>();

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
                programList.Add(values[0]);
                deptNameList.Add(values[1]);
                progQuantList.Add(values[2]);
                areaEachProgList.Add(values[3]);
                prefValProgList.Add(values[4]);
               


            }

            dataStack.Add(programList);
            dataStack.Add(deptNameList);
            dataStack.Add(progQuantList);
            dataStack.Add(areaEachProgList);
            dataStack.Add(prefValProgList);


            return dataStack;


        }

        
        //TO READ EXCEL FILE JUST FROM THE SUPPLIED PATH
        public static DataTable readExcel(string path)
        {

            string fullpath = "";
            fullpath = @"Provider = Microsoft.Jet.OLEDB.4.0; Data Source = " + path + ";Extended Properties=Excel 8.0";  //@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Book1.xls;Extended Properties=Excel 8.0"

            OleDbConnection con = new OleDbConnection(fullpath);
            OleDbDataAdapter da = new OleDbDataAdapter("select * from MyObject", con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }




        private DataTable GetDataTable(string sql, string connectionString)
        {
            DataTable dt = null;

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                {
                    using (OleDbDataReader rdr = cmd.ExecuteReader())
                    {
                        dt.Load(rdr);
                        return dt;
                    }
                }
            }
        }

        /*
        private void GetExcel(string path)
        {
            //string fullPathToExcel = "<Path to Excel file>"; //ie C:\Temp\YourExcel.xls
            string fullPathToExcel = path;
            string connString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=yes'", fullPathToExcel);
            DataTable dt = Function_Library.DatabaseFunctions.GetDataTable("SELECT * from [SheetName$]", connString);

            foreach (DataRow dr in dt.Rows)
            {
                //Do what you need to do with your data here
            }
        }
        */
    }
}
