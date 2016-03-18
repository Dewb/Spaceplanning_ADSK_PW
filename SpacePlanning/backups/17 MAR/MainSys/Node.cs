using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;

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


        // for root node
        public Node(int id, NodeType type, Polygon2d poly)
        {

            _id = id;
            _poly = poly;
            _nodeType = type;
            _parentNode = null;
            _deptAssigned = null;

        }

        // for container node
        public Node(int id, NodeType type)
        {

            _id = id;            
            _nodeType = type;
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
        }



        public Node ParentNode{

            get{
                return _parentNode;
            }

            set
            {
                _parentNode = value;
            }

        }

        public Node LeftNode
        {

            get
            {
                return _leftChildNode;
            }

            set
            {
                _leftChildNode = value;
            }

        }


        public Node RightNode
        {

            get
            {
                return _rightChildNode;
            }

            set
            {
                _rightChildNode = value;
            }

        }


        public NodeType NodeType
        {

            get
            {
                return _nodeType;
            }

            set
            {
                _nodeType = value;
            }

        }
    }
}
