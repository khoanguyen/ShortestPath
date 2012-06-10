using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    public class DijktraShortestPathFinder : IShortestPathFinder
    {

        private class NodeState
        {
            public const int START_NODE = 1;
            public const int END_NODE = 2;

            public RoadNode Node { get; set; }
            public bool Visited { get; set; }
            public int Role { get; set; }
            public double Cost { get; set; }
        }

        public IEnumerable<RoadNode[]> FindShortestPaths(RoadSystem roadSystem, RoadNode startNode, RoadNode endNode)
        {
            EnsureNodesInSameGraph(roadSystem, startNode, endNode);

            return new[] { new RoadNode[0] };
        }

        private void EnsureNodesInSameGraph(RoadSystem roadSystem, RoadNode node1, RoadNode node2)
        {
            if (!roadSystem.ContainsNode(node1) || !roadSystem.ContainsNode(node2))
                throw new InvalidOperationException("Cannot find shortest path on nodes from different RoadSystem");
        }

    }
}
