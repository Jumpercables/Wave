﻿using System;
using System.Collections.Generic;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace ESRI.ArcGIS.Carto
{
    /// <summary>
    /// Provides extension methods for the <see cref="IEnumLayer" /> enumeration.
    /// </summary>
    public static class LayerExtensions
    {
        #region Public Methods

        /// <summary>
        ///     Creates an <see cref="IEnumerable{T}" /> from an <see cref="ILayer" />
        /// </summary>
        /// <param name="source">An <see cref="IEnumLayer" /> to create an <see cref="IEnumerable{T}" /> from.</param>
        /// <returns>An <see cref="IEnumerable{T}" /> that contains the layers from the input source.</returns>
        public static IEnumerable<ILayer> AsEnumerable(this IEnumLayer source)
        {
            if (source != null)
            {
                source.Reset();
                ILayer layer = source.Next();
                while (layer != null)
                {
                    yield return layer;
                    layer = source.Next();
                }
            }
        }

        /// <summary>
        ///     Performs an identify operation with the provided geometry.
        ///     When identifying layers, typically a small envelope is passed in rather than a point to account for differences in
        ///     the precision of the display and the feature geometry.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="geometry">The geometry.</param>
        /// <param name="trackCancel">
        ///     The cancel tracker object to monitor the Esc key and to terminate processes at the request of
        ///     the user.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="IEnumerable{IFeatureIdentifyObj}" /> representing the features that have been identified.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">geometry</exception>
        /// <remarks>
        ///     On a FeatureIdentifyObject, you can do a QI to the IIdentifyObj interface to get more information about the
        ///     identified feature. The IIdentifyObj interface returns the window handle, layer, and name of the feature; it has
        ///     methods to flash the
        ///     feature in the display and to display a context menu at the Identify location.
        /// </remarks>
        public static IEnumerable<IFeatureIdentifyObj> Identify(this IFeatureLayer source, IGeometry geometry, ITrackCancel trackCancel = null)
        {
            if (geometry == null) throw new ArgumentNullException("geometry");

            if (source != null)
            {
                IIdentify2 identify = (IIdentify2) source;
                IArray array = identify.Identify(geometry, trackCancel);
                if (array != null)
                {
                    for (int i = 0; i < array.Count; i++)
                    {
                        if (trackCancel != null && !trackCancel.Continue())
                            break;

                        IFeatureIdentifyObj featureIdentify = array.Element[i] as IFeatureIdentifyObj;
                        yield return featureIdentify;
                    }
                }
            }
        }

        #endregion        
    }
}