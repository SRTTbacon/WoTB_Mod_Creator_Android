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
    public class Type_Mods_List
    {
        public string Name { get; private set; }
        public string Name_Text => IsBuilt ? Name : Name + "(ビルド中)";
        public Color Text_Color => IsBuilt ? Color.Aqua : Color.FromHex("#82bfc8");
        public int ID { get; private set; }
        public int ModType { get; private set; }
        public long Size { get; private set; }
        public bool IsBuilt = true;
        public Type_Mods_List(string Name, int ID, long Size, bool IsBuilt, int ModType)
        {
            this.Name = Name;
            this.ID = ID;
            this.Size = Size;
            this.IsBuilt = IsBuilt;
            this.ModType = ModType;
        }
    }
    public partial class Created_Mods_Page : ContentPage
    {
        public readonly List<List<Type_Mods_List>> Type_Mods = new List<List<Type_Mods_List>>();
        public bool IsLoaded = false;
        public bool IsVoiceBuilding = false;
        public bool IsOtherBuilding = false;
        private ViewCell Mods_List_LastCell = null;
        private int Page = 0;
        private bool IsMessageShowing = false;
        private bool IsBusyMode = false;
        private bool IsForceColorMode = false;
        public Created_Mods_Page()
        {
            InitializeComponent();
            Select_Type_B.Clicked += Select_Type_B_Click;
            Download_Install_B.Clicked += Download_Install_B_Click;
            Delete_B.Clicked += Delete_B_Click;
            Download_P.IsVisible = false;
            Download_T.IsVisible = false;
            Type_Mods.Add(new List<Type_Mods_List>());
            Type_Mods.Add(new List<Type_Mods_List>());
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
        private async void Select_Type_B_Click(object sender, EventArgs e)
        {
            if (IsBusyMode)
                return;
            string[] Set_Types = { "音声Mod", "その他のサウンドMod" };
            string Name = await DisplayActionSheet("表示タイプを選択してください。", "キャンセル", null, Set_Types);
            if (Name == null || Name == "キャンセル")
                return;
            if (Name == "音声Mod")
                Page = 0;
            else if (Name == "その他のサウンドMod")
                Page = 1;
            Select_Type_B.Text = "表示:" + Name;
            Set_Type_Mods();
        }
        private void Set_Type_Mods()
        {
            Mods_List.ItemsSource = null;
            Mods_List.SelectedItem = null;
            if (Type_Mods[Page].Count > 0)
                Mods_List.ItemsSource = Type_Mods[Page];
            Mod_Count_T.Text = "Modリスト:" + Type_Mods[Page].Count + "個";
            Set_Mod_Visible(false);
        }
        private void Mods_List_Tapped(object sender, EventArgs e)
        {
            if (IsForceColorMode)
                Clear_Mods_List_Color();
            if (Mods_List_LastCell != null)
                Mods_List_LastCell.View.BackgroundColor = Color.Transparent;
            ViewCell viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#82bfc8");
                Mods_List_LastCell = viewCell;
                Type_Mods_List Temp = (Type_Mods_List)Mods_List.SelectedItem;
                if (Temp.Size == -1)
                    Mod_Info_T.Text = "保存名:" + Temp.Name + " / サイズ:計測不可(ビルド中)";
                else
                {
                    double Now_Stream_Length = Math.Round(Temp.Size / 1024.0 / 1024.0, 1);
                    Mod_Info_T.Text = "保存名:" + Temp.Name + " / サイズ:" + Now_Stream_Length + "MB";
                }
            }
            Set_Mod_Visible(true);
        }
        private void Clear_Mods_List_Color()
        {
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = Mods_List_LastCell.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(Mods_List_LastCell);
                foreach (ViewCell cell in cells as ITemplatedItemsList<Cell>)
                    if (cell.BindingContext != null)
                        cell.View.BackgroundColor = Color.Transparent;
            }
            IsForceColorMode = false;
        }
        public void Add_Mod(int Page, string Name, int ID, long Size, bool IsBuilt)
        {
            if (IsLoaded)
            {
                bool IsExist = false;
                for (int Number = 0; Number < Type_Mods[Page].Count; Number++)
                {
                    if (Type_Mods[Page][Number].ID == ID)
                    {
                        Type_Mods[Page][Number] = new Type_Mods_List(Name, ID, Size, IsBuilt, Page);
                        IsExist = true;
                        break;
                    }
                }
                if (!IsExist)
                {
                    Type_Mods[Page].Add(new Type_Mods_List(Name, ID, Size, IsBuilt, Page));
                    if (IsBuilt)
                        Message_Feed_Out("保存名:" + Name + "のビルド作業が完了しました。リストに追加します。");
                    else
                        Message_Feed_Out("保存名:" + Name + "のビルド作業を開始します。");
                }
                else if (IsBuilt)
                    Message_Feed_Out("保存名:" + Name + "のビルド作業が完了しました。");
                else
                    Message_Feed_Out("保存名:" + Name + "のビルド作業を開始します。");
                Set_Type_Mods();
                ChangeBuildMode();
            }
        }
        private void Set_Mod_Visible(bool IsVisible)
        {
            Mod_Info_T.IsVisible = IsVisible;
            Download_Install_B.IsVisible = IsVisible;
            Delete_B.IsVisible = IsVisible;
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            Init();
        }
        public void Init()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                if (!Sub_Code.Main_Server.SFTPClient.IsConnected)
                    return;
                IsBusyMode = true;
                Message_T.Text = "サーバーから情報を取得しています...";
                if (Sub_Code.Main_Server.SFTPClient.File_Exist("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Voice_Projects/Voice_Projects_List.txt"))
                {
                    StreamReader str = Sub_Code.Main_Server.SFTPClient.GetFileRead("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Voice_Projects/Voice_Projects_List.txt");
                    while (!str.EndOfStream)
                    {
                        string Line = str.ReadLine();
                        if (Line.Contains("|"))
                        {
                            string[] Temp = Line.Split('|');
                            string Front = Temp[0];
                            string Back = Temp[1];
                            long Size = long.Parse(Temp[2]);
                            Type_Mods[0].Add(new Type_Mods_List(Back, int.Parse(Front), Size, true, 0));
                        }
                    }
                    str.Close();
                }
                if (Sub_Code.Main_Server.SFTPClient.File_Exist("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Voice_Projects/Voice_Creating_List.txt"))
                {
                    StreamReader str = Sub_Code.Main_Server.SFTPClient.GetFileRead("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Voice_Projects/Voice_Creating_List.txt");
                    while (!str.EndOfStream)
                    {
                        string Line = str.ReadLine();
                        if (Line.Contains("|"))
                        {
                            string[] Temp = Line.Split('|');
                            string Front = Temp[0];
                            string Back = Temp[1];
                            Type_Mods[0].Add(new Type_Mods_List(Back, int.Parse(Front), -1, false, 0));
                            IsVoiceBuilding = true;
                        }
                    }
                    str.Close();
                }
                if (Sub_Code.Main_Server.SFTPClient.File_Exist("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Other_Projects/Other_Projects_List.txt"))
                {
                    StreamReader str = Sub_Code.Main_Server.SFTPClient.GetFileRead("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Other_Projects/Other_Projects_List.txt");
                    while (!str.EndOfStream)
                    {
                        string Line = str.ReadLine();
                        if (Line.Contains("|"))
                        {
                            string[] Temp = Line.Split('|');
                            string Front = Temp[0];
                            string Back = Temp[1];
                            long Size = long.Parse(Temp[2]);
                            Type_Mods[1].Add(new Type_Mods_List(Back, int.Parse(Front), Size, true, 1));
                            IsOtherBuilding = true;
                        }
                    }
                    str.Close();
                }
                if (Sub_Code.Main_Server.SFTPClient.File_Exist("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Other_Projects/Other_Creating_List.txt"))
                {
                    StreamReader str = Sub_Code.Main_Server.SFTPClient.GetFileRead("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Other_Projects/Other_Creating_List.txt");
                    while (!str.EndOfStream)
                    {
                        string Line = str.ReadLine();
                        if (Line.Contains("|"))
                        {
                            string[] Temp = Line.Split('|');
                            string Front = Temp[0];
                            string Back = Temp[1];
                            Type_Mods[1].Add(new Type_Mods_List(Back, int.Parse(Front), -1, false, 1));
                        }
                    }
                    str.Close();
                }
                Set_Type_Mods();
                Message_T.Text = "";
                IsBusyMode = false;
            }
            if (!Sub_Code.Main_Server.SFTPClient.File_Exist("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Voice_Projects/Voice_Creating_List.txt"))
            {
                IsVoiceBuilding = false;
                for (int Number = 0; Number < Type_Mods[0].Count; Number++)
                    if (!Type_Mods[0][Number].IsBuilt)
                        Type_Mods[0][Number].IsBuilt = true;
            }
        }
        private async void Download_Install_B_Click(object sender, EventArgs e)
        {
            if (IsBusyMode || Mods_List.SelectedItem == null)
                return;
            Type_Mods_List Temp = (Type_Mods_List)Mods_List.SelectedItem;
            if (!Temp.IsBuilt)
            {
                Message_Feed_Out("ビルド中のため、インストールすることはできません。");
                return;
            }
            bool IsOK = await DisplayAlert("通信環境によっては時間がかかります。続行しますか?", null, "はい", "いいえ");
            if (IsOK)
            {
                IsMessageShowing = false;
                IsBusyMode = true;
                Message_T.Text = "Modファイルをダウンロードしています...";
                string Project_Dir = "/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Voice_Projects/" + Temp.ID;
                if (Page == 1)
                    Project_Dir = "/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Other_Projects/" + Temp.ID;
                if (!Directory.Exists(Sub_Code.ExDir + "/Downloads"))
                    Directory.CreateDirectory(Sub_Code.ExDir + "/Downloads");
                string[] Files = { };
                if (Page == 0)
                    Files = new []{ "reload.bnk", "ui_battle.bnk", "ui_battle_basic.bnk", "ui_chat_quick_commands.bnk", "voiceover_crew.bnk" };
                else if (Page == 1)
                    Files = new []{ "music_maps_america_lakville.bnk", "music_maps_america_overlord.bnk", "music_maps_chinese.bnk", "music_maps_desert_airfield.bnk",
                    "music_maps_desert_sand_river.bnk", "music_maps_europe_himmelsdorf.bnk","music_maps_europe_mannerheim.bnk","music_maps_europe_ruinberg.bnk","music_maps_japan.bnk",
                    "music_result_screen.bnk", "music_result_screen_basic.bnk", "music_battle.bnk", "hits.bnk", "hits_basic.bnk", "ui_buttons_tasks.bnk", "weapon.bnk", "weapon_basic.bnk" };
                Download_P.IsVisible = true;
                Download_T.IsVisible = true;
                foreach (string File_Now in Files)
                {
                    Sub_Code.File_Delete(Sub_Code.ExDir + "/Downloads/" + File_Now + ".dvpl");
                    if (Sub_Code.Main_Server.SFTPClient.File_Exist(Project_Dir + "/" + File_Now + ".dvpl"))
                    {
                        long File_Size_Full = Sub_Code.Main_Server.SFTPClient.GetFileSize(Project_Dir + "/" + File_Now + ".dvpl");
                        Message_T.Text = File_Now + ".dvplをダウンロードしています...";
                        Task task = Task.Run(() =>
                        {
                            Sub_Code.Main_Server.SFTPClient.DownloadFile(Project_Dir + "/" + File_Now + ".dvpl", Sub_Code.ExDir + "/Downloads/" + File_Now + ".dvpl");
                        });
                        while (true)
                        {
                            long File_Size_Now = 0;
                            if (File.Exists(Sub_Code.ExDir + "/Downloads/" + File_Now + ".dvpl"))
                            {
                                FileInfo fi = new FileInfo(Sub_Code.ExDir + "/Downloads/" + File_Now + ".dvpl");
                                File_Size_Now = fi.Length;
                            }
                            double Download_Percent = (double)File_Size_Now / File_Size_Full;
                            int Percent_INT = (int)Math.Round(Download_Percent * 100.0, MidpointRounding.AwayFromZero);
                            Download_P.Progress = Download_Percent;
                            Download_T.Text = "進捗:" + Percent_INT + "%";
                            if (File_Size_Now >= File_Size_Full)
                            {
                                Download_P.Progress = 0;
                                Download_T.Text = "進捗:0%";
                                break;
                            }
                            await Task.Delay(100);
                        }
                    }
                }
                Download_P.IsVisible = false;
                Download_T.IsVisible = false;
                if (Install_C.IsChecked)
                    Install_Mod();
                else
                    Message_Feed_Out("ダウンロードが完了しました。フォルダを確認し手動で導入してください。");
            }
        }
        private void Install_Mod()
        {
            Message_T.Text = "WoTBに適応しています...";
            string To_Dir = Sub_Code.WoTBDir;
            if (Sub_Code.WoTBDir == "" && !Directory.Exists("/storage/emulated/0/android/data/net.wargaming.wot.blitz/files/packs"))
            {
                Message_Feed_Out("ダウンロードが完了しました。この端末では手動で導入する必要があります。");
                IsBusyMode = false;
                return;
            }
            else if (Sub_Code.WoTBDir == "")
                To_Dir = "/storage/emulated/0/android/data/net.wargaming.wot.blitz/files";
            try
            {
                bool IsExist = false;
                if (Page == 0)
                {
                    string[] Files = { "reload.bnk", "ui_battle.bnk", "ui_battle_basic.bnk", "ui_chat_quick_commands.bnk", "voiceover_crew.bnk" };
                    foreach (string Now in Files)
                    {
                        if (File.Exists(Sub_Code.ExDir + "/Downloads/" + Now + ".dvpl"))
                        {
                            Sub_Code.File_Delete(To_Dir + "/packs/WwiseSound/" + Now);
                            if (Now == "voiceover_crew.bnk")
                                File.Copy(Sub_Code.ExDir + "/Downloads/" + Now + ".dvpl", Sub_Code.WoTBDir + "/packs/WwiseSound/ja/" + Now + ".dvpl", true);
                            else
                                File.Copy(Sub_Code.ExDir + "/Downloads/" + Now + ".dvpl", Sub_Code.WoTBDir + "/packs/WwiseSound/" + Now + ".dvpl", true);
                            Sub_Code.File_Delete(Sub_Code.ExDir + "/Downloads/" + Now + ".dvpl");
                            IsExist = true;
                        }
                    }
                }
                else if (Page == 1)
                {
                    string[] Files = { "music_maps_america_lakville.bnk", "music_maps_america_overlord.bnk", "music_maps_chinese.bnk", "music_maps_desert_airfield.bnk",
                    "music_maps_desert_sand_river.bnk", "music_maps_europe_himmelsdorf.bnk","music_maps_europe_mannerheim.bnk","music_maps_europe_ruinberg.bnk","music_maps_japan.bnk",
                    "music_result_screen.bnk", "music_result_screen_basic.bnk", "music_battle.bnk", "hits.bnk", "hits_basic.bnk", "ui_buttons_tasks.bnk", "weapon.bnk", "weapon_basic.bnk" };
                    foreach (string Now in Files)
                    {
                        if (File.Exists(Sub_Code.ExDir + "/Downloads/" + Now + ".dvpl"))
                        {
                            Sub_Code.File_Delete(To_Dir + "/packs/WwiseSound/" + Now);
                            File.Copy(Sub_Code.ExDir + "/Downloads/" + Now + ".dvpl", Sub_Code.WoTBDir + "/packs/WwiseSound/" + Now + ".dvpl", true);
                            Sub_Code.File_Delete(Sub_Code.ExDir + "/Downloads/" + Now + ".dvpl");
                            IsExist = true;
                        }
                    }
                }
                if (IsExist)
                    Message_Feed_Out("WoTBにModをインストールしました。");
                else
                    Message_Feed_Out("不明なエラーが発生しました。開発者へご連絡ください。");
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
                Message_Feed_Out("ダウンロードが完了しました。この端末では手動で導入する必要があります。");
            }
            IsBusyMode = false;
        }
        private async void Delete_B_Click(object sender, EventArgs e)
        {
            if (IsBusyMode || Mods_List.SelectedItem == null)
                return;
            Type_Mods_List Temp = (Type_Mods_List)Mods_List.SelectedItem;
            string Name = Temp.Name;
            if (!Temp.IsBuilt)
            {
                Message_Feed_Out("ビルド中は削除できません。");
                return;
            }
            bool IsOK = await DisplayAlert("選択中の項目を削除しますか?", null, "はい", "いいえ");
            if (IsOK)
            {
                Type_Mods[Page].Remove(Temp);
                Set_Type_Mods();
                Set_Mod_Visible(false);
                if (Page == 0)
                    Sub_Code.Main_Server.TCP_Server.Send("Response|Delete_Voice_Project|" + Sub_Code.UserName + "|" + Temp.ID + "|" + Temp.Name);
                else if (Page == 1)
                    Sub_Code.Main_Server.TCP_Server.Send("Response|Delete_Other_Project|" + Sub_Code.UserName + "|" + Temp.ID + "|" + Temp.Name);
                ChangeBuildMode();
                Message_Feed_Out("'" + Name + "'を削除しました。");
            }
        }
        public void Delete_Mod(int Mod_Page, int ID)
        {
            string Name = "";
            foreach (Type_Mods_List Info in Type_Mods[Mod_Page])
            {
                if (Info.ID == ID)
                {
                    Name = Info.Name;
                    Type_Mods[Page].Remove(Info);
                    break;
                }
            }
            Set_Type_Mods();
            Set_Mod_Visible(false);
            ChangeBuildMode();
            if (Name != "")
                Message_Feed_Out("ビルドに失敗したため、'" + Name + "'を削除しました。");
        }
        private void ChangeBuildMode()
        {
            IsVoiceBuilding = false;
            IsOtherBuilding = false;
            for (int Number = 0; Number < Type_Mods[0].Count; Number++)
            {
                if (!Type_Mods[0][Number].IsBuilt)
                {
                    IsVoiceBuilding = true;
                    break;
                }
            }
            for (int Number = 0; Number < Type_Mods[1].Count; Number++)
            {
                if (!Type_Mods[1][Number].IsBuilt)
                {
                    IsOtherBuilding = true;
                    break;
                }
            }
        }
    }
}