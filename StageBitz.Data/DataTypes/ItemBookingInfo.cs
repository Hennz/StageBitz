namespace StageBitz.Data.DataTypes
{
    public class ItemBookingInfo
    {
        /// <summary>
        /// Gets or sets the item brief item.
        /// </summary>
        /// <value>
        /// The item brief item.
        /// </value>
        public ItemBooking ItemBooking { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is delayed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is delayed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDelayed { get; set; }
    }
}