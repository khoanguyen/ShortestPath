using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    public class RoadNode
    {
        public int ID { get; private set; }
        public bool Crashed { get; private set; }
        public NodeRole Role { get; private set; }
        public LinkCollection Links { get; private set; }

        internal RoadNode(int id,
            bool crashed = false,
            NodeRole role = NodeRole.Normal)
        {
            this.ID = id;
            this.Crashed = crashed;
            this.Role = role;
            this.Links = new LinkCollection();
        }

        public bool IsLinkedTo(RoadNode node)
        {
            return Links.ContainsLinkTo(node);
        }

    }
}
