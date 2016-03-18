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

        public SpaceDataTree(Node root)
        {
            _root = root;
            _numNodes = 1;
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
            }
            else
            {
                parent.LeftNode = item;
                item.ParentNode = parent;
                _numNodes += 1;
            }

            return true;
        }

        // adds a new node to the tree
        internal bool addNewNodeSide(Node parent, Node item)
        {
            if (parent.LeftNode != null && parent.RightNode != null)
            {
                Trace.WriteLine("No Space, cant add new node");
                return false;
            }

            if (item.NodeType == NodeType.Container)
            {

                parent.RightNode = item;
                item.ParentNode = parent;
                _numNodes += 1;
            }
            else
            {
                parent.LeftNode = item;
                item.ParentNode = parent;
                _numNodes += 1;
            }

            return true;
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
