using StageBitz.Data;
using System;

namespace StageBitz.Logic.Business
{
    /// <summary>
    /// Base class for all BLs
    /// </summary>
    public class BaseBL : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBL"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public BaseBL(StageBitzDB dataContext)
        {
            DataContext = dataContext;
        }

        /// <summary>
        /// The data context var
        /// </summary>
        private StageBitzDB dataContextPrivate = null;

        /// <summary>
        /// Gets the data context.
        /// </summary>
        /// <value>
        /// The data context.
        /// </value>
        public StageBitzDB DataContext
        {
            get
            {
                return dataContextPrivate;
            }

            private set
            {
                dataContextPrivate = value;
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        public void SaveChanges()
        {
            DataContext.SaveChanges();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            DataContext.Dispose();
        }
    }
}