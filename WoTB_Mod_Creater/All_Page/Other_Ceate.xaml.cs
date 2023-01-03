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
    public class Other_File_Type
    {
        public string Full_Path { get; set; }
        public string Full_Path_Source { get; set; }
        public string Name_Text => Full_Path_Source != null ? Path.GetFileName(Full_Path_Source) : Path.GetFileName(Full_Path);
        public Music_Play_Time Music_Time = new Music_Play_Time();
        public bool Music_Feed_In = true;
        public Other_File_Type(string Full_Path, string Full_Path_Source = null, bool Music_Feed_In = false, Music_Play_Time Music_Time = null)
        {
            this.Full_Path = Full_Path;
            this.Full_Path_Source = Full_Path_Source;
            this.Music_Feed_In = Music_Feed_In;
            if (Music_Time != null)
                this.Music_Time = Music_Time;
        }
        public Other_File_Type Clone()
        {
            Other_File_Type Temp = (Other_File_Type)MemberwiseClone();
            Temp.Music_Time = Music_Time.Clone();
            return Temp;
        }
    }
    public class Other_Type_List
    {
        public List<Other_File_Type> Files = new List<Other_File_Type>();
        public string Name { get; set; }
        public string Name_Text => Name + " | " + Files.Count + "個";
        public Color Name_Color => Files.Count > 0 ? Color.Aqua : Color.FromHex("#BFFF2C8C");
        public int Index { get; private set; }
        public bool IsMusicType { get; private set; }
        public Other_Type_List(string Name, int Index, bool IsMusicType = false)
        {
            this.Name = Name;
            this.Index = Index;
            this.IsMusicType = IsMusicType;
        }
        public Other_Type_List Clone()
        {
            Other_Type_List a = (Other_Type_List)MemberwiseClone();
            List<Other_File_Type> Temp = new List<Other_File_Type>();
            foreach (Other_File_Type t in Files)
                Temp.Add(t.Clone());
            a.Files = Temp;
            return a;
        }
    }
    public class Music_Play_Time
    {
        public double Start_Time { get; set; }
        public double End_Time { get; set; }
        public Music_Play_Time()
        {
            Start_Time = 0;
            End_Time = 0;
        }
        public Music_Play_Time(double Set_Start_Time, double Set_End_Time)
        {
            Start_Time = Set_Start_Time;
            End_Time = Set_End_Time;
        }
        public Music_Play_Time Clone()
        {
            return (Music_Play_Time)MemberwiseClone();
        }
    }
    public partial class Other_Ceate : ContentPage
    {
        public readonly Other_Create_Project Other_Create_Project = new Other_Create_Project();
        public readonly List<List<Other_Type_List>> Other_List = new List<List<Other_Type_List>>();
        public readonly WMS_Load WMS_File = new WMS_Load();
        private ViewCell Other_Type_LastCell = null;
        private ViewCell Sound_File_LastCell = null;
        private string Project_Name = "";
        private int Mod_Page = 0;
        private bool IsLoaded = false;
        private bool IsForceColorMode = false;
        private bool IsPageOpen = false;
        bool IsMessageShowing = false;
        public Other_Ceate()
        {
            InitializeComponent();
            for (int Number = 0; Number < 3; Number++)
                Other_List.Add(new List<Other_Type_List>());
            Other_Type_Back_B.Clicked += Other_Type_Back_B_Clicked;
            Other_Type_Next_B.Clicked += Other_Type_Next_B_Clicked;
            Save_B.Clicked += Save_B_Clicked;
            Load_B.Clicked += Load_B_Clicked;
            Clear_B.Clicked += Clear_B_Clicked;
            Sound_Add_B.Clicked += Sound_Add_B_Clicked;
            Sound_Delete_B.Clicked += Sound_Delete_B_Clicked;
            Sound_Setting_B.Clicked += Sound_Setting_B_Clicked;
            Create_B.Clicked += Create_B_Clicked;
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
        public void Other_Init(int Mod_Page, List<List<Other_Type_List>> Other_List = null)
        {
            if (Other_List == null)
                Other_List = this.Other_List;
            if (Mod_Page == 0)
            {
                Other_List[0].Clear();
                Other_List[0].Add(new Other_Type_List("ロード1:America_lakville", 0, true));
                Other_List[0].Add(new Other_Type_List("ロード2:America_overlord", 1, true));
                Other_List[0].Add(new Other_Type_List("ロード3:Chinese", 2, true));
                Other_List[0].Add(new Other_Type_List("ロード4:Desert_airfield", 3, true));
                Other_List[0].Add(new Other_Type_List("ロード5:Desert_sand_river", 4, true));
                Other_List[0].Add(new Other_Type_List("ロード6:Europe_himmelsdorf", 5, true));
                Other_List[0].Add(new Other_Type_List("ロード7:Europe_mannerheim", 6, true));
                Other_List[0].Add(new Other_Type_List("ロード8:Europe_ruinberg", 7, true));
                Other_List[0].Add(new Other_Type_List("ロード9:Japan", 8, true));
                Other_List[0].Add(new Other_Type_List("ロード10:Russian_malinovka", 9, true));
                Other_List[0].Add(new Other_Type_List("ロード11:Russian_prokhorovka", 10, true));
                Other_List[0].Add(new Other_Type_List("リザルト:勝利-BGM", 11, true));
                Other_List[0].Add(new Other_Type_List("リザルト:勝利-音声", 12));
                Other_List[0].Add(new Other_Type_List("リザルト:引き分け-BGM", 13, true));
                Other_List[0].Add(new Other_Type_List("リザルト:引き分け-音声", 14));
                Other_List[0].Add(new Other_Type_List("リザルト:敗北-BGM", 15, true));
                Other_List[0].Add(new Other_Type_List("リザルト:敗北-音声", 16));
                Other_List[0].Add(new Other_Type_List("優勢:味方", 17, true));
                Other_List[0].Add(new Other_Type_List("優勢:敵", 18, true));
                Other_List[0].Add(new Other_Type_List("被弾:貫通-音声", 19));
                Other_List[0].Add(new Other_Type_List("被弾:非貫通-音声", 20));
            }
            else if (Mod_Page == 1)
            {
                Other_List[1].Clear();
                Other_List[1].Add(new Other_Type_List("コンテナ開封-ノーマル-SE", 0));
                Other_List[1].Add(new Other_Type_List("コンテナ開封-ノーマル-音声", 1));
                Other_List[1].Add(new Other_Type_List("コンテナ開封-レア-SE", 2));
                Other_List[1].Add(new Other_Type_List("コンテナ開封-レア-音声", 3));
                Other_List[1].Add(new Other_Type_List("購入-SE", 4));
                Other_List[1].Add(new Other_Type_List("購入-音声", 5));
                Other_List[1].Add(new Other_Type_List("売却-SE", 6));
                Other_List[1].Add(new Other_Type_List("売却-音声", 7));
                Other_List[1].Add(new Other_Type_List("チェックボックス-SE", 8));
                Other_List[1].Add(new Other_Type_List("チェックボックス-音声", 9));
                Other_List[1].Add(new Other_Type_List("小隊受信-SE", 10));
                Other_List[1].Add(new Other_Type_List("小隊受信-音声", 11));
                Other_List[1].Add(new Other_Type_List("モジュールの切り替え-SE", 12));
                Other_List[1].Add(new Other_Type_List("モジュールの切り替え-音声", 13));
                Other_List[1].Add(new Other_Type_List("戦闘開始-SE", 14));
                Other_List[1].Add(new Other_Type_List("戦闘開始-音声", 15));
                Other_List[1].Add(new Other_Type_List("ニュース-SE", 16));
                Other_List[1].Add(new Other_Type_List("ニュース-音声", 17));
                Other_List[1].Add(new Other_Type_List("車両購入-SE", 18));
                Other_List[1].Add(new Other_Type_List("車両購入-音声", 19));
                Set_Garage_Default_SE();
            }
            else if (Mod_Page == 2)
            {
                Other_List[2].Clear();
                Other_List[2].Add(new Other_Type_List("12～23mm:自車両-通常", 0));
                Other_List[2].Add(new Other_Type_List("12～23mm:自車両-ズーム時", 1));
                Other_List[2].Add(new Other_Type_List("12～23mm:他車両", 2));
                Other_List[2].Add(new Other_Type_List("20～45mm:自車両-通常", 3));
                Other_List[2].Add(new Other_Type_List("20～45mm:自車両-ズーム時", 4));
                Other_List[2].Add(new Other_Type_List("20～45mm:他車両", 5));
                Other_List[2].Add(new Other_Type_List("50～75mm:自車両-通常", 6));
                Other_List[2].Add(new Other_Type_List("50～75mm:自車両-ズーム時", 7));
                Other_List[2].Add(new Other_Type_List("50～75mm:他車両", 8));
                Other_List[2].Add(new Other_Type_List("85～107mm:自車両-通常", 9));
                Other_List[2].Add(new Other_Type_List("85～107mm:自車両-ズーム時", 10));
                Other_List[2].Add(new Other_Type_List("85～107mm:他車両", 11));
                Other_List[2].Add(new Other_Type_List("115～152mm:自車両-通常", 12));
                Other_List[2].Add(new Other_Type_List("115～152mm:自車両-ズーム時", 13));
                Other_List[2].Add(new Other_Type_List("115～152mm:他車両", 14));
                Other_List[2].Add(new Other_Type_List("152mm以上:自車両-通常", 15));
                Other_List[2].Add(new Other_Type_List("152mm以上:自車両-ズーム時", 16));
                Other_List[2].Add(new Other_Type_List("152mm以上:他車両", 17));
                Other_List[2].Add(new Other_Type_List("152mm以上_Extra:自車両-通常", 18));
                Other_List[2].Add(new Other_Type_List("152mm以上_Extra:自車両-ズーム時", 19));
                Other_List[2].Add(new Other_Type_List("152mm以上_Extra:他車両", 20));
                Other_List[2].Add(new Other_Type_List("音声(12～23mm以外):自車両", 21));
                Other_List[2].Add(new Other_Type_List("音声(12～23mm以外):他車両", 22));
            }
        }
        private void Set_Garage_Default_SE()
        {
            for (int Number = 0; Number < Other_List[1].Count; Number++)
            {
                if (Number == 0)
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/176974408.mp3");
                else if (Number == 2)
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/262333918.mp3");
                else if (Number == 4)
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/440745850.wav");
                else if (Number == 6)
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/394850995.wav");
                else if (Number == 8)
                {
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/376157875.wav");
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/118267228.wav");
                }
                else if (Number == 10)
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/166277761.wav");
                else if (Number == 12)
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/387372795.wav");
                else if (Number == 14)
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/843967064.mp3");
                else if (Number == 16)
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/493097325.wav");
                else if (Number == 18)
                    Set_Garage_Default_SE_By_Name(Number, Sub_Code.ExDir + "/UI_Battle_SE/437893724.mp3");
            }
        }
        private void Set_Garage_Default_SE_By_Name(int Index, string SE_Name)
        {
            Other_List[1][Index].Files.Add(new Other_File_Type(SE_Name));
        }
        private void Other_Type_ViewCell_Tapped(object sender, EventArgs e)
        {
            if (IsForceColorMode)
                Clear_Voice_Type_Color();
            else if (Other_Type_LastCell != null)
                Other_Type_LastCell.View.BackgroundColor = Color.Transparent;
            ViewCell viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#82bfc8");
                Other_Type_LastCell = viewCell;
                Change_Setting_Page_Visible();
            }
            Set_Item_Sound();
        }
        private void Change_Setting_Page_Visible()
        {
            Other_Type_List Temp = (Other_Type_List)Other_Type_L.SelectedItem;
            if (Temp != null && Mod_Page == 0 && ((Temp.Index >= 0 && Temp.Index <= 11) || Temp.Index == 13 || Temp.Index == 15 || Temp.Index == 17 || Temp.Index == 18))
                Sound_Setting_B.IsVisible = true;
            else
                Sound_Setting_B.IsVisible = false;
        }
        private void Sound_List_Tapped(object sender, EventArgs e)
        {
            if (Sound_File_LastCell != null)
                Sound_File_LastCell.View.BackgroundColor = Color.Transparent;
            ViewCell viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#9982bfc8");
                Sound_File_LastCell = viewCell;
            }
        }
        private void Clear_Voice_Type_Color()
        {
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = Other_Type_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(Other_Type_L);
                foreach (ViewCell cell in (cells as ITemplatedItemsList<Cell>).Cast<ViewCell>())
                    if (cell.BindingContext != null)
                        cell.View.BackgroundColor = Color.Transparent;
            }
            IsForceColorMode = false;
        }
        private void Set_Item_Sound()
        {
            Sound_File_L.ItemsSource = null;
            int Voice_Type_Index = Other_Type_L.SelectedItem != null ? ((Other_Type_List)Other_Type_L.SelectedItem).Index : -1;
            if (Voice_Type_Index == -1)
                return;
            Sound_File_L.ItemsSource = Other_List[Mod_Page][Voice_Type_Index].Files;
            Sound_File_L.SelectedItem = null;
        }
        private void Change_Other_List()
        {
            Sound_Setting_B.IsVisible = false;
            if (Mod_Page == 0)
                Mod_Name_T.Text = "いろいろ";
            else if (Mod_Page == 1)
                Mod_Name_T.Text = "ガレージSE";
            else if (Mod_Page == 2)
                Mod_Name_T.Text = "砲撃音Mod";
            Other_Type_L.SelectedItem = null;
            Other_Type_L.ItemsSource = null;
            Other_Type_L.ItemsSource = Other_List[Mod_Page];
            Sound_File_L.ItemsSource = null;
            Sound_File_L.SelectedItem = null;
        }
        private void Set_Voice_Type_Index(int Index)
        {
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = Other_Type_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(Other_Type_L);
                int Count = 0;
                foreach (ViewCell cell in (cells as ITemplatedItemsList<Cell>).Cast<ViewCell>())
                {
                    if (cell.BindingContext != null && Count == Index)
                    {
                        cell.View.BackgroundColor = Color.FromHex("#82bfc8");
                        break;
                    }
                    else
                        cell.View.BackgroundColor = Color.Transparent;
                    Count++;
                }
            }
        }
        public void Other_Load_From_File(string WMS_File)
        {
            try
            {
                Other_Init(0);
                Other_Init(1);
                Other_Init(2);
                if (WMS_Load.IsFullWMSFile(WMS_File))
                {
                    _ = this.WMS_File.WMS_Load_File(WMS_File, Other_List);
                    Project_Name = this.WMS_File.Project_Name;
                }
                else
                {
                    Message_Feed_Out("エラー:セーブデータが破損しています。");
                    return;
                }
                Change_Other_List();
            }
            catch
            {
                Message_Feed_Out("エラー:セーブデータが破損しています。");
            }
        }
        private void Other_Type_Back_B_Clicked(object sender, EventArgs e)
        {
            if (Mod_Page > 0)
            {
                Mod_Page--;
                Change_Other_List();
            }
        }
        private void Other_Type_Next_B_Clicked(object sender, EventArgs e)
        {
            if (Mod_Page < 2)
            {
                Mod_Page++;
                Change_Other_List();
            }
        }
        private async void Save_B_Clicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("セーブ", "プロジェクト名を指定してください。", "決定", "キャンセル", null, -1, null, Project_Name);
            if (result != null)
            {
                if (!Sub_Code.IsSafePath(result, true))
                {
                    DependencyService.Get<IMessage>().Message_Alert("エラー:使用できない文字が含まれています。", Message_Length.Short);
                    return;
                }
                else if (File.Exists(Sub_Code.ExDir + "/Saves/" + result + ".wms"))
                {
                    bool result_01 = await DisplayAlert("セーブデータを上書きしますか?", null, "はい", "いいえ");
                    if (!result_01)
                        return;
                }
                if (!Directory.Exists(Sub_Code.ExDir + "/Saves"))
                    _ = Directory.CreateDirectory(Sub_Code.ExDir + "/Saves");
                WMS_Save Save = new WMS_Save();
                Save.Add_Sound(Other_List, WMS_File, WMS_Save.WMS_Save_Mode.All);
                WMS_File.Dispose();
                Save.Create(Sub_Code.ExDir + "/Saves/" + result + ".wms", result, true);
                Save.Dispose();
                Other_Load_From_File(Sub_Code.ExDir + "/Saves/" + result + ".wms");
                Sub_Code.Delete_Other_Mods();
                Message_Feed_Out("セーブしました。");
            }
        }
        private async void Load_B_Clicked(object sender, EventArgs e)
        {
            List<string> Save_Names = new List<string>();
            if (!Directory.Exists(Sub_Code.ExDir + "/Saves"))
                _ = Directory.CreateDirectory(Sub_Code.ExDir + "/Saves");
            foreach (string Files in Directory.GetFiles(Sub_Code.ExDir + "/Saves", "*.wms", SearchOption.TopDirectoryOnly))
                Save_Names.Add(Path.GetFileNameWithoutExtension(Files));
            if (Save_Names.Count == 0)
            {
                await DisplayActionSheet("プロジェクトを選択してください。", "キャンセル", null, new[] { "!プロジェクトが存在しません!" });
                Save_Names.Clear();
            }
            else if (!IsPageOpen)
            {
                IsPageOpen = true;
                string Ex = ".wms";
                Sub_Code.Select_Files_Window.Window_Show("Other_Create_Load", Sub_Code.ExDir + "/Saves", Ex, true, false);
                await Navigation.PushModalAsync(Sub_Code.Select_Files_Window);
            }
        }
        private async void Clear_B_Clicked(object sender, EventArgs e)
        {
            bool IsOK = await DisplayAlert("内容をクリアしますか?", null, "はい", "いいえ");
            if (IsOK)
            {
                Other_Init(0);
                Other_Init(1);
                Other_Init(2);
                Change_Other_List();
                Message_Feed_Out("内容をクリアしました。");
            }
        }
        private async void Sound_Add_B_Clicked(object sender, EventArgs e)
        {
            if (Other_Type_L.SelectedItem == null)
            {
                Message_Feed_Out("イベント名が選択されていません。");
                return;
            }
            if (Sub_Code.IsUseSelectPage && !IsPageOpen && Sub_Code.HasSystemPermission)
            {
                IsPageOpen = true;
                string Ex = ".aac|.mp3|.wav|.ogg|.aiff|.flac|.m4a|.mp4";
                Sub_Code.Select_Files_Window.Window_Show("Other_Create", "", Ex);
                await Navigation.PushModalAsync(Sub_Code.Select_Files_Window);
            }
            else
            {
                FilePickerFileType customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "audio/*" } }
                });
                PickOptions options = new PickOptions
                {
                    FileTypes = customFileType,
                    PickerTitle = "サウンドファイルを選択してください。"
                };
                IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(options);
                if (result != null)
                {
                    Other_Type_List Temp = (Other_Type_List)Other_Type_L.SelectedItem;
                    bool IsExist = false;
                    if (!Directory.Exists(Sub_Code.ExDir + "/Temp/Other_Mods"))
                        Directory.CreateDirectory(Sub_Code.ExDir + "/Temp/Other_Mods");
                    List<string> Files = Temp.Files.Select(h => h.Full_Path).ToList();
                    List<Other_File_Type> Match = new List<Other_File_Type>();
                    for (int Number_01 = 0; Number_01 < Other_List.Count; Number_01++)
                        for (int Number_02 = 0; Number_02 < Other_List[Number_01].Count; Number_02++)
                            foreach (Other_File_Type File_Type in Other_List[Number_01][Number_02].Files)
                                Match.Add(File_Type);
                    foreach (FileResult file_result in result)
                    {
                        string Name_Only = Path.GetFileName(file_result.FullPath);
                        if (!Files.Contains(Name_Only))
                        {
                            Other_File_Type Source = null;
                            foreach (Other_File_Type File_Now in Match)
                            {
                                if (File_Now.Full_Path_Source != null && Path.GetFileName(File_Now.Full_Path_Source) == Name_Only)
                                {
                                    if (Sub_Code.File_Equal(File_Now.Full_Path, file_result.FullPath))
                                    {
                                        Source = File_Now;
                                        break;
                                    }
                                }
                            }
                            if (Source != null)
                                Temp.Files.Add(new Other_File_Type(Source.Full_Path, Source.Full_Path_Source, Temp.IsMusicType));
                            else
                            {
                                int Random_ID = Sub_Code.r.Next(1000000, 9999999);
                                File.Copy(file_result.FullPath, Sub_Code.ExDir + "/Temp/Other_Mods/" + Random_ID + Path.GetExtension(file_result.FullPath), true);
                                Temp.Files.Add(new Other_File_Type(Sub_Code.ExDir + "/Temp/Other_Mods/" + Random_ID + Path.GetExtension(file_result.FullPath), file_result.FullPath, Temp.IsMusicType));
                            }
                        }
                        else
                            IsExist = true;
                    }
                    if (IsExist)
                        Message_Feed_Out("既に追加されているファイルが存在します。");
                    Files.Clear();
                    Match.Clear();
                    string Cache_Dir = Path.GetDirectoryName(Sub_Code.ExDir) + "/cache";
                    if (Directory.Exists(Cache_Dir))
                        Directory.Delete(Cache_Dir, true);
                    Change_Other_List();
                    Set_Voice_Type_Index(Temp.Index);
                    Other_Type_L.SelectedItem = Temp;
                    Set_Item_Sound();
                    Change_Setting_Page_Visible();
                    IsForceColorMode = true;
                }
            }
        }
        private void Sound_Delete_B_Clicked(object sender, EventArgs e)
        {
            if (Other_Type_L.SelectedItem == null)
            {
                Message_Feed_Out("イベント名が選択されていません。");
                return;
            }
            if (Sound_File_L.SelectedItem == null)
            {
                Message_Feed_Out("削除したいサウンドを選択してください。");
                return;
            }
            int Other_Type_Index = ((Other_Type_List)Other_Type_L.SelectedItem).Index;
            int Sound_Index = Other_List[Mod_Page][Other_Type_Index].Files.Select(h => h.Full_Path).ToList().IndexOf(((Other_File_Type)Sound_File_L.SelectedItem).Full_Path);
            Other_List[Mod_Page][Other_Type_Index].Files.RemoveAt(Sound_Index);
            WMS_File.Delete_Sound(Mod_Page, Other_Type_Index, Sound_Index);
            Change_Other_List();
            Set_Voice_Type_Index(Other_Type_Index);
            Other_Type_L.SelectedItem = Other_List[Mod_Page][Other_Type_Index];
            Set_Item_Sound();
            Change_Setting_Page_Visible();
            IsForceColorMode = true;
        }
        private void Sound_Setting_B_Clicked(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            else if (Other_Type_L.SelectedItem == null)
            {
                Message_Feed_Out("イベントを選択してください。");
                return;
            }
            else if (((Other_Type_List)Other_Type_L.SelectedItem).Files.Count == 0)
            {
                Message_Feed_Out("イベント内にサウンドが含まれていません。");
                return;
            }
            IsPageOpen = true;
            Navigation.PushModalAsync(Sub_Code.Other_Mods_Setting_Window);
            Sub_Code.Other_Mods_Setting_Window.Window_Show(((Other_Type_List)Other_Type_L.SelectedItem).Index);
        }
        private void Create_B_Clicked(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            IsPageOpen = true;
            Navigation.PushAsync(Other_Create_Project);
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            IsPageOpen = false;
            if (!IsLoaded)
            {
                IsLoaded = true;
                Other_Init(0);
                Other_Init(1);
                Other_Init(2);
                Change_Other_List();
            }
        }
        public void Selected_Files()
        {
            Other_Type_List Temp = (Other_Type_List)Other_Type_L.SelectedItem;
            bool IsExist = false;
            List<string> Files = Temp.Files.Select(h => h.Full_Path).ToList();
            foreach (string file_result in Sub_Code.Select_Files_Window.Get_Select_Files())
            {
                if (!Files.Contains(file_result))
                    Temp.Files.Add(new Other_File_Type(file_result, null, Temp.IsMusicType));
                else
                    IsExist = true;
            }
            if (IsExist)
                Message_Feed_Out("既に追加されているファイルが存在します。");
            Change_Other_List();
            Set_Voice_Type_Index(Temp.Index);
            Other_Type_L.SelectedItem = Temp;
            Set_Item_Sound();
            Change_Setting_Page_Visible();
            IsForceColorMode = true;
        }
        public void Load_File()
        {
            List<string> file_result = Sub_Code.Select_Files_Window.Get_Select_Files();
            if (file_result.Count == 0)
                return;
            if (WMS_Load.VersionCheck(file_result[0]) == 1 && !Sub_Code.HasSystemPermission)
            {
                Message_Feed_Out(".apkで作成したセーブファイルは使用できません。");
                return;
            }
            Other_Load_From_File(file_result[0]);
            if (WMS_File.Version >= 3)
                Message_Feed_Out("ロードしました。");
            else
                Message_Feed_Out("ロードしました。WoTBの更新によりロケットのサウンドが削除され、新たなイベントが追加されました。");
        }
    }
}