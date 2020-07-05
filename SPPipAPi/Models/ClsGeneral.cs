using System;

using System.Configuration;

using System.IO;

using System.Net;
using System.Text;


namespace SPPipAPi.Models
{
    public class ClsGeneral
    {

        public static string DoWebGetRequest(string url, string data)
        {
            WebRequest request = WebRequest.Create(url + data);

            request.ContentType = "Plain/text; charset=UTF-8";

            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;


            WebResponse response = request.GetResponse();

            // Display the status.
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            response.Close();
            return responseFromServer;

        }
        public static string DoWebreqeust(string url, string json)
        {
            // create a request
            HttpWebRequest request = (HttpWebRequest)
            WebRequest.Create(url); request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";


            // turn our request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(json);

            // this is important - make sure you specify type this way
            request.ContentType = "application/json; charset=UTF-8";
            request.Accept = "application/json";
            request.ContentLength = postBytes.Length;
            // request.CookieContainer = Cookies;
            //request.UserAgent = currentUserAgent;
            Stream requestStream = request.GetRequestStream();

            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            // grab te response and print it out to the console along with the status code
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result;
            using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
            {
                result = rdr.ReadToEnd();
            }

            return result;

        }

        public static String getConfigvalue(String key)
        {
            if (ConfigurationManager.AppSettings[key] != null)
                return ConfigurationManager.AppSettings[key];
            else
                return "";
        }

    }
}