using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    internal class ProgramData
    {

        // Two private variables for example purposes
        private int _progrID;
        private string _progName;
        private string _progDept;
        private int _progQuanity;
        private double _progUnitArea;
        private int _progPrefValue;
        private List<int> _progAdjList;

        public List<Cell> _progrCell;

        // We make the constructor for this object internal because the 
        // Dynamo user should construct an object through a static method
        public ProgramData(int programID,string programName,string programDept,
            int programQuant,double programUnitArea, int programPrefValue, List<int> programAdjList,List<Cell> programCell)
        {
            _progrID = programID;
            _progName = programName;
            _progDept = programDept;
            _progQuanity = programQuant;
            _progUnitArea = programUnitArea;
            _progPrefValue = programPrefValue;
            _progAdjList = programAdjList;
            _progrCell = programCell;


        }



    }
}
