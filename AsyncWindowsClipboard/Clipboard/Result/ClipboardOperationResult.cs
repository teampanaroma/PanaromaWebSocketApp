﻿using AsyncWindowsClipboard.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncClipboardService.Clipboard
{
    internal sealed class ClipboardOperationResult : IClipboardOperationResult
    {
        public static IClipboardOperationResult SuccessResult = new ClipboardOperationResult(ClipboardOperationResultCode.Success);

        public ClipboardOperationResult(ClipboardOperationResultCode resultCode, string message, IEnumerable<uint> errorCodes) :
            this
            (
                resultCode: resultCode,
                message: $"{message}{Environment.NewLine}The error codes are : {string.Join(",", errorCodes?.Select(e => e.ToString()))} (see: {nameof(LastErrors)} property)"
            )
        {
            if(errorCodes == null) throw new ArgumentNullException(nameof(errorCodes));
            LastErrors = errorCodes.ToArray();
        }

        public ClipboardOperationResult(ClipboardOperationResultCode resultCode, string message, uint errorCode) : this
            (
                resultCode: resultCode,
                message: $"{message}{Environment.NewLine}The last error code is {nameof(errorCode)} (see: {nameof(LastError)} property)"
            )
        {
            LastError = errorCode;
        }

        public ClipboardOperationResult(ClipboardOperationResultCode resultCode, string message) : this(resultCode)
        {
            Message = message;
        }

        public ClipboardOperationResult(ClipboardOperationResultCode resultCode)
        {
            ResultCode = resultCode;
            LastError = NativeMethods.GetLastError();
        }

        public ClipboardOperationResultCode ResultCode { get; }
        public string Message { get; }
        public uint? LastError { get; }
        public uint[] LastErrors { get; set; }
        public bool IsSuccessful => ResultCode == ClipboardOperationResultCode.Success;

        public override string ToString()
        {
            return Message;
        }
    }
}