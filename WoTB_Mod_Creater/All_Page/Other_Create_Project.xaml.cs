using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WoTB_Mod_Creater.Class;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.All_Page
{
    public class Other_Mod_Type_List
    {
        public string Mod_Name { get; private set; }
        public Color Mod_Color { get; private set; }
        public int Index { get; private set; }
        public Other_Mod_Type_List(string Mod_Name, Color Mod_Color, int Index)
        {
            this.Mod_Name = Mod_Name;
            this.Mod_Color = Mod_Color;
            this.Index = Index;
        }
    }
    public partial class Other_Create_Project : ContentPage
    {
        private readonly List<Other_Mod_Type_List> Other_Mod_Types = new List<Other_Mod_Type_List>();
        private SFTP_Client SFTPClient = null;
        private ViewCell Mod_Type_LastCell = null;
        private long Upload_Full_Length = 0;
        public int Get_Other_Project_Exist = -1;
        private int Random_ID = 0;
        private int Select_Index = -1;
        public bool IsServerSending = false;
        private bool IsMessageShowing = false;
        private bool IsLoaded = false;
        public Other_Create_Project()
        {
            InitializeComponent();
            Server_Create_B.Clicked += Server_Create_B_Clicked;
            Other_Mod_Types.Add(new Other_Mod_Type_List("すべて", Color.Red, 0));
            Other_Mod_Types.Add(new Other_Mod_Type_List("ロードBGM", Color.Aqua, 1));
            Other_Mod_Types.Add(new Other_Mod_Type_List("リザルト", Color.Aqua, 2));
            Other_Mod_Types.Add(new Other_Mod_Type_List("優勢", Color.Aqua, 3));
            Other_Mod_Types.Add(new Other_Mod_Type_List("被弾", Color.Aqua, 4));
            Other_Mod_Types.Add(new Other_Mod_Type_List("ガレージSE", Color.Aqua, 5));
            Other_Mod_Types.Add(new Other_Mod_Type_List("砲撃音", Color.Aqua, 6));
            Mod_Type_L.SelectedItem = null;
            Mod_Type_L.ItemsSource = Other_Mod_Types;
            Set_Voice_Type_Index(0);
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
        private void Mod_Type_ViewCell_Tapped(object sender, EventArgs e)
        {
            if (Mod_Type_LastCell != null)
                Mod_Type_LastCell.View.BackgroundColor = Color.Transparent;
            ViewCell viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#9982bfc8");
                Mod_Type_LastCell = viewCell;
            }
        }
        private void Set_Voice_Type_Index(int Index)
        {
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = Mod_Type_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            Mod_Type_L.SelectedItem = Other_Mod_Types[Index];
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(Mod_Type_L);
                int Count = 0;
                foreach (ViewCell cell in cells as ITemplatedItemsList<Cell>)
                {
                    if (cell.BindingContext != null && Count == Index)
                    {
                        cell.View.BackgroundColor = Color.FromHex("#82bfc8");
                        Mod_Type_LastCell = cell;
                        break;
                    }
                    else
                        cell.View.BackgroundColor = Color.Transparent;
                    Count++;
                }
            }
        }
        private async void Server_Create_B_Clicked(object sender, EventArgs e)
        {
            if (Sub_Code.Voice_Create_Window.Create_Project_Window.IsServerSending)
            {
                Message_Feed_Out("'音声Mod'でファイルをアップロード中です。しばらくお待ちください。");
                return;
            }
            if (!IsServerSending)
            {
                if (Mod_Type_L.SelectedItem == null)
                {
                    Message_Feed_Out("Modタイプが選択されていません。");
                    return;
                }
                Other_Mod_Type_List Type = (Other_Mod_Type_List)Mod_Type_L.SelectedItem;
                if (!IsExistSound(Type.Index))
                {
                    Message_Feed_Out("サウンドファイルが含まれていないため作成できません。");
                    return;
                }
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
                else if (Project_Name_T.Text.Contains("\\"))
                {
                    Message_Feed_Out("保存名に使用不可な文字が含まれています。");
                    return;
                }
                else if (Sub_Code.Created_Mods_Window.IsOtherBuilding)
                {
                    Message_Feed_Out("ビルド中のプロジェクトが存在します。時間を置いて再度実行してください。");
                    return;
                }
                IsServerSending = true;
                Get_Other_Project_Exist = -1;
                Select_Index = Type.Index;
                Sub_Code.Main_Server.TCP_Server.Send("Response|Get_Other_Project_Exist|" + Sub_Code.UserName + "|" + Project_Name_T.Text);
                Message_T.Text = "サーバーの応答を待っています...";
                int Start_Time = Environment.TickCount;
                bool NoResponse = false;
                while (Get_Other_Project_Exist == -1 && !NoResponse)
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
                else if (Get_Other_Project_Exist == 1)
                {
                    Message_Feed_Out("指定した保存名は既に存在します。既に存在するModを削除するか、別の名前を指定してください。");
                    IsServerSending = false;
                    return;
                }
                IsMessageShowing = false;
                Message_T.Text = "サウンドファイルをまとめています...";
                await Task.Delay(50);
                WMS_Save Save = new WMS_Save();
                Random_ID = Sub_Code.r.Next(100000, 999999);
                if (Select_Index == 0)
                    Save.Add_Sound(Sub_Code.Other_Create_Window.Other_List, Sub_Code.Other_Create_Window.WMS_File, WMS_Save.WMS_Save_Mode.All);
                else if (Select_Index == 1)
                    Save.Add_Sound(Sub_Code.Other_Create_Window.Other_List, Sub_Code.Other_Create_Window.WMS_File, WMS_Save.WMS_Save_Mode.Load_BGM);
                else if (Select_Index == 2)
                    Save.Add_Sound(Sub_Code.Other_Create_Window.Other_List, Sub_Code.Other_Create_Window.WMS_File, WMS_Save.WMS_Save_Mode.Result);
                else if (Select_Index == 3)
                    Save.Add_Sound(Sub_Code.Other_Create_Window.Other_List, Sub_Code.Other_Create_Window.WMS_File, WMS_Save.WMS_Save_Mode.Dominance);
                else if (Select_Index == 4)
                    Save.Add_Sound(Sub_Code.Other_Create_Window.Other_List, Sub_Code.Other_Create_Window.WMS_File, WMS_Save.WMS_Save_Mode.Hit);
                else if (Select_Index == 5)
                    Save.Add_Sound(Sub_Code.Other_Create_Window.Other_List, Sub_Code.Other_Create_Window.WMS_File, WMS_Save.WMS_Save_Mode.Garage);
                else if (Select_Index == 6)
                    Save.Add_Sound(Sub_Code.Other_Create_Window.Other_List, Sub_Code.Other_Create_Window.WMS_File, WMS_Save.WMS_Save_Mode.Gun);
                Save.Create(Sub_Code.ExDir + "/" + Random_ID + ".wms", Project_Name_T.Text, true);
                Save.Dispose();
                FileInfo info = new FileInfo(Sub_Code.ExDir + "/" + Random_ID + ".wms");
                Message_T.Text = "サウンドデータを送信しています...";
                Upload_Full_Length = info.Length;
                SFTPClient = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                if (!SFTPClient.Directory_Exist("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Other_Projects"))
                {
                    _ = SFTPClient.Directory_Create("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName);
                    _ = SFTPClient.Directory_Create("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Other_Projects");
                }
                SFTPClient.AsyncUploadFile(Sub_Code.ExDir + "/" + Random_ID + ".wms", "/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Other_Projects/" + Random_ID + ".wms", false);
                await File_Uploading();
                Sub_Code.Main_Server.TCP_Server.Send("Other_Create|" + Sub_Code.UserName + "|" + Random_ID + "|" + Project_Name_T.Text + "|" + Volume_Set_C.IsChecked + " | " + Select_Index);
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
                    File.Delete(Sub_Code.ExDir + "/" + Random_ID + ".wms");
                    Sub_Code.Created_Mods_Window.Add_Mod(1, Project_Name_T.Text, Random_ID, -1, false);
                    Message_Feed_Out("サーバーにデータを送信しました。Modのビルドを開始します...");
                });
            }
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                Sub_Code.Created_Mods_Window.Init();
            }
        }
        private bool IsExistSound(int Index)
        {
            bool IsExist = false;
            List<List<Other_Type_List>> Temp = Sub_Code.Other_Create_Window.Other_List;
            if (Index == 0)
            {
                foreach (List<Other_Type_List> Type_01 in Temp)
                {
                    foreach (Other_Type_List Type_02 in Type_01)
                    {
                        if (Type_02.Files.Count > 0)
                        {
                            IsExist = true;
                            break;
                        }
                    }
                    if (IsExist)
                        break;
                }
            }
            else if (Index == 1)
            {
                foreach (Other_Type_List Type_02 in Temp[0])
                {
                    if (Type_02.Files.Count > 0 && Type_02.Index <= 10)
                    {
                        IsExist = true;
                        break;
                    }
                }
            }
            else if (Index == 2)
            {
                foreach (Other_Type_List Type_02 in Temp[0])
                {
                    if (Type_02.Files.Count > 0 && Type_02.Index >= 11 && Type_02.Index <= 16)
                    {
                        IsExist = true;
                        break;
                    }
                }
            }
            else if (Index == 3)
            {
                if (Temp[0][17].Files.Count > 0 || Temp[0][18].Files.Count > 0)
                    IsExist = true;
            }
            else if (Index == 4)
            {
                if (Temp[0][19].Files.Count > 0 || Temp[0][20].Files.Count > 0)
                    IsExist = true;
            }
            else if (Index == 5)
            {
                foreach (Other_Type_List Type_02 in Temp[1])
                {
                    if (Type_02.Files.Count > 0)
                    {
                        IsExist = true;
                        break;
                    }
                }
            }
            else if (Index == 6)
            {
                foreach (Other_Type_List Type_02 in Temp[2])
                {
                    if (Type_02.Files.Count > 0)
                    {
                        IsExist = true;
                        break;
                    }
                }
            }
            return IsExist;
        }
    }
}