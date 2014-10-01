﻿using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using Miner.Interop;
using System;
using System.Linq;

using Miner.Interop.Process;

namespace Samples
{
    internal class Program
    {
        #region Public Methods
        
        /// <summary>
        ///     Updates the KVA on the transformer unit records.
        /// </summary>
        /// <param name="transformerClass">The transformer class.</param>
        /// <param name="oids">The list of the object ids that identify the features.</param>
        /// <param name="kva">The kva rating.</param>
        /// <returns>
        ///     Returns a <see cref="int" /> representing the records affected.
        /// </returns>
        public int UpdateKva(IFeatureClass transformerClass, int[] oids, int kva)
        {
            IRelationshipClass relationshipClas = transformerClass.GetRelationshipClass(esriRelRole.esriRelRoleAny, "TRANSFORMERUNIT");

            int recordsAffected = transformerClass.Fetch(oids, true, feature =>
            {
                // Iterate through all of the related objects for the transformer.
                ISet set = relationshipClas.GetObjectsRelatedToObject((IObject)feature);
                foreach (IRow row in set.AsEnumerable<IRow>())
                {
                    row.Update("KVA", kva, true); // Use the "Update" extension method because it will only update the field when the values are different.
                    row.SaveChanges(); // Use the "SaveChanges" extension method because it will only call store when one or more fields have changed.
                }
            });

            return recordsAffected;
        }

        /// <summary>
        ///     Update all of the TAG field with the specified value for all features
        ///     that reside within the Design Tab.
        /// </summary>
        /// <param name="tag">The tag information.</param>
        public void UpdateTags(string tag)
        {
            ID8List list = ArcMap.Application.GetDesignTab();
            foreach (var item in list.Where(o => o is IMMProposedObject).Select(o => (IMMProposedObject)o.Value))
            {
                IMMFieldManager fieldManager = item.FieldManager;
                IMMFieldAdapter fieldAdapter = fieldManager.FieldByName("TAG");
                fieldAdapter.Value = tag;
                item.Update(null);
            }
        }

        /// <summary>
        /// Validates the updates for the edits made within an ArcFM session by performing 
        /// a version difference between the session and it's parent version.
        /// </summary>
        public void ValidateUpdates()
        {
            // Load the application workspace.
            IMMPxApplication pxApplication = ArcMap.Application.GetPxApplication();
            IWorkspace workspace = ((IMMPxApplicationEx2)pxApplication).Workspace;

            // Load the active session.
            Session session = pxApplication.GetSession();

            // Determine the differences between the two versions.
            IVersion childVersion = workspace.FindVersion(session.VersionName);
            IVersion parentVersion = workspace.FindVersion(childVersion.VersionInfo.Parent.VersionName);
            
            // Iterate through all of the differences to feature classes.
            var differences = childVersion.GetDifferences(parentVersion, null, (s, table) => table is IFeatureClass, esriDifferenceType.esriDifferenceTypeUpdateUpdate);
            foreach (var table in differences)
            {
                Console.WriteLine("Table: {0}", table.Key);
                
                foreach (var differenceRow in table.Value)
                {
                    Console.WriteLine("+++++++++++ {0} +++++++++++", differenceRow.OID);
   
                    foreach (var index in differenceRow.FieldIndices.AsEnumerable())
                    {
                         Console.WriteLine("Original: {0} -> Changed: {1}", differenceRow.Original.GetValue(index, DBNull.Value), differenceRow.Changed.GetValue(index, DBNull.Value));
                    }                                     
                }
            }
        }


        #endregion Public Methods

        #region Private Methods

        private static void Main(string[] args)
        {
        }

        #endregion Private Methods
    }
}