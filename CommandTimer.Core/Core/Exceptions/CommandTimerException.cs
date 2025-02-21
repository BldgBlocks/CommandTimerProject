using System;

namespace CommandTimer.Core;

public static class Exceptions {

    /// <summary>
    /// Base class for custom exceptions.
    /// </summary>
    public abstract class CommandTimerException : Exception {

        protected CommandTimerException(string message) : base(message) {

        }

        protected CommandTimerException(string message, Exception innerException) : base(message, innerException) {

        }

        public abstract int ErrorCode { get; init; }

    }

    public sealed class ApplicationException : CommandTimerException {
        public ApplicationException(string message) : base(message) { }

        public ApplicationException(string message, Exception innerException) : base(message, innerException) { }

        public override int ErrorCode { get; init; } = 1;
    }

    public sealed class ControlNotFoundException : CommandTimerException {
        public ControlNotFoundException(string message) : base(message) { }

        public ControlNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        public override int ErrorCode { get; init; } = 2;
    }
}
