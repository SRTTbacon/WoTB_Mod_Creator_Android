using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WoTB_Mod_Creater.Class;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.All_Page
{
    public class SE_Type_List
    {
        public string Name { get; set; }
        public string Name_Text => IsEnable ? Name + " : " + "有効" : Name + " : " + "無効";
        public bool IsEnable { get; set; }
        public int Index { get; set; }
        public Color Name_Color => !IsEnable ? Color.FromHex("#BFFF2C8C") : Color.Aqua;
        public List<string> Files = new List<string>();
        public SE_Type_List(string Name, int Index)
        {
            this.Name = Name;
            this.Index = Index;
            IsEnable = true;
        }
    }
    public partial class Create_Project : ContentPage
    {
        private readonly List<SE_Type_List> SE_Types = new List<SE_Type_List>();
        private SFTP_Client SFTPClient = null;
        private ViewCell SE_Type_LastCell = null;
        public int Get_Voice_Project_Exist = -1;
        private int Uploaded_IP = 0;
        private int Random_ID = 0;
        private long Upload_Full_Length = 0;
        public bool IsServerSending = false;
        private bool IsLoaded = false;
        private bool IsMessageShowing = false;
        private bool IsForceColorMode = false;
        public Create_Project()
        {
            InitializeComponent();
            Disable_B.Clicked += Disable_B_Click;
            Enable_B.Clicked += Enable_B_Click;
            Server_Create_B.Clicked += Server_Create_B_Click;
            Command_Copy_B.Clicked += Command_Copy_B_Clicked;
            SE_Types.Add(new SE_Type_List("時間切れ&占領ポイントMax", 0));
            SE_Types.Add(new SE_Type_List("クイックコマンド", 1));
            SE_Types.Add(new SE_Type_List("弾薬庫破損", 2));
            SE_Types.Add(new SE_Type_List("自車両大破", 3));
            SE_Types.Add(new SE_Type_List("貫通", 4));
            SE_Types.Add(new SE_Type_List("敵モジュール破損", 5));
            SE_Types.Add(new SE_Type_List("無線機破損", 6));
            SE_Types.Add(new SE_Type_List("燃料タンク破損", 7));
            SE_Types.Add(new SE_Type_List("非貫通", 8));
            SE_Types.Add(new SE_Type_List("装填完了", 9));
            SE_Types.Add(new SE_Type_List("第六感", 10));
            SE_Types.Add(new SE_Type_List("敵発見", 11));
            SE_Types.Add(new SE_Type_List("戦闘開始前タイマー", 12));
            SE_Types.Add(new SE_Type_List("ロックオン", 13));
            SE_Types.Add(new SE_Type_List("アンロック", 14));
            SE_Types.Add(new SE_Type_List("ノイズ音", 15));
            Set_SE_Type_L();
            Set_Button_State(-1);
            Command_T.Text = "作成待ち...";
        }
        private async void Command_Copy_B_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (Command_T.Text == "作成待ち...")
                {
                    Message_Feed_Out("先に'作成'ボタンを押して下さい。");
                    return;
                }
                await Clipboard.SetTextAsync(Command_T.Text);
                Message_Feed_Out("クリップボードにコピーしました。");
            }
            catch
            {
                Message_Feed_Out("コピーに失敗しました。");
            }
        }
        public void Set_Project_Name(string Project_Name)
        {
            if (string.IsNullOrWhiteSpace(Project_Name_T.Text))
                Project_Name_T.Text = Project_Name;
        }
        private async void Message_Feed_Out(string Message, int Seconds = 2)
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
                if (Number >= 60 * Seconds)
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
        private void Set_Button_State(int Index)
        {
            if (Index == -1)
            {
                Disable_B.BackgroundColor = Color.DarkGray;
                Disable_B.TextColor = Color.Gray;
                Enable_B.BackgroundColor = Color.DarkGray;
                Enable_B.TextColor = Color.Gray;
                return;
            }
            if (SE_Types[Index].IsEnable)
            {
                Disable_B.BackgroundColor = Color.Transparent;
                Disable_B.TextColor = Color.Aqua;
                Enable_B.BackgroundColor = Color.DarkGray;
                Enable_B.TextColor = Color.Gray;
            }
            else
            {
                Disable_B.BackgroundColor = Color.DarkGray;
                Disable_B.TextColor = Color.Gray;
                Enable_B.BackgroundColor = Color.Transparent;
                Enable_B.TextColor = Color.Aqua;
            }
        }
        private void Set_SE_Type_L()
        {
            SE_Type_L.ItemsSource = null;
            SE_Type_L.ItemsSource = SE_Types;
        }
        private void Set_SE_Type_Index(int Index)
        {
            if (SE_Types.Count - 1 < Index)
                return;
            SE_Type_L.SelectedItem = SE_Types[Index];
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = SE_Type_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(SE_Type_L);
                int Count = 0;
                foreach (ViewCell cell in (cells as ITemplatedItemsList<Cell>).Cast<ViewCell>())
                {
                    cell.View.BackgroundColor = cell.BindingContext != null && Count == Index ? Color.FromHex("#82bfc8") : Color.Transparent;
                    Count++;
                }
            }
        }
        private void SE_Type_ViewCell_Tapped(object sender, EventArgs e)
        {
            if (IsForceColorMode)
                Clear_SE_Type_Color();
            if (SE_Type_LastCell != null)
                SE_Type_LastCell.View.BackgroundColor = Color.Transparent;
            ViewCell viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#82bfc8");
                SE_Type_LastCell = viewCell;
            }
            IEnumerable<PropertyInfo> pInfos = SE_Type_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(SE_Type_L);
                int Count = 0;
                foreach (ViewCell cell in (cells as ITemplatedItemsList<Cell>).Cast<ViewCell>())
                {
                    if (cell == viewCell)
                    {
                        Set_Button_State(Count);
                        break;
                    }
                    Count++;
                }
            }
        }
        private void Clear_SE_Type_Color()
        {
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = SE_Type_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(SE_Type_L);
                foreach (ViewCell cell in (cells as ITemplatedItemsList<Cell>).Cast<ViewCell>())
                    if (cell.BindingContext != null)
                        cell.View.BackgroundColor = Color.Transparent;
            }
            IsForceColorMode = false;
        }
        private void Disable_B_Click(object sender, EventArgs e)
        {
            if (SE_Type_L.SelectedItem != null)
            {
                int SE_Type_Index = ((SE_Type_List)SE_Type_L.SelectedItem).Index;
                SE_Types[SE_Type_Index].IsEnable = false;
                Set_Button_State(SE_Type_Index);
                Set_SE_Type_L();
                Set_SE_Type_Index(SE_Type_Index);
                IsForceColorMode = true;
            }
        }
        private void Enable_B_Click(object sender, EventArgs e)
        {
            if (SE_Type_L.SelectedItem != null)
            {
                int SE_Type_Index = ((SE_Type_List)SE_Type_L.SelectedItem).Index;
                SE_Types[SE_Type_Index].IsEnable = true;
                Set_Button_State(SE_Type_Index);
                Set_SE_Type_L();
                Set_SE_Type_Index(SE_Type_Index);
                IsForceColorMode = true;
            }
        }
        private async void Server_Create_B_Click(object sender, EventArgs e)
        {
            if (Sub_Code.Other_Create_Window.Other_Create_Project.IsServerSending)
            {
                Message_Feed_Out("'その他のサウンドMod'でファイルをアップロード中です。しばらくお待ちください。");
                return;
            }
            if (!IsServerSending)
            {
                if (string.IsNullOrWhiteSpace(Project_Name_T.Text))
                {
                    Message_Feed_Out("エラー:保存名が入力されていないか、空白の可能性があります。");
                    return;
                }
                else if (Project_Name_T.Text.Contains("|"))
                {
                    Message_Feed_Out("保存名に'|'を使用することはできません。");
                    return;
                }
                else if (Project_Name_T.Text.Contains("\\") || Project_Name_T.Text.Contains("/"))
                {
                    Message_Feed_Out("保存名に使用不可な文字が含まれています。");
                    return;
                }
                else if (Sub_Code.Created_Mods_Window.IsVoiceBuilding)
                {
                    Message_Feed_Out("ビルド中のプロジェクトが存在します。時間を置いて再度実行してください。");
                    return;
                }
                else if (!Sub_Code.IsSafePath(Project_Name_T.Text, true))
                {
                    Message_Feed_Out("保存名に使用不可な文字が含まれています。");
                    return;
                }
                IsServerSending = true;
                if (Sub_Code.Main_Server.IsLogined)
                {
                    Get_Voice_Project_Exist = -1;
                    Sub_Code.Main_Server.TCP_Server.Send("Response|Get_Voice_Project_Exist|" + Sub_Code.UserName + "|" + Project_Name_T.Text);
                    Message_T.Text = "サーバーの応答を待っています...";
                    int Start_Time = Environment.TickCount;
                    bool NoResponse = false;
                    while (Get_Voice_Project_Exist == -1 && !NoResponse)
                    {
                        if (Start_Time + 5000 <= Environment.TickCount)
                            NoResponse = true;
                        await Task.Delay(50);
                    }
                    if (NoResponse)
                    {
                        Message_Feed_Out("サーバーが5秒以内に応答しませんでした。時間を置いて再度実行してください。");
                        IsServerSending = false;
                        return;
                    }
                    else if (Get_Voice_Project_Exist == 1)
                    {
                        Message_Feed_Out("指定した保存名は既に存在します。既に存在するModを削除するか、別の名前を指定してください。");
                        IsServerSending = false;
                        return;
                    }
                    IsMessageShowing = false;
                }
                Message_T.Text = "サウンドファイルをまとめています...";
                await Task.Delay(50);
                WVS_Save Save = new WVS_Save();
                Save.Add_Sound(Sub_Code.Voice_Create_Window.Voice_One_Full_Name, Sub_Code.Voice_Create_Window.WVS_File, 0);
                Save.Add_Sound(Sub_Code.Voice_Create_Window.Voice_Two_Full_Name, Sub_Code.Voice_Create_Window.WVS_File, 1);
                Save.Add_Sound(Sub_Code.Voice_Create_Window.Voice_Three_Full_Name, Sub_Code.Voice_Create_Window.WVS_File, 2);
                Random_ID = Sub_Code.r.Next(100000, 999999);
                Save.Create(Sub_Code.ExDir + "/Generate_Project.wvs", Project_Name_T.Text, false, Volume_Set_C.IsChecked, Default_Voice_Mode_C.IsChecked, SE_Types);
                Save.Dispose();
                if (Sub_Code.Main_Server.IsLogined)
                {
                    FileInfo info = new FileInfo(Sub_Code.ExDir + "/" + Random_ID + ".wvs");
                    Message_T.Text = "サウンドデータを送信しています...";
                    Upload_Full_Length = info.Length;
                    SFTPClient = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                    if (!SFTPClient.Directory_Exist("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Voice_Projects"))
                    {
                        _ = SFTPClient.Directory_Create("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName);
                        _ = SFTPClient.Directory_Create("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Voice_Projects");
                    }
                    SFTPClient.AsyncUploadFile(Sub_Code.ExDir + "/" + Random_ID + ".wvs", "/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Voice_Projects/" + Random_ID + ".wvs", false);
                    await File_Uploading();
                    string SE_Mode = "";
                    foreach (SE_Type_List SE in SE_Types)
                    {
                        if (SE_Mode == "")
                            SE_Mode = SE.IsEnable.ToString();
                        else
                            SE_Mode += "|" + SE.IsEnable;
                    }
                    string message = "Voice_Create|" + Sub_Code.UserName + "|" + Random_ID + "|" + Project_Name_T.Text + "|" + Volume_Set_C.IsChecked + "|" +
                            Default_Voice_Mode_C.IsChecked + "|" + SE_Mode;
                    Sub_Code.Main_Server.TCP_Server.Send(message);
                }
                else
                {
                    if (File.Exists(Sub_Code.ExDir + "/Uploaded_File_ID.dat"))
                    {
                        Message_T.Text = "アップロード済みのプロジェクトを削除しています...";
                        await Task.Delay(50);
                        StreamReader str = Sub_Code.File_Decrypt_To_Stream(Sub_Code.ExDir + "/Uploaded_File_ID.dat", "Google_Drive_Uploaded_ID");
                        string Line = str.ReadLine();
                        str.Close();
                        File.Delete(Sub_Code.ExDir + "/Uploaded_File_ID.dat");
                        Sub_Code.Drive.Delete_File(Line);
                    }
                    Message_T.Text = "プロジェクトファイルをアップロードしています...";
                    await Task.Delay(50);
                    Uploaded_IP = Sub_Code.r.Next(1000000, int.MaxValue);
                    if (Sub_Code.Drive.Upload_File(Sub_Code.ExDir + "/Generate_Project.wvs", out string File_ID, Uploaded_IP + ".wvs"))
                    {
                        Command_T.Text = ".swvs-id " + Uploaded_IP;
                        StreamWriter stw = File.CreateText(Sub_Code.ExDir + "/Temp_1010.dat");
                        stw.Write(File_ID);
                        stw.Close();
                        Sub_Code.File_Encrypt(Sub_Code.ExDir + "/Temp_1010.dat", Sub_Code.ExDir + "/Uploaded_File_ID.dat", "Google_Drive_Uploaded_ID", true);
                        Message_T.Text = "プロジェクトファイルをアップロードしました。表示されているコマンドを専用のDiscordサーバーに送信してください。";
                    }
                    else
                        Message_Feed_Out("アップロードに失敗しました。DiscordのBot(SRTT-Yuna)からビルドしてください。", 5);
                }
                IsServerSending = false;
            }
        }
        private async Task File_Uploading()
        {
            while (!SFTPClient.IsUploaded && SFTPClient.IsUploading)
            {
                if (IsServerSending)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        double Max_Stream_Length = Upload_Full_Length / 1024.0 / 1024.0;
                        double Now_Stream_Length = SFTPClient.Uploaded_Size / 1024.0 / 1024.0;
                        Max_Stream_Length = Math.Round(Max_Stream_Length, 1);
                        Now_Stream_Length = Math.Round(Now_Stream_Length, 1);
                        Message_T.Text = "サウンドデータを送信しています...\n" + Now_Stream_Length + " / " + Max_Stream_Length + "MB";
                    });
                }
                await Task.Delay(50);
            }
            if (IsServerSending)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SFTPClient.Close();
                    IsServerSending = false;
                    File.Delete(Sub_Code.ExDir + "/" + Random_ID + ".wvs");
                    Sub_Code.Created_Mods_Window.Add_Mod(0, Project_Name_T.Text, Random_ID, -1, false);
                    Message_Feed_Out("サーバーにデータを送信しました。音声Modのビルドを開始します...");
                });
            }
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                if (Sub_Code.Main_Server.IsLogined)
                    Sub_Code.Created_Mods_Window.Init();
            }
        }
    }
}