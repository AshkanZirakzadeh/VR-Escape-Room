using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CirrusPlay.PortalLibrary.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Quick and easy way to add a requested list of items to a collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enm"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> AddTo<T>(this IEnumerable<T> enm, ICollection<T> list)
        {
            foreach (var o in enm)
                list.Add(o);

            return enm;
        }

        public static IEnumerable<T> Each<T>(this IEnumerable<T> em, Action<T> predicate)
        {
            foreach (var e in em)
                predicate(e);
            return em;
        }

        public static IEnumerable<T> EachNotNull<T>(this IEnumerable<T> em, Action<T> predicate)
        {
            foreach (var e in em)
                if (e != null) predicate(e);
            return em;
        }

        /// <summary>
        /// Specialized implementation of Each which replaces the underlying foreach with a for loop to remove possible, unnecessary allocations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IList<T> Each<T>(this IList<T> list, Action<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
                predicate(list[i]);
            return list;
        }
    }
}
