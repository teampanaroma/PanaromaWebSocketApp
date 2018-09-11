using System;
using System.Collections.Generic;

namespace Panaroma.Communication.Application
{
    public sealed class InternalCommunication
    {
        private static readonly InternalCommunication _internalCommunication = new InternalCommunication();
        public List<Exception> Exceptions = new List<Exception>();
        public List<NotificationWindows> NotificationWindowses = new List<NotificationWindows>();
        public bool IsSuccess { get; set; }

        public bool HasError { get; set; }

        public object Results { get; set; }

        public bool ShowDesktop { get; set; } = true;

        public string Method { get; set; }

        public static InternalCommunication GetInternalCommunication()
        {
            return _internalCommunication;
        }
    }
}