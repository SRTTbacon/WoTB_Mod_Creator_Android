using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using WoTB_Mod_Creater.Class;
using WoTB_Mod_Creater.Droid.Class;
using static Android.Provider.Settings;

[assembly: Xamarin.Forms.Dependency(typeof(Interface_Code.Message_Android))]
[assembly: Xamarin.Forms.Dependency(typeof(Interface_Code.FileSystem))]
[assembly: Xamarin.Forms.Dependency(typeof(Interface_Code.AndroidSystem))]
namespace WoTB_Mod_Creater.Droid.Class
{
    public class Interface_Code
    {
        public static Stream Drive_API_Stream = null;
        public class Message_Android : IMessage
        {
            public void Message_Alert(string Message, Message_Length Length_Type)
            {
                if (Length_Type == Message_Length.Long)
                    Toast.MakeText(Application.Context, Message, ToastLength.Long).Show();
                else if (Length_Type == Message_Length.Short)
                    Toast.MakeText(Application.Context, Message, ToastLength.Short).Show();
            }
        }
        public class FileSystem : IFileSystem
        {
            public string GetExDir()
            {
                return Application.Context.GetExternalFilesDir(null).AbsolutePath;
            }
            public void UnZip(string filePath, string folderPath)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                if (!folderPath.Contains(fileName))
                    folderPath += "/" + fileName;
                var directoryInfo = Directory.CreateDirectory(folderPath);
                using ZipArchive archive = ZipFile.Open(filePath, ZipArchiveMode.Read, Encoding.UTF8);
                IOrderedEnumerable<ZipArchiveEntry> allTextFiles = archive.Entries.OrderBy(e => e.FullName);
                foreach (ZipArchiveEntry entry in allTextFiles)
                {
                    string unzipFilePath = Path.Combine(
                        folderPath, entry.FullName.Replace('/', '\\'));
                    if (entry.FullName.EndsWith("/"))
                        Directory.CreateDirectory(unzipFilePath);
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(unzipFilePath));
                        ExtractToFile(entry, unzipFilePath);
                    }
                }
            }
            public void ExtractToFile(ZipArchiveEntry entry, string filePath)
            {
                using FileStream fs2 = File.Open(filePath, FileMode.CreateNew, FileAccess.Write);
                using Stream strm = entry.Open();
                strm.CopyTo(fs2);
            }
        }
        public class AndroidSystem : IAndroidSystem
        {
            public string Get_Device_ID()
            {
                Context context = Application.Context;
                return Secure.GetString(context.ContentResolver, Secure.AndroidId);
            }
            public bool Get_File_Permission()
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    Context context = Application.Context;
                    if (context.CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage) == Permission.Granted && context.CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        return true;
                }
                else
                    return true;
                return false;
            }
            public List<string> Get_Applications()
            {
                List<string> Temp = new List<string>();
                IList<ApplicationInfo> apps = Application.Context.PackageManager.GetInstalledApplications(PackageInfoFlags.Activities);
                foreach (ApplicationInfo app in apps)
                {
                    try
                    {
                        ApplicationInfo appInfo = Application.Context.PackageManager.GetApplicationInfo(app.PackageName, 0);
                        string appLabel = Application.Context.PackageManager.GetApplicationLabel(appInfo);
                        Temp.Add(appLabel);
                    }
                    catch { }
                }
                return Temp;
            }
            public bool Get_WoTB_Exist()
            {
                IList<ApplicationInfo> apps = Application.Context.PackageManager.GetInstalledApplications(PackageInfoFlags.Activities);
                foreach (ApplicationInfo app in apps)
                {
                    try
                    {
                        ApplicationInfo appInfo = Application.Context.PackageManager.GetApplicationInfo(app.PackageName, 0);
                        string appLabel = Application.Context.PackageManager.GetApplicationLabel(appInfo);
                        if (appLabel == "World of Tanks")
                            return true;
                    }
                    catch { }
                }
                apps.Clear();
                return false;
            }
            public string Get_WoTB_Path()
            {
                IList<ApplicationInfo> apps = Application.Context.PackageManager.GetInstalledApplications(PackageInfoFlags.Activities);
                foreach (ApplicationInfo app in apps)
                {
                    try
                    {
                        ApplicationInfo appInfo = Application.Context.PackageManager.GetApplicationInfo(app.PackageName, 0);
                        string appLabel = Application.Context.PackageManager.GetApplicationLabel(appInfo);
                        if (appLabel == "World of Tanks")
                        {
                            Context a = Application.Context.CreatePackageContext(appInfo.PackageName, PackageContextFlags.IgnoreSecurity);
                            return a.GetExternalFilesDir(null).AbsolutePath;
                        }
                    }
                    catch { }
                }
                apps.Clear();
                return "";
            }
            public Stream Get_Drive_API()
            {
                return Drive_API_Stream;
            }
        }
    }
}