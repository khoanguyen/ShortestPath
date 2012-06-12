using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    /// <summary>
    /// Link collection contains links to nodes and distance of the links
    /// 
    /// Each RoadNode has a LinkCollection to contain link from itself to the other nodes
    /// </summary>
    public class LinkCollection : IEnumerable<KeyValuePair<RoadNode, double>>
    {
        /// <summary>
        /// Dictionary of links
        /// </summary>
        private Dictionary<RoadNode, double> _links = new Dictionary<RoadNode, double>();

        /// <summary>
        /// Indexer for retrieving distance from collection owner to given node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public double this[RoadNode node]
        {
            get
            {
                // Return double.PositiveInfinity if link ot given node is not found
                if (!_links.ContainsKey(node)) return double.PositiveInfinity;

                // Return distance of the link
                return _links[node];
            }
        }

        /// <summary>
        /// Count the number of link in this Link collection
        /// </summary>
        public int Count
        {
            get
            {
                return _links.Count;
            }
        }

        /// <summary>
        /// Internal constructor used by RoadSystem
        /// </summary>
        internal LinkCollection() { }

        /// <summary>
        /// Create a link with given weight to the given node
        /// 
        /// Internal method used by RoadSystem
        /// </summary>
        /// <param name="node"></param>
        /// <param name="weight"></param>
        internal void Link(RoadNode node, double weight)
        {
            _links[node] = weight;
        }

        /// <summary>
        /// Check if there is a link to given node or not
        /// 
        /// Internal method used by RoadSystem
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal bool ContainsLinkTo(RoadNode node)
        {
            return _links.ContainsKey(node);
        }

        #region IEnumerable Implementation
        /// <summary>
        /// Implement of Generic IEnumerable
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<RoadNode, double>> GetEnumerator()
        {
            foreach (KeyValuePair<RoadNode, double> kp in _links) yield return kp;
        }

        /// <summary>
        /// Implement of IEnumerable
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<RoadNode, double> kp in _links) yield return kp;
        }
        #endregion
    }
}
