﻿using System;

using ESRI.ArcGIS.Geodatabase;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wave.Extensions.Miner.Tests
{
    [TestClass]
    public class RowExtensionTest : MinerTests
    {
        #region Public Methods

        [TestMethod]
        [TestCategory("Miner")]
        public void IRow_GetDomain_IsNull()
        {
            var testClass = base.GetTestClass();
            var feature = testClass.Fetch(1);
            Assert.IsNotNull(feature);

            var domain = feature.GetDomain(0);
            Assert.IsNull(domain);
        }

        [TestMethod]
        [TestCategory("Miner")]
        [ExpectedException(typeof (IndexOutOfRangeException))]
        public void IRow_GetValue_FieldModelName_IndexOutOfRangeException()
        {
            IFeatureClass testClass = base.GetTestClass();
            IFeature feature = testClass.Fetch(1);
            Assert.IsNotNull(feature);

            object value = feature.GetValue("@%4123%1", string.Empty, false);
            Assert.AreEqual(string.Empty, value);
        }

        [TestMethod]
        [TestCategory("Miner")]
        public void IRow_GetValue_FieldModelName_IsNotNull()
        {
            IFeatureClass testClass = base.GetTestClass();
            IFeature feature = testClass.Fetch(1);
            Assert.IsNotNull(feature);

            object value = feature.GetValue("FEEDERID", string.Empty, false);
            Assert.IsNotNull(value);
        }

        [TestMethod]
        [TestCategory("Miner")]
        [ExpectedException(typeof (MissingFieldModelNameException))]
        public void IRow_GetValue_FieldModelName_MissingFieldModelNameException()
        {
            IFeatureClass testClass = base.GetTestClass();
            IFeature feature = testClass.Fetch(1);
            Assert.IsNotNull(feature);

            object value = feature.GetValue("@%4123%1", string.Empty, true);
            Assert.IsNotNull(value);
        }

        [TestMethod]
        [TestCategory("Miner")]
        public void IRow_Update_FieldModelName_EqualityCompare_False()
        {
            var testClass = base.GetTestClass();
            var feature = testClass.Fetch(1);
            Assert.IsNotNull(feature);

            string value = feature.GetValue("FEEDERID", string.Empty, true);
            bool saveChanges = feature.Update("FEEDERID", value, true, true);
            Assert.IsFalse(saveChanges);
        }

        [TestMethod]
        [TestCategory("Miner")]
        public void IRow_Update_FieldModelName_EqualityCompare_True()
        {
            var testClass = base.GetTestClass();
            var feature = testClass.Fetch(1);
            Assert.IsNotNull(feature);

            bool saveChanges = feature.Update("FEEDERID", 12345123123, true, true);
            Assert.IsTrue(saveChanges);
        }

        #endregion
    }
}