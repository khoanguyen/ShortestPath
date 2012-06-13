using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RoadSystemLib;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace UnitTest
{
    [TestFixture]
    public class RoadSystemTest
    {
        private RoadSystem _roadSystem;

        #region SetUp and TearDown
        /// <summary>
        /// Test SetUp
        /// </summary>
        [SetUp]
        public void SetUpTest()
        {
            _roadSystem = new RoadSystem();
        }

        /// <summary>
        /// Test TearDown
        /// </summary>
        [TearDown]
        public void TearDownTest()
        {
            _roadSystem = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion

        #region Test Methods
        /// <summary>
        /// Test Create New Node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="crashed"></param>
        /// <param name="role"></param>
        [Test]
        [TestCaseSource("CreateNewNodeTestCase")]
        public void TestCreateNewNode(int id, bool crashed, NodeRole role)
        {
            var newNode = _roadSystem.CreateNewNode(id, crashed, role);
            Assert.AreEqual(id, newNode.ID);
            Assert.AreEqual(crashed, newNode.Crashed);
            Assert.AreEqual(role, newNode.Role);
            Assert.IsTrue(_roadSystem.Contains(newNode));
            Assert.AreSame(_roadSystem[id], newNode);
        }

        /// <summary>
        /// Test Create New Node with default value
        /// </summary>
        [Test]
        public void TestCreateNewNode_DefaultValue()
        {
            var newNode = _roadSystem.CreateNewNode(1);
            Assert.AreEqual(1, newNode.ID);
            Assert.IsFalse(newNode.Crashed);
            Assert.AreEqual(NodeRole.Normal, newNode.Role);
            Assert.IsTrue(_roadSystem.Contains(newNode));
            Assert.AreSame(_roadSystem[1], newNode);

            newNode = _roadSystem.CreateNewNode(2, true);
            Assert.AreEqual(2, newNode.ID);
            Assert.IsTrue(newNode.Crashed);
            Assert.AreEqual(NodeRole.Normal, newNode.Role);
            Assert.IsTrue(_roadSystem.Contains(newNode));
            Assert.AreSame(_roadSystem[2], newNode);

            newNode = _roadSystem.CreateNewNode(3, role: NodeRole.Finish);
            Assert.AreEqual(3, newNode.ID);
            Assert.IsFalse(newNode.Crashed);
            Assert.AreEqual(NodeRole.Finish, newNode.Role);
            Assert.IsTrue(_roadSystem.Contains(newNode));
            Assert.AreSame(_roadSystem[3], newNode);

            newNode = _roadSystem.CreateNewNode(4, role: NodeRole.Start);
            Assert.AreEqual(4, newNode.ID);
            Assert.IsFalse(newNode.Crashed);
            Assert.AreEqual(NodeRole.Start, newNode.Role);
            Assert.IsTrue(_roadSystem.Contains(newNode));
            Assert.AreSame(_roadSystem[4], newNode);
        }

        /// <summary>
        /// Test Create New Node with dupplicated ID
        /// </summary>
        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException),
            UserMessage = "Given id has already been used by another node")]
        public void TestCreateNewNode_DuplicatedID()
        {
            _roadSystem.CreateNewNode(1);
            _roadSystem.CreateNewNode(1);
        }

        /// <summary>
        /// Test Count
        /// </summary>
        [Test]
        public void TestCount()
        {
            _roadSystem.CreateNewNode(1);
            Assert.AreEqual(1, _roadSystem.Count);

            _roadSystem.CreateNewNode(2);
            Assert.AreEqual(2, _roadSystem.Count);
        }

        /// <summary>
        /// Test ContainsNode
        /// </summary>
        [Test]
        public void TestContainsNode()
        {
            RoadSystem anotherSystem = new RoadSystem();
            var anotherNode = anotherSystem.CreateNewNode(2);
            var newNode = _roadSystem.CreateNewNode(1);

            Assert.IsFalse(_roadSystem.Contains(anotherNode));
            Assert.IsTrue(_roadSystem.Contains(newNode));
        }

        /// <summary>
        /// Test Indexer
        /// </summary>
        [Test]
        public void TestIndexer()
        {
            var newNode = _roadSystem.CreateNewNode(1);

            Assert.AreSame(newNode, _roadSystem[1]);
            Assert.AreSame(null, _roadSystem[2]);
        }

        /// <summary>
        /// Test Link
        /// </summary>
        /// <param name="weight"></param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(double.MaxValue)]
        public void TestLink(double weight)
        {
            var node1 = _roadSystem.CreateNewNode(1);
            var node2 = _roadSystem.CreateNewNode(2);
            var node3 = _roadSystem.CreateNewNode(3);

            _roadSystem.Link(node1, node2, weight);

            Assert.IsTrue(node1.IsLinkedTo(node2));
            Assert.IsFalse(node1.IsLinkedTo(node3));
            Assert.IsFalse(node2.IsLinkedTo(node3));
            Assert.IsTrue(node2.IsLinkedTo(node1));

            Assert.AreEqual(weight, node1.Links[node2]);
            Assert.AreEqual(weight, node2.Links[node1]);
        }

        /// <summary>
        /// Test Link with nodes not in the same system
        /// </summary>
        [Test]
        [ExpectedException(ExpectedException = typeof(InvalidOperationException),
            UserMessage = "Cannot link nodes from different RoadSystem")]
        public void TestLink_NodesNotInSameSystem()
        {
            var anotherSystem = new RoadSystem();
            var anotherNode = anotherSystem.CreateNewNode(2);
            var node = _roadSystem.CreateNewNode(1);

            _roadSystem.Link(node, anotherNode, 1);
        }

        /// <summary>
        /// Test Link with Invalid Weight
        /// </summary>
        /// <param name="weight"></param>
        [TestCase(-1)]
        [TestCase(double.MinValue)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(double.PositiveInfinity)]
        [ExpectedException(ExpectedException = typeof(ArgumentException),
            UserMessage = "Weight should be a non-negative number and less than PositiveInfinity")]
        public void TestLink_InvalidWeight(double weight)
        {
            var node1 = _roadSystem.CreateNewNode(1);
            var node2 = _roadSystem.CreateNewNode(2);

            _roadSystem.Link(node1, node2, weight);
        }

        /// <summary>
        /// Test Load
        /// </summary>
        [Test]
        public void TestLoad()
        {
            var reader = File.OpenText(@"Samples\SuccessRoadSystem.xml");

            RoadSystem loadedSystem = RoadSystem.Load(reader.ReadToEnd());

            Assert.AreEqual(12, loadedSystem.Count);
        }

        /// <summary>
        /// Test LoadFrom Xml
        /// </summary>
        [Test]
        public void TestLoadFromXml()
        {
            RoadSystem loadedSystem = RoadSystem.LoadFromXml(@"Samples\SuccessRoadSystem.xml");

            Assert.AreEqual(12, loadedSystem.Count);
        }

        /// <summary>
        /// Test fail cases of Load
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="exceptionType"></param>
        /// <param name="message"></param>
        [Test]
        [TestCaseSource("LoadFailTestCase")]
        public void TestLoad_FailCases(string filename, Type exceptionType, string message )
        {
            var reader = File.OpenText(Path.Combine("Samples", filename));
            try
            {
                RoadSystem loadedSystem = RoadSystem.Load(reader.ReadToEnd());
                Assert.Fail("Exepected error did not happened");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(typeof(LoadRoadSystemException), ex.GetType());
                Regex regex = new Regex("^" + message);
                Assert.IsTrue(regex.IsMatch(ex.Message));
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test Fail cases LoadFromXML
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="exceptionType"></param>
        /// <param name="message"></param>
        [Test]
        [TestCaseSource("LoadFailTestCase")]
        public void TestLoadFromXml_FailCases(string filename, Type exceptionType, string message)
        {
            try
            {
                RoadSystem loadedSystem = RoadSystem.LoadFromXml(Path.Combine("Samples", filename));
                Assert.Fail("Exepected error did not happened");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(typeof(LoadRoadSystemException), ex.GetType());
                Regex regex = new Regex("^" + message);
                Assert.IsTrue(regex.IsMatch(ex.Message));
            }
        }
        #endregion

        #region Datasource for Data-Driven Test
        /// <summary>
        /// Test cases for Create New Node
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object[]> CreateNewNodeTestCase()
        {
            return new[] {
                new object[] {1, true, NodeRole.Normal  },
                new object[] {1, true, NodeRole.Start   },
                new object[] {1, true, NodeRole.Finish  },
                new object[] {1, false, NodeRole.Normal },
                new object[] {1, false, NodeRole.Start  },
                new object[] {1, false, NodeRole.Finish },
            };
        }

        /// <summary>
        /// Test cases for fail cases of Load and LoadFromXMl
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object[]> LoadFailTestCase()
        {
            return new[] {
                new object[] {"FailXML.xml", typeof(LoadRoadSystemException), @"Error while loading" },
                new object[] {"MultipleStartNode.xml", typeof(LoadRoadSystemException), @"Error while loading" },
                new object[] {"MultipleEndNode.xml", typeof(LoadRoadSystemException), @"Error while loading" },
                new object[] {"NoNode.xml", typeof(LoadRoadSystemException), "No node found" },
                new object[] {"NoStartNode.xml", typeof(LoadRoadSystemException), "No Start node found" },
                new object[] {"NoEndNode.xml", typeof(LoadRoadSystemException), "No End node found" },
                new object[] {"Only1Node.xml", typeof(LoadRoadSystemException), "There only 1 node in the road system" },
                new object[] {"NodeWithoutID1.xml", typeof(LoadRoadSystemException), "Node without ID" },
                new object[] {"NodeWithoutID2.xml", typeof(LoadRoadSystemException), "Node without ID" },
                new object[] {"NodeWithInvalidID.xml", typeof(LoadRoadSystemException), "Node with invalid ID. ID should be a number" },
                new object[] {"LinkWithoutWeight.xml", typeof(LoadRoadSystemException), "Link does not have weight attribute" },
                new object[] {"LinkWithoutRef.xml", typeof(LoadRoadSystemException), "Link does not have ref attribute" },
                new object[] {"LinkWithInvalidWeight.xml", typeof(LoadRoadSystemException), "Link with invalid weight" },
                new object[] {"LinkWithInvalidRef.xml", typeof(LoadRoadSystemException), "Link with invalid ref ID" },
                new object[] {"LinkToNonExist.xml", typeof(LoadRoadSystemException), "Link points to non-existing node" },
                new object[] {"DifferentWeight.xml", typeof(LoadRoadSystemException), "Different weight between 2 nodes is not allowed" },
            };
        }
        #endregion
    }
}
