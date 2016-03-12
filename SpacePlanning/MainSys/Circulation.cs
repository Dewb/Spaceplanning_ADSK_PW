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
    public class Circulation
    {


        [MultiReturn(new[] { "Neighbour", "SharedEdge"})]
        public static Dictionary<string, object> FindPolyAdjacentEdge(Polygon2d polyA, Polygon2d polyB)
        {
            /*
                check line by line the adjacent line between two polys
                then join the adjacent line to make one line
                return the adjacent line
            */

            bool check = false;
            if (polyA == null || polyB == null)
            {
                return null;
            }
            Line2d joinedLine = null;
            bool isNeighbour = false;
            double eps = 200;
            Polygon2d polyAReg = new Polygon2d(polyA.Points);
            Polygon2d polyBReg = new Polygon2d(polyB.Points);


            for(int i = 0; i < polyAReg.Points.Count; i++)
            {
                int a = i+1;
                if (i == polyAReg.Points.Count - 1)
                {
                    a = 0;
                }
                Line2d lineA = new Line2d(polyAReg.Points[i], polyAReg.Points[a]);
                for(int j = 0; j < polyBReg.Points.Count; j++)
                {
                    int b = j + 1;
                    if (j == polyAReg.Points.Count - 1)
                    {
                        b = 0;
                    }
                    Line2d lineB = new Line2d(polyAReg.Points[i], polyAReg.Points[b]);
                    bool checkAdj = GraphicsUtility.LineAdjacencyCheck(lineA, lineB);
                    if (checkAdj)
                    {
                        joinedLine = GraphicsUtility.JoinCollinearLines(lineA, lineB);
                        isNeighbour = true;
                        //break;
                    }


                }
            }


           
            
           

            //"Neighbour", "SharedEdgeA", "SharedEdgeB" 

            return new Dictionary<string, object>
            {
                { "Neighbour", (isNeighbour) },
                { "SharedEdge", (joinedLine) }
            };

        }

    }
}
