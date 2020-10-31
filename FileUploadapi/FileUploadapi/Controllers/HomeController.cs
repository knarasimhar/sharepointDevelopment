using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Http.Cors;
using System.Net.Http.Headers;
//using File = Microsoft.SharePoint.Client.File;

namespace FileUploadapi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
      public class HomeController : Controller
    {
        string strSiteURL = "http://sharepoint1:8080/sites/testlocal/", strUSER = "pruser1", strPWD = "%Dalab^!065#";
        //string SITE_API_URL = "";
        string strDomainName = ConfigurationManager.AppSettings["DomainName"].ToString();
        public HomeController()
        {
            if (ConfigurationManager.AppSettings["SITE_URL"] != null)
                strSiteURL = ConfigurationManager.AppSettings["SITE_URL"].ToString();
            //if (ConfigurationManager.AppSettings["SITE_API_URL"] != null)
            //    SITE_API_URL = ConfigurationManager.AppSettings["SITE_API_URL"].ToString();
            if (ConfigurationManager.AppSettings["SITE_URL_USER"] != null)
                strUSER = ConfigurationManager.AppSettings["SITE_URL_USER"].ToString();
            if (ConfigurationManager.AppSettings["SITE_URL_PWD"] != null)
                strPWD = ConfigurationManager.AppSettings["SITE_URL_PWD"].ToString();
            //if (ConfigurationManager.AppSettings["AD_USER_URL"] != null)
            //    strADUserURL = ConfigurationManager.AppSettings["AD_USER_URL"].ToString();


            //strPWD = HttpUtility.UrlEncode(strPWD);
        }
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
        [System.Web.Http.HttpGet]
        public ActionResult FileUpload()
        {
            return View();
        }

       
        [System.Web.Http.HttpGet]
        public ActionResult DownloadFilesFromSharePoint()
        {

          

            ClientContext ctx = new ClientContext(strSiteURL);
            ctx.Credentials = new NetworkCredential(strUSER,strPWD);

            FileCollection files = ctx.Web.GetFolderByServerRelativeUrl("Shared%20Documents/").Files;
          
           // return fInfo.Stream;
            ctx.Load(files);
            ctx.ExecuteQuery();

            foreach (Microsoft.SharePoint.Client.File file in files)
            {
                if (ctx.HasPendingRequest)
                    ctx.ExecuteQuery();
                FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctx, file.ServerRelativeUrl);
                ctx.ExecuteQuery();

                var filePath = @"C:\docs\spdocs\" + file.Name;
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    fileInfo.Stream.CopyTo(fileStream);

                }

            }
            return View();            
        }
        [System.Web.Http.HttpGet]
        public ActionResult DownloadFile(string filename)
        {
            ClientContext ctx = new ClientContext(strSiteURL);
            ctx.Credentials = new NetworkCredential(strUSER, strPWD);

            FileCollection files = ctx.Web.GetFolderByServerRelativeUrl("Shared%20Documents/").Files;

            ctx.Load(files);
            ctx.ExecuteQuery();

            foreach (Microsoft.SharePoint.Client.File file in files)
            {
                //if()
                string ename = file.Name;
                if (ename.Contains(filename))
                {
                    if (ctx.HasPendingRequest)
                        ctx.ExecuteQuery();
                    FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctx, file.ServerRelativeUrl);
                    ctx.ExecuteQuery();

                    var filePath = @"C:\docs\spdocs\" + file.Name;
                    using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        fileInfo.Stream.CopyTo(fileStream);
                    }
                }
            }
            return View();
        }

    }
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class FileController : ApiController
    {
        string strSiteURL = "http://sharepoint1:8080/sites/testlocal/", strUSER = "pruser1", strPWD = "%Dalab^!065#";
        //string SITE_API_URL = "";
        string strDomainName = ConfigurationManager.AppSettings["DomainName"].ToString();
        public FileController()
        {
            if (ConfigurationManager.AppSettings["SITE_URL"] != null)
                strSiteURL = ConfigurationManager.AppSettings["SITE_URL"].ToString();
            //if (ConfigurationManager.AppSettings["SITE_API_URL"] != null)
            //    SITE_API_URL = ConfigurationManager.AppSettings["SITE_API_URL"].ToString();
            if (ConfigurationManager.AppSettings["SITE_URL_USER"] != null)
                strUSER = ConfigurationManager.AppSettings["SITE_URL_USER"].ToString();
            if (ConfigurationManager.AppSettings["SITE_URL_PWD"] != null)
                strPWD = ConfigurationManager.AppSettings["SITE_URL_PWD"].ToString();
            //if (ConfigurationManager.AppSettings["AD_USER_URL"] != null)
            //    strADUserURL = ConfigurationManager.AppSettings["AD_USER_URL"].ToString();


            //strPWD = HttpUtility.UrlEncode(strPWD);
        }

        [System.Web.Http.HttpPost]
        public async Task<string> Upload()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // extract file name and file contents
                var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                    .FirstOrDefault(p => p.Name.ToLower() == "filename");
                string fileName = (fileNameParam == null) ? "" : fileNameParam.Value.Trim('"');
              //  byte[] file = await provider.Contents[0].ReadAsByteArrayAsync();

               // var result     = string.Format("Received '{0}' with length: {1}", fileName, file.Length);
                //return result;
                if (!Request.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

             
              

                string fName = Path.GetFileName(fileName);
                //string path = fileName.Substring(fileName) //Path.Combine(Path.GetTempPath() /*e.FileId*/);
                //store the path in Sessio  
                // Session["FileDirectory"] = path;
                //get the site  
                ClientContext context = new ClientContext(strSiteURL);
                context.Credentials = new NetworkCredential(strUSER, strPWD);
                //get the web object  
                Web oWebsite = context.Web;
                context.Load(oWebsite);
                //execute the web object  
                context.ExecuteQuery();
                
                List CurrentList = context.Web.Lists.GetByTitle("Documents");
                context.Load(CurrentList.RootFolder);
                context.ExecuteQuery();
                //form the url to get the entire path of file like D://folder/test.txt  
                String fileURL = CurrentList.RootFolder.ServerRelativeUrl.ToString() + "/";// + (fileName);
                if (!Directory.Exists(fileURL)) Directory.CreateDirectory(fileURL);
                 foreach (var file in provider.Contents)
                {
                    var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                    var buffer = await file.ReadAsByteArrayAsync();
                    System.IO.File.WriteAllBytes(fileURL + filename, buffer);
                    //Do whatever you want with filename and its binary data.
                }
                using (FileStream fileStream = new FileStream(fileURL + fileName, FileMode.Open))
                    Microsoft.SharePoint.Client.File.SaveBinaryDirect(context, fileURL + fName, fileStream, true);
                return "Uploaded successfully...";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
        [System.Web.Http.Route("api/file/Download")]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Download(string File)
        { 

            ClientContext ctx = new ClientContext(strSiteURL);
            ctx.Credentials = new NetworkCredential(strUSER, strPWD);
            List CurrentList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(CurrentList.RootFolder);
            ctx.ExecuteQuery();
            Microsoft.SharePoint.Client.File file = ctx.Web.GetFileByServerRelativeUrl(CurrentList.RootFolder.ServerRelativeUrl.ToString() + "/" + File);
           
            // return fInfo.Stream;
            ctx.Load(file);
            ctx.ExecuteQuery();

          
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            if (!file.Exists) return result=null;
            FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctx, file.ServerRelativeUrl);

            // result.Content.file
            result.Content = new StreamContent(fileInfo.Stream);
            result.Content.Headers.Add("content-disposition", "attachment; filename=" + File);
            //fileInfo.Dispose();
            
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
            
        }


        [System.Web.Http.Route("api/file/Delete")]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage Delete(string File)
        {



            ClientContext ctx = new ClientContext(strSiteURL);
            ctx.Credentials = new NetworkCredential(strUSER, strPWD);
            List CurrentList = ctx.Web.Lists.GetByTitle("Documents");
            ctx.Load(CurrentList.RootFolder);
            ctx.ExecuteQuery();
            Microsoft.SharePoint.Client.File file = ctx.Web.GetFileByServerRelativeUrl(CurrentList.RootFolder.ServerRelativeUrl.ToString() + "/" + File);

            // return fInfo.Stream;
            ctx.Load(file);
            ctx.ExecuteQuery();
            if (!file.Exists) return getHttpResponseMessage("file not exists delete");
            file.DeleteObject();
            ctx.ExecuteQuery();
            return getHttpResponseMessage("deleted successfully");

        }

        private HttpResponseMessage getHttpResponseMessage(string Resp)
        {

            return new HttpResponseMessage { Content = new StringContent(Resp, System.Text.Encoding.UTF8, "application/json") };

        }

    }
}
