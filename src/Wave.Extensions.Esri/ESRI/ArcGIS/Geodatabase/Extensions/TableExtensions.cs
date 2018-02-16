﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase.Internal;
using ESRI.ArcGIS.GeoDatabaseUI;
using ESRI.ArcGIS.Geometry;

namespace ESRI.ArcGIS.Geodatabase
{
    /// <summary>
    ///     Provides extension methods for the <see cref="ESRI.ArcGIS.Geodatabase.ITable" /> interface.
    /// </summary>
    public static class TableExtensions
    {
        #region Public Methods

        /// <summary>
        ///     Performs the calculation by executing the pre-expression and expression.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The filter used to access the rows on which the calculation will be performed.</param>
        /// <param name="fieldName">Field to perform the calculation on.</param>
        /// <param name="expression">Expression or value applied to a field in each row of the cursor.</param>
        /// <param name="preExpression">
        ///     A pre-calculation determination of a value or variable that may be used as the expression
        ///     (or value) of the calculation.
        /// </param>
        /// <param name="showErrorPrompt">if set to <c>true</c> show a message prompt when an error occurs during calculation.</param>
        /// <param name="callback">The call back routine.</param>
        /// <returns>
        ///     Returns a <see cref="IEnvelope" /> representing the features that have been modified by the calculation. This
        ///     envelope can be used to refresh the display for the calculated area only. If the table used in the calculate is
        ///     non-spatial, a null is returned.
        /// </returns>
        public static IEnvelope Calculate(this ITable source, IQueryFilter filter, string fieldName, string expression, string preExpression, bool showErrorPrompt, ICalculatorCallback callback)
        {
            ICalculator calculator = new CalculatorClass();
            calculator.Callback = callback;
            calculator.Cursor = source.Search(filter, false);
            calculator.Field = fieldName;
            calculator.Expression = expression;
            calculator.PreExpression = preExpression;
            calculator.ShowErrorPrompt = showErrorPrompt;

            return calculator.Calculate();
        }

        /// <summary>
        ///     Creates a "google-like" attribute expression query filter based on the specified keyword.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="keyword">The keyword.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>
        ///     Returns a <see cref="string" /> representing the query necessary to locate the keyword.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public static string CreateExpression(this ITable source, string keyword, ComparisonOperator comparisonOperator, LogicalOperator logicalOperator, params string[] fieldNames)
        {
            List<IField> fields = new List<IField>();

            foreach (var fieldName in fieldNames)
            {
                int index = source.FindField(fieldName);
                if (index == -1)
                    throw new IndexOutOfRangeException(string.Format("The '{0}' doesn't have a {1} field.", ((IDataset) source).Name, fieldName));

                var field = source.Fields.Field[index];
                fields.Add(field);
            }

            return source.CreateExpression(keyword, comparisonOperator, logicalOperator, fields.ToArray());
        }

        /// <summary>
        ///     Creates a "google-like" attribute expression query filter based on the specified keyword.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="keyword">The keyword.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        /// <returns>
        ///     Returns a <see cref="string" /> representing the query necessary to locate the keyword.
        /// </returns>
        public static string CreateExpression(this ITable source, string keyword, ComparisonOperator comparisonOperator, LogicalOperator logicalOperator)
        {
            return source.CreateExpression(keyword, comparisonOperator, logicalOperator, source.Fields.AsEnumerable().ToArray());
        }

        /// <summary>
        ///     Creates a "google-like" attribute expression query filter based on the specified keyword and fields.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="keyword">The keyword.</param>
        /// <param name="comparisonOperator">The comparison operator.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>
        ///     Returns a <see cref="string" /> representing the query necessary to locate the keyword.
        /// </returns>
        public static string CreateExpression(this ITable source, string keyword, ComparisonOperator comparisonOperator, LogicalOperator logicalOperator, params IField[] fields)
        {
            return new QueryBuilder(source).Build(keyword, comparisonOperator, logicalOperator, fields);
        }

        /// <summary>
        ///     Creates a row in the table with the default values.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        ///     Returns a <see cref="IRow" /> representing the new row.
        /// </returns>
        public static IRow CreateNew(this ITable source)
        {
            if (source == null) return null;

            var row = source.CreateRow();
            IRowSubtypes rowSubtypes = row as IRowSubtypes;
            if (rowSubtypes != null) rowSubtypes.InitDefaultValues();

            return row;
        }

