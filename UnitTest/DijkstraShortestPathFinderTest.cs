using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RoadSystemLib;
using System.IO;

namespace UnitTest
{
    [TestFixture]
    public class DijkstraShortestPathFinderTest
    {
        private DijkstraShortestPathFinder _finder = new DijkstraShortestPathFinder();

        #region Test Methods
        /// <summary>
        /// Test Dijkstra Finder
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="expected"></param>
        [Test]
        [TestCaseSource("FinderTestCase")]
        public void TestFinder(string inputFile, int[] expected)
        {
            // Load RoadSystem
            var system = LoadSystem(inputFile);

            // Get Shortest Path
            var result = _finder.FindShortestPath(system);

            // Compare the result with expected
            Assert.IsTrue(AreArrayEqual(expected,
                result.Select(roadNode => roadNode.ID).ToArray()));            
        }
        #endregion

        #region Datasource for Data-Driven Test
        /// <summary>
        /// Test cases for testing finder
        /// </summary>
        /// <returns></returns>
        private IEnumerable<object[]> FinderTestCase()
        {
            return new[] {
                new object[] { "System1.xml", new int[] {1, 2, 3} },
                new object[] { "System2.xml", new int[] {1, 2, 5} },
                new object[] { "System3.xml", new int[] {1, 4, 2, 5, 3} },
                new object[] { "CrashedEndNode.xml", new int[0] },
                new object[] { "CrashedStartNode.xml", new int[0] },
                new object[] { "SystemWithCrashedNode.xml", new int[] {1, 2, 3, 5, 6} },
                new object[] { "UnreachableEndNode.xml", new int[0] },
                new object[] { "SampleRoadSystem.xml", new int[] {1, 6, 8, 9, 10} }
            };
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Load RoadSystem
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private RoadSystem LoadSystem(string filename)
        {
            return RoadSystem.LoadFromXml(Path.Combine("DijkstraSamples", filename));
        }

        /// <summary>
        /// Comapre 2 arrays
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        private bool AreArrayEqual(int[] a1, int[] a2)
        {
            // Return false if lengths are different
            if (a1.Length != a2.Length) return false;
            
            // Compare elements between 2 arrays
            for (int i = 0; i < a1.Length; i++)
            {
                // return false if elements at index i are different
                if (a1[i] != a2[i]) return false;
            }

            // Return true
            return true;
        }
        #endregion
    }
}
