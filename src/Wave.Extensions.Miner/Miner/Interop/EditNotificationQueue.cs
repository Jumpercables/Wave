﻿using ESRI.ArcGIS.esriSystem;

using Miner.Geodatabase.Edit;

namespace Miner.Interop
{
    /// <summary>
    ///     A singleton wrapper for the <see cref="IMMEditNotificationQueue" /> for the ESRI Editor extension.
    /// </summary>
    public static class EditNotificationQueue
    {
        #region Public Properties

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static IMMEditNotificationQueue Instance
        {
            get
            {
                UID uid = new UID();
                uid.Value = ArcFM.Extensions.Guid.EditNotificationQueue;

                IExtension extension = Editor.FindExtension(uid);
                IMMEditNotificationQueue notifyQueue = (IMMEditNotificationQueue) extension;
                return notifyQueue;
            }
        }

        #endregion
    }
}