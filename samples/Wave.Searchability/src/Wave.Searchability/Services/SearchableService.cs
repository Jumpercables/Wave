﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

using Miner.Framework;
using Miner.Interop;

using Wave.Searchability.Data;

namespace Wave.Searchability.Services
{
    /// <summary>
    ///     A search service that allows for 'google-like' search capabilities
    ///     in the scense that given a keyword it will search all of the approriate fields in the table or feature classes
    ///     specified in the configurations.
    /// </summary>
    public abstract class SearchableService<TSearchableRequest>
        where TSearchableRequest : SearchableRequest, new()
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SearchableService{TSearchableRequest}" /> class.
        /// </summary>
        protected SearchableService()
        {
            this.Response = new SearchableResponse();
        }

        #endregion

        #region Protected Properties

        /// <summary>
        ///     Gets a value indicating whether there is a pending cancellation.
        /// </summary>
        /// <value>
        ///     <c>true</c> if there is a pending cancellation; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool CancellationPending
        {
            get { return this.Response != null && this.Threshold > 0 && this.Response.Count >= this.Threshold; }
        }

        /// <summary>
        ///     Gets or sets the response.
        /// </summary>
        /// <value>
        ///     The response.
        /// </value>
        protected SearchableResponse Response { get; set; }

        /// <summary>
        ///     Gets or sets the threshold.
        /// </summary>
        /// <value>
        ///     The threshold.
        /// </value>
        protected int Threshold { get; set; }

        #endregion

        #region Public Methods
        
