using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using WoTB_Mod_Creater.Class;
using WoTB_Mod_Creater.Class.Wwise;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.All_Page.Mod_Distribution
{
	public class Mod_Type_List
    {
        public string Name_Text { get; set; }
        public double Mod_Size = 0;
        public Mod_Type_List(string Name)
        {
            Name_Text = Name;
        }
    }
	public partial class Mod_Distribution_Page : ContentPage
	{
        public readonly Mod_Upload Mod_Upload_Window = new Mod_Upload();
        private List<Mod_Type_List> Mod_List = new List<Mod_Type_List>();
        private ViewCell Mod_List_LastCell = null;
        private Mod_Type_List Select_Mod = null;
        private Wwise_File_Extract_V2 Wwise_Bnk = null;
        private string Mod_Select_Name = "";
        private int Stream = 0;
        private int Sound_Max_Index = 0;
        private int Sound_Select_Now = -1;
        private bool IsMessageShowing = false;
        private bool IsDownloading = false;
        private bool IsPageOpen = false;
        public Mod_Distribution_Page()
		{
            InitializeComponent();
            Download_B.Clicked += Download_B_Clicekd;
            Install_B.Clicked += Install_B_Clicked;
            Back_B.Clicked += Back_B_Clicked;
            Random_Play_B.Clicked += Random_Play_B_Clicked;
            Stop_B.Clicked += Stop_B_Clicked;
            Play_B.Clicked += Play_B_Clicked;
            Mod_Upload_B.Clicked += Mod_Upload_B_Clicked;
            Mod_Change_B.Clicked += Mod_Change_B_Clicked;
            Mod_L.ItemSelected += Mod_L_ItemSelected;
            Sound_Index_S.ValueChanged += Sound_Index_S_ValueChanged;
            Sound_Volume_S.ValueChanged += Sound_Volume_S_ValueChanged;
            Change_Control_Visibility(false);
            Download_P.IsVisible = false;
            Download_T.IsVisible = false;
            Sound_Volume_S.Value = 75;
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
        private void Clear_Mod_L_Color()
        {
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = Mod_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(Mod_L);
                foreach (ViewCell cell in cells as ITemplatedItemsList<Cell>)
                    if (cell.BindingContext != null)
                        cell.View.BackgroundColor = Color.Transparent;
            }
            Mod_L.SelectedItem = null;
        }
        //配布されているModを更新
        private void Mod_List_Update()
        {
            Mod_L.ItemsSource = null;
            Mod_L.SelectedItem = null;
            List<string> Mods_Read = Sub_Code.Main_Server.SFTPClient.GetFileLines("/WoTB_Voice_Mod/Mods/Mod_Names_Wwise.dat");
            Mods_Read.Sort();
            Mod_List.Clear();
            foreach (string Line in Mods_Read)
                Mod_List.Add(new Mod_Type_List(Line));
            Mods_Read.Clear();
            Mod_L.ItemsSource = Mod_List;
            if (Mod_List.Count == 0)
                Message_T.Text = "現在配布されているModはありません。";
            else if (Message_T.Text == "現在配布されているModはありません。")
                Message_T.Text = "";
        }
        private void Mod_List_Tapped(object sender, EventArgs e)
        {
            if (!IsDownloading)
            {
                if (Mod_List_LastCell != null)
                    Mod_List_LastCell.View.BackgroundColor = Color.Transparent;
                ViewCell viewCell = (ViewCell)sender;
                if (viewCell.View != null)
                {
                    viewCell.View.BackgroundColor = Color.FromHex("#82bfc8");
                    Mod_List_LastCell = viewCell;
                }
            }
            else
            {
                ViewCell viewCell = (ViewCell)sender;
                if (viewCell.View != null)
                {
                    viewCell.View.BackgroundColor = Color.Transparent;
                    if (Mod_List_LastCell != null)
                        Mod_List_LastCell.View.BackgroundColor = Color.FromHex("#82bfc8");
                }
                Mod_L.SelectedItem = Select_Mod;
            }
        }
        private void Mod_L_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null && !IsDownloading)
            {
                Message_T.Text = "";
                string Bank_Name = ((Mod_Type_List)e.SelectedItem).Name_Text;
                if (Mod_Select_Name != "")
                {
                    //.bnkファイルが選択されたらサウンドを読み込む
                    try
                    {
                        Bass.BASS_ChannelStop(Stream);
                        if (Wwise_Bnk != null)
                            Wwise_Bnk.Bank_Clear();
                        string Dir = Sub_Code.ExDir + "/Download_Mods/" + Mod_Select_Name;
                        if (Path.GetExtension(Dir + "/" + Bank_Name) == ".dvpl")
                            Wwise_Bnk = new Wwise_File_Extract_V2(Dir + "/Temp/" + Bank_Name.Replace(".dvpl", ""));
                        else
                            Wwise_Bnk = new Wwise_File_Extract_V2(Dir + "/" + Bank_Name);
                        Sound_Select_Now = -1;
                        Sound_Max_Index = Wwise_Bnk.Wwise_Get_Numbers();
                        Sound_Index_S.Value = 0;
                        if (Sound_Max_Index == 1)
                            Sound_Index_S.Maximum = 0.1;
                        else
                            Sound_Index_S.Maximum = Sound_Max_Index - 1;
                        Sound_Index_T.Text = "1/" + Sound_Max_Index;
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                        Message_Feed_Out("エラー:ファイルを読み取れませんでした。");
                        return;
                    }
                }
                else
                {
                    //Modが選択されたら詳細を表示
                    if (!Sub_Code.Main_Server.SFTPClient.File_Exist("/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Configs.dat"))
                    {
                        Clear_Mod_L_Color();
                        Message_Feed_Out("選択したModは現在利用できません。");
                        return;
                    }
                    XDocument xml2 = XDocument.Load(Sub_Code.Main_Server.SFTPClient.GetFileRead("/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Configs.dat"));
                    XElement item2 = xml2.Element("Mod_Upload_Config");
                    if (bool.Parse(item2.Element("IsPassword").Value))
                    {
                        Password_T.Text = "";
                        Password_T.IsVisible = true;
                    }
                    else
                        Password_T.IsVisible = false;
                    string IsEnableR18 = bool.Parse(item2.Element("IsEnableR18").Value) ? "あり" : "なし";
                    string IsBGMMode = bool.Parse(item2.Element("IsBGMMode").Value) ? "あり" : "なし";
                    R18Mode_T.Text = "R18音声:" + IsEnableR18;
                    BGM_T.Text = "戦闘BGM:" + IsBGMMode;
                    Mod_Creator_T.Text = "配布者:" + item2.Element("UserName").Value;
                    Download_B.IsVisible = true;
                    R18Mode_T.IsVisible = true;
                    BGM_T.IsVisible = true;
                    Explanation_T.Text = item2.Element("Explanation").Value;
                    Mod_Change_ChangeEnable(Sub_Code.UserName == item2.Element("UserName").Value);
                    if (Explanation_T.Text == "")
                        Explanation_T.Text = "説明なし";
                    Explanation_Scrool.IsVisible = true;
                }
            }
        }
        private void Mod_Change_ChangeEnable(bool IsEnable)
        {
            Mod_Change_B.IsEnabled = IsEnable;
            Mod_Change_B.BorderColor = IsEnable ? Color.Aqua : Color.Gray;
            Mod_Change_B.TextColor = IsEnable ? Color.Aqua : Color.Gray;
            if (IsEnable)
                Mod_Change_B.BackgroundColor = Color.Transparent;
        }
        private void Mod_Upload_ChangeEnable(bool IsEnable)
        {
            Mod_Upload_B.IsEnabled = IsEnable;
            Mod_Upload_B.BorderColor = IsEnable ? Color.Aqua : Color.Gray;
            Mod_Upload_B.TextColor = IsEnable ? Color.Aqua : Color.Gray;
            if (IsEnable)
                Mod_Upload_B.BackgroundColor = Color.Transparent;
        }
        private void Change_Control_Visibility(bool IsModSelected)
        {
            Download_B.IsVisible = false;
            R18Mode_T.IsVisible = false;
            BGM_T.IsVisible = false;
            Password_T.IsVisible = false;
            Back_B.IsVisible = IsModSelected;
            Random_Play_B.IsVisible = IsModSelected;
            Stop_B.IsVisible = IsModSelected;
            Play_B.IsVisible = IsModSelected;
            Install_B.IsVisible = IsModSelected;
            Sound_Index_S.IsVisible = IsModSelected;
            Sound_Index_T.IsVisible = IsModSelected;
            Sound_Volume_S.IsVisible = IsModSelected;
            Sound_Volume_T.IsVisible = IsModSelected;
            Explanation_Scrool.IsVisible = false;
            Mod_Upload_ChangeEnable(!IsModSelected);
            Mod_Change_ChangeEnable(false);
        }
        private async void Sample_Download()
        {
            //選択しているModをサーバーからダウンロード
            try
            {
                string Bank_Name = ((Mod_Type_List)Mod_L.SelectedItem).Name_Text;
                if (!Sub_Code.Main_Server.SFTPClient.Directory_Exist("/WoTB_Voice_Mod/Mods/" + Bank_Name))
                {
                    Message_Feed_Out("Modが見つかりませんでした。削除された可能性があります。");
                    return;
                }
                if (!Directory.Exists(Sub_Code.ExDir + "/Download_Mods/" + Bank_Name) || Directory.GetFiles(Sub_Code.ExDir + "/Download_Mods/" + Bank_Name, "*", SearchOption.AllDirectories).Length == 0)
                {
                    if (Directory.Exists(Sub_Code.ExDir + "/Download_Mods/" + Bank_Name))
                        Directory.Delete(Sub_Code.ExDir + "/Download_Mods/" + Bank_Name, true);
                    Directory.CreateDirectory(Sub_Code.ExDir + "/Download_Mods/" + Bank_Name);
                    Select_Mod = (Mod_Type_List)Mod_L.SelectedItem;
                    IsDownloading = true;
                    AbsoluteLayout.SetLayoutBounds(Explanation_Scrool, new Rectangle(0.45, 0.69, 0.7, 0.15));
                    Message_T.Text = "ダウンロードを準備しています...";
                    Sub_Code.Main_Server.TCP_Server.Send("Message|" + Sub_Code.UserName + "->配布Mod:" + Bank_Name + "をダウンロードしています...");
                    await Task.Delay(50);
                    Download_P.Progress = 0;
                    Download_T.Text = "進捗:0%";
                    Download_P.IsVisible = true;
                    Download_T.IsVisible = true;
                    foreach (string File_Name in Sub_Code.Main_Server.SFTPClient.GetFiles("/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Files", false, false))
                    {
                        try
                        {
                            Message_T.Text = File_Name + "をダウンロードしています...";
                            long File_Size_Full = Sub_Code.Main_Server.SFTPClient.GetFileSize("/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Files/" + File_Name);
                            Task task = Task.Run(() =>
                            {
                                Sub_Code.Main_Server.SFTPClient.DownloadFile("/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Files/" + File_Name, Sub_Code.ExDir + "/Download_Mods/" + Bank_Name + "/" + File_Name);
                            });
                            while (true)
                            {
                                long File_Size_Now = 0;
                                if (File.Exists(Sub_Code.ExDir + "/Download_Mods/" + Bank_Name + "/" + File_Name))
                                {
                                    FileInfo fi = new FileInfo(Sub_Code.ExDir + "/Download_Mods/" + Bank_Name + "/" + File_Name);
                                    File_Size_Now = fi.Length;
                                }
                                double Download_Percent = (double)File_Size_Now / File_Size_Full * 100;
                                int Percent_INT = (int)Math.Round(Download_Percent, MidpointRounding.AwayFromZero);
                                Download_P.Progress = Percent_INT / 100.0;
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
                        catch (Exception e)
                        {
                            Sub_Code.Error_Log_Write(e.Message);
                        }
                    }
                    //Voice_Set.FTP_Server.DownloadDirectory(Voice_Set.Special_Path + "/Download_Mods/" + Bank_Name, "/WoTB_Voice_Mod/Mods/" + Bank_Name + "/Files");
                    string Dir = Sub_Code.ExDir + "/Download_Mods/" + Bank_Name;
                    string[] Dir_Files = Directory.GetFiles(Dir, "*.dvpl", SearchOption.TopDirectoryOnly);
                    foreach (string Files in Dir_Files)
                    {
                        Directory.CreateDirectory(Dir + "/Temp");
                        DVPL.DVPL_UnPack(Files, Dir + "/Temp/" + Path.GetFileName(Files).Replace(".dvpl", ""), false);
                    }
                    Download_P.IsVisible = false;
                    Download_T.IsVisible = false;
                    IsDownloading = false;
                    Message_Feed_Out(Bank_Name + "をダウンロードしました。");
                }
                Mod_Select_Name = Bank_Name;
                Mod_L.SelectedItem = null;
                Mod_L.ItemsSource = null;
                Mod_List.Clear();
                string[] Dir1 = Directory.GetFiles(Sub_Code.ExDir + "/Download_Mods/" + Bank_Name, "*.bnk", SearchOption.TopDirectoryOnly);
                foreach (string Name in Dir1)
                    Mod_List.Add(new Mod_Type_List(Path.GetFileName(Name)));
                string[] Dir2 = Directory.GetFiles(Sub_Code.ExDir + "/Download_Mods/" + Bank_Name, "*.bnk.dvpl", SearchOption.TopDirectoryOnly);
                foreach (string Name in Dir2)
                    Mod_List.Add(new Mod_Type_List(Path.GetFileName(Name)));
                Mod_List = Mod_List.OrderBy(h => h.Name_Text).ToList();
                Mod_L.ItemsSource = Mod_List;
                AbsoluteLayout.SetLayoutBounds(Explanation_Scrool, new Rectangle(0.45, 0.755, 0.7, 0.15));
                Change_Control_Visibility(true);
            }
            catch (Exception e)
            {
                Message_Feed_Out("エラーが発生しました。詳しくはError_Log.txtを参照してください。");
                Sub_Code.Error_Log_Write(e.Message);
                AbsoluteLayout.SetLayoutBounds(Explanation_Scrool, new Rectangle(0.45, 0.755, 0.7, 0.15));
                IsDownloading = false;
                Change_Control_Visibility(false);
                Clear_Mod_L_Color();
                return;
            }
        }
        private void Download_B_Clicekd(object sender, EventArgs e)
        {
            if (!IsDownloading && Mod_L.SelectedItem != null)
            {
                if (Password_T.IsVisible)
                {
                    //Modにパスワードがかかっている場合テキストボックスに入力されている文字と比較して同じだったらダウンロードが開始される
                    XDocument xml2 = XDocument.Load(Sub_Code.Main_Server.SFTPClient.GetFileRead("/WoTB_Voice_Mod/Mods/" + ((Mod_Type_List)Mod_L.SelectedItem).Name_Text + "/Configs.dat"));
                    XElement item2 = xml2.Element("Mod_Upload_Config");
                    if (item2.Element("Password").Value == Password_T.Text)
                    {
                        Password_T.IsVisible = false;
                        Sample_Download();
                    }
                    else
                        Message_Feed_Out("パスワードが違います。");
                }
                else
                    Sample_Download();
            }
        }
        private void Install_B_Clicked(object sender, EventArgs e)
        {
            try
            {
                Message_T.Text = "WoTBに適応しています...";
                string To_Dir = Sub_Code.WoTBDir;
                if (Sub_Code.WoTBDir == "" && !Directory.Exists("/storage/emulated/0/android/data/net.wargaming.wot.blitz/files/packs"))
                {
                    Message_Feed_Out("この端末では手動で導入する必要があります。");
                    return;
                }
                else if (Sub_Code.WoTBDir == "")
                    To_Dir = "/storage/emulated/0/android/data/net.wargaming.wot.blitz/files";
                string[] Dir = Directory.GetFiles(Sub_Code.ExDir + "/Download_Mods/" + Mod_Select_Name, "*.*", SearchOption.TopDirectoryOnly);
                List<string> FEV_List = new List<string>();
                foreach (string Name in Dir)
                {
                    string Name_Only = Path.GetFileName(Name).Replace(".dvpl", "");
                    if (Name_Only == "voiceover_crew.bnk")
                    {
                        string WoTB_Path = Sub_Code.WoTBDir + "/Data/WwiseSound/ja/voiceover_crew.bnk";
                        Sub_Code.DVPL_File_Delete(Sub_Code.WoTBDir + "/Data/WwiseSound/ja/voiceover_crew.bnk");
                        Sub_Code.DVPL_File_Copy(Name.Replace(".dvpl", ""), WoTB_Path, true);
                    }
                    else if (Name_Only.Contains(".bnk") || Name_Only.Contains(".pck"))
                    {
                        Message_T.Text = "ファイルをコピーしています...";
                        Sub_Code.DVPL_File_Delete(Sub_Code.WoTBDir + "/Data/WwiseSound/" + Path.GetFileName(Name).Replace(".dvpl", ""));
                        Sub_Code.DVPL_File_Copy(Name.Replace(".dvpl", ""), Sub_Code.WoTBDir + "/Data/WwiseSound/" + Path.GetFileName(Name).Replace(".dvpl", ""), true);
                    }
                }
                Message_Feed_Out("Modをインストールしました。");
            }
            catch
            {
                Message_Feed_Out("この端末では手動で導入する必要があります。");
            }
        }
        private void Back_B_Clicked(object sender, EventArgs e)
        {
            if (!IsDownloading)
            {
                Change_Control_Visibility(false);
                if (Wwise_Bnk != null)
                {
                    Wwise_Bnk.Bank_Clear();
                    Wwise_Bnk = null;
                }
                if (Directory.Exists(Sub_Code.ExDir + "/Download_Mods/" + Mod_Select_Name) && Mod_Select_Name != "")
                    Directory.Delete(Sub_Code.ExDir + "/Download_Mods/" + Mod_Select_Name, true);
                if (File.Exists(Sub_Code.ExDir + "/Temp/Temp_03.ogg"))
                    File.Delete(Sub_Code.ExDir + "/Temp/Temp_03.ogg");
                Mod_Select_Name = "";
                Message_T.Text = "";
                Sound_Index_S.Value = 0;
                Sound_Index_S.Maximum = 0.1;
                Mod_List_Update();
            }
        }
        private async Task Set_Bank_Play(int Voice_Number)
        {
            if (IsBusy || Wwise_Bnk == null || Wwise_Bnk.IsClear)
                return;
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            if (Sound_Select_Now != Voice_Number)
            {
                Message_T.Text = "変換しています...";
                await Task.Delay(50);
                if (!Directory.Exists(Sub_Code.ExDir + "/Temp"))
                    Directory.CreateDirectory(Sub_Code.ExDir + "/Temp");
                Wwise_Bnk.Wwise_Extract_To_Ogg_File(Voice_Number, Sub_Code.ExDir + "/Temp/Temp_03.ogg", true);
                Sound_Select_Now = Voice_Number;
            }
            int StreamHandle = Bass.BASS_StreamCreateFile(Sub_Code.ExDir + "/Temp/Temp_03.ogg", 0, 0, BASSFlag.BASS_STREAM_DECODE);
            Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Sound_Volume_S.Value / 100);
            Bass.BASS_ChannelPlay(Stream, true);
            Message_T.Text = "";
        }
        private async void Random_Play_B_Clicked(object sender, EventArgs e)
        {
            if (Sound_Max_Index == 0)
                return;
            int Number = Sub_Code.r.Next(0, Sound_Max_Index - 1);
            await Set_Bank_Play(Number);
            Sound_Index_S.Value = Number;
        }
        private void Stop_B_Clicked(object sender, EventArgs e)
        {
            Bass.BASS_ChannelPause(Stream);
        }
        private async void Play_B_Clicked(object sender, EventArgs e)
        {
            //スライダーの位置のサウンドを再生
            await Set_Bank_Play((int)Sound_Index_S.Value);
        }
        private void Sound_Index_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Sound_Index_T.Text = (int)e.NewValue + "/" + Sound_Max_Index;
        }
        private void Sound_Volume_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)e.NewValue / 100);
            Sound_Volume_T.Text = "音量:" + (int)e.NewValue;
        }
        private async void Mod_Upload_B_Clicked(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            IsPageOpen = true;
            await Navigation.PushAsync(Mod_Upload_Window);
        }
        private void Mod_Change_B_Clicked(object sender, EventArgs e)
        {
            Message_Feed_Out("この機能はまだ実装されていません。");
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            IsPageOpen = false;
            if (!IsDownloading)
                Mod_List_Update();
        }
    }
}