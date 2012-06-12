using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    /// <summary>
    /// A node in a RoadSystem
    /// </summary>
    public class RoadNode
    {
        /// <summary>
        /// ID of RoadNode
        /// </summary>
        public int ID { get; private set; }
        
        /// <summary>
        /// Indicate the node is crashed
        /// </summary>
        public bool Crashed { get; private set; }

        /// <summary>
        /// Role of a RoadNode        
        /// </summary>
        public NodeRole Role { get; private set; }

        /// <summary>
        /// Links from current node to the other nodes
        /// </summary>
        public LinkCollection Links { get; private set; }

        /// <summary>
        /// Internal constructor used by RoadSystem
        /// </summary>
        /// <param name="id"></param>
        /// <param name="crashed"></param>
        /// <param name="role"></param>
        internal RoadNode(int id,
            bool crashed = false,
            NodeRole role = NodeRole.Normal)
        {
            this.ID = id;
            this.Crashed = crashed;
            this.Role = role;
            this.Links = new LinkCollection();
        }

        /// <summary>
        /// Check if this node links to the given node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool IsLinkedTo(RoadNode node)
        {
            return Links.ContainsLinkTo(node);
        }

    }
}
