using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    /// <summary>
    /// Shortest path finder using Dijkstra
    /// </summary>
    public class DijkstraShortestPathFinder : IShortestPathFinder
    {       
        /// <summary>
        /// Comparer class for comparing 2 RoadNodes
        /// </summary>
        private class NodeComparer : IComparer<RoadNode>
        {
            /// <summary>
            /// Distance dictionary for storing RoadNodes' distances
            /// </summary>
            private Dictionary<RoadNode, double> _distance;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="distance">Distance dictionary</param>
            public NodeComparer(Dictionary<RoadNode, double> distance)
            {
                _distance = distance;
            }

            /// <summary>
            /// Compare 2 given RoadNodes
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns>-1 if distance of x lesser than y, 1 for vice versa and 0 if they are equal</returns>
            public int Compare(RoadNode x, RoadNode y)
            {
                return DistanceOf(x).CompareTo(DistanceOf(y));
            }

            /// <summary>
            /// Retrieve distance of the given RoadNode
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            private double DistanceOf(RoadNode node)
            {
                // Return double PositiveInfinitive if node's distance doesn't exist in the dictionary
                if (!_distance.ContainsKey(node)) return double.PositiveInfinity;

                // Return the node's distance
                return _distance[node];
            }
        }

        /// <summary>
        /// Find shortest path from the given RoadSystem
        /// </summary>
        /// <param name="roadSystem"></param>
        /// <returns></returns>
        public IEnumerable<RoadNode> FindShortestPath(RoadSystem roadSystem)
        {
            // Return empty path if start node or end node is crashed
            if (roadSystem.StartNode.Crashed || roadSystem.EndNode.Crashed)
                return new RoadNode[0];

            // List for storing result
            var result = new List<RoadNode>();

            // Distance dictionary
            Dictionary<RoadNode, double> distance = new Dictionary<RoadNode, double>();

            // Previous dictionary for storing previous node of a node
            Dictionary<RoadNode, RoadNode> previous = new Dictionary<RoadNode, RoadNode>();

            // List of nodes that are needed to be visited
            List<RoadNode> visiting = new List<RoadNode>();

            // Found shortest path's distance
            double shortestPathDistance = double.PositiveInfinity;

            // Comparer for sorting the visiting list
            var comparer = new NodeComparer(distance);

            // Initialize nodes' distance with double.PositiveInfinitive
            roadSystem.ForEach(node => {
                distance[node] = double.PositiveInfinity;
                if (!node.Crashed) visiting.Add(node);
            });

            // Set start node's distance to 0
            distance[roadSystem.StartNode] = 0;

            // Sort visiting collection
            visiting.Sort(comparer);

            while (visiting.Count > 0 && distance[visiting[0]] != double.PositiveInfinity)
            {
                var visited = visiting[0];
                visiting.Remove(visited);

                // Stop visit end node and nodes that have distance bigger than found shortest distance
                if (distance[visited] > shortestPathDistance) continue;

                // update distance of the nodes which visited links to
                visited.Links.ForEach(link =>
                {
                    // destination node
                    var node = link.Key;

                    // Update linked node distance if it's not crashed
                    if (!node.Crashed)
                    {
                        // Calculate new distance from visited to link
                        var newDistance = distance[visited] + link.Value;

                        // Set distance of destination node with new distance 
                        // if new distance is shorter than current 
                        // and set previous node of destination node to visited
                        if (distance[node] > newDistance)
                        {
                            distance[node] = newDistance;
                            previous[node] = visited;
                        }

                        // Check if destination node is end node
                        if (node.Role == NodeRole.Finish)
                        {
                            // Set found shortest distance
                            shortestPathDistance = shortestPathDistance > distance[node] ?
                                distance[node] : shortestPathDistance;

                            // Remove destination node from visiting
                            // don't need to check the end node
                            visiting.Remove(node);
                        }
                    }
                });
                
                // Resort visiting to bring the unvisited node 
                // with shortest distance to the head of the visiting list
                visiting.Sort(comparer);
            }

            // Return empty if end node is not rached
            if (distance[roadSystem.EndNode] == double.PositiveInfinity)
                return new RoadNode[0];

            // Extract the shortest path 
            RoadNode routeNode = roadSystem.EndNode;
            result.Add(routeNode);
            do
            {                
                routeNode = previous[routeNode];
                result.Insert(0, routeNode);
            } while (routeNode != roadSystem.StartNode);

            // Return the result
            return result.AsEnumerable();
        }

        private void EnsureStartEndNodes(RoadSystem roadSystem)
        {
            if (roadSystem.StartNode == null && roadSystem.EndNode == null)
                throw new InvalidOperationException("RoadSystem should have both start node and end node");
        }
    }
}
