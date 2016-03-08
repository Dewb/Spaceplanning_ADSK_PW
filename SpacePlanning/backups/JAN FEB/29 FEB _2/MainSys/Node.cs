using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;

namespace SpacePlanning
{
    internal class Node
    {

        private Polygon2d _poly;
        private int _key;
        private Line2d _splitLine;
        private DeptData _deptAssigned = null;
        private Node _childNodeA = null;
        private Node _childNodeB = null;
        private Node _parentNode = null;
        private int _treeLevel;

      

        public Node(Polygon2d poly, int key, Line2d splitLine, int treeLevel)
        {
            _poly = poly;
            _key = key;
            _splitLine = splitLine;
            _treeLevel = treeLevel;
            _childNodeA = null;
            _childNodeB = null;
            _parentNode = null;
        }
    }
}
