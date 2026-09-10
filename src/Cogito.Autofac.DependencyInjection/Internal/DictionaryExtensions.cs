// Ported from Cogito.Core (https://github.com/alethic/Cogito), Cogito.Collections.DictionaryExtensions.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Cogito.Autofac.DependencyInjection.Internal
{

    /// <summary>
    /// Provides extension methods for working with dictionaries.
    /// </summary>
    static class DictionaryExtensions
    {

        /// <summary>
        /// Gets the value for the specified key or creates it. This is not thread-safe, except for a
        /// <see cref="ConcurrentDictionary{TKey, TValue}"/>, which provides its own implementation.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> create)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (create == null)
                throw new ArgumentNullException(nameof(create));

            if (self is ConcurrentDictionary<TKey, TValue> concurrent)
                return concurrent.GetOrAdd(key, create);

            return self.TryGetValue(key, out var v) ? v : self[key] = create(key);
        }

    }

}
