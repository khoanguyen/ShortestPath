using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    public class RoadSystem : IEnumerable<RoadNode>
    {
        private object _linkLock = new object();
        private Dictionary<int, RoadNode> _nodes = new Dictionary<int, RoadNode>();

        public RoadNode StartNode { get; private set; }
        public RoadNode EndNode { get; private set; }

        public RoadNode this[int id]
        {
            get
            {
                if (!_nodes.ContainsKey(id)) return null;
                return _nodes[id];
            }
        }

        public void Link(RoadNode node1, RoadNode node2, double weight)
        {
            lock (_linkLock)
            {
                EnsureNodesInSameSystem(node1, node2);
                EnsureValidWeight(weight);

                node1.Links.Link(node2, weight);
                node2.Links.Link(node1, weight);
            }
        }

        public RoadNode CreateNewNode(int id, bool crashed = false, NodeRole role = NodeRole.Normal)
        {
            lock (_nodes)
            {
                EnsureIDNotUsed(id);

                var newNode = new RoadNode(id, crashed, role);
                _nodes.Add(id, newNode);

                if (role == NodeRole.Start)
                {
                    EnsureNoStartNode();
                    StartNode = newNode;
                }
                else if (role == NodeRole.Finish)
                {
                    EnsureNoEndNode();
                    EndNode = newNode;
                }

                return newNode;
            }
        }

        public bool ContainsNode(RoadNode node)
        {
            return _nodes.ContainsKey(node.ID) && _nodes[node.ID] == node;
        }

        #region IEnumerable Implementation
        public IEnumerator<RoadNode> GetEnumerator()
        {
            foreach (RoadNode node in _nodes.Values) yield return node;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (RoadNode node in _nodes.Values) yield return node;
        }
        #endregion

        #region Validation Methods
        private void EnsureNoStartNode()
        {
            if (StartNode != null)
                throw new InvalidOperationException("Multiple start node is not allowed");
        }

        private void EnsureNoEndNode()
        {
            if (EndNode != null)
                throw new InvalidOperationException("Multiple end node is not allowed");
        }

        private void EnsureNodesInSameSystem(RoadNode node1, RoadNode node2)
        {
            if (!ContainsNode(node1) || !ContainsNode(node2))
                throw new InvalidOperationException("Cannot link nodes from different RoadSystem");
        }

        private void EnsureIDNotUsed(int id)
        {
            if (_nodes.ContainsKey(id))
                throw new ArgumentException("Given id has already been used by another node");
        }

        private void EnsureValidWeight(double weight)
        {
            if (weight < 0)
                throw new ArgumentException("Weight should be a non-negative number");
        }
        #endregion
    }
}
