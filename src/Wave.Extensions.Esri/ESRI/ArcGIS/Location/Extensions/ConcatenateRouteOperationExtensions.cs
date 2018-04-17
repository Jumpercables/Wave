﻿using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace ESRI.ArcGIS.Location
{
    /// <summary>
    ///     Removes redundant information from event tables or separates event tables having more than one descriptive
    ///     attribute into individual tables
    /// </summary>
    public static class ConcatenateRouteOperationExtensions
    {
        #region Public Methods

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
        public static ITable Concatenate(this ITable table, IRouteEventProperties2 source, IWorkspace outputWorkspace, string outputTableName, IRouteEventProperties2 output, ITrackCancel trackCancel, params string[] concatenateFields)
        {
            var outputName = new TableNameClass();
            outputName.WorkspaceName = (IWorkspaceName)((IDataset)outputWorkspace).FullName;
            outputName.Name = outputTableName;

            outputWorkspace.Delete(outputName);

            IRouteMeasureEventGeoprocessor2 gp = new RouteMeasureGeoprocessorClass();
            gp.InputEventProperties = source;
            gp.InputTable = table;
            gp.KeepZeroLengthLineEvents = false;

            return gp.Concatenate2(output, concatenateFields, outputName, trackCancel, "");
        }

        #endregion
    }
}