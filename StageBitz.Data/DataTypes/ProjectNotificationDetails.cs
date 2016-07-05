using System.Collections.Generic;
namespace StageBitz.Data.DataTypes
{
    /// <summary>
    /// Poco class for project notification details.
    /// </summary>
    public class ProjectNotificationDetails
    {
        /// <summary>
        /// Gets or sets the project id.
        /// </summary>
        /// <value>
        /// The project id.
        /// </value>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the project team updates status.
        /// </summary>
        /// <value>
        /// The project team updates status.
        /// </value>
        public UpdateStatus ProjectTeamUpdatesStatus { get; set; }

        /// <summary>
        /// Gets or sets the item brief update status.
        /// </summary>
        /// <value>
        /// The item brief update status.
        /// </value>
        public UpdateStatus ItemBriefUpdateStatus { get; set; }

        /// <summary>
        /// Gets or sets the schedule update status.
        /// </summary>
        /// <value>
        /// The schedule update status.
        /// </value>
        public UpdateStatus ScheduleUpdateStatus { get; set; }

        /// <summary>
        /// Gets or sets the project update status.
        /// </summary>
        /// <value>
        /// The project update status.
        /// </value>
        public UpdateStatus ProjectUpdateStatus { get; set; }

        /// <summary>
        /// Gets or sets the task update status.
        /// </summary>
        /// <value>
        /// The task update status.
        /// </value>
        public UpdateStatus TaskUpdateStatus { get; set; }

        /// <summary>
        /// Gets or sets the item list update status.
        /// </summary>
        /// <value>
        /// The item list update status.
        /// </value>
        public UpdateStatus ItemListUpdateStatus { get; set; }
    }

    /// <summary>
    /// Poco class for update status.
    /// </summary>
    public class UpdateStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance has updates for given date.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has updates for given date; otherwise, <c>false</c>.
        /// </value>
        public bool HasUpdatesForGivenDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has updates for full duration.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has updates for full duration; otherwise, <c>false</c>.
        /// </value>
        public bool HasUpdatesForFullDuration { get; set; }


        /// <summary>
        /// Gets or sets the user list.
        /// </summary>
        /// <value>
        /// The user list.
        /// </value>
        public IEnumerable<int> UserList { get; set; } 
    }
}