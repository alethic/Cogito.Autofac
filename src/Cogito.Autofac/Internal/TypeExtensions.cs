// Ported from Cogito.Core (https://github.com/alethic/Cogito), Cogito.Reflection.TypeExtensions.

using System;

namespace Cogito.Autofac.Internal
{

    /// <summary>
    /// Provides extension methods for working with <see cref="Type"/> instances.
    /// </summary>
    static class TypeExtensions
    {

        /// <summary>
        /// Returns <c>true</c> if the given type is a constructed version of the given generic type definition.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericTypeDefinition"></param>
        /// <returns></returns>
        public static bool IsInstanceOfGenericType(this Type type, Type genericTypeDefinition)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (genericTypeDefinition == null)
                throw new ArgumentNullException(nameof(genericTypeDefinition));

            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition;
        }

    }

}
