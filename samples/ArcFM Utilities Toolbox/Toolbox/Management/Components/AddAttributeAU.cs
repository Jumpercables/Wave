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
    ///     A geoprocessing tool that allows for assigning "Attribute" AUs to fields on an object class.
    /// </summary>
    public class AddAttributeAU : BaseConfigTopLevelFunction
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddAttributeAU" /> class.
        /// </summary>
        /// <param name="functionFactory">The function factory.</param>
        public AddAttributeAU(IGPFunctionFactory functionFactory)
            : base("AddAttributeAU", "Add Attribute AU", functionFactory)
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

                list.Add(this.CreateCompositeParameter("in_table", "Table or Feature Class", esriGPParameterType.esriGPParameterTypeRequired, esriGPParameterDirection.esriGPParameterDirectionInput, new DETableTypeClass(), new DEFeatureClassTypeClass()));

                var parameter = this.CreateParameter("in_subtype", "Subtype", esriGPParameterType.esriGPParameterTypeRequired, esriGPParameterDirection.esriGPParameterDirectionInput, new GPStringTypeClass());
                parameter.AddDependency("in_table");

                list.Add(parameter);
               
                parameter = this.CreateMultiValueParameter("in_field", "Field", esriGPParameterType.esriGPParameterTypeRequired, esriGPParameterDirection.esriGPParameterDirectionInput, new GPStringTypeClass(), true);
                parameter.AddDependency("in_table");

                list.Add(parameter);

                parameter = this.CreateParameter("in_value", "Auto Updater", esriGPParameterType.esriGPParameterTypeRequired, esriGPParameterDirection.esriGPParameterDirectionInput, new GPAutoValueType<IMMAttrAUStrategy>());
                
                var components = this.LoadComponents<IMMAttrAUStrategy>(AttrAutoUpdateStrategy.CatID);
                parameter.Domain = base.CreateDomain(components);

                list.Add(parameter);

                string[] values = { "mmEventFeatureCreate", "mmEventFeatureUpdate", "mmEventFeatureDelete" };
                string[] names = { "On Create", "On Update", "On Delete" };

                parameter = this.CreateMultiValueParameter("in_actions", "Actions", esriGPParameterType.esriGPParameterTypeRequired, esriGPParameterDirection.esriGPParameterDirectionInput, new GPStringTypeClass(), true);
                parameter.Domain = base.CreateDomain(names, values);
                
                list.Add(parameter);              

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
            IObjectClass table = utilities.OpenTable(value);
            if (table != null)
            {
                IMMConfigTopLevel configTopLevel = ConfigTopLevel.Instance;
                configTopLevel.Workspace = utilities.GetWorkspace(value);

                // Load all of the subtypes when the user specified "All" or "-1".
                int subtype = parameters["in_subtype"].Cast(-1);
                var subtypeCodes = new List<int>(new[] { subtype });
                if (subtype == -1)
                {
                    ISubtypes subtypes = (ISubtypes)table;
                    subtypeCodes.AddRange(subtypes.Subtypes.AsEnumerable().Select(o => o.Key));
                }

                var actions = ((IGPMultiValue) parameters["in_actions"]).AsEnumerable().Select(o => o.GetAsText());
                var values = ((IGPAutoValue)parameters["in_value"]).UID;
                var uids = new Dictionary<mmEditEvent, IEnumerable<IUID>>();

                IGPMultiValue fields = (IGPMultiValue)parameters["in_field"];
                var fieldNames = fields.AsEnumerable().Select(o => o.GetAsText());
                var indexes = table.Fields.ToDictionary(o => fieldNames.Contains(o.Name)).Select(o => o.Value);
                foreach (var index in indexes)
                {
                    // Enumerate through all of the subtypes making changes.
                    foreach (var subtypeCode in subtypeCodes)
                    {
                        // Load the configurations for the table and subtype.
                        IMMSubtype mmsubtype = configTopLevel.GetSubtypeByID(table, subtypeCode, false);

                        // Load the field configurations.
                        IMMField mmfield = null;
                        mmsubtype.GetField(index, ref mmfield);

                        // Update the list to have these UIDs removed.
                        ID8List list = (ID8List) mmfield;
                        base.Add(uids, list, messages);
                    }
                }

                // Commit the changes to the database.
                configTopLevel.SaveFeatureClassToDB(table);

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
                IObjectClass table = utilities.OpenTable(value);
                if (table != null)
                {
                    IGPParameterEdit3 parameter = (IGPParameterEdit3)parameters["in_field"];
                    parameter.Domain = base.GetFields(table);

                    parameter = (IGPParameterEdit3) parameters["in_subtype"];
                    parameter.Domain = base.GetSubtypes(table);

                   
                }
            }
        }

        #endregion
    }
}