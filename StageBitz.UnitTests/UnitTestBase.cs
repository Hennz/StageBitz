using Microsoft.VisualStudio.TestTools.UnitTesting;
using StageBitz.Data;
using System;
using System.Data.Common;

namespace StageBitz.UnitTests
{
    /// <summary>
    /// Base class for Unit tests.
    /// </summary>
    [TestClass]
    public class UnitTestBase : IDisposable
    {
        /// <summary>
        /// Initialized Data Context object.
        /// </summary>
        private StageBitzDB dataContext = new StageBitzDB();

        /// <summary>
        /// Gets or sets the data context.
        /// </summary>
        /// <value>
        /// The data context.
        /// </value>
        public StageBitzDB DataContext
        {
            get
            {
                return this.dataContext;
            }

            set
            {
                this.dataContext = value;
            }
        }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        public DbTransaction Scope
        {
            get;
            set;
        }

        /// <summary>
        /// Tests the start.
        /// </summary>
        [TestInitialize]
        public void TestStart()
        {
            this.DataContext = new StageBitzDB();
            this.DataContext.Connection.Open();
            this.Scope = this.DataContext.Connection.BeginTransaction();
        }

        /// <summary>
        /// Tests the end.
        /// </summary>
        [TestCleanup]
        public void TestEnd()
        {
            this.Scope.Rollback();
            this.DataContext.Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            this.DataContext.Dispose();
        }
    }
}