        /// <summary>
        ///     Deletes the table. You must have exlusive rights to the table in order to delete it.
        ///     Otherwise an error will be thrown.
        /// </summary>
        public static void Delete(this ITable source)
        {
            IDataset ds = (IDataset) source;
            ISchemaLock schemaLock = (ISchemaLock) ds;

            try
            {
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

                ds.Delete();
            }
            finally
            {
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
            }
        }

        /// <summary>
        ///     Determines if the table exists.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Returns a <see cref="bool" /> representing <c>true</c> when the table exists otherwise false</returns>
        public static bool Exists(this ITable source)
        {
            IDataset ds = (IDataset) source;
            IWorkspace2 workspace = (IWorkspace2) ds.Workspace;

            return workspace.NameExists[esriDatasetType.esriDTTable, ds.Name];
        }

        /// <summary>
        ///     Exports the source table using the query filter to the table in the output workspace.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="tableName">Name of the output table.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="handle">The handle.</param>
        /// <param name="errors">The errors that occured during the export.</param>
        /// <returns>
        ///     Returns a <see cref="ITable" /> representing the feature class that was exported.
        /// </returns>
        public static ITable Export(this ITable source, IQueryFilter filter, string tableName, IWorkspace workspace, int handle, out IEnumInvalidObject errors)
        {
            IEnumFieldError fieldError;
            return source.Export(filter, tableName, workspace, source.Fields, handle, null, out errors, out fieldError);
        }

        /// <summary>
        ///     Exports the source table using the query filter to the table in the output workspace.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="tableName">Name of the output table.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="handle">The handle.</param>
        /// <param name="surrogate">The surrogate for the event handler.</param>
        /// <param name="errors">The errors that occured during the export.</param>
        /// <returns>
        ///     Returns a <see cref="ITable" /> representing the feature class that was exported.
        /// </returns>
        public static ITable Export(this ITable source, IQueryFilter filter, string tableName, IWorkspace workspace, int handle, FeatureProgress surrogate, out IEnumInvalidObject errors)
        {
            IEnumFieldError fieldError;
            return source.Export(filter, tableName, workspace, source.Fields, handle, surrogate, out errors, out fieldError);
        }

        /// <summary>
        ///     Exports the source table using the query filter to the table in the output workspace.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="tableName">Name of the output table.</param>
        /// <param name="workspace">The output workspace.</param>
        /// <param name="requiredFields">The required fields.</param>
        /// <param name="handle">The handle.</param>
        /// <param name="surrogate">The surrogate for the event handler.</param>
        /// <param name="invalid">The errors that occured during the export.</param>
        /// <param name="errors">The field errors.</param>
        /// <returns>
        ///     Returns a <see cref="IFeatureClass" /> representing the feature class that was exported.
        /// </returns>
        public static ITable Export(this ITable source, IQueryFilter filter, string tableName, IWorkspace workspace, IFields requiredFields, int handle, FeatureProgress surrogate, out IEnumInvalidObject invalid, out IEnumFieldError errors)
        {
            var ds = (IDataset) source;
            var input = ds.Workspace.Define(ds.Name, new TableNameClass());

            var output = workspace.Define(tableName, new TableNameClass());
            workspace.Delete(output);

            IFieldChecker fieldChecker = new FieldCheckerClass();
            fieldChecker.InputWorkspace = ds.Workspace;
            fieldChecker.ValidateWorkspace = workspace;

            IFields targetFields;
            fieldChecker.Validate(requiredFields, out errors, out targetFields);

            var featureDataConverter = new FeatureDataConverterClass();
            if (surrogate != null)
            {
                surrogate.Advise(featureDataConverter);
            }

            invalid = featureDataConverter.ConvertTable(input, filter, null, output, targetFields, "", 1000, handle);

            return ((IName) output).Open() as ITable;
        }

        /// <summary>
        ///     Queries for the rows that have the specified object ids.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="oids">The list of object ids.</param>
        /// <returns>
        ///     Returns a <see cref="List{IRow}" /> representing the rows returned from the query.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">oids</exception>
        public static IList<IRow> Fetch(this ITable source, params int[] oids)
        {
            if (source == null) return null;
            if (oids == null) throw new ArgumentNullException("oids");

            using (ComReleaser cr = new ComReleaser())
            {
                ICursor cursor = source.GetRows(oids, false);
                cr.ManageLifetime(cursor);

                return cursor.AsEnumerable().ToList();
            }
        }

