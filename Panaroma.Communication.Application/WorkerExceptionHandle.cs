using System;

namespace Panaroma.Communication.Application
{
    public class WorkerExceptionHandle
    {
        public Exception Exception { get; }

        public WorkerExceptionHandle(Exception exception)
        {
            Exception = exception;
        }
    }
}