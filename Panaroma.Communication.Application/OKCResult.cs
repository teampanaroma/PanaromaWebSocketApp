using System;

namespace Panaroma.Communication.Application
{
    public static class OKCResult
    {
        public static void SetToCommunicationResult(bool hasError, object result, TcpCommand tcpCommand,
            Exception exception = null, bool showDesktop = false)
        {
            if (hasError)
            {
                InternalCommunication.GetInternalCommunication().HasError = true;
                InternalCommunication.GetInternalCommunication().ShowDesktop = showDesktop;
                InternalCommunication.GetInternalCommunication().Results =
                    (exception != null ? exception.Message : null) ?? result;
                InternalCommunication.GetInternalCommunication().Exceptions.Add(exception);
                InternalCommunication.GetInternalCommunication().NotificationWindowses.Add(new NotificationWindows()
                {
                    Header = tcpCommand.Header,
                    NotificationType = NotificationType.Error,
                    Description = tcpCommand.Description + (!string.IsNullOrEmpty(result.ToString())
                                      ? result
                                      : (string.IsNullOrEmpty(exception?.Message)
                                          ? (exception != null ? exception.Message : null)
                                          : "")),
                    Time = Helpers.DateTimeHelper.GetDateTime()
                });
            }
            else
            {
                InternalCommunication.GetInternalCommunication().IsSuccess = true;
                InternalCommunication.GetInternalCommunication().ShowDesktop = showDesktop;
                InternalCommunication.GetInternalCommunication().Results = result;
                InternalCommunication.GetInternalCommunication().Method = tcpCommand.Method;
                InternalCommunication.GetInternalCommunication().NotificationWindowses.Add(new NotificationWindows()
                {
                    Header = tcpCommand.Header,
                    Description = tcpCommand.Description + " Başarılı.",
                    NotificationType = NotificationType.Success,
                    Time = Helpers.DateTimeHelper.GetDateTime()
                });
            }
        }
    }
}