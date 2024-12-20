using System;

using Autofac.Builder;

namespace Cogito.Autofac
{

    /// <summary>
    /// Provides a sort order for attribute based registration. Lower orders are returned first when requesting an <see cref="IOrderedEnumerable{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterOrderAttribute :
        RegisterBuilderAttribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="order"></param>
        public RegisterOrderAttribute(int order)
        {
            Order = order;
        }

        /// <summary>
        /// Order of resolution.
        /// </summary>
        public int Order { get; set; }

        public override IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Build<TLimit, TActivatorData, TRegistrationStyle>(
            Type type, 
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.OrderBy(i => Order);
        }
    }

}