        /// <summary>
        ///     Queries for the features that satisfies the attribute and/or spatial query as specified by an
        ///     <paramref name="filter" /> object and projects the results into a new form.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="filter">The attribute and/or spatial requirement that the features must satisify.</param>
        /// <param name="selector">Projects each element of a sequence into a new form.</param>
        /// <returns>
        ///     Returns a <see cref="List{TResult}" /> representing the results of the query projected to the type.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">selector</exception>
        public static IList<TResult> Fetch<TResult>(this ITable source, IQueryFilter filter, Func<IRow, TResult> selector)
        {
            if (source == null) return null;
            if (selector == null) throw new ArgumentNullException("selector");

            using (ComReleaser cr = new ComReleaser())
            {
                ICursor cursor = source.Search(filter, false);
                cr.ManageLifetime(cursor);

                return cursor.AsEnumerable().Select(selector).ToList();
            }
        }

        /// <summary>
        ///     Queries for the rows that have the specified object ids and projects the results into a new form.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="selector">Projects each element of a sequence into a new form.</param>
        /// <param name="oids">The list of object ids.</param>
        /// <returns>
        ///     Returns a <see cref="List{TResult}" /> representing the results of the query projected to the type.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     selector
        ///     or
        ///     oids
        /// </exception>
        public static IList<TResult> Fetch<TResult>(this ITable source, Func<IRow, TResult> selector, params int[] oids)
        {
            if (source == null) return null;
            if (selector == null) throw new ArgumentNullException("selector");
            if (oids == null) throw new ArgumentNullException("oids");

            using (ComReleaser cr = new ComReleaser())
            {
                ICursor cursor = source.GetRows(oids, false);
                cr.ManageLifetime(cursor);

                return cursor.AsEnumerable().Select(selector).ToList();
            }
        }

        /// <summary>
        ///     Queries for the rows that satisfy the attribute query as specified by an <paramref name="filter" /> object.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The attribute requirement that the rows must satisify.</param>
        /// <returns>
        ///     Returns a <see cref="List{IRow}" /> representing the rows returned from the query.
        /// </returns>
        public static IList<IRow> Fetch(this ITable source, IQueryFilter filter)
        {
            if (source == null) return null;

            using (ComReleaser cr = new ComReleaser())
            {
                ICursor cursor = source.Search(filter, false);
                cr.ManageLifetime(cursor);

                return cursor.AsEnumerable().ToList();
            }
        }

        /// <summary>
        ///     Queries for the rows that satisfies the attribute and/or spatial query as specified by an
        ///     <paramref name="filter" /> object
        ///     and executes the specified <paramref name="action" /> on each feature returned from the query.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The attribute and/or spatial requirement that the rows must satisify.</param>
        /// <param name="action">The action to take for each row in the cursor.</param>
        /// <returns>
        ///     Returns a <see cref="int" /> representing the number of features affected by the action.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">action</exception>
        /// <exception cref="ArgumentNullException">action</exception>
        public static int Fetch(this ITable source, IQueryFilter filter, Func<IRow, bool> action)
        {
            return source.Fetch(filter, action, true);
        }

        /// <summary>
        ///     Queries for the rows that satisfies the attribute and/or spatial query as specified by an
        ///     <paramref name="filter" /> object
        ///     and executes the specified <paramref name="action" /> on each feature returned from the query.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The attribute and/or spatial requirement that the rows must satisify.</param>
        /// <param name="action">The action to take for each row in the cursor.</param>
        /// <param name="recycling">
        ///     if set to <c>true</c> when the cursor rehydrates a single row object on each fetch and can be
        ///     used to optimize read-only access.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="int" /> representing the number of features affected by the action.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">action</exception>
        /// <exception cref="ArgumentNullException">action</exception>
        public static int Fetch(this ITable source, IQueryFilter filter, Func<IRow, bool> action, bool recycling)
        {
            if (source == null) return 0;
            if (action == null) throw new ArgumentNullException("action");

            int recordsAffected = 0;

            using (ComReleaser cr = new ComReleaser())
            {
                ICursor cursor = source.Search(filter, recycling);
                cr.ManageLifetime(cursor);

                foreach (var row in cursor.AsEnumerable())
                {
                    if (!action(row))
                        return recordsAffected;

                    recordsAffected++;
                }
            }

            return recordsAffected;
        }

