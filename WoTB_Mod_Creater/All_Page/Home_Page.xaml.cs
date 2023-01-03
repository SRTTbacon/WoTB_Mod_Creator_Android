using Xamarin.Forms;
using WoTB_Mod_Creater.Class;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Plugin.Permissions;
using System.Collections.Generic;
using System.Linq;
using WoTB_Mod_Creater.All_Page.Mod_Distribution;

namespace WoTB_Mod_Creater.All_Page
{
    public partial class Home_Page : ContentPage
    {
        private int IsBelowLowestVersion = 0;
        private bool IsInternetEnable = false;
        private bool IsMessageShowing = false;
        private bool IsPageOpen = false;
        private bool IsLoaded = false;
        private bool IsBassLoaded = false;
        private bool IsLogined = false;
        private readonly Music_Player Music_Player_Window = new Music_Player();
        private readonly Mod_Distribution_Page Mod_Window = new Mod_Distribution_Page();
        private readonly Tools Tools_Window = new Tools();
        private readonly List<string> SRTTbacon_Phone_ID = new List<string>();
        public Home_Page()
        {
            InitializeComponent();
            Voice_Create_B.Clicked += Voice_Create_B_Click;
            Other_Sound_B.Clicked += Other_Sound_B_Click;
            Create_Mod_Info_B.Clicked += Create_Mod_Info_B_Click;
            Music_Player_B.Clicked += Music_Player_B_Click;
            Mod_Distribution_B.Clicked += Mod_Distribution_B_Clicked;
            Tools_B.Clicked += Tools_B_Clicked;
            UseSelectPage_C.CheckedChanged += UseSelectPage_C_CheckedChanged;
            //自分の端末ではグローバルIPでサーバーに接続できないため、区別する
            string ID = DependencyService.Get<IAndroidSystem>().Get_Device_ID();
            IEnumerable<ConnectionProfile> profiles = Connectivity.ConnectionProfiles;
            SRTTbacon_Phone_ID.Add("c3cb9933bfa8480c");
            SRTTbacon_Phone_ID.Add("43a3e5629d1d2c66");
            Sub_Code.Error_Log_Write(ID);
            if (SRTTbacon_Phone_ID.Contains(ID) && profiles.Contains(ConnectionProfile.WiFi))
                SRTTbacon_Server.IsSRTTbaconOwnerMode = true;
            SRTTbacon_Server.IP = SRTTbacon_Server.IsSRTTbaconOwnerMode ? SRTTbacon_Server.IP_Local : SRTTbacon_Server.IP_Global;
            UseSelectPage_C.IsVisible = false;
            UseSelectPage_T.IsVisible = false;
        }
        private async void Message_Feed_Out(string Message)
        {
            if (IsMessageShowing)
            {
                IsMessageShowing = false;
                await Task.Delay(1000 / 59);
            }
            Message_T.Text = Message;
            IsMessageShowing = true;
            Message_T.Opacity = 1;
            int Number = 0;
            bool IsForce = false;
            while (Message_T.Opacity > 0)
            {
                if (!IsMessageShowing)
                {
                    IsForce = true;
                    break;
                }
                Number++;
                if (Number >= 120)
                    Message_T.Opacity -= 0.025;
                await Task.Delay(1000 / 60);
            }
            if (!IsForce)
            {
                IsMessageShowing = false;
                Message_T.Text = "";
                Message_T.Opacity = 1;
            }
        }
        private async void Voice_Create_B_Click(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            /*if (!IsInternetEnable)
            {
                Message_Feed_Out("インターネットに接続されていません。");
                return;
            }
            await Login();
            if (await CheckLoginMessage())
                return;
            else */if (!await Get_Permission_State())
            {
                Message_Feed_Out("ストレージへのアクセス許可をする必要があります。");
                return;
            }
            IsPageOpen = true;
            await Navigation.PushAsync(Sub_Code.Voice_Create_Window);
        }
        private async void Other_Sound_B_Click(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            if (!IsInternetEnable)
            {
                Message_Feed_Out("インターネットに接続されていません。");
                return;
            }
            await Login();
            if (await CheckLoginMessage())
                return;
            else if (!await Get_Permission_State())
            {
                Message_Feed_Out("ストレージへのアクセス許可をする必要があります。");
                return;
            }
            if (!File.Exists(Sub_Code.ExDir + "/UI_Battle_SE/176974408.mp3"))
            {
                Message_T.Text = "必要なファイルをダウンロードしています...(約1.5MB)";
                await Task.Delay(75);
                Sub_Code.Main_Server.SFTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/UI_Battle_SE.zip", Sub_Code.ExDir + "/UI_Battle_SE.zip");
                DependencyService.Get<IFileSystem>().UnZip(Sub_Code.ExDir + "/UI_Battle_SE.zip", Sub_Code.ExDir + "/UI_Battle_SE");
                File.Delete(Sub_Code.ExDir + "/UI_Battle_SE.zip");
                Message_Feed_Out("ダウンロードが完了しました。");
            }
            IsPageOpen = true;
            await Navigation.PushAsync(Sub_Code.Other_Create_Window);
        }
        private async void Create_Mod_Info_B_Click(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            if (!IsInternetEnable)
            {
                Message_Feed_Out("インターネットに接続されていません。");
                return;
            }
            await Login();
            if (await CheckLoginMessage())
                return;
            else if (!await Get_Permission_State())
            {
                Message_Feed_Out("ストレージへのアクセス許可をする必要があります。");
                return;
            }
            IsPageOpen = true;
            await Navigation.PushAsync(Sub_Code.Created_Mods_Window);
        }
        private async void Music_Player_B_Click(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            if (!await Get_Permission_State())
            {
                Message_Feed_Out("ストレージへのアクセス許可をする必要があります。");
                return;
            }
            IsPageOpen = true;
            await Navigation.PushAsync(Music_Player_Window);
        }
        private async void Mod_Distribution_B_Clicked(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            if (!IsInternetEnable)
            {
                Message_Feed_Out("インターネットに接続されていません。");
                return;
            }
            await Login();
            if (await CheckLoginMessage())
                return;
            else if (!await Get_Permission_State())
            {
                Message_Feed_Out("ストレージへのアクセス許可をする必要があります。");
                return;
            }
            if (!File.Exists(Sub_Code.ExDir + "/WEM_To_OGG.bin"))
            {
                Message_T.Text = "必要なファイルをダウンロードしています...(約70KB)";
                await Task.Delay(50);
                Sub_Code.Main_Server.SFTPClient.DownloadFile("/WoTB_Voice_Mod/Update/Wwise/packed_codebooks_aoTuV_603.bin", Sub_Code.ExDir + "/WEM_To_OGG.bin");
                Message_T.Text = "";
            }
            IsPageOpen = true;
            await Navigation.PushAsync(Mod_Window);
        }
        private async void Tools_B_Clicked(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            if (!await Get_Permission_State())
            {
                Message_Feed_Out("ストレージへのアクセス許可をする必要があります。");
                return;
            }
            IsPageOpen = true;
            await Navigation.PushAsync(Tools_Window);
        }
        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            IsPageOpen = false;
            if (!IsBassLoaded)
            {
                IsBassLoaded = true;
                try
                {
                    _ = Directory.GetDirectories("/storage/emulated/0", "*", SearchOption.TopDirectoryOnly);
                    Sub_Code.HasSystemPermission = true;
                }
                catch { }
                if (!Sub_Code.HasSystemPermission)
                {
                    UseSelectPage_C.IsVisible = false;
                    UseSelectPage_T.IsVisible = false;
                }
                Register_B.Clicked += Register_B_Click;
                Login_B.Clicked += Login_B_Click;
                NetworkAccess current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                    IsInternetEnable = true;
                try
                {
                    if (!Directory.Exists(Sub_Code.ExDir + "/Configs"))
                        Directory.CreateDirectory(Sub_Code.ExDir + "/Configs");
                    foreach (string WVS_File in Directory.GetFiles(Sub_Code.ExDir, "*.wvs", SearchOption.TopDirectoryOnly))
                        File.Delete(WVS_File);
                    foreach (string WMS_File in Directory.GetFiles(Sub_Code.ExDir, "*.wms", SearchOption.TopDirectoryOnly))
                        File.Delete(WMS_File);
                }
                catch { }
                if (File.Exists(Sub_Code.ExDir + "/User.dat"))
                    Hide_Login_Layout();
                Un4seen.Bass.Bass.BASS_Init(-1, 48000, Un4seen.Bass.BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                Sub_Code.Select_Files_Window.Selected += delegate (string Page_Name)
                {
                    if (Page_Name == "Music_Player")
                        Music_Player_Window.Selected_Files();
                    else if (Page_Name == "Other_Create")
                        Sub_Code.Other_Create_Window.Selected_Files();
                    else if (Page_Name == "Other_Create_Load")
                        Sub_Code.Other_Create_Window.Load_File();
                    else if (Page_Name == "Voice_Create")
                        Sub_Code.Voice_Create_Window.Selected_Files();
                    else if (Page_Name == "Voice_Create_Load")
                        Sub_Code.Voice_Create_Window.Load_File();
                    else if (Page_Name == "Mod_Upload")
                        Mod_Window.Mod_Upload_Window.Selected_Files();
                    else if (Page_Name == "DVPL_Unpack")
                        Tools_Window.UnPack_Selected_Files();
                    else if (Page_Name == "DVPL_Pack")
                        Tools_Window.Pack_Selected_Files();
                };
                if (Directory.Exists(Sub_Code.ExDir + "/Server/Download_Mods"))
                    foreach (string Dir in Directory.GetDirectories(Sub_Code.ExDir + "/Server/Download_Mods", "*", SearchOption.TopDirectoryOnly))
                        Directory.Delete(Dir, true);
                Message_T.Text = "ファイルを整理しています...";
                await Task.Delay(50);
                Sub_Code.Delete_Temp_Files();
                Sub_Code.Delete_Voice_Mods();
                Sub_Code.Delete_Other_Mods();
                Message_T.Text = "";
            }
        }
        private async void Register_B_Click(object sender, EventArgs e)
        {
            if (!IsInternetEnable)
            {
                Message_Feed_Out("インターネットに接続されていません。");
                return;
            }
            await Login();
            if (Sub_Code.Main_Server.Server_OK)
            {
                if (UserName_T.Text == null || UserName_T.Text == "")
                {
                    Message_Feed_Out("ユーザー名を空白にすることはできません。");
                    return;
                }
                if (UserName_T.Text.Length >= 21)
                {
                    Message_Feed_Out("ユーザー名が長すぎます。20文字以下にしてください。");
                    return;
                }
                if (UserName_T.Text.Contains("\n"))
                {
                    Message_Feed_Out("ユーザー名に改行は使用できません。");
                    return;
                }
                if (UserName_T.Text.CountOf("  ") >= 1)
                {
                    Message_Feed_Out("ユーザー名に空白を2つ連続で付けることはできません。");
                    return;
                }
                if (Sub_Code.Main_Server.UserExist(UserName_T.Text))
                {
                    Message_Feed_Out("そのユーザー名は既に登録されています。別のユーザー名でお試しください。");
                    return;
                }
                StreamWriter Chat = File.CreateText(Sub_Code.ExDir + "/Temp_User_Chat.dat");
                Chat.WriteLine("---ここで管理者と1:1でチャットできます。---");
                Chat.Close();
                _ = Sub_Code.Main_Server.SFTPClient.UploadFile(Sub_Code.ExDir + "/Temp_User_Chat.dat", "/WoTB_Voice_Mod/Users/" + Sub_Code.ExDir + "_Chat.dat");
                _ = Sub_Code.Main_Server.SFTPClient.File_Append("/WoTB_Voice_Mod/Accounts.dat", UserName_T.Text + ":" + Password_T.Text + "\n");
                StreamWriter stw = File.CreateText(Sub_Code.ExDir + "/Temp_User_Set.dat");
                stw.WriteLine(UserName_T.Text + ":" + Password_T.Text);
                stw.Close();
                _ = Sub_Code.File_Encrypt(Sub_Code.ExDir + "/Temp_User_Set.dat", Sub_Code.ExDir + "/User.dat", "SRTTbacon_Server_User_Pass_Save", true);
                if (Sub_Code.Main_Server.Login())
                {
                    Sub_Code.Main_Server.IsLogined = true;
                    IsLogined = true;
                    Message_Feed_Out("アカウントを登録しました。ご利用ありがとうございます！！！");
                    Sub_Code.Main_Server.TCP_Server.Send("Register|" + Sub_Code.UserName);
                }
            }
            else
                Message_Feed_Out("サーバーに接続されませんでした。時間を空けて再接続ボタンを押してみてください。");
        }
        private async void Login_B_Click(object sender, EventArgs e)
        {
            if (!IsInternetEnable)
            {
                Message_Feed_Out("インターネットに接続されていません。");
                return;
            }
            await Login();
            bool IsNotLogin = false;
            if (UserName_T.Text == null || UserName_T.Text == "")
                IsNotLogin = true;
            else if (UserName_T.Text.Length >= 21)
                IsNotLogin = true;
            else if (UserName_T.Text.Contains("\n"))
                IsNotLogin = true;
            else if (UserName_T.Text.CountOf("  ") >= 1)
                IsNotLogin = true;
            if (IsNotLogin)
            {
                Message_Feed_Out("ユーザー名またはパスワードが間違えています。");
                return;
            }
            if (Sub_Code.Main_Server.Server_OK)
            {
                if (Sub_Code.Main_Server.Account_Exist(UserName_T.Text, Password_T.Text))
                {
                    StreamWriter stw = File.CreateText(Sub_Code.ExDir + "/Temp_User_Set.dat");
                    stw.WriteLine(UserName_T.Text + ":" + Password_T.Text);
                    stw.Close();
                    _ = Sub_Code.File_Encrypt(Sub_Code.ExDir + "/Temp_User_Set.dat", Sub_Code.ExDir + "/User.dat", "SRTTbacon_Server_User_Pass_Save", true);
                    if (Sub_Code.Main_Server.Login())
                    {
                        Message_Feed_Out("ログインしました。ご利用頂きありがとうございます!!!");
                        if (!SRTTbacon_Server.IsSRTTbaconOwnerMode)
                            Sub_Code.Main_Server.TCP_Server.Send("AndroidLogin|" + Sub_Code.UserName + "|" + SRTTbacon_Server.Version);
                        IsLogined = true;
                        Hide_Login_Layout();
                    }
                }
                else
                    Message_Feed_Out("ユーザー名またはパスワードが間違えています。");
            }
            else
                Message_Feed_Out("サーバーに接続されませんでした。時間を空けて再接続ボタンを押してみてください。");
        }
        private void Hide_Login_Layout(bool IsShowMode = false)
        {
            if (!IsShowMode)
            {
                UserName_T.IsVisible = false;
                Password_T.IsVisible = false;
                Register_B.IsVisible = false;
                Login_B.IsVisible = false;
                if (Sub_Code.HasSystemPermission)
                {
                    UseSelectPage_C.IsVisible = true;
                    UseSelectPage_T.IsVisible = true;
                }
            }
            else
            {
                UserName_T.IsVisible = true;
                Password_T.IsVisible = true;
                Register_B.IsVisible = true;
                Login_B.IsVisible = true;
                UseSelectPage_C.IsVisible = false;
                UseSelectPage_T.IsVisible = false;
            }
        }
        private async Task<bool> CheckLoginMessage()
        {
            if (IsBelowLowestVersion == 1)
            {
                Message_Feed_Out("アプリをアップデートしてください。");
                return true;
            }
            if (!Sub_Code.Main_Server.IsLogined)
            {
                Message_Feed_Out("アカウント登録またはログインしてからお試しください。");
                Hide_Login_Layout(true);
                return true;
            }
            if (IsBelowLowestVersion == 0)
            {
                Message_T.Text = "サーバーの応答を待っています...";
                if (IsLogined)
                    Sub_Code.Main_Server.TCP_Server.Send("AndroidLogin|" + Sub_Code.UserName + "|" + SRTTbacon_Server.Version);
                int Start_Time = Environment.TickCount;
                bool NoResponse = false;
                while (IsBelowLowestVersion == 0 && !NoResponse)
                {
                    if (Start_Time + 5000 <= Environment.TickCount)
                        NoResponse = true;
                    await Task.Delay(50);
                }
                if (NoResponse)
                {
                    Message_Feed_Out("サーバーが5秒以内に応答しませんでした。時間を置いて再度実行してください。");
                    return true;
                }
                if (IsBelowLowestVersion == 1)
                {
                    Message_Feed_Out("アプリをアップデートしてください。");
                    return true;
                }
                Message_T.Text = "";
            }
            IsLogined = true;
            return !Sub_Code.Main_Server.IsLogined;
        }
        private async Task<bool> Get_Permission_State()
        {
            Plugin.Permissions.Abstractions.PermissionStatus status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
            if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                    return true;
            }
            else
                return true;
            return false;
        }
        private async Task Connect()
        {
            if (IsInternetEnable)
                await Sub_Code.Main_Server.Server_Connect();
            else
            {
                Message_Feed_Out("インターネットに接続されていません。接続後アプリを再起動してください。");
                return;
            }
            if (!IsLoaded)
            {
                IsLoaded = true;
                if (Sub_Code.Main_Server.TCP_Server.IsConnected)
                {
                    Sub_Code.Main_Server.TCP_Server.DataReceive += (msg) =>
                    {
                        try
                        {
                            if (Sub_Code.Main_Server.TCP_Server.IsConnected)
                            {
                                string[] Message_Temp = msg.Split('|');
                                if (Message_Temp[0] == "Public")
                                {
                                }
                                else if (Message_Temp[0] == Sub_Code.UserName + "_Private")
                                {
                                }
                                else if (Message_Temp[0].Contains("Response"))
                                {
                                    if (Message_Temp[1].Contains("Get_Voice_Project_Exist"))
                                        Sub_Code.Voice_Create_Window.Create_Project_Window.Get_Voice_Project_Exist = Message_Temp[2].Contains("True") ? 1 : 0;
                                    else if (Message_Temp[1].Contains("Get_Other_Project_Exist"))
                                        Sub_Code.Other_Create_Window.Other_Create_Project.Get_Other_Project_Exist = Message_Temp[2].Contains("True") ? 1 : 0;
                                    else if (Message_Temp[1].Contains("Voice_Create"))
                                        Sub_Code.Created_Mods_Window.Add_Mod(0, Path.GetFileName(Message_Temp[3]), int.Parse(Message_Temp[2]), long.Parse(Message_Temp[4]), true);
                                    else if (Message_Temp[1].Contains("Other_Create"))
                                        Sub_Code.Created_Mods_Window.Add_Mod(1, Path.GetFileName(Message_Temp[3]), int.Parse(Message_Temp[2]), long.Parse(Message_Temp[4]), true);
                                    else if (Message_Temp[1].Contains("Delete_Voice_Data"))
                                        Sub_Code.Created_Mods_Window.Delete_Mod(0, int.Parse(Message_Temp[2]));
                                    else if (Message_Temp[1].Contains("Delete_Other_Data"))
                                        Sub_Code.Created_Mods_Window.Delete_Mod(1, int.Parse(Message_Temp[2]));
                                    else if (Message_Temp[1].Contains("Below_Lowest_Version"))
                                    {
                                        bool IsBelow = bool.Parse(Message_Temp[2]);
                                        if (IsBelow)
                                            IsBelowLowestVersion = 2;
                                        else
                                            IsBelowLowestVersion = 1;
                                    }
                                }
                            }
                        }
                        catch (Exception e1)
                        {
                            Sub_Code.Error_Log_Write(e1.Message);
                        }
                    };
                }
            }
        }
        private async Task Login()
        {
            if (IsInternetEnable && !Sub_Code.Main_Server.TCP_Server.IsConnected)
                await Connect();
            if (IsInternetEnable && Sub_Code.Main_Server.TCP_Server.IsConnected && Sub_Code.UserName == "")
            {
                if (File.Exists(Sub_Code.ExDir + "/User.dat"))
                {
                    //失敗する可能性を考慮して5回繰り返す
                    for (int Number = 0; Number < 5; Number++)
                    {
                        if (Sub_Code.Main_Server.Login())
                        {
                            Hide_Login_Layout();
                            return;
                        }
                        if (Number == 4)
                            Hide_Login_Layout(true);
                    }
                }
            }
            else if (!IsInternetEnable)
                Message_Feed_Out("インターネットに接続されていません。");
            else if (!Sub_Code.Main_Server.TCP_Server.IsConnected)
                Message_Feed_Out("サーバーに接続できませんでした。");
        }
        private void UseSelectPage_C_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Sub_Code.IsUseSelectPage = e.Value;
        }
    }
}