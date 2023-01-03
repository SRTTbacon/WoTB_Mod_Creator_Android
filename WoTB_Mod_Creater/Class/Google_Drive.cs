using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.Class
{
    public class Google_Drive
    {
        public bool IsConnected => Service != null;
        private readonly DriveService Service = null;
        private readonly string[] Scopes = { DriveService.Scope.Drive };
        private const string Parent_Dir_ID = "1b6UclE6f_miY20Hj30W_k1c7qck9SKK8";
        public Google_Drive()
        {
            Google.Apis.Auth.OAuth2.GoogleCredential credential;
            try
            {
                credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromStream(DependencyService.Get<IAndroidSystem>().Get_Drive_API()).CreateScoped(Scopes);
            }
            catch
            {
                return;
            }
            Google.Apis.Services.BaseClientService.Initializer init = new Google.Apis.Services.BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "WoTB_Mod_Creator"
            };
            Service = new DriveService(init);
        }
        public bool Delete_File(string File_ID)
        {
            try
            {
                FilesResource.DeleteRequest req = Service.Files.Delete(File_ID);
                req.Fields = "id, name";
                req.SupportsAllDrives = true;
                req.Execute();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Upload_File(string From_File, out string Uploaded_URL, string Upload_Name = "")
        {
            Uploaded_URL = "";
            if (Upload_Name == "")
                Upload_Name = Path.GetFileName(From_File);
            if (!File.Exists(From_File))
                return false;
            Google.Apis.Upload.IUploadProgress prog;
            FileStream fsu = new FileStream(From_File, FileMode.Open);
            try
            {
                Google.Apis.Drive.v3.Data.File meta = new Google.Apis.Drive.v3.Data.File
                {
                    Name = Upload_Name,
                    MimeType = "application/unknown",
                    Parents = new List<string>() { Parent_Dir_ID }
                };
                FilesResource.CreateMediaUpload req = Service.Files.Create(meta, fsu, "application/unknown");
                req.Fields = "id, name";
                prog = req.Upload();
                Uploaded_URL = req.ResponseBody.Id;
            }
            catch (Exception e)
            {
                fsu.Close();
                Console.WriteLine(e.Message);
                return false;
            }
            fsu.Close();
            if (prog.Exception != null)
                Console.WriteLine(prog.Exception.Message);
            return prog.Status == Google.Apis.Upload.UploadStatus.Completed;
        }
        public bool Download_File(string To_File, string Uploaded_URL)
        {
            try
            {
                if (Uploaded_URL.Contains("/file/d/"))
                {
                    string Temp_01 = Uploaded_URL.Substring(Uploaded_URL.IndexOf("/file/d/") + 8);
                    if (Temp_01.Contains("/"))
                        Uploaded_URL = Temp_01.Substring(0, Temp_01.IndexOf('/'));
                    else
                        Uploaded_URL = Temp_01;
                }
                FilesResource.GetRequest request = Service.Files.Get(Uploaded_URL);
                FileStream fs = new FileStream(To_File, FileMode.Create);
                Google.Apis.Download.IDownloadProgress Progress = request.DownloadWithStatus(fs);
                fs.Close();
                if (Progress.Exception != null)
                    Console.WriteLine(Progress.Exception.Message);
                return Progress.Status == Google.Apis.Download.DownloadStatus.Completed && File.Exists(To_File);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}