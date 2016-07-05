namespace StageBitz.Data.DataTypes
{
    /// <summary>
    /// Poco class for Company ItemBookings
    /// </summary>
    public class CompanyItemBooking
    {

        /// <summary>
        /// Gets or sets the item booking.
        /// </summary>
        /// <value>
        /// The item booking.
        /// </value>
        public ItemBooking ItemBooking { get; set; }


        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        public Company Company { get; set; }
    }
}
