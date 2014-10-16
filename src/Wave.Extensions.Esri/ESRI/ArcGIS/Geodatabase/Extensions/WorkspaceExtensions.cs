﻿using System;
using System.Globalization;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase.Internal;

namespace ESRI.ArcGIS.Geodatabase
{

    #region Enumerations

    /// <summary>
    ///     Enumeration of the various supported relational (DBMS) geodatabase.
    /// </summary>
    public enum DBMS
    {
        /// <summary>
        ///     A local Microsoft Access geodatabase.
        /// </summary>
        Access,

        /// <summary>
        ///     A local ESRI File geodatabase.
        /// </summary>
        File,

        /// <summary>
        ///     A remote Oracle geodatabase.
        /// </summary>
        Oracle,

        /// <summary>
        ///     A remote Microsoft SQL Server geodatabase.
        /// </summary>
        SqlServer,

        /// <summary>
        ///     A remote IBM DB2 geodatabase.
        /// </summary>
        Db2,

        /// <summary>
        ///     A remote Informix geodatabase.
        /// </summary>
        Informix,

        /// <summary>
        ///     A remote Postgre SQL geodatabase.
        /// </summary>
        PostgreSql,

        /// <summary>
        ///     An unknown geodatabase.
        /// </summary>
        Unknown
    }

    #endregion

    /// <summary>
    ///     Provides extension methods for the <see cref="ESRI.ArcGIS.Geodatabase.IWorkspace" /> interface.
    /// </summary>
    public static class WorkspaceExtensions
    {
        #region Public Methods

        /// <summary>
        ///     Gets the database management system that is used with conjunction of the <paramref name="workspace" />.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <returns>
        ///     The <see cref="DBMS" /> enumeration value.
        /// </returns>
        public static DBMS GetDBMS(this IWorkspace workspace)
        {
            if (workspace.Type == esriWorkspaceType.esriLocalDatabaseWorkspace)
            {
                UID uid = workspace.WorkspaceFactory.GetClassID();
                if (uid.Value.ToString() == "{71FE75F0-EA0C-4406-873E-B7D53748AE7E}")
                    return DBMS.File;

                return DBMS.Access;
            }

            IDatabaseConnectionInfo2 dci = workspace as IDatabaseConnectionInfo2;
            if (dci != null)
            {
                switch (dci.ConnectionDBMS)
                {
                    case esriConnectionDBMS.esriDBMS_DB2:
                        return DBMS.Db2;

                    case esriConnectionDBMS.esriDBMS_Informix:
                        return DBMS.Informix;

                    case esriConnectionDBMS.esriDBMS_Oracle:
                        return DBMS.Oracle;

                    case esriConnectionDBMS.esriDBMS_PostgreSQL:
                        return DBMS.PostgreSql;

                    case esriConnectionDBMS.esriDBMS_SQLServer:
                        return DBMS.SqlServer;
                }
            }

            return DBMS.Unknown;
        }

        /// <summary>
        ///     Finds the <see cref="IDomain" /> that equals the specified <paramref name="domainName" /> using a non case
        ///     sensitive comparison.
        /// </summary>
        /// <param name="source">The workspace.</param>
        /// <param name="domainName">Name of the domain.</param>
        /// <returns>
        ///     Returns a <see cref="IDomain" /> representing the domain with the given name; otherwise <c>null</c>.
        /// </returns>
        public static IDomain GetDomain(this IWorkspace source, string domainName)
        {
            IWorkspaceDomains wd = (IWorkspaceDomains) source;
            IEnumDomain domains = wd.Domains;

            foreach (var domain in domains.AsEnumerable())
            {
                if (domain.Name.Equals(domainName, StringComparison.OrdinalIgnoreCase))
                    return domain;
            }

            return null;
        }


        /// <summary>
        ///     Gets the formatted date time for the workspace.
        /// </summary>
        /// <param name="source">The workspace.</param>
        /// <param name="dateTime">The date time.</param>
        /// <returns>
        ///     Returns <see cref="String" /> representing the formatted date time for the workspace.
        /// </returns>
        public static string GetFormattedDate(this IWorkspace source, DateTime dateTime)
        {
            // Depending on the workspace we need to format the date time differently.
            DBMS dbms = GetDBMS(source);
            switch (dbms)
            {
                case DBMS.Access:

                    // Access - #3/11/2005#
                    return string.Format(CultureInfo.InvariantCulture, "#{0}#", dateTime.ToShortDateString());

                case DBMS.File:

                    // FileGeodatabase - date '3/11/2005'
                    return string.Format(CultureInfo.InvariantCulture, "date '{0}'", dateTime.ToShortDateString());

                case DBMS.Oracle:

                    // Oracle - 01-NOV-2005
                    return string.Format(CultureInfo.InvariantCulture, "{0}", dateTime.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));

                case DBMS.SqlServer:

                    // SqlServer - '3/11/2005'
                    return string.Format(CultureInfo.InvariantCulture, "{0}", dateTime.ToShortDateString());
            }

            return dateTime.ToShortTimeString();
        }

