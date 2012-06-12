using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    /// <summary>
    /// Exception for indicating errors while loading RoadSystem
    /// </summary>
    public class LoadRoadSystemException : Exception
    {
        public LoadRoadSystemException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
