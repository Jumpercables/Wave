﻿using System.Runtime.Serialization;
using System.ServiceModel;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

using Miner.Framework;

using Wave.Searchability.Data;

namespace Wave.Searchability.Services
{
    /// <summary>
    ///     A service contract for searching the active map session for table(s), class(es) and relationship(s).
    /// </summary>
    [ServiceContract]
    public interface IMapSearchService : ISearchableService<MapSearchServiceRequest>
    {
    }

    /// <summary>
    ///     The map-based search service for querying a set of table(s), class(es) and relationship(s) using the data in the
    ///     active session, and additional the matching rows are linked to a feature so that it can be located on a map.
    ///     When tables are searched, matches will be linked to the feature that participates in a relationship with the
    ///     matched non-spatial row.
    /// </summary>
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public sealed class MapSearchService : SearchableService<MapSearchServiceRequest>, IMapSearchService
    {
        #region Protected Methods

        /// <summary>
        ///     Adds the specified row (or feature) to the response.
        /// </summary>
        /// <param name="row">The row or feature.</param>
        /// <param name="layer">The feature layer for the row (when the row is a feature class).</param>
        /// <param name="isFeatureClass">if set to <c>true</c> when the row is a feature class.</param>
        /// <param name="request">The request.</param>
        protected override void Add(IRow row, IFeatureLayer layer, bool isFeatureClass, MapSearchServiceRequest request)
        {
            if (isFeatureClass)
            {
                var feature = (IFeature) row;
                var relOp = (IRelationalOperator) Document.ActiveView.Extent.Envelope;

                switch (request.Extent)
                {
                    case MapSearchServiceExtent.WithinCurrentExtent:
                        if (relOp.Within(feature.Shape))
                            base.Add(row, layer, true, request);

                        break;

                    case MapSearchServiceExtent.WithinCurrentOrOverlappingExtent:
                        if (relOp.Within(feature.Shape) || relOp.Overlaps(feature.Shape))
                            base.Add(row, layer, true, request);

                        break;

                    default:
                        base.Add(row, layer, true, request);
                        break;
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///     The requests that are issued to the searchable service.
    /// </summary>
    [DataContract(Name = "request")]
    public class MapSearchServiceRequest : TextSearchServiceRequest
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MapSearchServiceRequest" /> class.
        /// </summary>
        public MapSearchServiceRequest()
        {
            this.Extent = MapSearchServiceExtent.WithinAnyExtent;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the extent.
        /// </summary>
        /// <value>
        ///     The extent.
        /// </value>
        [DataMember(Name = "extent")]
        public MapSearchServiceExtent Extent { get; set; }

        #endregion
    }

    /// <summary>
    ///     An enumeration of the searchable extents.
    /// </summary>
    public enum MapSearchServiceExtent
    {
        /// <summary>
        ///     Within any extent.
        /// </summary>
        WithinAnyExtent = 0,

        /// <summary>
        ///     Within the current extent.
        /// </summary>
        WithinCurrentExtent = 1,

        /// <summary>
        ///     Within current or overlapping extent.
        /// </summary>
        WithinCurrentOrOverlappingExtent = 2
    }
}