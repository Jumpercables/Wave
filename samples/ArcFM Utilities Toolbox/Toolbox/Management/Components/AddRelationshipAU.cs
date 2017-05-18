﻿using System.Collections.Generic;
using System.Linq;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessing;

using Miner.ComCategories;
using Miner.Geodatabase;
using Miner.Interop;

namespace Wave.Geoprocessing.Toolbox.Management
{
    /// <summary>
    ///     A geoprocessing tool that allows for assigning "Relationship" AUs to a relationship class.
    /// </summary>
    public class AddRelationshipAU : BaseConfigTopLevelFunction
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddRelationshipAU" /> class.
        /// </summary>
        /// <param name="functionFactory">The function factory.</param>
        public AddRelationshipAU(IGPFunctionFactory functionFactory)
            : base("AddRelationshipAU", "Add Relationship AU", functionFactory)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the list of parameters accepted by the geoprocessing function.
        /// </summary>
        /// <value>
        ///     The ParameterInfo property is the place where a function tool's parameters are defined. It returns an IArray of
        ///     parameter objects (IGPParameter); these objects define the characteristics of the input and output parameters.
        ///     At the minimum, your function should output a Boolean value containing success or failure.
        /// </value>
        public override IArray ParameterInfo
        {
            get
            {
                IArray list = new ArrayClass();

                list.Add(this.CreateCompositeParameter("in_table", "Relationship Class", esriGPParameterType.esriGPParameterTypeRequired, esriGPParameterDirection.esriGPParameterDirectionInput, new DERelationshipClassTypeClass()));

                list.Add(this.CreateMultiValueParameter("in_create", "Create", esriGPParameterType.esriGPParameterTypeOptional, esriGPParameterDirection.esriGPParameterDirectionInput, new GPAutoValueType<IMMRelationshipAUStrategy>()));
                list.Add(this.CreateMultiValueParameter("in_delete", "Delete", esriGPParameterType.esriGPParameterTypeOptional, esriGPParameterDirection.esriGPParameterDirectionInput, new GPAutoValueType<IMMRelationshipAUStrategy>()));

                list.Add(this.CreateParameter("out_results", "Results", esriGPParameterType.esriGPParameterTypeDerived, esriGPParameterDirection.esriGPParameterDirectionOutput, new GPBooleanTypeClass()));

                return list;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Executes the geoprocessing function using the given array of parameter values.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="trackCancel">The track cancel.</param>
        /// <param name="environmentManager">Provides access to all the current environments and settings of the current client.</param>
        /// <param name="messages">The messages that are reported to the user.</param>
        /// <param name="utilities">
        ///     The utilities object that provides access to the properties and methods of a geoprocessing
        ///     objects.
        /// </param>
        protected override void Execute(Dictionary<string, IGPValue> parameters, ITrackCancel trackCancel, IGPEnvironmentManager environmentManager, IGPMessages messages, IGPUtilities2 utilities)
        {
            IGPValue value = parameters["in_table"];
            IRelationshipClass relClass = utilities.OpenRelationshipClass(value);
            if (relClass != null)
            {
                IMMConfigTopLevel configTopLevel = ConfigTopLevel.Instance;
                configTopLevel.Workspace = utilities.GetWorkspace(value);

                IGPMultiValue onCreate = (IGPMultiValue) parameters["in_create"];
                IGPMultiValue onDelete = (IGPMultiValue) parameters["in_delete"];

                var uids = new Dictionary<mmEditEvent, IEnumerable<IUID>>();
                uids.Add(mmEditEvent.mmEventRelationshipCreated, onCreate.AsEnumerable().Cast<IGPAutoValue>().Select(o => o.UID));
                uids.Add(mmEditEvent.mmEventRelationshipDeleted, onDelete.AsEnumerable().Cast<IGPAutoValue>().Select(o => o.UID));

                // Update the list to have these UIDs.
                ID8List list = (ID8List) configTopLevel.GetRelationshipClass(relClass);
                base.Add(uids, list, messages);

                // Commit the changes to the database.
                configTopLevel.SaveRelationshipClasstoDB(relClass);

                // Success.
                parameters["out_results"].SetAsText("true");
            }
            else
            {
                // Failure.
                parameters["out_results"].SetAsText("false");
            }
        }

        /// <summary>
        ///     Pre validates the given set of values.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="environmentManager">Provides access to all the current environments and settings of the current client.</param>
        /// <param name="utilities">
        ///     The utilities object that provides access to the properties and methods of a geoprocessing
        ///     objects.
        /// </param>
        protected override void UpdateParameters(Dictionary<string, IGPParameter> parameters, IGPEnvironmentManager environmentManager, IGPUtilities2 utilities)
        {
            IGPValue value = utilities.UnpackGPValue(parameters["in_table"]);
            if (!value.IsEmpty())
            {
                IRelationshipClass relClass = utilities.OpenRelationshipClass(value);
                if (relClass != null)
                {
                    var components = this.LoadComponents<IMMRelationshipAUStrategy>(RelationshipAutoupdateStrategy.CatID);

                    IGPParameterEdit3 parameter = (IGPParameterEdit3) parameters["in_create"];
                    parameter.Domain = base.CreateDomain(components, o => o.Enabled[relClass, mmEditEvent.mmEventRelationshipCreated]);

                    parameter = (IGPParameterEdit3)parameters["in_delete"];
                    parameter.Domain = base.CreateDomain(components, o => o.Enabled[relClass, mmEditEvent.mmEventRelationshipDeleted]);
                }
            }
        }

        #endregion
    }
}