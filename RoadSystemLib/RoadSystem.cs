using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Security.Permissions;

namespace RoadSystemLib
{
    /// <summary>
    /// A Road system with RoadNodes and links between nodes
    /// 
    /// Each RoadSystem only have 1 Start Node and 1 End Node, multiple of these types of node is not allowed    
    /// </summary>
    public class RoadSystem : IEnumerable<RoadNode>
    {
        /// <summary>
        /// Lock for avoid race-condition
        /// </summary>
        private object _linkLock = new object();

        /// <summary>
        /// Dictionary for containing RoadNodes
        /// </summary>
        private Dictionary<int, RoadNode> _nodes = new Dictionary<int, RoadNode>();

        #region Properties and Indexer
        /// <summary>
        /// Start Node of RoadSystem
        /// </summary>
        public RoadNode StartNode { get; private set; }

        /// <summary>
        /// End Node of RoadSystem
        /// </summary>
        public RoadNode EndNode { get; private set; }

        /// <summary>
        /// Count the number of RoadNodes in this RoadSystem
        /// </summary>
        public int Count
        {
            get
            {
                return _nodes.Count;
            }
        }

        /// <summary>
        /// Indexer fro retrieving RoadNode from its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RoadNode this[int id]
        {
            get
            {
                if (!_nodes.ContainsKey(id)) return null;
                return _nodes[id];
            }
        } 
        #endregion

