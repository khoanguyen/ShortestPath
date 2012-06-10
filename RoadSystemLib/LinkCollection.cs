using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    public class LinkCollection : IEnumerable<KeyValuePair<RoadNode, double>>
    {
        private Dictionary<RoadNode, double> _links = new Dictionary<RoadNode, double>();

        public double this[RoadNode node]
        {
            get
            {
                if (!_links.ContainsKey(node)) return double.PositiveInfinity;
                return _links[node];
            }
        }

        internal LinkCollection() { }

        internal void Link(RoadNode node, double weight)
        {
            _links[node] = weight;
        }

        internal bool ContainsLinkTo(RoadNode node)
        {
            return _links.ContainsKey(node);
        }

        #region IEnumerable Implementation
        public IEnumerator<KeyValuePair<RoadNode, double>> GetEnumerator()
        {
            foreach (KeyValuePair<RoadNode, double> kp in _links) yield return kp;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<RoadNode, double> kp in _links) yield return kp;
        }
        #endregion
    }
}
