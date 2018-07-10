using System;

namespace Cogito.Autofac
{

    /// <summary>
    /// Provides a sort order for attribute based registration. Higher priorities are registered first.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterPriorityAttribute :
        Attribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="priority"></param>
        public RegisterPriorityAttribute(int priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// Priority of the registration. Higher priorities are registered first.
        /// </summary>
        public int Priority { get; set; }

    }

}
