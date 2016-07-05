namespace StageBitz.Data.DataTypes
{
    /// <summary>
    /// Poco class for project user infos.
    /// </summary>
    public class ProjectUserInfo
    {
        /// <summary>
        /// Gets or sets the project id.
        /// </summary>
        /// <value>
        /// The project id.
        /// </value>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>
        /// The name of the project.
        /// </value>
        public string ProjectName { get; set; }

        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        /// <value>
        /// The name of the company.
        /// </value>
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the company id.
        /// </summary>
        /// <value>
        /// The company id.
        /// </value>
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>
        /// The user id.
        /// </value>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        /// <value>
        /// The user email.
        /// </value>
        public string UserEmail { get; set; }

        /// <summary>
        /// Gets or sets the project email notification code id.
        /// </summary>
        /// <value>
        /// The project email notification code id.
        /// </value>
        public int ProjectEmailNotificationCodeId { get; set; }

        /// <summary>
        /// Gets or sets the company email notification code id.
        /// </summary>
        /// <value>
        /// The company email notification code id.
        /// </value>
        public int CompanyEmailNotificationCodeId { get; set; }
    }
}