        /// <summary>
        ///     Searches the active map using the specified <paramref name="request" /> for the specified keywords.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        ///     Returns a <see cref="SearchableResponse" /> representing the contents of the results.
        /// </returns>
        public virtual SearchableResponse Find(TSearchableRequest request)
        {
            var layers = Document.ActiveMap.Where<IFeatureLayer>(layer => layer.Valid).DistinctBy(o => o.FeatureClass.ObjectClassID).ToList();
            this.SearchLayers(request, layers);

            var tables = Document.ActiveMap.GetTables().DistinctBy(o => ((IDataset) o).Name).ToList();
            this.SearchTables(request, tables, layers);

            return this.Response;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Adds the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="request">The request.</param>
        protected virtual void Add(IRow row, IFeatureLayer layer, TSearchableRequest request)
        {
            var name = ((IDataset) layer.FeatureClass).Name;
            var list = new List<int>();

            if (!this.Response.ContainsKey(name))
                this.Response.Add(name, list);
            else
                list = this.Response[name];

            if (!list.Contains(row.OID))
                list.Add(row.OID);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Associates the specified object with the layer (when specified) otherwise it associates it with the relationship
        ///     path.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="path">The path.</param>
        /// <param name="index">The index.</param>
        /// <param name="layers">The layers.</param>
        /// <param name="request">The request.</param>
        private void Attach(IObject search, IFeatureLayer layer, List<string> path, int index, List<IFeatureLayer> layers, TSearchableRequest request)
        {
            if (path == null) return;
            if (path.Count == 0) return;
            if (index < 0 || index > path.Count) return;

            bool reset = (layer == null);

            IObjectClass searchClass = search.Class;
            IEnumRelationshipClass relationshipClasses = searchClass.RelationshipClasses[esriRelRole.esriRelRoleAny];
            foreach (var relClass in relationshipClasses.AsEnumerable())
            {
                // A stop has been requested or the threshold has been reached.
                if (this.CancellationPending) return;

                // Invalid relationship class.
                if (relClass.RelationshipClassID == -1)
                    continue;

                IDataset dataset = (IDataset) relClass;

                // When the path names match continue.
                if (!dataset.Name.Equals(path[index], StringComparison.CurrentCultureIgnoreCase)
                    && !path[index].Equals(Searchable.Any))
                    continue;

                // Obtain the related objects.
                ISet set = relClass.GetObjectsRelatedToObject(search);
                set.Reset();

                // There are no related objects continue to next
                if (set.Count == 0)
                    continue;

                // When the layer is not provided attempt to find it.
                if (layer == null)
                    layer = this.GetRelationshipLayer(searchClass, relClass, layers);

                if (layer == null)
                    continue;

                IObjectClass layerClass = layer.FeatureClass;

                // Iterate through the object locating the related layer.
                foreach (var obj in set.AsEnumerable<IObject>())
                {
                    if (obj.Class == layerClass)
                    {
                        // Add the related object layer.
                        this.Add(obj, layer, request);
                    }
                    else
                    {
                        // Move up the path to the next relationship.
                        this.Attach(obj, layer, path, index - 1, layers, request);
                    }

                    // A stop has been requested or the threshold has been reached.
                    if (this.CancellationPending) return;
                }

                // Reset the layer when it was provided to the method empty.
                if (reset) layer = null;
            }
        }


        /// <summary>
        ///     Gets the layer that is associated to the specified relationship.
        /// </summary>
        /// <param name="searchClass">The search class.</param>
        /// <param name="relClass">The rel class.</param>
        /// <param name="sources">The sources.</param>
        /// <returns>
        ///     The <see cref="IFeatureLayer" /> that is associated to the relationship; otherwise <c>null</c>.
        /// </returns>
        private IFeatureLayer GetRelationshipLayer(IObjectClass searchClass, IRelationshipClass relClass, IEnumerable<IFeatureLayer> sources)
        {
            // Used to make sure the layers belong to the same workspace.
            IMMWorkspaceManager manager = new MMWorkspaceManager();

            // Make sure we have a valid layer in the map.
            IWorkspace targetWorkspace = ((IDataset) relClass).Workspace;

            // Make sure we are not searching the same class.
            IObjectClass oclass = (searchClass == relClass.OriginClass) ? relClass.DestinationClass : relClass.OriginClass;
            string name = ((IDataset) oclass).Name;

            var layers = sources.Where(layer => ((IDataset) layer.FeatureClass).Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            foreach (IFeatureLayer layer in layers)
            {
                IWorkspace sourceWorkspace = ((IDataset) layer.FeatureClass).Workspace;
                if (!manager.IsSameDatabase(targetWorkspace, sourceWorkspace))
                    continue;

                return layer;
            }

            return null;
        }

        /// <summary>
        ///     Searches the layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <param name="item">The item.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="request">The request.</param>
        private void SearchLayer(IFeatureLayer layer, SearchableTable item, IQueryFilter filter, TSearchableRequest request)
        {
            if (filter == null || string.IsNullOrEmpty(filter.WhereClause))
                return;

            if (item.IsFeatureClass && item.LayerDefinition)
            {
                IFeatureLayerDefinition featureLayerDefinition = (IFeatureLayerDefinition) layer;
                if (!string.IsNullOrEmpty(featureLayerDefinition.DefinitionExpression))
                    filter.WhereClause = string.Format("({0}) {1} ({2})", filter.WhereClause, LogicalOperator.And, featureLayerDefinition.DefinitionExpression);
            }

            IFeatureClass searchClass = layer.FeatureClass;
            foreach (var feature in searchClass.Fetch(filter))
            {
                if (this.CancellationPending)
                    return;

                this.Add(feature, layer, request);
            }
        }

        /// <summary>
        ///     Searches the layers.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="layers">The layers.</param>
        private void SearchLayers(TSearchableRequest request, List<IFeatureLayer> layers)
        {
            var sets = request.Items.Select(set => set.Tables.Where(table => table.IsFeatureClass));
            foreach (var set in sets)
            {
                foreach (var table in set)
                {
                    var item = table;

                    foreach (var layer in layers.Where(o => ((IDataset) o).Name.Equals(item.Name) || (item.NameAsClassModelName && o.FeatureClass.IsAssignedClassModelName(item.Name))))
                    {
                        var filter = layer.FeatureClass.CreateQuery(request.Keywords, request.ComparisonOperator, request.LogicalOperator, item.Fields.Select(o => o.Name).ToArray());
                        this.SearchLayer(layer, table, filter, request);

                        if (this.CancellationPending) return;

                        this.TraverseRelationships(layer.FeatureClass, null, null, item.Relationships, request, layers);
                    }
                }
            }
        }

        /// <summary>
        ///     Searches the relationship.
        /// </summary>
        /// <param name="searchClass">The search class.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="item">The item.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="request">The request.</param>
        /// <param name="layers">The layers.</param>
        private void SearchRelationship(IObjectClass searchClass, IFeatureLayer layer, SearchableItem item, SearchableRelationship relationship, TSearchableRequest request, List<IFeatureLayer> layers)
        {
            IQueryFilter filter = searchClass.CreateQuery(request.Keywords, request.ComparisonOperator, request.LogicalOperator, relationship.Fields.Select(o => o.Name).ToArray());
            if (filter == null || string.IsNullOrEmpty(filter.WhereClause))
                return;

            List<string> path = item != null ? item.Path : new List<string>(new[] {relationship.Name});

            foreach (var row in ((ITable) searchClass).Fetch(filter))
            {
                if (this.CancellationPending)
                    return;

                this.Attach((IObject) row, layer, path, path.Count - 1, layers, request);
            }
        }

        /// <summary>
        ///     Searches the table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="item">The item.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="layers">The layers.</param>
        /// <param name="request">The request.</param>
        private void SearchTable(ITable table, SearchableItem item, IQueryFilter filter, List<IFeatureLayer> layers, TSearchableRequest request)
        {
            if (filter == null || string.IsNullOrEmpty(filter.WhereClause))
                return;

            if (!item.Relationships.Any())
            {
                item.Relationships.Add(new SearchableRelationship());
            }

            foreach (var row in table.Fetch(filter))
            {
                if (this.CancellationPending) return;

                foreach (var relationship in item.Relationships)
                    this.Attach((IObject) row, null, relationship.Path, relationship.Path.Count - 1, layers, request);
            }
        }

        /// <summary>
        ///     Searches the tables.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="tables">The tables.</param>
        /// <param name="layers">The layers.</param>
        private void SearchTables(TSearchableRequest request, List<ITable> tables, List<IFeatureLayer> layers)
        {
            var sets = request.Items.Select(set => set.Tables.Where(table => !table.IsFeatureClass));
            foreach (var set in sets)
            {
                foreach (var table in set)
                {
                    var item = table;

                    foreach (var standalone in tables.Where(o => ((IDataset) o).Name.Equals(item.Name) || (item.NameAsClassModelName && o.IsAssignedClassModelName(item.Name))))
                    {
                        var searchClass = (IObjectClass) standalone;

                        var filter = searchClass.CreateQuery(request.Keywords, request.ComparisonOperator, request.LogicalOperator, item.Fields.Select(o => o.Name).ToArray());
                        this.SearchTable(standalone, item, filter, layers, request);

                        if (this.CancellationPending) return;

                        this.TraverseRelationships(searchClass, null, null, item.Relationships, request, layers);
                    }
                }
            }
        }

        /// <summary>
        ///     Traverses the relationships.
        /// </summary>
        /// <param name="objectClass">The search class.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="item">The item.</param>
        /// <param name="relationships">The relationships.</param>
        /// <param name="request">The request.</param>
        /// <param name="layers">The layers.</param>
        private void TraverseRelationships(IObjectClass objectClass, IFeatureLayer layer, SearchableItem item, IEnumerable<SearchableRelationship> relationships, TSearchableRequest request, List<IFeatureLayer> layers)
        {
            if (objectClass == null || relationships == null)
                return;

            foreach (var relationship in relationships)
            {
                if (this.CancellationPending)
                    break;

                IEnumRelationshipClass relationshipClasses = objectClass.RelationshipClasses[esriRelRole.esriRelRoleAny];
                foreach (var relationshipClass in relationshipClasses.AsEnumerable())
                {
                    if (this.CancellationPending)
                        return;

                    string name = ((IDataset) relationshipClass).Name;
                    if (relationship.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) || relationship.Name.Equals(Searchable.Any))
                    {
                        IObjectClass searchClass = objectClass == relationshipClass.OriginClass ? relationshipClass.DestinationClass : relationshipClass.OriginClass;
                        this.SearchRelationship(searchClass, layer, item, relationship, request, layers);

                        if (this.CancellationPending)
                            return;

                        this.TraverseRelationships(searchClass, layer, relationship, relationship.Relationships, request, layers);
                    }
                }
            }
        }

        #endregion
    }
}