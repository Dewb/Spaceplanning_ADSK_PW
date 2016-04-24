using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SpacePlanning
{
    ///<summary>
    ///this class creates the SPACE DATA TREE OBJECT
    ///the space data tree is a binary tree where in each node has two children , left and right
    ///this object is created while departments/ programs are allocated on site on the back end.
    ///this is done at the back end for fast access of dept/ prog data for dept topology computation
    ///and circulation network search
    ///based on the split of the site, each node of the tree is assigned information including two children , left and right
    ///left child , stores a node type of 'Space' while the right child stores the node type container 
    ///further details about the 'space' and 'container' is provided in the Node Class
    /// </summary>
    public class SpaceDataTree
    {
        private Node _root;
        private int _numNodes;
        private List<string> _nodeTypeString;
        private List<Point> _centerPtsNodeList;
        private List<Node> _nodeList;
        private List<double> _radiusNodeList;
        private Point _centerPt;
        private double _spaceX;
        private double _spaceY;

        //constructor
        public SpaceDataTree(Node root, Point origin, double X, double Y)
        {
            _root = root;
            _numNodes = 1;
            _nodeTypeString = new List<string>();
            string str = NodeType.Container.ToString();
            _nodeTypeString.Add(str);
            _centerPt = origin;
            _centerPtsNodeList = new List<Point>();
            _radiusNodeList = new List<double>();
            _radiusNodeList.Add(root.RadiusNodeExtra);
            _centerPtsNodeList.Add(origin);
            _spaceX = X;
            _spaceY = Y;            
        }

        //returns the list containting radius for each node in the tree
        public List<double> RadiusNodeList
        {
            get { return _radiusNodeList;  }            
        }

        //returns the Origin Point for each node in the tree
        public List<Point> CenterPtNodeList
        {
            get { return _centerPtsNodeList; }           
        }

        //returns the NodeType for each node in the form of a list of strings
        public List<string> NodeTypeList
        {
            get { return _nodeTypeString; }           
        }

        //returns number of nodes in the tree
        public int NumberOfNodes
        {
            get { return _numNodes; }           
        }

        //returns the Root Node of the tree
        //sets the root node of a tree and rebuilds the tree
        public Node Root
        {
            get { return _root; }
            set
            {                
                _root = value;
                if(_root != null )_numNodes = 1;
            }
        }

        // adds a new node to the tree
        internal bool AddNewNode(Node parent, Node item)
        {
           if(parent.LeftNode != null && parent.RightNode != null) return false;
           if(item.NodeType == NodeType.Container)
            {                
                parent.RightNode = item;
                item.ParentNode = parent;
                _numNodes += 1;
                _nodeTypeString.Add(NodeType.Container.ToString());
            }
            else
            {
                parent.LeftNode = item;
                item.ParentNode = parent;
                _numNodes += 1;
                _nodeTypeString.Add(NodeType.Space.ToString());
            }
            return true;
        }

        // checks of the given node has a parent
        private static Node CheckParentValid(Node node)
        {
            if(node.ParentNode == null) return node;
            else return node.ParentNode;
        }

        // makes origin point for the node
        private Point PointForNode(Point parentPt, int mul)
        {
            double x = parentPt.X + mul * _spaceX;
            double y = parentPt.Y +  _spaceY;
            return Point.ByCoordinates(x, y); ;
        }

        // add the following entries into the space data tree
        private void InsertNodeData(Node parent, Node item, bool container)
        {
            double rad = parent.RadiusNode;
            double proportion, addOn;
            if (container)
            {
                proportion = parent.Proportion;
                addOn = rad / proportion;
                parent.RightNode = item;
                item.ParentNode = parent;
                _numNodes += 1;
                Point cen = PointForNode(parent.CenterPoint, -1);
                item.CenterPoint = cen;
                item.RadiusNode = rad;
                item.RadiusNodeExtra = rad + addOn;
                _radiusNodeList.Add(rad + addOn);
                _centerPtsNodeList.Add(cen);
                _nodeTypeString.Add(NodeType.Container.ToString());
            }
            else
            {
                addOn = 0;
                parent.LeftNode = item;
                item.ParentNode = parent;
                _numNodes += 1;
                Point cen = PointForNode(parent.CenterPoint, 1);
                item.CenterPoint = cen;
                item.RadiusNode = rad;
                item.RadiusNodeExtra = rad + addOn;
                _radiusNodeList.Add(rad + addOn);
                _centerPtsNodeList.Add(cen);
                _nodeTypeString.Add(NodeType.Space.ToString());
            }
           
        }

        // adds a new node to the tree
        internal Node AddNewNodeSide(Node parent, Node item)
        {
            if (parent.LeftNode != null && parent.RightNode != null) return CheckParentValid(parent);
            if (item.NodeType == NodeType.Container)
            {
                if(parent.RightNode != null) return CheckParentValid(parent);
                else InsertNodeData(parent, item, true);                
            }
            else
            {
                if (parent.LeftNode != null) return CheckParentValid(parent);
                else InsertNodeData(parent, item, false);
            }
            return null;
        }
        
        //add a node to a filled spot in the tree
        internal void AddNode(Node parent, Node item)
        {
            Node parentLeftBefore = parent.LeftNode;
            Node parentRightBefore = parent.RightNode;
            parent.RightNode = item;
            if(parentRightBefore != null && parentLeftBefore != null)
            {
                if (parentRightBefore.NodeType == NodeType.Container) item.RightNode = parentRightBefore;
                else  item.LeftNode = parentLeftBefore;
            }
            else
            { 
                if (item.NodeType == NodeType.Container) parent.RightNode = item;
                else parent.LeftNode = item;
            }
            _numNodes += 1;
        }

        //removes the left node
        internal void RemoveLeftNode(Node Parent)
        {
            if (Parent.LeftNode != null) Parent.LeftNode = null;
        }

        //removes the right node
        internal void RemoveRightNode(Node Parent)
        {
            if (Parent.RightNode != null) Parent.RightNode = null;
        }

        //to remove only the node but keep their children
        internal void RemoveParentKeepChildren(Node parent,Node item)
        {
            if(parent.RightNode == item)
            {
                parent.RightNode = item.RightNode;
                parent.RightNode.LeftNode = item.LeftNode;
            }
            else
            {
                parent.LeftNode = item.LeftNode;
                parent.LeftNode.RightNode = item.RightNode;
            }
        }

        // returns random nodetype result
        internal static NodeType GenerateNodeType(double k)
        {
            if (k < 0.5) return NodeType.Container;
            else return NodeType.Space;
        }

        // returns random nodetype result
        internal static NodeType GenerateBalancedNodeType(bool tag)
        {
            if (tag) return NodeType.Container;
            else return NodeType.Space;
        }

    }
}
