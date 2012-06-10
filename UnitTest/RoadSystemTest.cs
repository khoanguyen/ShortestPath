using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RoadSystemLib;
using System.Threading;

namespace UnitTest
{
    [TestFixture]
    public class RoadSystemTest
    {
        private RoadSystem _roadSystem;

        #region SetUp and TearDown
        [SetUp]
        public void SetUpTest()
        {
            _roadSystem = new RoadSystem();
        }

        [TearDown]
        public void TearDownTest()
        {
            _roadSystem = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion

        #region Test Methods
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

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException),
            UserMessage = "Given id has already been used by another node")]
        public void TestCreateNewNode_DuplicatedID()
        {
            _roadSystem.CreateNewNode(1);
            _roadSystem.CreateNewNode(1);
        }

        [Test]
        public void TestContainsNode()
        {
            RoadSystem anotherSystem = new RoadSystem();
            var anotherNode = anotherSystem.CreateNewNode(2);
            var newNode = _roadSystem.CreateNewNode(1);

            Assert.IsFalse(_roadSystem.Contains(anotherNode));
            Assert.IsTrue(_roadSystem.Contains(newNode));
        }

        [Test]
        public void TestIndexer()
        {
            var newNode = _roadSystem.CreateNewNode(1);

            Assert.AreSame(newNode, _roadSystem[1]);
            Assert.AreSame(null, _roadSystem[2]);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(double.MaxValue)]
        [TestCase(double.PositiveInfinity)]
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

        [TestCase(-1)]
        [TestCase(double.MinValue)]
        [TestCase(double.NegativeInfinity)]
        [ExpectedException(ExpectedException = typeof(ArgumentException),
            UserMessage = "Weight should be a non-negative number")]
        public void TestLink_InvalidWeight(double weight)
        {
            var node1 = _roadSystem.CreateNewNode(1);
            var node2 = _roadSystem.CreateNewNode(2);

            _roadSystem.Link(node1, node2, weight);
        }

        [Test]
        public void TestCreateNewNode_ThreadSafe()
        {

        }

        [Test]
        public void TestLink_ThreadSafe()
        {

        }
        #endregion

        #region Datasource for Data-Driven Test
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
        #endregion

        #region Helpers
        public Thread[] CreateThread(ParameterizedThreadStart execute, int numberOfThread)
        {
            Thread[] result = new Thread[numberOfThread];
            for (var i = 0; i < numberOfThread; result[i++] = new Thread(execute)) ;
            return result;
        }
        #endregion
    }
}
