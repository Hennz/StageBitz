using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StageBitz.Common.Exceptions
{
    /// <summary>
    /// Represents a DB Concurrency vialation in StageBitz system.
    /// </summary>
    [Serializable]
    public class ConcurrencyException : StageBitzException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="relatedId">The related identifier.</param>
        public ConcurrencyException(ExceptionOrigin origin, int relatedId)
            : base(origin, relatedId, string.Format("DB concurrency vialoation occured in {0} with RelatedId {1}.", origin, relatedId))
        {
        }
    }
}