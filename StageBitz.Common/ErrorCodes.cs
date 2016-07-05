namespace StageBitz.Common
{
    /// <summary>
    /// Enum class for error codes in error popup.
    /// </summary>
    public enum ErrorCodes
    {
        None = 0,
        NoEditPermissionForItemInItemBrief = 100,
        ItemBookingDelayed = 101,
        QuantityUpdateFailed = 102,
        ActionNotPerformed = 103,
        ItemBookingPeriodPassed = 104,
        InventoryLocationDeleted = 105,
        NoEditPermissionForInventory = 106,
        ItemNotVisible = 107,
        ItemDeleted = 108
    }
}