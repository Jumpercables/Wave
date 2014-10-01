﻿using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

using Miner;
using Miner.Geodatabase;
using Miner.Interop;
using Miner.Interop.Process;

namespace ESRI.ArcGIS.Framework
{
    /// <summary>
    ///     Provides extension methods for the <see cref="ESRI.ArcGIS.Framework.IApplication" /> interface.
    /// </summary>
    public static class ApplicationExtensions
    {
        #region Public Methods

        /// <summary>
        ///     Returns the reference to the ArcFM Attribute Editor.
        /// </summary>
        /// <param name="source">The application reference.</param>
        /// <returns>Returns the <see cref="Miner.Interop.IMMAttributeEditor" /> reference; otherwise <c>null</c>.</returns>
        public static IMMAttributeEditor GetAttributeEditor(this IApplication source)
        {
            if (source == null) return null;

            UID uid = new UIDClass();
            uid.Value = ArcFM.Extensions.Guid.AttributeEditor;

            IEditor editor = source.GetEditor();
            if (editor == null) return null;

            return editor.FindExtension(uid) as IMMAttributeEditor;
        }

        /// <summary>
        ///     Returns the contents of the CU tab.
        /// </summary>
        /// <param name="source">The application reference.</param>
        /// <returns>Returns a <see cref="Miner.Interop.ID8List" /> representing the contents on the tab.</returns>
        public static ID8List GetCUTab(this IApplication source)
        {
            if (source == null) return null;

            return source.FindExtensionByName(ArcFM.Extensions.Name.CUTab) as ID8List;
        }


        /// <summary>
        ///     Returns the contents of the Design tab in the ArcFM Attribute Editor.
        /// </summary>
        /// <param name="source">The application reference.</param>
        /// <returns>Returns a <see cref="Miner.Interop.ID8List" /> representing the contents on the tab.</returns>
        public static ID8List GetDesignTab(this IApplication source)
        {
            if (source == null) return null;

            return source.FindExtensionByName(ArcFM.Extensions.Name.DesignTab) as ID8List;
        }

        /// <summary>
        ///     Returns the reference to the Designer Extension
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        ///     Returns the <see cref="Miner.Interop.IMMDesignerImpl" /> reference; otherwise
        ///     <c>null</c>.
        /// </returns>
        public static IMMDesignerImpl GetDesigner(this IApplication source)
        {
            if (source == null) return null;

            return source.FindExtensionByName(ArcFM.Extensions.Name.Designer) as IMMDesignerImpl;
        }


        /// <summary>
        ///     Returns the login object from the <paramref name="source" /> reference.
        /// </summary>
        /// <param name="source">The application reference.</param>
        /// <returns>Returns the <see cref="Miner.Interop.IMMLogin2" /> representing the object that was used to log into ArcFM.</returns>
        public static IMMLogin2 GetLogin(this IApplication source)
        {
            if (source == null) return null;

            var login = source.GetProperties() as IMMLogin2;
            return login;
        }

        /// <summary>
        ///     Returns the workspace that was used to log into the <paramref name="source" /> application.
        /// </summary>
        /// <param name="source">The application reference.</param>
        /// <returns>Returns the <see cref="IWorkspace" /> representing database that was logged into from the ArcFM Login screen.</returns>
        public static IWorkspace GetLoginWorkspace(this IApplication source)
        {
            if (source == null) return null;

            var login = source.GetProperties() as IMMLogin2;
            if (login != null) return login.LoginObject.LoginWorkspace;

            return null;
        }

        /// <summary>
        ///     Returns the ArcFM properties extension.
        /// </summary>
        /// <param name="source">The application reference.</param>
        /// <returns>Returns the <see cref="Miner.Interop.IMMPropertiesExt" /> reference; otherwise <c>null</c>.</returns>
        public static IMMPropertiesExt GetProperties(this IApplication source)
        {
            if (source == null) return null;

            return source.FindExtensionByName(ArcFM.Extensions.Name.Properties) as IMMPropertiesExt;
        }

        /// <summary>
        ///     Gets the process framework application reference from the <paramref name="source" /> reference.
        /// </summary>
        /// <param name="source">The application reference.</param>
        /// <returns>
        ///     Returns the <see cref="Miner.Interop.Process.IMMPxApplication" /> representing the process framework
        ///     reference.
        /// </returns>
        public static IMMPxApplication GetPxApplication(this IApplication source)
        {
            if (source == null) return null;

            IMMPxIntegrationCache2 cache = PxIntegrationCache.Instance;
            return (cache == null) ? null : cache.Application;
        }

        /// <summary>
        ///     Returns the contents of the QAQC tab in the ArcFM Attribute Editor.
        /// </summary>
        /// <param name="source">The application reference.</param>
        /// <returns>Returns a <see cref="Miner.Interop.ID8List" /> representing the contents on the tab.</returns>
        public static ID8List GetQAQCTab(this IApplication source)
        {
            if (source == null) return null;

            return source.FindExtensionByName(ArcFM.Extensions.Name.QAQCTab) as ID8List;
        }

        /// <summary>
        ///     Returns the contents of the Selection tab in the ArcFM Attribute Editor.
        /// </summary>
        /// <param name="source">The application reference.</param>
        /// <returns>Returns a <see cref="Miner.Interop.ID8List" /> representing the contents on the tab.</returns>
        public static ID8List GetSelectionTab(this IApplication source)
        {
            if (source == null) return null;

            return source.FindExtensionByName(ArcFM.Extensions.Name.SelectionTab) as ID8List;
        }

        /// <summary>
        ///     Returns the contents of the Targets tab in the ArcFM Attribute Editor.
        /// </summary>
        /// <param name="source">The application reference.</param>
        /// <returns>Returns a <see cref="Miner.Interop.ID8List" /> representing the contents on the tab.</returns>
        public static ID8List GetTargetTab(this IApplication source)
        {
            if (source == null) return null;

            return source.FindExtensionByName(ArcFM.Extensions.Name.TargetsTab) as ID8List;
        }


        /// <summary>
        ///     Refresh the cached ArcFM properties (such as the model names and AU properties).
        /// </summary>
        /// <param name="source">The application reference.</param>
        public static void Reset(this IApplication source)
        {
            if (source == null) return;

            IMMConfigTopLevel ctl = ConfigTopLevel.Instance;
            ctl.Workspace = null;

            var au = (IMMAutoUpdater3) AutoUpdaterModeReverter.Instance;
            au.FlushCache();

            var ext = (IExtension) ModelNameManager.Instance;
            ext.Shutdown();

            object application = source;
            ext.Startup(ref application);
        }

        #endregion
    }
}