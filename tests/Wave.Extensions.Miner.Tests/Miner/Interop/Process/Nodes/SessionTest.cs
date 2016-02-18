﻿using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Miner;
using Miner.Interop;
using Miner.Interop.Process;

namespace Wave.Extensions.Miner.Tests
{
    [TestClass]
    public class SessionTest : ProcessTests
    {
        #region Constructors

        public SessionTest()
            : base(mmProductInstallation.mmPIArcFM)
        {
        }

        #endregion

        #region Public Methods

        [TestMethod]
        [TestCategory("Miner")]
        public void IPxSession_CreateNew_IsTrue()
        {
            using (Session session = new Session(base.PxApplication))
            {                
                Assert.AreEqual(true, session.Valid);
                Assert.AreEqual(base.PxApplication.User.Name, session.CreateUser);

                session.Delete();
            }
        }

        [TestMethod]
        [TestCategory("Miner")]
        public void IPxSession_Initialize_IsTrue()
        {
            DataTable table = base.PxApplication.ExecuteQuery("SELECT SESSION_ID FROM " + base.PxApplication.GetQualifiedTableName(ArcFM.Process.SessionManager.Tables.Session));
            if (table.Rows.Count > 0)
            {
                int nodeID = table.Rows[0].Field<int>(0);
                using (Session session = new Session(base.PxApplication, nodeID))
                {
                    Assert.AreEqual(true, session.Valid);
                }
            }
        }

        #endregion
    }
}