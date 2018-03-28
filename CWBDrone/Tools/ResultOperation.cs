using System;

namespace CWBDrone.Tools
{
    public class ResultOperation
    {
        public bool IsSuccess { get; }
        public bool IsError { get => !IsSuccess; }
        public Exception Exception { get; } = null;

        private ResultOperation(bool success, Exception exception)
        {
            IsSuccess = success;
            Exception = exception;
        }

        public static ResultOperation FromSuccess() => new ResultOperation(true, null);
        public static ResultOperation FromError(Exception exception) => new ResultOperation(false, exception);
    }
}
