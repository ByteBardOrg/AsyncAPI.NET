namespace ByteBard.AsyncAPI.Readers
{
    /// <summary>
    /// Unmapped member handling.
    /// </summary>
    public enum UnmappedMemberHandling
    {
        /// <summary>
        /// Add error to diagnostics for unmapped members.
        /// </summary>
        Error,

        /// <summary>
        /// Ignore unmapped members.
        /// </summary>
        Ignore,
    }
}