        /// <summary>
        ///     Queries for the rows that satisfy the attribute query as specified by an <paramref name="filter" /> object
        ///     and executes the specified <paramref name="action" /> on each row returned from the query.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The attribute requirement that features must satisify.</param>
        /// <param name="action">The action to take for each feature in the cursor.</param>
        /// <returns>
        ///     Returns a <see cref="int" /> representing the number of rows affected by the action.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">action</exception>
        /// <exception cref="ArgumentNullException">action</exception>
        /// <remarks>
        ///     Uses a recycling cursors rehydrate a single feature object on each fetch and can be used to optimize read-only
        ///     access
        /// </remarks>
        public static int Fetch(this ITable source, IQueryFilter filter, Action<IRow> action)
        {
            return source.Fetch(filter, action, true);
        }

        /// <summary>
        ///     Queries for the rows that satisfy the attribute query as specified by an <paramref name="filter" /> object
        ///     and executes the specified <paramref name="action" /> on each row returned from the query.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The attribute requirement that features must satisify.</param>
        /// <param name="action">The action to take for each feature in the cursor.</param>
        /// <param name="recycling">
        ///     if set to <c>true</c> when the cursor rehydrates a single row object on each fetch and can be
        ///     used to optimize read-only access.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="int" /> representing the number of rows affected by the action.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">action</exception>
        /// <exception cref="ArgumentNullException">action</exception>
        /// <remarks>
        ///     Uses a recycling cursors rehydrate a single feature object on each fetch and can be used to optimize read-only
        ///     access
        /// </remarks>
        public static int Fetch(this ITable source, IQueryFilter filter, Action<IRow> action, bool recycling)
        {
            if (source == null) return 0;
            if (action == null) throw new ArgumentNullException("action");

            int recordsAffected = 0;

            using (ComReleaser cr = new ComReleaser())
            {
                ICursor cursor = source.Search(filter, recycling);
                cr.ManageLifetime(cursor);

                foreach (var row in cursor.AsEnumerable())
                {
                    action(row);

                    recordsAffected++;
                }
            }

            return recordsAffected;
        }


        /// <summary>
        ///     Returns the row from the <paramref name="source" /> with the specified <paramref name="oid" /> when the row doesn't
        ///     exist it will return <c>null</c>.
        /// </summary>
        /// <param name="source">The table.</param>
        /// <param name="oid">The key for the row in the table.</param>
        /// <returns>
        ///     Returns an <see cref="IRow" /> representing the row for the oid; otherwise <c>null</c>
        /// </returns>
        public static IRow Fetch(this ITable source, int oid)
        {
            try
            {
                if (source == null) return null;

                return source.GetRow(oid);
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == (int) fdoError.FDO_E_ROW_NOT_FOUND)
                    return null;

                throw;
            }
        }

