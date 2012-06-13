using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Windows.Threading;
using ShortestPath.ViewModel;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace UnitTest
{
    /// <summary>
    /// MainViewModel Test
    /// </summary>
    [TestFixture]
    public class MainViewModelTest
    {
        private MainViewModel _viewModel;

        /// <summary>
        /// Test SetUp
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // Create a DispatcherFrame object as a mock dispatcher for the view-model
            DispatcherFrame dispatcherFrame = new DispatcherFrame();
            
            // Initialize viewModel
            _viewModel = new MainViewModel(dispatcherFrame);
        }

        /// <summary>
        /// Test TearDown
        /// </summary>
        public void TearDown()
        {            
            _viewModel = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Test LoadFilesAsync method
        /// </summary>
        /// <param name="description"></param>
        /// <param name="input"></param>
        [Test]
        [TestCaseSource("LoadFilesAsyncTestCases")]
        public void TestLoadFilesAsync(string description, Dictionary<string, string> input)
        {
            // Call LoadFilesAsync and wait for it finishes its job
            var ar = _viewModel.LoadFilesAsync(input.Keys);
            ar.AsyncWaitHandle.WaitOne();

            // Assert the result
            foreach (var item in _viewModel.ResultItems)
            {
                Assert.AreEqual(input[Path.Combine("DijkstraSamples", item.Filename)], item.Result);
            }
        }

        /// <summary>
        /// Test LoadFilesAsync fail loading cases
        /// </summary>
        [Test(Description="Fail Loading cases")]
        public void TestLoadFilesAsync_FailLoad()
        {
            // Input and expected results
            Dictionary<string, string> input = new Dictionary<string, string>
            {
                {@"Samples\FailXML.xml", @"Error while loading" },
                {@"Samples\MultipleStartNode.xml", @"Error while loading" },
                {@"Samples\MultipleEndNode.xml", @"Error while loading" },
                {@"Samples\NoNode.xml", "No node found" },
                {@"Samples\NoStartNode.xml", "No Start node found" },
                {@"Samples\NoEndNode.xml", "No End node found" },
                {@"Samples\Only1Node.xml", "There only 1 node in the road system" },
                {@"Samples\NodeWithoutID1.xml", "Node without ID" },
                {@"Samples\NodeWithoutID2.xml", "Node without ID" },
                {@"Samples\NodeWithInvalidID.xml", "Node with invalid ID. ID should be a number" },
                {@"Samples\LinkWithoutWeight.xml", "Link does not have weight attribute" },
                {@"Samples\LinkWithoutRef.xml", "Link does not have ref attribute" },
                {@"Samples\LinkWithInvalidWeight.xml", "Link with invalid weight" },
                {@"Samples\LinkWithInvalidRef.xml", "Link with invalid ref ID" },
                {@"Samples\LinkToNonExist.xml", "Link points to non-existing node" },
                {@"Samples\DifferentWeight.xml", "Different weight between 2 nodes is not allowed" }            
            };

            // Call LoadFilesAsync and wait for it finishes its job           
            var ar = _viewModel.LoadFilesAsync(input.Keys);
            ar.AsyncWaitHandle.WaitOne();

            // Assert the Result
            foreach (var item in _viewModel.ResultItems)
            {
                Assert.IsTrue(item.HasError);
                Regex regex = new Regex("^" + input[Path.Combine("Samples", item.Filename)]);
                Assert.IsTrue(regex.IsMatch(item.Result));
            }
        }

        /// <summary>
        /// Test cases for LoadFilesAsync good cases
        /// </summary>
        /// <returns></returns>
        private IEnumerable<object[]> LoadFilesAsyncTestCases()
        {
            return new object[][] {
                new object[] { "Shortest path found", new Dictionary<string, string> {
                        { @"DijkstraSamples\System1.xml", "1, 2, 3"},
                        { @"DijkstraSamples\System2.xml", "1, 2, 5"},
                        { @"DijkstraSamples\System3.xml", "1, 4, 2, 5, 3"},
                        { @"DijkstraSamples\SampleRoadSystem.xml", "1, 6, 8, 9, 10"},
                    }
                },
                new object[] { "No path found", new Dictionary<string, string> {
                        { @"DijkstraSamples\UnreachableEndNode.xml", "No path found"},
                        { @"DijkstraSamples\CrashedStartNode.xml", "No path found"},
                        { @"DijkstraSamples\CrashedEndNode.xml", "No path found"},
                    }
                }
            };
        }
    }
}
