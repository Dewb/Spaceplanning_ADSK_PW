using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;
using Autodesk.DesignScript.Geometry;

namespace SpacePlanning
{
    public class Node
    {

        
        private int _id;
        private Line2d _splitLine;       
        private Node _leftChildNode;
        private Node _rightChildNode;
        private Node _parentNode;
        private NodeType _nodeType;
        private Polygon2d _poly;
        private DeptData _deptAssigned;
        private bool _check;
        private bool _isRoot = false;
        private Point _centerPt;
        private double _radiusNode;
        private double _radiusAddContainer;
        private double _extraRadius;
        private double _prop = 1.2;


        // constructor for root node
        public Node(int id, NodeType type, bool flag, Point centerPt, double radius)
        {

            _id = id;
            _poly = null;
            _nodeType = type;
            _parentNode = null;
            _deptAssigned = null;
            _check = false;
            _isRoot = flag;
            _centerPt = centerPt;
            _radiusNode = radius;
            _radiusAddContainer = _radiusNode / _prop;

            if(type == NodeType.Container)
            {
                _extraRadius = _radiusNode + _radiusAddContainer;
            }    



        }

        // constructor for container node
        public Node(int id, NodeType type)
        {
            _id = id;            
            _nodeType = type;
            _check = false;
            _poly = null;
            _parentNode = null;
            _deptAssigned = null;


        }


        public Node( int id, Node parent, Node left, Node right, NodeType type, Polygon2d poly, Line2d splitLine, DeptData dept)
        {           
            _id = id;            
            _leftChildNode = left;
            _rightChildNode = right;
            _parentNode = parent;
            _nodeType = type;
            _deptAssigned = dept;
            _poly = poly;
            _splitLine = splitLine;
            _check = false;
        }


        public double RadiusNode
        {
            get { return _radiusNode; }
            set { _radiusNode = value; }
        }

        public double Proportion
        {
            get { return _prop; }
        }

        public double RadiusNodeExtra
        {
            get { return _extraRadius; }
            set { _extraRadius = value; }
        }

        public Point CenterPoint
        {
            get { return _centerPt; }
            set { _centerPt = value; }
        }

        public bool IsRoot
        {
            get { return _isRoot; }
            set { _isRoot = value; }
        }

        public bool Check
        {
            get { return _check; }
            set { _check = value; }
        }

        public Node ParentNode{
            get { return _parentNode; }
            set { _parentNode = value; }
        }

        public Node LeftNode
        {
            get { return _leftChildNode; }
            set { _leftChildNode = value; }

        }


        public Node RightNode
        {
            get { return _rightChildNode; }
            set { _rightChildNode = value; }
        }


        public NodeType NodeType
        {
            get { return _nodeType; }
            set { _nodeType = value; }

        }
    }
}
