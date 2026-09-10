// Ported from Cogito.Core (https://github.com/alethic/Cogito), Cogito.Collections.CollectionExtensions.

using System;
using System.Collections.Generic;

namespace Cogito.Autofac.DependencyInjection.Internal
{

    /// <summary>
    /// Provides extension methods for working with collections.
    /// </summary>
    static class CollectionExtensions
    {

        /// <summary>
        /// Adds all of the items to the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="add"></param>
        public static void AddRange<T>(this ICollection<T> self, IEnumerable<T> add)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (add == null)
                throw new ArgumentNullException(nameof(add));

            foreach (var i in add)
                self.Add(i);
        }

    }

}
