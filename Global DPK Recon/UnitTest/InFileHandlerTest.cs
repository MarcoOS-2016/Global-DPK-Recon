using GIC.Business;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest
{
    
    
    /// <summary>
    ///This is a test class for InFileHandlerTest and is intended
    ///to contain all InFileHandlerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InFileHandlerTest
    {
        
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for TriggerNotification
        ///</summary>
        [TestMethod()]
        [DeploymentItem("GIC.Business.dll")]
        public void TriggerNotificationTest()
        {
            InFileHandler_Accessor target = new InFileHandler_Accessor(); // TODO: Initialize to an appropriate value

            target.ReconDate = DateTime.Now;
            target.GLVTransDate = DateTime.Now.AddDays(-3);
            target.TotalVairanceItems = 12345;
            target.ReconResultLink = @"file://\\wn7-hgt6822\DPK_Recon_Result\DPK_Recon_Result_20150331_230126.csv";

            target.TriggerNotification();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
