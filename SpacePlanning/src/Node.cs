using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;
using Autodesk.DesignScript.Geometry;


//#########################################################################################################################
//this class makes a NODE OBJECT, which stores links to two choldren, left and right and its parent 
//the node data type is used to build the SpaceData Tree
//if a node is root then it has null parent
//a node can be of two types, 'Space' or 'Container'
//a space node type represents a node which stores information of space ( either program or dept )
//a space node type has no children
//space node types are assigned to the left child and container node types are assigned to the right child of a parent node
//the last container in a Space Data Tree has both left and right  child as space node
//a container node type represents a node which stores further sub chains of nodes ( both space and container nodetype)
//#########################################################################################################################



namespace SpacePlanning
{
    public class Node
    {
        private int _id;
        private Line2d _splitLine;       
        private Node _left;
        private Node _right;
        private Node _parent;
        private NodeType _nodeType;
        private Polygon2d _poly;
        private DeptData _deptAssigned;
        private bool _check;
        private bool _isRoot = false;
        private Point _centerPt;
        private double _radius;
        private double _radiusForContainer;
        private double _extraRadius;
        private double _prop = 1.2;


        // constructor for root node
        public Node(int id, NodeType type, bool flag, Point centerPt, double radius)
        {

            _id = id;
            _poly = null;
            _nodeType = type;
            _parent = null;
            _deptAssigned = null;
            _check = false;
            _isRoot = flag;
            _centerPt = centerPt;
            _radius = radius;
            _radiusForContainer = _radius / _prop;

            if(type == NodeType.Container)
            {
                _extraRadius = _radius + _radiusForContainer;
            }    



        }

        // constructor for container node
        public Node(int id, NodeType type)
        {
            _id = id;            
            _nodeType = type;
            _check = false;
            _poly = null;
            _parent = null;
            _deptAssigned = null;


        }


        public Node( int id, Node parent, Node left, Node right, NodeType type, Polygon2d poly, Line2d splitLine, DeptData dept)
        {           
            _id = id;            
            _left = left;
            _right = right;
            _parent = parent;
            _nodeType = type;
            _deptAssigned = dept;
            _poly = poly;
            _splitLine = splitLine;
            _check = false;
        }


        public double RadiusNode
        {
            get { return _radius; }
            set { _radius = value; }
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
            get { return _parent; }
            set { _parent = value; }
        }

        public Node LeftNode
        {
            get { return _left; }
            set { _left = value; }

        }


        public Node RightNode
        {
            get { return _right; }
            set { _right = value; }
        }


        public NodeType NodeType
        {
            get { return _nodeType; }
            set { _nodeType = value; }

        }
    }
}