        /// <summary>
        ///     Gets the name of the SQL function.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sqlFunctionName">Name of the SQL function.</param>
        /// <returns>
        ///     Returns a <see cref="string" /> representing the name of the function.
        /// </returns>
        /// <exception cref="System.NotSupportedException">The function is not supported.</exception>
        public static string GetFunctionName(this IWorkspace source, esriSQLFunctionName sqlFunctionName)
        {
            ISQLSyntax sqlSyntax = (ISQLSyntax) source;
            string functionName = sqlSyntax.GetFunctionName(sqlFunctionName);
            if (!string.IsNullOrEmpty(functionName))
                return functionName;

            throw new NotSupportedException("The function is not supported.");
        }


        /// <summary>
        ///     Determines whether the specified workspace is the  <paramref name="database" /> system.
        /// </summary>
        /// <param name="source">The workspace.</param>
        /// <param name="database">The database.</param>
        /// <returns>
        ///     Returns a <see cref="bool" /> representing <c>true</c> if the specified workspace is DBMS; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDBMS(this IWorkspace source, DBMS database)
        {
            return GetDBMS(source) == database;
        }


        /// <summary>
        ///     Determines whether the predicate is supported by the workspace.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        ///     Returns the <see cref="bool" /> representing <c>true</c> when the predicate is supported; otherwise
        ///     <c>false</c>
        /// </returns>
        public static bool IsPredicateSupported(this IWorkspace source, esriSQLPredicates predicate)
        {
            // Cast to the ISQLSyntax interface and get the supportedPredicates value.
            ISQLSyntax sqlSyntax = (ISQLSyntax) source;
            int supportedPredicates = sqlSyntax.GetSupportedPredicates();

            // Cast the predicate value to an integer and use bitwise arithmetic to check for support.
            int predicateValue = (int) predicate;
            int supportedValue = predicateValue & supportedPredicates;

            return supportedValue > 0;
        }

        /// <summary>
        ///     Starts editing the workspace using the specified <paramref name="withUndoRedo" /> and
        ///     <paramref name="multiuserEditSessionMode" /> parameters.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="withUndoRedo">if set to <c>true</c> when the changes are reverted when the edits are aborted.</param>
        /// <param name="multiuserEditSessionMode">
        ///     The edit session mode that can be used to indicate non-versioned or versioned
        ///     editing for workspaces that support multiuser editing.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="IEditableWorkspace" /> respresenting an editable workspace (that is disposable).
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     The workspace does not support the edit session
        ///     mode.;multiuserEditSessionMode
        /// </exception>
        /// <remarks>
        ///     The <see cref="IEditableWorkspace" /> implements the <see cref="IDisposable" /> interface, meaning it should be
        ///     used within a using statement.
        /// </remarks>
        public static IEditableWorkspace StartEditing(this IWorkspace source, bool withUndoRedo = true, esriMultiuserEditSessionMode multiuserEditSessionMode = esriMultiuserEditSessionMode.esriMESMVersioned)
        {
            IEditableWorkspace editableWorkspace = new EditableWorkspace(source);
            editableWorkspace.StartEditing(withUndoRedo, multiuserEditSessionMode);

            return editableWorkspace;
        }

        /// <summary>
        ///     Encapsulates the <paramref name="operation" /> by the necessary start and stop edit constructs using the specified
        ///     <paramref name="withUndoRedo" /> and
        ///     <paramref name="multiuserEditSessionMode" /> parameters.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="operation">
        ///     The edit operation delegate that handles making the necessary edits. When the delegate returns
        ///     <c>true</c> the edits will be saved; otherwise they will not be saved.
        /// </param>
        /// <param name="withUndoRedo">if set to <c>true</c> when the changes are reverted when the edits are aborted.</param>
        /// <param name="multiuserEditSessionMode">
        ///     The edit session mode that can be used to indicate non-versioned or versioned
        ///     editing for workspaces that support multiuser editing.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     The workspace does not support the edit session
        ///     mode.;multiuserEditSessionMode
        /// </exception>
        public static void StartEditing(this IWorkspace source, Func<bool> operation, bool withUndoRedo = true, esriMultiuserEditSessionMode multiuserEditSessionMode = esriMultiuserEditSessionMode.esriMESMVersioned)
        {
            using (var editableWorkspace = source.StartEditing(withUndoRedo, multiuserEditSessionMode))
            {
                editableWorkspace.StopEditing(operation());
            }
        }

        /// <summary>
        ///     Updates the Multiversioned views to point to the current version.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <remarks>
        ///     Before issuing any queries against the view, you must ensure that they will take place against the correct version.
        /// </remarks>
        public static void UpdateMultiversionedViews(this IWorkspace source)
        {
            IVersion version = source as IVersion;
            if (version != null)
            {
                string versionName = version.VersionName;

                if (source.IsDBMS(DBMS.Oracle))
                {
                    string commandText = string.Format("begin sde.version_util.set_current_version('{0}'); end;", versionName);
                    source.ExecuteSQL(commandText);
                }
                else if (source.IsDBMS(DBMS.SqlServer))
                {
                    string commandText = string.Format("begin sde.set_current_version '{0}'; end;", versionName);
                    source.ExecuteSQL(commandText);
                }
            }
        }

        #endregion
    }
}