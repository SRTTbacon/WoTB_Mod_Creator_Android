using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using WoTB_Mod_Creater.All_Page;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.Class
{
    public class SRTTbacon_Server
    {
        public static string IP_Local = "192.168.0.119";
        public static string IP_Global = "srttbacon.jp";
        public static string Name = "SRTTbacon_und_GuminoAme_SFTP_Server_V2";
        public static string Password = "Twilight_Silhouette_V2";
        public const string Version = "1.0";
        public static int TCP_Port = 50000;
        public static int SFTP_Port = 25839;
        public static bool IsSRTTbaconOwnerMode = false;
        public static string IP = "";
    }
    public class Server
    {
        public bool IsLogined = false;
        public bool Server_OK = false;
        public TCP_Client TCP_Server = new TCP_Client();
        public SFTP_Client SFTPClient = null;
        public async Task Server_Connect()
        {
            try
            {
                if (!TCP_Server.IsConnected)
                {
                    await TCP_Server.Connect(SRTTbacon_Server.IP, SRTTbacon_Server.TCP_Port);
                    if (!TCP_Server.IsConnected)
                    {
                        Server_OK = false;
                        throw new Exception("サーバーに接続できませんでした...");
                    }
                }
                if (SFTPClient == null || !SFTPClient.IsConnected)
                    SFTPClient = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                Server_OK = true;
            }
            catch (Exception e)
            {
                IsLogined = false;
                Server_OK = false;
                Sub_Code.Error_Log_Write(e.Message.Replace(SRTTbacon_Server.IP_Global + ":" + SRTTbacon_Server.TCP_Port, "").Replace(SRTTbacon_Server.IP_Local + ":" + SRTTbacon_Server.TCP_Port, ""));
            }
        }
        //ログインできるか
        public bool Login()
        {
            string Dir = Sub_Code.ExDir;
            if (File.Exists(Dir + "/User.dat"))
            {
                StreamReader str = Sub_Code.File_Decrypt_To_Stream(Dir + "/User.dat", "SRTTbacon_Server_User_Pass_Save");
                string Login_Read = str.ReadLine();
                str.Close();
                string User_Name = Login_Read.Substring(0, Login_Read.IndexOf(':'));
                string Password = Login_Read.Substring(Login_Read.IndexOf(':') + 1);
                if (Account_Exist(User_Name, Password))
                {
                    Sub_Code.UserName = User_Name;
                    IsLogined = true;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
        //アカウントが存在するか
        public bool Account_Exist(string UserName, string Password)
        {
            if (!SFTPClient.IsConnected)
                return false;
            StreamReader streamReader = SFTPClient.GetFileRead("/WoTB_Voice_Mod/Accounts.dat");
            while (streamReader.EndOfStream == false)
            {
                string Line = streamReader.ReadLine();
                if (Line.Contains(":"))
                {
                    string UserName_Line = Line.Substring(0, Line.IndexOf(':'));
                    string Password_Line = Line.Substring(Line.IndexOf(':') + 1);
                    if (UserName_Line == UserName && Password_Line == Password)
                    {
                        streamReader.Close();
                        return true;
                    }
                }
            }
            streamReader.Dispose();
            return false;
        }
        //ユーザー名が既に存在するか
        public bool UserExist(string UserName)
        {
            StreamReader streamReader = SFTPClient.GetFileRead("/WoTB_Voice_Mod/Accounts.dat");
            while (streamReader.EndOfStream == false)
            {
                string Line = streamReader.ReadLine();
                if (Line.Contains(":"))
                {
                    string UserName_Line = Line.Substring(0, Line.IndexOf(':'));
                    if (UserName_Line == UserName)
                    {
                        streamReader.Close();
                        return true;
                    }
                }
            }
            streamReader.Dispose();
            return false;
        }
    }
    public static partial class StringExtensions
    {
        //文字列に指定した文字が何個あるか取得
        public static int CountOf(this string self, string Name)
        {
            int count = 0;
            int index = self.IndexOf(Name, 0);
            while (index != -1)
            {
                count++;
                index = self.IndexOf(Name, index + Name.Length);
            }
            return count;
        }
    }
    public class Sub_Code
    {
        public static readonly Voice_Create Voice_Create_Window = new Voice_Create();
        public static readonly Other_Ceate Other_Create_Window = new Other_Ceate();
        public static readonly Other_Mods_Setting Other_Mods_Setting_Window = new Other_Mods_Setting();
        public static readonly Created_Mods_Page Created_Mods_Window = new Created_Mods_Page();
        public static readonly Select_Files Select_Files_Window = new Select_Files();
        public static readonly Server Main_Server = new Server();
        public static readonly Google_Drive Drive = new Google_Drive();
        public static readonly Random r = new Random();
        public static string UserName = "";
        public static readonly string ExDir = DependencyService.Get<IFileSystem>().GetExDir();
        public static readonly string WoTBDir = DependencyService.Get<IAndroidSystem>().Get_WoTB_Path();
        public static int Chat_Mode = 0;
        public static bool HasSystemPermission = false;
        public static bool IsUseSelectPage = true;
        private static int Show_Message_Time = int.MinValue;
        //ファイルを暗号化
        //引数:元ファイルのパス,暗号先のパス,元ファイルを削除するか
        public static bool File_Encrypt(string From_File, string To_File, string Password, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_File))
                    return false;
                using (FileStream eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                using (FileStream eofs = new FileStream(To_File, FileMode.Create, FileAccess.Write))
                    FileEncryptor.Encrypt(eifs, eofs, Password);
                if (IsFromFileDelete)
                    File.Delete(From_File);
                return true;
            }
            catch (Exception e)
            {
                Error_Log_Write(e.Message);
                return false;
            }
        }
        public static bool File_Delete(string File_Path)
        {
            try
            {
                File.Delete(File_Path);
                return true;
            }
            catch
            {
                return false;
            }
        }
        //ファイルを復号化
        //引数:元ファイルのパス,復号先のパス,元ファイルを削除するか
        public static bool File_Decrypt_To_File(string From_File, string To_File, string Password, bool IsFromFileDelete)
        {
            try
            {
                if (!File.Exists(From_File))
                    return false;
                using (FileStream eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                using (FileStream eofs = new FileStream(To_File, FileMode.Create, FileAccess.Write))
                    FileEncryptor.Decrypt_To_File(eifs, eofs, Password);
                if (IsFromFileDelete)
                    File.Delete(From_File);
                return true;
            }
            catch (Exception e)
            {
                Error_Log_Write(e.Message);
                return false;
            }
        }
        public static StreamReader File_Decrypt_To_Stream(string From_File, string Password)
        {
            try
            {
                StreamReader str = null;
                using (FileStream eifs = new FileStream(From_File, FileMode.Open, FileAccess.Read))
                    str = FileEncryptor.Decrypt_To_Stream(eifs, Password);
                return str;
            }
            catch
            {
                return null;
            }
        }
        //.dvplを抜いたファイルをコピーする
        public static bool DVPL_File_Copy(string FromFilePath, string ToFilePath, bool IsOverWrite)
        {
            FromFilePath = FromFilePath.Replace(".dvpl", "");
            ToFilePath = ToFilePath.Replace(".dvpl", "");
            if (File.Exists(FromFilePath) || File.Exists(FromFilePath + ".dvpl"))
            {
                if (File.Exists(FromFilePath))
                {
                    File.Copy(FromFilePath, ToFilePath, IsOverWrite);
                    return true;
                }
                if (File.Exists(FromFilePath + ".dvpl"))
                {
                    File.Copy(FromFilePath + ".dvpl", ToFilePath + ".dvpl", IsOverWrite);
                    return true;
                }
            }
            return false;
        }
        //.dvplを抜いたファイルを削除する
        public static bool DVPL_File_Delete(string FilePath)
        {
            bool IsDelected = false;
            FilePath = FilePath.Replace(".dvpl", "");
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                IsDelected = true;
            }
            if (File.Exists(FilePath + ".dvpl"))
            {
                File.Delete(FilePath + ".dvpl");
                IsDelected = true;
            }
            return IsDelected;
        }
        public static void Error_Log_Write(string Text)
        {
            DateTime dt = DateTime.Now;
            string Time = Get_Time_Now(dt, ".", 1, 6);
            string Dir = DependencyService.Get<IFileSystem>().GetExDir();
            if (Text.EndsWith("\n"))
                File.AppendAllText(Dir + "/Error_Log.txt", Time + ":" + Text);
            else
                File.AppendAllText(Dir + "/Error_Log.txt", Time + ":" + Text + "\n");
        }
        public static string Get_Time_Now(DateTime dt, string Between, int First, int End)
        {
            if (First > End)
                return "";
            if (First == End)
                return Get_Time_Index(dt, First);
            string Temp = "";
            for (int Number = First; Number <= End; Number++)
            {
                if (Number != End)
                    Temp += Get_Time_Index(dt, Number) + Between;
                else
                    Temp += Get_Time_Index(dt, Number);
            }
            return Temp;
        }
        private static string Get_Time_Index(DateTime dt, int Index)
        {
            if (Index > 0 && Index < 7)
            {
                if (Index == 1)
                    return dt.Year.ToString();
                else if (Index == 2)
                    return dt.Month.ToString();
                else if (Index == 3)
                    return dt.Day.ToString();
                else if (Index == 4)
                    return dt.Hour.ToString();
                else if (Index == 5)
                    return dt.Minute.ToString();
                else if (Index == 6)
                    return dt.Second.ToString();
            }
            return "";
        }
        public static bool IsSafePath(string path, bool IsFileName)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            char[] invalidChars = IsFileName ? Path.GetInvalidFileNameChars() : Path.GetInvalidPathChars();
            return path.IndexOfAny(invalidChars) < 0 && !Regex.IsMatch(path, @"(^|\\|/)(CON|PRN|AUX|NUL|CLOCK\$|COM[0-9]|LPT[0-9])(\.|\\|/|$)", RegexOptions.IgnoreCase);
        }
        public static void Show_Message(string Message, Message_Length Type = Message_Length.Short)
        {
            if (Show_Message_Time + 3000 <= Environment.TickCount)
            {
                Show_Message_Time = Environment.TickCount;
                DependencyService.Get<IMessage>().Message_Alert(Message, Type);
            }
        }
        public static void Delete_Temp_Files()
        {
            Delete_Files(ExDir + "/Temp");
        }
        public static void Delete_Music_Files()
        {
            Delete_Files(ExDir + "/Temp/Music");
        }
        public static void Delete_Voice_Mods()
        {
            Delete_Files(ExDir + "/Temp/Voice_Mods");
        }
        public static void Delete_Other_Mods()
        {
            Delete_Files(ExDir + "/Temp/Other_Mods");
        }
        private static void Delete_Files(string Dir)
        {
            if (Directory.Exists(Dir))
                foreach (string File_Now in Directory.GetFiles(Dir, "*.*", SearchOption.TopDirectoryOnly))
                    File.Delete(File_Now);
        }
        public static bool File_Equal(string path1, string path2)
        {
            if (path1 == path2)
                return true;
            FileStream fs1 = null;
            FileStream fs2 = null;
            try
            {
                fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read, FileShare.Read);
                fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fs1.Length != fs2.Length)
                    return false;
                int file1byte;
                int file2byte;
                long End_Length = fs1.Length / 50;
                long Now_Length = 0;
                do
                {
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                    Now_Length++;
                }
                while ((file1byte == file2byte) && (file1byte != -1) && End_Length >= Now_Length);
                return (file1byte - file2byte) == 0;
            }
            finally
            {
                using (fs1)
                { }
                using (fs2)
                { }
            }
        }
    }
}
public class WwiseHash
{
    //GUIDからShortIDを生成
    public static uint HashGUID(string ID)
    {
        Regex alphanum = new Regex("[^0-9A-Za-z]");
        string filtered = alphanum.Replace(ID, "");
        List<byte> guidBytes = new List<byte>();
        int[] byteOrder = { 3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15 };
        for (int i = 0; i < byteOrder.Length; i++)
            guidBytes.Add(byte.Parse(filtered.Substring(byteOrder[i] * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        return FnvHash(guidBytes.ToArray(), false);
    }
    public static uint HashString(string Name)
    {
        return FnvHash(Encoding.ASCII.GetBytes(Name.ToLowerInvariant()), true);
    }
    static uint FnvHash(byte[] input, bool use32bits)
    {
        uint prime = 16777619;
        uint offset = 2166136261;
        uint mask = 1073741823;
        uint hash = offset;
        for (int i = 0; i < input.Length; i++)
        {
            hash *= prime;
            hash ^= input[i];
        }
        if (use32bits)
            return hash;
        else
            return (hash >> 30) ^ (hash & mask);
    }
}