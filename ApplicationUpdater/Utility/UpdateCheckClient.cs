using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace Alfa.Windows.ApplicationUpdater
{
    public class UpdateCheckClient
    {
        /// <summary>
        /// Checks for the updates sends Installed softwares to given api url
        /// </summary>
        /// <param name="updateRequestDTO"></param>
        /// <param name="apiUrl"></param>
        /// <returns></returns>
        public static UpdateResponseDTO checkForUpdates(UpdateRequestDTO updateRequestDTO, string apiUrl)
        {
            ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            WebRequest request = WebRequest.CreateHttp(apiUrl);
            request.ContentType = "text/json";
            request.Method = "POST";

            var json = new JavaScriptSerializer().Serialize(updateRequestDTO);

            using(var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)request.GetResponse();
            UpdateResponseDTO result;
            using(var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                //var result = streamReader.ReadToEnd();
                result = new JavaScriptSerializer().Deserialize<UpdateResponseDTO>(streamReader.ReadToEnd());
            }

            return result;
        }

        /// <summary>
        /// Download updates from returned packet
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <param name="tmpPath"></param>
        /// <returns></returns>

        public static byte[] downloadUpdate(string downloadUrl, string tmpPath)
        {
            byte[] getbytes;
            if(File.Exists(tmpPath))
            {
                File.Delete(tmpPath);
            }

            using(var client = new WebClient())
            {
                client.DownloadFile(downloadUrl, tmpPath);

                getbytes = File.ReadAllBytes(tmpPath);
                //File.Delete(downloadFile);
            }

            return getbytes;
        }

        /// <summary>
        /// Download Ftp file from  ftp
        /// </summary>
        /// <param name="url"></param>
        /// <param name="savePath"></param>
        /// <param name="UserName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] DownloadFtpFile(string url, string savePath, string UserName, string password)
        {
            byte[] getbytes;
            try
            {
                if(File.Exists(savePath))
                    File.Delete(savePath);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);

                request.Method = WebRequestMethods.Ftp.DownloadFile;

                request.Credentials = new NetworkCredential(UserName, password);

                request.UseBinary = true;

                using(FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using(Stream rs = response.GetResponseStream())

                    {
                        using(FileStream ws = new FileStream(savePath, FileMode.Create))

                        {
                            byte[] buffer = new byte[2048];

                            int bytesRead = rs.Read(buffer, 0, buffer.Length);

                            while(bytesRead > 0)

                            {
                                ws.Write(buffer, 0, bytesRead);

                                bytesRead = rs.Read(buffer, 0, buffer.Length);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Download FTP Error: " + ex.Message);
            }

            return getbytes = File.ReadAllBytes(savePath);
        }

        private static bool AcceptAllCertifications(object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certification,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static bool CheckConnection()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        public static IList<FtpInfo> GetFtpUserInformation(string ApiMethod)
        {
            ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            WebRequest request = WebRequest.CreateHttp(ApiMethod);
            request.ContentType = "text/json";
            request.Method = "GET";

            var httpResponse = (HttpWebResponse)request.GetResponse();
            IList<FtpInfo> result;
            using(var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                //var result = streamReader.ReadToEnd();
                result = new JavaScriptSerializer().Deserialize<IList<FtpInfo>>(streamReader.ReadToEnd());
            }

            return result;
        }
    }
}