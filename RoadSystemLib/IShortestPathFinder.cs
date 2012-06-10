using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    public interface IShortestPathFinder
    {
        IEnumerable<RoadNode[]> FindShortestPaths(RoadSystem roadSystem, RoadNode startNode, RoadNode endNode);
    }
}
