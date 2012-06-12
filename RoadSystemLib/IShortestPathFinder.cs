using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    /// <summary>
    /// Interface for implementing shortest path finder
    /// </summary>
    public interface IShortestPathFinder
    {
        /// <summary>
        /// Find shortest path from the given RoadSystem
        /// </summary>
        /// <param name="roadSystem">Given RoadSystem</param>
        /// <returns>Enumerable of Roadnode</returns>
        IEnumerable<RoadNode> FindShortestPath(RoadSystem roadSystem);
    }
}
