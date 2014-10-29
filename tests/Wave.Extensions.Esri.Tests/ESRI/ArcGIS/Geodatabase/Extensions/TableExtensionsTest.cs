﻿using System.Linq;

using ESRI.ArcGIS.Geodatabase;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wave.Extensions.Esri.Tests
{
    [TestClass]
    public class TableExtensionsTest : EsriTests
    {
        #region Public Methods

        [TestMethod]
        public void ITable_Distinct_LessThan_100()
        {
            var testClass = base.GetTestTable();
            var rows = testClass.Distinct(testClass.OIDFieldName, testClass.OIDFieldName + " < 100");
            Assert.IsTrue(rows.Count < 100);
        }


        [TestMethod]
        public void ITable_Fetch_Filter_Action_Equals_6()
        {
            var testClass = base.GetTestTable();

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = testClass.OIDFieldName + " IN (1,2,3,4,5,6)";

            int rowsAffected = testClass.Fetch(filter, feature => { });
            Assert.AreEqual(6, rowsAffected);
        }

        [TestMethod]
        public void ITable_Fetch_Filter_Func_Equals_1()
        {
            var testClass = base.GetTestTable();

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = testClass.OIDFieldName + " IN (1,2,3,4,5,6)";

            int rowsAffected = testClass.Fetch(filter, row => true);
            Assert.AreEqual(1, rowsAffected);
        }

        [TestMethod]
        public void ITable_Fetch_Filter_Projection_Equals_6()
        {
            var testClass = base.GetTestTable();

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = testClass.OIDFieldName + " IN (1,2,3,4,5,6)";

            var list = testClass.Fetch(filter, row => row.OID);

            Assert.AreEqual(6, list.Count);
        }

        [TestMethod]
        public void ITable_Fetch_List_Equals_1()
        {
            var testClass = base.GetTestTable();
            var list = testClass.Fetch(1);

            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void ITable_GetXDocument_NotNull()
        {
            var testClass = base.GetTestTable();

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = testClass.OIDFieldName + " = 1";

            var xdoc = testClass.GetXDocument(filter, field => field.Type == esriFieldType.esriFieldTypeOID);

            Assert.IsNotNull(xdoc);
            Assert.AreEqual(xdoc.Elements().Count(), 1);
        }

        #endregion
    }
}