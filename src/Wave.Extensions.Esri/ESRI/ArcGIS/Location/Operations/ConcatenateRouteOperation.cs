﻿using System;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace ESRI.ArcGIS.Location
{
    /// <summary>
    /// Removes redundant information from event tables or separates event tables having more than one descriptive
    /// attribute into individual tables
    /// </summary>
    /// <seealso cref="ESRI.ArcGIS.Location.RouteOperation{ESRI.ArcGIS.Location.ConcatenateRouteEventData}" />
    public class ConcatenateRouteOperation : RouteOperation<ConcatenateRouteEventData>
    {
        #region Public Methods

        /// <summary>
        /// Events will be aggregated where the to-measure of one event matches the from-measure of the next event. This option
        /// is applicable only for line events.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="workspace">The workspace that contains the event data.</param>
        /// <param name="trackCancel">The object that allows for monitoring the progress.</param>
        /// <returns>
        /// Returns a <see cref="ITable" /> representing the table that has been created.
        /// </returns>
        /// <exception cref="System.ArgumentException">This operation is applicable only for line events.</exception>
        public override ITable Execute(ConcatenateRouteEventData eventData, IWorkspace workspace, ITrackCancel trackCancel)
        {
            var eventTable = workspace.GetTable("", eventData.EventTableName);
            var linear = eventData.Segmentation as RouteMeasureLineSegmentation;
            if (linear == null) throw new ArgumentException("This operation is applicable only for line events.");

            return this.Execute(eventTable, linear, workspace, eventData.OutputTableName, eventData.Segmentation, trackCancel, eventData.Fields);
        }

        /// <summary>
        ///     Events will be aggregated where the to-measure of one event matches the from-measure of the next event. This option
        ///     is applicable only for line events.
        /// </summary>
        /// <param name="table">The table whose rows will be aggregated.</param>
        /// <param name="source">Parameter consisting of the route location fields and the type of events in the input event table.</param>
        /// <param name="outputWorkspace"></param>
        /// <param name="outputTableName">The table to be created.</param>
        /// <param name="output">
        ///     Parameter consisting of the route location fields and the type of events in the concatenate event
        ///     table.
        /// </param>
        /// <param name="trackCancel">The object that allows for monitoring the progress.</param>
        /// <param name="concatenateFields">The field(s)used to aggregate rows.</param>
        /// <returns>Returns a <see cref="ITable" /> representing the table that has been created.</returns>
        public ITable Execute(ITable table, RouteMeasureLineSegmentation source, IWorkspace outputWorkspace, string outputTableName, RouteMeasureSegmentation output, ITrackCancel trackCancel, params string[] concatenateFields)
        {
            IDatasetName outputName = outputWorkspace.CreateTableName(outputTableName);
            outputWorkspace.Delete(outputName);

            IRouteMeasureEventGeoprocessor2 gp = new RouteMeasureGeoprocessorClass();
            gp.InputEventProperties = source.EventProperties;
            gp.InputTable = table;
            gp.KeepZeroLengthLineEvents = false;

            return gp.Concatenate2(output.EventProperties, concatenateFields, outputName, trackCancel, "");
        }

        #endregion
    }
}