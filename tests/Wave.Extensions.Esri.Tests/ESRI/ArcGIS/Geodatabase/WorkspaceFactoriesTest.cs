﻿using System;
using System.Data;
using System.Data.Common;
using System.IO;

using ESRI.ArcGIS.Geodatabase;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Wave.Extensions.Esri.Tests.Properties;

namespace Wave.Extensions.Esri.Tests
{
    [TestClass]
    public class WorkspaceFactoriesTests : EsriTests
    {
        #region Public Methods        

        [TestMethod]
        [TestCategory("ESRI")]
        [ExpectedException(typeof (DirectoryNotFoundException))]
        public void WorkspaceFactories_GetFactory_DirectoryNotFoundException()
        {
            Assert.IsNull(WorkspaceFactories.GetFactory("@C:\\kadhfakjfh.gdb"));
        }

        [TestMethod]
        [TestCategory("ESRI")]
        [ExpectedException(typeof (FileNotFoundException))]
        public void WorkspaceFactories_GetFactory_Empty_FileNotFoundException()
        {
            Assert.IsNull(WorkspaceFactories.GetFactory(""));
        }

        [TestMethod]
        [TestCategory("ESRI")]
        [ExpectedException(typeof (FileNotFoundException))]
        public void WorkspaceFactories_GetFactory_FileNotFoundException()
        {
            Assert.IsNull(WorkspaceFactories.GetFactory("@C:\\kadhfakjfh.sde"));
        }

        [TestMethod]
        [TestCategory("ESRI")]
        public void WorkspaceFactories_GetFactory_NotNull()
        {
            Assert.IsNotNull(WorkspaceFactories.GetFactory(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.Minerville))));
        }

        [TestMethod]
        [TestCategory("ESRI")]
        [ExpectedException(typeof (ArgumentNullException))]
        public void WorkspaceFactories_Open_ArgumentNullException()
        {
            Assert.IsNull(WorkspaceFactories.Open(null));
        }

        [TestMethod]
        [TestCategory("ESRI")]
        [ExpectedException(typeof (FileNotFoundException))]
        public void WorkspaceFactories_Open_FileNotFoundException()
        {
            Assert.IsNull(WorkspaceFactories.Open("@C:\\*.sde"));
        }

        [TestMethod]
        [TestCategory("ESRI")]
        public void WorkspaceFactories_Open_NotNull()
        {
            Assert.IsNotNull(WorkspaceFactories.Open(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.Minerville))));
        }

        #endregion
    }
}