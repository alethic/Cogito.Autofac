#if NET462 || NET47

using System;
using System.Collections.Generic;
using System.Text;

namespace Cogito.Autofac
{

    static class EnumerableExtensions
    {

        /// <summary>
        /// Prepends the given item to the front of the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item)
        {
            yield return item;

            foreach (var i in source)
                yield return i;
        }

    }

}

#endif
