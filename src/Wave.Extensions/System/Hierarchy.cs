﻿using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    ///     Provides methods and properties for the hierarchy data structure.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [ComVisible(false)]
    [ClassInterface(ClassInterfaceType.None)]
    public class Hierarchy<TValue> : IHierarchy<TValue>
        where TValue : class
    {
        #region IHierarchy<TValue> Members

        /// <summary>
        ///     Gets or sets the child nodes.
        /// </summary>
        /// <value>
        ///     The child nodes.
        /// </value>
        public IEnumerable<IHierarchy<TValue>> Children { get; set; }

        /// <summary>
        ///     Gets or sets the depth.
        /// </summary>
        /// <value>
        ///     The depth.
        /// </value>
        public int Depth { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public TValue Value { get; set; }

        /// <summary>
        ///     Gets or sets the parent.
        /// </summary>
        /// <value>
        ///     The parent.
        /// </value>
        public TValue Parent { get; set; }

        #endregion
    }
}