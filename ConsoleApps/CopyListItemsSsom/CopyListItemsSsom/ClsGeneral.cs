﻿using Microsoft.SharePoint.Client.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

using System.Configuration;

using System.IO;
using System.Linq;
using System.Net;
using System.Text;


namespace CopyListItemsSsom
{
    public class ClsGeneral
    {
        public static string GetJsonStringFromQueryString(string queryString)
        {
            var nvs = System.Web.HttpUtility.ParseQueryString(queryString);
            var dict = nvs.AllKeys.ToDictionary(k => k, k => nvs[k]);
            return "[" + JsonConvert.SerializeObject(dict, new KeyValuePairConverter()) + "]";
        }
        public static string DoWebGetRequest(string url, string data)
        {
            WebRequest request = WebRequest.Create(url + data);

           // request.ContentType = "Plain/text; charset=UTF-8";

            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            //request.ContentType = "application/json; charset=UTF-8";
            request.ContentType = "application/json; odata=nometadata";
            
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
            if (ConfigurationSettings.AppSettings[key] != null)
                return ConfigurationSettings.AppSettings[key];
            else
                return "";
        }

    }
}