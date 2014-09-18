using System;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Diagnostics;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Network and Web utilities.
	/// </summary>
	public static class Net
	{
		/// <summary>
		/// Default request timeout, in seconds.
		/// </summary>
		public readonly static Byte REQUEST_TIMEOUT = 5;				// In seconds

		/// <summary>
		/// Returns a flag indicating that the network is connected and available.
		/// </summary>
		/// <returns>True or false.</returns>
		public static bool IsNetworkConnected()
		{
			return NetworkInterface.GetIsNetworkAvailable();
		}

        /// <summary>
        /// Flush local DNS.
        /// </summary>
        public static void FlushDNS()
        {
            var info = new ProcessStartInfo("ipconfig.exe", "/release");
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.CreateNoWindow = false;
            var process = Process.Start(info);
            process.WaitForExit();

            info = new ProcessStartInfo("ipconfig.exe", "/renew");
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.CreateNoWindow = false;
            process = Process.Start(info);
            process.WaitForExit();
        }

        public static int RestartService(string serviceName)
        {
            serviceName = Utility.GetStringValue(serviceName).Trim();

            var info = new ProcessStartInfo("Net.exe", "stop " + serviceName);
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.CreateNoWindow = false;
            var process = Process.Start(info);
            process.WaitForExit();

            info = new ProcessStartInfo("Net.exe", "start " + serviceName);
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.CreateNoWindow = false;
            process = Process.Start(info);
            process.WaitForExit();
            return process.ExitCode;
        }

		/// <summary>
		/// Checks to see if the target of a URL exists, using the default Request timeout.
		/// </summary>
		/// <param name="url">Url to check.</param>
		/// <param name="timedOut">Contains TRUE if the request timed out.</param>
		/// <returns>True if the target exists.</returns>
		static public Boolean UrlResourceExists(String url, ref Boolean timedOut)
		{
			return UrlResourceExists(url, REQUEST_TIMEOUT, ref timedOut);
		}

		/// <summary>
		/// Checks to see if the target of a URL exists.
		/// </summary>
		/// <param name="url">Url to check.</param>
		/// <param name="requestTimeOut">Desired request timeout.</param>
		/// <param name="timedOut">Contains TRUE if the request timed out.</param>
		/// <returns>True if the target exists.</returns>
		static public Boolean UrlResourceExists(String url, int requestTimeOut, ref Boolean timedOut)
		{
			HttpWebResponse response = null;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.AllowAutoRedirect = false;

			timedOut = false;
            request.Timeout = REQUEST_TIMEOUT * 1000;        
            try
            {			
                response = (HttpWebResponse)request.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException wex)
            {
				if (wex.Status != WebExceptionStatus.Timeout)
					timedOut = true;
				return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                request = null;
                response = null;
            }
		}

        public class PostResponse
        {
            public Exception Exception { get; internal set; }
            public string String { get; internal set; }
        }

        public static PostResponse Post(string uri, System.Collections.Specialized.NameValueCollection pairs)
        {
            byte[] rawData = null;
            var response = new PostResponse();
            try
            {
                using (WebClient client = new WebClient())
                {
                    rawData = client.UploadValues(uri, pairs);
                }
                response.String = System.Text.Encoding.UTF8.GetString(rawData);
            }
            catch (Exception ex)
            {
                response.Exception = ex;                
            }
            return response;
        }

		/// <summary>
		/// Downloads any kind of file from the given URL, using the default timeout.
		/// </summary>
		/// <param name="url">Url to open.</param>		
		/// <returns>Path to temporary file.</returns>
		static public String DownloadFile(String url)
		{
			return DownloadFile(url, REQUEST_TIMEOUT);
		}

        static public byte[] DownloadData(string url)
        {
            return DownloadData(url, Enumerations.HttpRequestMethod.Get);
        }

        static public byte[] DownloadData(string url, Enumerations.HttpRequestMethod requestMethod)
        {
            return DownloadData(url, requestMethod, REQUEST_TIMEOUT);
        }

        /// <summary>
        /// Downloads binary data from a URL.
        /// </summary>
        /// <typeparam name="T">Desired type.</typeparam>
        /// <param name="url">Source URL.</param>
        /// <param name="timeout">Timeout, in seconds.</param>
        /// <returns></returns>
        static public byte[] DownloadData(string url, Enumerations.HttpRequestMethod requestMethod, int timeout)
        {
            byte[] result;
            byte[] buffer = new byte[4096];
            WebRequest request = WebRequest.Create(url);
            request.Timeout = timeout * 1000;
            request.Method = requestMethod.GetDescription();
            //if (contentLength > 0)
            //{
            //    request.ContentLength = contentLength;
            //}

            using (WebResponse response = request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = responseStream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);

                        } while (count != 0);

                        result = memoryStream.ToArray();
                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Downloads any kind of file from the given URL.
        /// </summary>
        /// <param name="url">Url to open.</param>
		/// <param name="timeout">Timeout in seconds.</param>
        /// <returns>Path to temporary file.</returns>
        static public String DownloadFile(String url, int timeout)
        {
			// Prepare the url
			System.Uri uri = new System.Uri(url);
            string dir = Path.GetTempPath();
			string fileName = Path.GetTempFileName();		// Ensure random file name		
			Byte[] data = null;
			int size = 0;

			HttpWebResponse response = null;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			Stream destination = null;
			BinaryWriter writer = null;

			request.Timeout = timeout * 1000;
			request.AllowAutoRedirect = false;
			try
			{
				response = (HttpWebResponse)request.GetResponse();
				Stream stream = response.GetResponseStream();
				destination = File.Open(fileName, FileMode.Create, FileAccess.Write);
				writer = new BinaryWriter(destination);
				data = new Byte[2048];
				do
				{
					size = stream.Read(data, 0, data.Length);
					if (size > 0) writer.Write(data, 0, size);
				} while (size > 0);
				return fileName;
			}
			catch
			{
				return string.Empty;
			}
			finally
			{
				if (writer != null) 
				{
					writer.Flush();
					writer.Close();
					writer = null;
				}

				if (response != null)
				{
					response.Close();
					response = null;
				}
			}
        }
	}
}
