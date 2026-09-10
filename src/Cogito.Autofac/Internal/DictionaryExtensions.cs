// Ported from Cogito.Core (https://github.com/alethic/Cogito), Cogito.Collections.DictionaryExtensions.

using System;
using System.Collections.Generic;

namespace Cogito.Autofac.Internal
{

    /// <summary>
    /// Provides extension methods for working with dictionaries.
    /// </summary>
    static class DictionaryExtensions
    {

        /// <summary>
        /// Gets the value for the specified key, or the default value of the type.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return self.TryGetValue(key, out var v) ? v : default;
        }

    }

}
