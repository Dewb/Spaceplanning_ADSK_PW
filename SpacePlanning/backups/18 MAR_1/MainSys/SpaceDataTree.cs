﻿using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacePlanning
{
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

        public List<double> RadiusNodeList
        {
            get
            {
                return _radiusNodeList;
            }
        }

        public List<Point> CenterPtNodeList
        {
            get
            {
                return _centerPtsNodeList;
            }
        }


        public List<string> NodeTypeList
        {
            get
            {
                return _nodeTypeString;
            }
        }

        public int NumberOfNodes
        {
            get
            {
                return _numNodes;
            }
        }

        public Node Root
        {
            get
            {
                return _root;
            }
            set
            {
                _numNodes = 1;
                _root = value;
            }
        }

        // adds a new node to the tree
        internal bool addNewNode(Node parent, Node item)
        {
            if(parent.LeftNode != null && parent.RightNode != null)
            {
                Trace.WriteLine("No Space, cant add new node");
                return false;
            }

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

        private static Node checkParentValid(Node node)
        {
            if(node.ParentNode == null)
            {
                return node;
            }
            else
            {
                return node.ParentNode;
            }
        }

        // makes origin point for the node
        private Point PointForNode(Point parentPt, int mul)
        {
            double x = parentPt.X + mul * _spaceX;
            double y = parentPt.Y +  _spaceY;
            Point cen = Point.ByCoordinates(x, y);
            return cen;
        }


        // adds a new node to the tree
        internal Node addNewNodeSide(Node parent, Node item)
        {

            // case1
            if (parent.LeftNode != null && parent.RightNode != null)
            {
                Trace.WriteLine("No Space, cant add new node");
                return checkParentValid(parent);
            }


            // case2
            if (item.NodeType == NodeType.Container)
            {

                if(parent.RightNode != null)
                {
                    Trace.WriteLine("Right Node not empty");
                    return checkParentValid(parent);
                }
                else
                {
                    parent.RightNode = item;
                    item.ParentNode = parent;
                    _numNodes += 1;
                    Point cen = PointForNode(parent.CenterPoint,1);
                    item.CenterPoint = cen;
                    double rad = parent.RadiusNode;
                    double proportion = parent.Proportion;
                    double addOn = rad / proportion;
                    item.RadiusNode = rad;
                    item.RadiusNodeExtra = rad + addOn;
                    _radiusNodeList.Add(rad + addOn);
                    _centerPtsNodeList.Add(cen);
                    _nodeTypeString.Add(NodeType.Container.ToString());
                }
                
            }
            else
            {
                if (parent.LeftNode != null)
                {
                    Trace.WriteLine("Left Node not empty");
                    return checkParentValid(parent); 
                }
                else
                {
                    parent.LeftNode = item;
                    item.ParentNode = parent;
                    _numNodes += 1;
                    Point cen = PointForNode(parent.CenterPoint, -1);
                    item.CenterPoint = cen;
                    _centerPtsNodeList.Add(cen);
                    double rad = parent.RadiusNode;
                    item.RadiusNode = rad;
                    _radiusNodeList.Add(rad);
                    _nodeTypeString.Add(NodeType.Space.ToString());
                }
            }

            return null;
        }




        //add a node to a filled spot in the tree
        internal void addNode(Node parent, Node item)
        {
            Node parentLeftBefore = parent.LeftNode;
            Node parentRightBefore = parent.RightNode;
            parent.RightNode = item;

           
            if(parentRightBefore != null && parentLeftBefore != null)
            {
                if (parentRightBefore.NodeType == NodeType.Container)
                {
                    item.RightNode = parentRightBefore;
                }
                else
                {
                    item.LeftNode = parentLeftBefore;
                }
            }
            else
            {
                if (item.NodeType == NodeType.Container)
                {
                    parent.RightNode = item;
                }
                else
                {
                    parent.LeftNode = item;
                }
            }
                    

            _numNodes += 1;

        }


        //to remove node and their children both
        internal void removeNodeAll(Node parent, bool left)
        {
            if (left)
            {
                parent.LeftNode = null;
            }
            else
            {
                parent.RightNode = null;
            }           

        }


        //to remove only the node but keep their children
        internal void removeOnlyNode(Node parent,Node item)
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

    }
}