        #region Static methods for loading xml content, xml file
        /// <summary>
        /// Load RoadSystem from XML string. 
        /// The input content should have at least 2 nodes, with 1 start node and 1 end node.
        /// Link to non-existing node or without Reference ID is not allowed.        
        /// Node without a valid ID is not allowed.
        /// All Node ID and Reference ID should be integer, all the weight should be valid number.        
        /// Different weight between 2 linked node is not allowed
        /// </summary>
        /// <param name="content">XML string</param>
        /// <exception cref="LoadRoadSystemException">Error while loading RoadSystem</exception>
        /// <returns></returns>
        public static RoadSystem Load(string content)
        {
            try
            {
                RoadSystem result = new RoadSystem();
                Dictionary<XElement, RoadNode> nodesDic = new Dictionary<XElement, RoadNode>();

                // Parse the input string to XML document
                var document = XDocument.Parse(content);
                
                // Get all Node XElements
                var nodes = document.Root.Descendants("node");
                
                // Create all nodes
                nodes.ForEach(node =>
                {
                    // Ensure valid Node XElement
                    ValidateNodeElement(node);

                    // Get all attrbutes
                    var statusAttribute = node.Attribute("status");
                    var roleAttribute = node.Attribute("role");
                    var idAttribute = node.Attribute("id");

                    // Get input values for creating a node
                    bool crashed = statusAttribute == null ? false : statusAttribute.Value == "crash";
                    NodeRole role = roleAttribute == null ? NodeRole.Normal :
                        roleAttribute.Value == "start" ? NodeRole.Start :
                        roleAttribute.Value == "finish" ? NodeRole.Finish :
                        NodeRole.Normal;

                    // Creat a new node in the RoadSystem and add that node into the dictionary
                    nodesDic[node] = result.CreateNewNode(int.Parse(idAttribute.Value.Trim()), crashed, role);
                });

                // Link nodes
                nodes.ForEach(node =>
                {
                    // Get all Links element
                    var links = node.Descendants("link");

                    // Links the nodes
                    links.ForEach(link =>
                    {
                        // Ensure the valid Link element
                        ValidateLinkElement(result, link);

                        // Get all Attributes
                        var refAttribute = link.Attribute("ref");
                        var weightAttribute = link.Attribute("weight");

                        // Get ref ID and weight
                        var id = int.Parse(refAttribute.Value.Trim());
                        var linkedNode = result[id];
                        double weight = double.Parse(weightAttribute.Value.Trim());

                        // Ensure that the link between 2 nodes has the same weight
                        EnsureLinkable(nodesDic[node], linkedNode, weight);

                        // Link 2 nodes
                        result.Link(nodesDic[node], linkedNode, weight);
                    });
                });

                // Ensure the final RoadSystem is valid as the loading criterias
                ValidateRoadSystem(result);

                // Return the result
                return result;
            }
            catch (LoadRoadSystemException ex)
            {
                // Throw LoadRoadSystemException
                throw ex;
            }
            catch (Exception ex)
            {
                // Wrap catched Exception with LoadRoadSystemException and throw it
                throw new LoadRoadSystemException("Error while loading content : " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Load RoadSystem from XML file. 
        /// The input content should have at least 2 nodes, with 1 start node and 1 end node.
        /// Link to non-existing node or without Reference ID is not allowed.        
        /// Node without a valid ID is not allowed.
        /// All Node ID and Reference ID should be integer, all the weight should be valid number.        
        /// Different weight between 2 linked node is not allowed
        /// </summary>
        /// <param name="content">XML string</param>
        /// <exception cref="LoadRoadSystemException">Error while loading RoadSystem</exception>
        /// <returns></returns>
        public static RoadSystem LoadFromXml(string filePath)
        {
            StreamReader reader = null;
            try
            {
                // Open XML file
                reader = File.OpenText(filePath);                 

                // Load the RoadSystem from XML file's content
                return Load(reader.ReadToEnd());
            }
            catch (LoadRoadSystemException ex)
            {
                // Throw LoadRoadSystemException
                throw ex;
            }
            catch (Exception ex)
            {
                // Wrap catched Exception with LoadRoadSystemException and throw it
                throw new LoadRoadSystemException("Error while loading " + Path.GetFileName(filePath) + " : " + ex.Message, ex);
            }
            finally
            {
                // Close file reader
                if (reader != null) reader.Close();
            }
        } 
        #endregion

        /// <summary>
        /// Link 2 nodes from this RoadSystem
        /// </summary>
        /// <param name="node1">node1</param>
        /// <param name="node2">node2</param>
        /// <param name="weight">Weight between 2 nodes. Weight should be non-negative and less than double.PositiveInfinitive</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="InvalidOperationException"/>
        public void Link(RoadNode node1, RoadNode node2, double weight)
        {
            // Lock to avoid the race-condition
            lock (_linkLock)
            {
                // Ensure linked nodes are from this system
                EnsureNodesInSameSystem(node1, node2);

                // Ensure valid weight
                EnsureValidWeight(weight);

                // Link nodes from both side
                node1.Links.Link(node2, weight);
                node2.Links.Link(node1, weight);
            }
        }

        /// <summary>
        /// Create a new RoadNode in this RoadSystem and return it
        /// </summary>
        /// <param name="id">RoadNode ID</param>
        /// <param name="crashed">Indicate the node is crashed</param>
        /// <param name="role">RoadNode's role</param>
        /// <returns>New RoadNode</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="InvalidOperationException"/>
        public RoadNode CreateNewNode(int id, bool crashed = false, NodeRole role = NodeRole.Normal)
        {
            // Lock to avoid the race-condition
            lock (_nodes)
            {
                // Ensure that the given ID is not occupied
                EnsureIDNotUsed(id);

                // Create a new node from given arguments
                var newNode = new RoadNode(id, crashed, role);

                // Add node tho nodes collection
                _nodes.Add(id, newNode);

                // Set start/end node if role is StartNode/EndNode
                if (role == NodeRole.Start)
                {
                    // Ensure no start node set
                    EnsureNoStartNode();
                    StartNode = newNode;
                }
                else if (role == NodeRole.Finish)
                {
                    // Ensure no end node set
                    EnsureNoEndNode();
                    EndNode = newNode;
                }

                // return new created node
                return newNode;
            }
        }

        /// <summary>
        /// Check if this RadSystem contains the given node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool ContainsNode(RoadNode node)
        {
            return _nodes.ContainsKey(node.ID) && _nodes[node.ID] == node;
        }

        #region IEnumerable Implementation
        /// <summary>
        /// Implement generic IEnumerable
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RoadNode> GetEnumerator()
        {
            foreach (RoadNode node in _nodes.Values) yield return node;
        }

        /// <summary>
        /// Implement IEnumerable
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (RoadNode node in _nodes.Values) yield return node;
        }
        #endregion

        #region Validation Methods
        /// <summary>
        /// Ensure Linkable nodes with valid weight
        /// Used by Load functions
        /// </summary>
        /// <param name="fromNode"></param>
        /// <param name="toNode"></param>
        /// <param name="weight"></param>
        private static void EnsureLinkable(RoadNode fromNode, RoadNode toNode, double weight)
        {
            if (toNode.IsLinkedTo(fromNode) && toNode.Links[fromNode] != weight)
                throw new LoadRoadSystemException("Different weight between 2 nodes is not allowed");
        }

        /// <summary>
        /// Validate RoadSystem
        /// </summary>
        /// <param name="system"></param>
        private static void ValidateRoadSystem(RoadSystem system)
        {
            if (system.Count == 0)
                throw new LoadRoadSystemException("No node found");
            if (system.Count == 1)
                throw new LoadRoadSystemException("There only 1 node in the road system");
            if (system.StartNode == null)
                throw new LoadRoadSystemException("No Start node found");
            if (system.EndNode == null)
                throw new LoadRoadSystemException("No End node found");            
        }

        /// <summary>
        /// Validate link XElement agianst the given system
        /// </summary>
        /// <param name="system"></param>
        /// <param name="nodeElement"></param>
        private static void ValidateLinkElement(RoadSystem system, XElement nodeElement)
        {
            double weight = 0;
            var refAttribute = nodeElement.Attribute("ref");
            var weightAttribute = nodeElement.Attribute("weight");
            int id = 0;

            if (refAttribute == null)
                throw new LoadRoadSystemException("Link does not have ref attribute");
            
            if (weightAttribute == null)
                throw new LoadRoadSystemException("Link does not have weight attribute");
            
            if (!double.TryParse(weightAttribute.Value.Trim(), out weight) || weight < 0 || weight == double.PositiveInfinity)
                throw new LoadRoadSystemException("Link with invalid weight");

            if (!int.TryParse(refAttribute.Value.Trim(), out id))
                throw new LoadRoadSystemException("Link with invalid ref ID");

            if (system[id] == null)
                throw new LoadRoadSystemException("Link points to non-existing node");
        }

        /// <summary>
        /// Validate node XElement
        /// </summary>
        /// <param name="nodeElement"></param>
        private static void ValidateNodeElement(XElement nodeElement)
        {
            var idAttribute = nodeElement.Attribute("id");
            int id = 0;

            if (idAttribute == null || idAttribute.Value.Trim() == string.Empty)
                throw new LoadRoadSystemException("Node without ID");

            if (!int.TryParse(idAttribute.Value.Trim(), out id))
                throw new LoadRoadSystemException("Node with invalid ID. ID should be a number");
        }

        /// <summary>
        /// Ensure No start node set
        /// </summary>
        private void EnsureNoStartNode()
        {
            if (StartNode != null)
                throw new InvalidOperationException("Multiple start node is not allowed");
        }

        /// <summary>
        /// Ensure no end node set
        /// </summary>
        private void EnsureNoEndNode()
        {
            if (EndNode != null)
                throw new InvalidOperationException("Multiple end node is not allowed");
        }

        /// <summary>
        /// Ensure 2 nodes are from this system
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        private void EnsureNodesInSameSystem(RoadNode node1, RoadNode node2)
        {
            if (!ContainsNode(node1) || !ContainsNode(node2))
                throw new InvalidOperationException("Cannot link nodes from different RoadSystem");
        }

        /// <summary>
        /// Ensure ID is not used
        /// </summary>
        /// <param name="id"></param>
        private void EnsureIDNotUsed(int id)
        {
            if (_nodes.ContainsKey(id))
                throw new ArgumentException("Given id has already been used by another node");
        }

        /// <summary>
        /// Ensure valid weight
        /// </summary>
        /// <param name="weight"></param>
        private void EnsureValidWeight(double weight)
        {
            if (weight < 0 || weight == double.PositiveInfinity)
                throw new ArgumentException("Weight should be a non-negative number and less than PositiveInfinity");
        }
        #endregion
    }
}
