using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoadSystemLib
{
    public static class Extension
    {
        /// <summary>
        /// ForEach Extension for IEnumerable of T. It's equipvalence to foreach loop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="action">Action for execute on each item</param>
        public static void ForEach<T>(this IEnumerable<T> target, Action<T> action)
        {
            // Iterate throw collection and execute action on each item
            foreach (T item in target) action(item);
        }
    }
}
