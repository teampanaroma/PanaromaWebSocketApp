﻿using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace AsyncWindowsClipboard.Exceptions
{
    [Serializable()]
    /// <summary>
    ///     This type of exception is thrown if there has been any errors during communication with windows api.
    /// </summary>
    /// <seealso cref="Win32Exception" />
    public sealed class ClipboardWindowsApiException : Win32Exception, ISerializable
    {
        public ClipboardWindowsApiException(uint error) : base((int)error)
        {
        }
        public ClipboardWindowsApiException(string message) : base(message)
        {
        }
        public ClipboardWindowsApiException(uint error, string message) : base((int)error, message)
        {
        }
    }
}