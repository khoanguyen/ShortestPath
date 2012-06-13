using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShortestPath.Model
{
    /// <summary>
    /// Class for storing result from ShortestPath finder
    /// </summary>
    public class ResultItem
    {
        /// <summary>
        /// Loaded filename
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Result string, could be an array of node IDs or an error message
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Indicate that there was an error while loading file
        /// </summary>
        public bool HasError { get; set; }        
    }
}
