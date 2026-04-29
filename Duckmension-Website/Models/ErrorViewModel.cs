namespace Duckmension_Website.Models
{
    /// <summary>
    /// View model for displaying error information to users.
    /// Used to show error pages with request tracing information for debugging.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// The unique request identifier for tracing errors in logs.
        /// Useful for matching client-side errors with server-side error logs.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Determines whether the request ID should be displayed to the user.
        /// Returns true only if the RequestId has a value (not null or empty).
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