        /// <summary>
        ///     Gets the name of the delta (either the A or D) table for the versioned <paramref name="source" />.
        /// </summary>
        /// <param name="source">The versioned table or feature class.</param>
        /// <param name="delta">The delta (indicate the A or D) table.</param>
        /// <returns>
        ///     Returns a <see cref="string" /> representing the name of the delta table.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">delta</exception>
        /// <exception cref="System.ArgumentException">
        ///     The delta string must be 1 char long.
        ///     or
        ///     The delta string must contain only 'A' or 'D' chars.
        ///     or
        ///     The table must be versioned for it have a delta table.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The delta string must be 1 char long.
        ///     or
        ///     The delta string must contain only 'A' or 'D' chars.
        /// </exception>
        public static string GetDeltaTableName(this ITable source, string delta)
        {
            if (source == null) return null;
            if (delta == null) throw new ArgumentNullException("delta");

            if (delta.Length != 1)
                throw new ArgumentException("The delta string must be 1 char long.");

            if (delta.Any(@char => @char != 'A' && @char != 'D' && @char != 'a' && @char != 'd'))
                throw new ArgumentException("The delta string must contain only 'A' or 'D' chars.");

            IVersionedTable versionedTable = source as IVersionedTable;
            if (versionedTable == null)
                throw new ArgumentException("The table must be versioned for it have a delta table.");

            string className = ((IDataset) source).Name;
            int index = className.IndexOf('.');
            if (index > 0)
            {
                string ownerName = source.GetSchemaName();
                string tableName = source.GetTableName();

                using (var cr = new ComReleaser())
                {
                    IWorkspace workspace = ((IDataset) source).Workspace;
                    var fws = (IFeatureWorkspace) workspace;
                    var syntax = (ISQLSyntax) workspace;
                    string functionName = syntax.GetFunctionName(esriSQLFunctionName.esriSQL_UPPER);

                    IQueryDef queryDef = fws.CreateQueryDef();
                    queryDef.Tables = "sde.table_registry";
                    queryDef.SubFields = "registration_id";
                    queryDef.WhereClause = string.Format("{2}(table_name) = {2}('{0}') AND {2}(owner) = {2}('{1}')", tableName, ownerName, functionName);

                    ICursor cursor = queryDef.Evaluate();
                    cr.ManageLifetime(cursor);

                    IRow row = cursor.NextRow();
                    return (row != null) ? string.Format("{0}.{1}{2}", ownerName, delta, row.Value[0]) : null;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the registration identifier.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Returns a <see cref="int" /> representing the registation identifier.</returns>
        public static int GetRegistrationId(this ITable source)
        {
            if (source == null) return -1;

            string className = ((IDataset) source).Name;
            int index = className.IndexOf('.');
            if (index > 0)
            {
                string ownerName = source.GetSchemaName();
                string tableName = source.GetTableName();

                using (var cr = new ComReleaser())
                {
                    IWorkspace workspace = ((IDataset) source).Workspace;
                    var fws = (IFeatureWorkspace) workspace;
                    var syntax = (ISQLSyntax) workspace;
                    string functionName = syntax.GetFunctionName(esriSQLFunctionName.esriSQL_UPPER);

                    IQueryDef queryDef = fws.CreateQueryDef();
                    queryDef.Tables = "sde.table_registry";
                    queryDef.SubFields = "registration_id";
                    queryDef.WhereClause = string.Format("{2}(table_name) = {2}('{0}') AND {2}(owner) = {2}('{1}')", tableName, ownerName, functionName);

                    ICursor cursor = queryDef.Evaluate();
                    cr.ManageLifetime(cursor);

                    IRow row = cursor.NextRow();
                    return (row != null) ? TypeCast.Cast(row.Value[0], -1) : -1;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Gets the name of the owner or schema name of the table.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        ///     Returns a <see cref="string" /> representing the name of the owner.
        /// </returns>
        public static string GetSchemaName(this ITable source)
        {
            if (source == null) return null;

            string className = ((IDataset) source).Name;
            int index = className.IndexOf('.');
            if (index > 0)
            {
                string ownerName = className.Substring(0, index);
                return ownerName;
            }

            return null;
        }

        /// <summary>
        ///     Finds the code of the subtype that has the specified <paramref name="subtypeName" />.
        /// </summary>
        /// <param name="source">The table.</param>
        /// <param name="subtypeName">Name of the subtype.</param>
        /// <returns>
        ///     Returns a <see cref="int" /> representing the code of the subtype; otherwise <c>-1</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">subtypeName</exception>
        public static int GetSubtypeCode(this ITable source, string subtypeName)
        {
            if (source == null) return -1;
            if (subtypeName == null) throw new ArgumentNullException("subtypeName");

            ISubtypes subtypes = (ISubtypes) source;
            if (subtypes.HasSubtype) return subtypes.DefaultSubtypeCode;

            foreach (var subtype in subtypes.Subtypes.AsEnumerable().Where(subtype => subtype.Value.Equals(subtypeName, StringComparison.OrdinalIgnoreCase)))
            {
                return subtype.Key;
            }

            return -1;
        }

        /// <summary>
        ///     Gets the subtype name that has the specified <paramref name="subtypeCode" />.
        /// </summary>
        /// <param name="source">The table.</param>
        /// <param name="subtypeCode">The subtype code.</param>
        /// <returns>
        ///     Returns a <see cref="int" /> representing the name of the subtype; otherwise <c>null</c>.
        /// </returns>
        public static string GetSubtypeName(this ITable source, int subtypeCode)
        {
            if (source == null) return null;

            ISubtypes subtypes = (ISubtypes) source;
            if (subtypes.HasSubtype) return null;

            foreach (var subtype in subtypes.Subtypes.AsEnumerable().Where(subtype => subtype.Key == subtypeCode))
            {
                return subtype.Value;
            }

            return null;
        }

        /// <summary>
        ///     Gets the subtype code and name that are assigned to the source.
        /// </summary>
        /// <param name="source">The object class.</param>
        /// <returns>
        ///     Returns a <see cref="IEnumerable{T}" /> representing code and name of the subtypes; otherwise <c>null</c>.
        /// </returns>
        public static IEnumerable<KeyValuePair<int, string>> GetSubtypes(this ITable source)
        {
            if (source == null) return null;

            ISubtypes subtypes = source as ISubtypes;
            if (subtypes == null) return null;

            return subtypes.Subtypes.AsEnumerable();
        }

        /// <summary>
        ///     Gets the name of the table (without the owner or schema name).
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        ///     Returns a <see cref="string" /> representing the name of the table.
        /// </returns>
        public static string GetTableName(this ITable source)
        {
            if (source == null) return null;

            string className = ((IDataset) source).Name;
            int index = className.IndexOf('.');
            if (index > 0)
            {
                string tableName = className.Substring(index + 1, className.Length - index - 1);
                return tableName;
            }

            return className;
        }

        /// <summary>
        ///     Converts the contents returned from the attribute query into an XML document.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The attribute query filter.</param>
        /// <param name="predicate">
        ///     The predicate to determine if the field should be included; otherwise <c>null</c> for all
        ///     fields.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="XDocument" /> representing the contents of the query.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        public static XDocument GetXDocument(this ITable source, IQueryFilter filter, Predicate<IField> predicate)
        {
            return source.GetXDocument(filter, predicate, "Table");
        }

        /// <summary>
        ///     Converts the contents returned from the attribute query into an XML document.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filter">The attribute query filter.</param>
        /// <param name="predicate">
        ///     The predicate to determine if the field should be included; otherwise <c>null</c> for all
        ///     fields.
        /// </param>
        /// <param name="elementName">Name of the element.</param>
        /// <returns>
        ///     Returns a <see cref="XDocument" /> representing the contents of the query.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        public static XDocument GetXDocument(this ITable source, IQueryFilter filter, Predicate<IField> predicate, string elementName)
        {
            if (source == null) return null;
            if (predicate == null) throw new ArgumentNullException("predicate");

            using (ComReleaser cr = new ComReleaser())
            {
                ICursor cursor = source.Search(filter, true);
                cr.ManageLifetime(cursor);

                return cursor.GetXDocument(elementName, predicate);
            }
        }

        /// <summary>
        ///     Determines whether the table is a feature class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Returns a <see cref="bool" /> representing <c>true</c> when the table is a feature class.</returns>
        public static bool IsFeatureClass(this ITable source)
        {
            return source is IFeatureClass;
        }

        /// <summary>
        ///     Joins the source table with the specified foreign table name.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="foreign">The foreign table.</param>
        /// <param name="primarykeyFieldName">Name of the primarykey field.</param>
        /// <param name="foreignKeyFieldName">Name of the foreign key field.</param>
        /// <param name="subFields">The sub fields.</param>
        /// <returns></returns>
        public static IQueryDef Join(this ITable source, ITable foreign, string primarykeyFieldName, string foreignKeyFieldName, string subFields)
        {
            IDataset ds = (IDataset) foreign;
            return source.Join(ds.Name, primarykeyFieldName, foreignKeyFieldName, subFields);
        }

        /// <summary>
        ///     Joins the source table with the specified foreign table name.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="foreignTableName">Name of the foreign table.</param>
        /// <param name="primaryKeyFieldName">Name of the primary key field.</param>
        /// <param name="foreignKeyFieldName">Name of the foreign key field.</param>
        /// <param name="subFields">The sub fields.</param>
        /// <returns></returns>
        public static IQueryDef Join(this ITable source, string foreignTableName, string primaryKeyFieldName, string foreignKeyFieldName, string subFields)
        {
            IDataset ds = (IDataset) source;
            IQueryDef queryDef = ((IFeatureWorkspace) ds.Workspace).CreateQueryDef();
            queryDef.Tables = string.Format("{0}, {1}", ds.Name, foreignTableName);
            queryDef.SubFields = subFields;
            queryDef.WhereClause = string.Format("{0}.{1} = {2}.{3}", ds.Name, primaryKeyFieldName, foreignTableName, foreignKeyFieldName);

            return queryDef;
        }

        /// <summary>
        ///     Joins the source table with the specified foreign table.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="foreign">The foreign table.</param>
        /// <param name="primaryKeyFieldName">Name of the primary key field.</param>
        /// <param name="foreignKeyFieldName">Name of the foreign key field.</param>
        /// <param name="subFields">The sub fields.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static ITable Join(this ITable source, ITable foreign, string primaryKeyFieldName, string foreignKeyFieldName, string subFields, string tableName)
        {
            IQueryDef queryDef = source.Join(foreign, primaryKeyFieldName, foreignKeyFieldName, subFields);

            IDataset ds = (IDataset) source;
            return queryDef.Evaluate(primaryKeyFieldName, true, ds.Workspace, tableName);
        }

        /// <summary>
        ///     Joins the specified foreign class with the source class to create an in memory relationship.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="foreignClass">The foreign class.</param>
        /// <param name="primaryKeyField">The primary key field.</param>
        /// <param name="foreignKeyField">The foreign key field.</param>
        /// <param name="cardinality">The cardinality.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        ///     Returns a <see cref="IRelationshipClass" /> representing the relationship between the two classes.
        /// </returns>
        public static IRelationshipClass Join(this IObjectClass source, IObjectClass foreignClass, string primaryKeyField, string foreignKeyField, esriRelCardinality cardinality, string name)
        {
            var joinName = name ?? string.Format("{0}_{1}", ((IDataset) source).Name, ((IDataset) foreignClass).Name);

            var factory = new MemoryRelationshipClassFactory();
            return factory.Open(joinName, source, primaryKeyField, foreignClass, foreignKeyField, "Forward", "Backward", cardinality);
        }

        /// <summary>
        ///     Copies the rows from the <paramref name="data" /> table to the source table.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="data">The data that will be copied.</param>
        /// <param name="filter">The filter that will be used prior to copying the data.</param>
        /// <param name="progress">The progress callback.</param>
        /// <returns>
        ///     Returns a <see cref="List{T}" /> representing the object ids of the records loaded.
        /// </returns>
        /// <remarks>
        ///     Assumes that the source and data table have the same schema.
        /// </remarks>
        public static List<int> Load(this ITable source, ITable data, IQueryFilter filter = null, IFeatureProgress progress = null)
        {
            if (progress != null)
            {
                progress.FeatureClassName = ((IDataset) source).Name;
                progress.MinFeatures = 0;
                progress.MaxFeatures = data.RowCount(filter);
                progress.Position = 0;
                progress.StepValue = 1;
            }

            var oids = new List<int>();
            var fields = source.Fields.ToDictionary(f => f.Editable);

            using (ComReleaser cr = new ComReleaser())
            {
                ICursor cursor = data.Search(filter, false);
                cr.ManageLifetime(cursor);

                ICursor insert = source.Insert(true);
                cr.ManageLifetime(insert);

                var buffer = source.CreateRowBuffer();

                foreach (var rows in cursor.AsEnumerable().Batch(1000))
                {
                    foreach (var row in rows)
                    {
                        foreach (var field in fields)
                            buffer.Update(field.Value, row.Value[field.Value], false);

                        var oid = (int) insert.InsertRow(buffer);
                        oids.Add(oid);

                        if (progress != null)
                        {
                            progress.Step();
                        }
                    }

                    insert.Flush();
                }

                insert.Flush();
            }

            return oids;
        }

        /// <summary>
        ///     Truncates the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <remarks>
        ///     The truncate method is only available for local geodatabase datasets and non-versioned SDE feature classes and
        ///     tables. This method will truncate the non-versioned table or feature class table. Since this method ignores any
        ///     schma locks that might be present on the table it should be used with caution.
        /// </remarks>
        public static void Truncate(this ITable source)
        {
            ((ITableWrite2) source).Truncate();
        }

        #endregion
    }
}