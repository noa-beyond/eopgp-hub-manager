namespace Domain.Enums
{
    /// <summary>
    /// Represents the status of an order in the processing pipeline.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order is ready to start or has been delivered.
        /// </summary>
        Started = 1,

        /// <summary>
        /// Order is currently being processed.
        /// </summary>
        Processing = 2,

        /// <summary>
        /// Order is completed.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Processing of the order failed.
        /// </summary>
        Failed = 4
    }
}