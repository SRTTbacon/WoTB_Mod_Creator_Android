using System;
using System.IO;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Essentials;
using WoTB_Mod_Creater.Class;
using System.Linq;
using System.Reflection;

namespace WoTB_Mod_Creater.All_Page
{
    public class Voice_Type_List
    {
        public string Name { get; set; }
        public string Name_Text => Name + " : " + Count + "個";
        public int Count { get; set; }
        public int Index { get; private set; }
        public Color Name_Color => Count == 0 ? Color.FromHex("#BFFF2C8C") : Color.Aqua;
        public Voice_Type_List(string Name, int Index)
        {
            this.Name = Name;
            Count = 0;
            this.Index = Index;
        }
    }
    public class Voice_Sound_List
    {
        public string Full_Name { get; private set; }
        public string Full_Name_Source { get; private set; }
        public string Name_Text => Full_Name_Source != null ? Path.GetFileName(Full_Name_Source) : Path.GetFileName(Full_Name);
        public Voice_Sound_List(string Full_Name, string Full_Name_Source = null)
        {
            this.Full_Name = Full_Name;
            this.Full_Name_Source = Full_Name_Source;
        }
    }
    public partial class Voice_Create : ContentPage
    {
        public readonly Create_Project Create_Project_Window = new Create_Project();
        private readonly List<Voice_Type_List> Voice_One_Types = new List<Voice_Type_List>();
        private readonly List<Voice_Type_List> Voice_Two_Types = new List<Voice_Type_List>();
        private readonly List<Voice_Type_List> Voice_Three_Types = new List<Voice_Type_List>();
        public readonly List<List<Voice_Sound_List>> Voice_One_Full_Name = new List<List<Voice_Sound_List>>();
        public readonly List<List<Voice_Sound_List>> Voice_Two_Full_Name = new List<List<Voice_Sound_List>>();
        public readonly List<List<Voice_Sound_List>> Voice_Three_Full_Name = new List<List<Voice_Sound_List>>();
        public readonly WVS_Load WVS_File = new WVS_Load();
        private ViewCell Voice_Type_LastCell = null;
        private ViewCell Sound_File_LastCell = null;
        private string Project_Name = "";
        private ushort Voice_Page = 0;
        private bool IsPageOpen = false;
        private bool IsForceColorMode = false;
        public Voice_Create()
        {
            InitializeComponent();
            Load_B.Clicked += Load_B_Click;
            Save_B.Clicked += Save_B_Click;
            Voice_Type_Back_B.Clicked += Voice_Type_Back_B_Click;
            Voice_Type_Next_B.Clicked += Voice_Type_Next_B_Click;
            Sound_Add_B.Clicked += Sound_Add_B_Click;
            Sound_Delete_B.Clicked += Sound_Delete_B_Click;
            Create_B.Clicked += Create_B_Click;
            Clear_B.Clicked += Clear_B_Click;
            Init_Voice_Type();
            Set_Item_Type();
            Create_B.IsEnabled = true;
        }
        private void Init_Voice_Type()
        {
            Voice_One_Types.Clear();
            Voice_Two_Types.Clear();
            Voice_Three_Types.Clear();
            Voice_One_Full_Name.Clear();
            Voice_Two_Full_Name.Clear();
            Voice_Three_Full_Name.Clear();
            //1ページ目
            Voice_One_Types.Add(new Voice_Type_List("味方にダメージ", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("弾薬庫", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("敵への無効弾", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("敵への貫通弾", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("敵への致命弾", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("敵への跳弾", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("車長負傷", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("操縦手負傷", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("敵炎上", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("敵撃破", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("エンジン破損", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("エンジン大破", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("エンジン復旧", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("自車両火災", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("自車両消火", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("燃料タンク破損", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("主砲破損", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("主砲大破", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("主砲復旧", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("砲手負傷", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("装填手負傷", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("通信機破損", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("通信手負傷", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("戦闘開始", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("観測装置破損", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("観測装置大破", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("観測装置復旧", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("履帯破損", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("履帯大破", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("履帯復旧", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("砲塔破損", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("砲塔大破", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("砲塔復旧", Voice_One_Types.Count));
            Voice_One_Types.Add(new Voice_Type_List("自車両大破", Voice_One_Types.Count));
            //2ページ目
            Voice_Two_Types.Add(new Voice_Type_List("敵発見", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("第六感", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("了解", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("拒否", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("救援を請う", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("攻撃せよ!", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("攻撃中", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("陣地を占領せよ!", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("陣地を防衛せよ!", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("固守せよ!", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("ロックオン(Steam版のみ)", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("アンロック(Steam版のみ)", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("装填完了", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("マップクリック時", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("戦闘終了時", Voice_Two_Types.Count));
            Voice_Two_Types.Add(new Voice_Type_List("戦闘BGM", Voice_Two_Types.Count));
            //3ページ目
            Voice_Three_Types.Add(new Voice_Type_List("チャット:味方-送信", Voice_Three_Types.Count));
            Voice_Three_Types.Add(new Voice_Type_List("チャット:味方-受信", Voice_Three_Types.Count));
            Voice_Three_Types.Add(new Voice_Type_List("チャット:全体-送信", Voice_Three_Types.Count));
            Voice_Three_Types.Add(new Voice_Type_List("チャット:全体-受信", Voice_Three_Types.Count));
            Voice_Three_Types.Add(new Voice_Type_List("チャット:小隊-送信", Voice_Three_Types.Count));
            Voice_Three_Types.Add(new Voice_Type_List("チャット:小隊-受信", Voice_Three_Types.Count));
            for (int Number = 0; Number < Voice_One_Types.Count; Number++)
                Voice_One_Full_Name.Add(new List<Voice_Sound_List>());
            for (int Number = 0; Number < Voice_Two_Types.Count; Number++)
                Voice_Two_Full_Name.Add(new List<Voice_Sound_List>());
            for (int Number = 0; Number < Voice_Three_Types.Count; Number++)
                Voice_Three_Full_Name.Add(new List<Voice_Sound_List>());
        }
        private void Set_Item_Type()
        {
            Voice_Type_L.ItemsSource = null;
            if (Voice_Page == 0)
                Voice_Type_L.ItemsSource = Voice_One_Types;
            else if (Voice_Page == 1)
                Voice_Type_L.ItemsSource = Voice_Two_Types;
            else if (Voice_Page == 2)
                Voice_Type_L.ItemsSource = Voice_Three_Types;
            Voice_Type_Page_T.Text = "イベントリスト" + (Voice_Page + 1);
            Voice_Type_L.SelectedItem = null;
            Sound_File_L.ItemsSource = null;
            Sound_File_L.SelectedItem = null;
        }
        private void Set_Item_Sound()
        {
            Sound_File_L.ItemsSource = null;
            int Voice_Type_Index = Voice_Type_L.SelectedItem != null ? ((Voice_Type_List)Voice_Type_L.SelectedItem).Index : -1;
            if (Voice_Type_Index == -1)
                return;
            if (Voice_Page == 0)
                Sound_File_L.ItemsSource = Voice_One_Full_Name[Voice_Type_Index];
            else if (Voice_Page == 1)
                Sound_File_L.ItemsSource = Voice_Two_Full_Name[Voice_Type_Index];
            else if (Voice_Page == 2)
                Sound_File_L.ItemsSource = Voice_Three_Full_Name[Voice_Type_Index];
            Sound_File_L.SelectedItem = null;
        }
        private async void Load_B_Click(object sender, EventArgs e)
        {
            List<string> Save_Names = new List<string>();
            if (!Directory.Exists(Sub_Code.ExDir + "/Saves"))
                _ = Directory.CreateDirectory(Sub_Code.ExDir + "/Saves");
            foreach (string Files in Directory.GetFiles(Sub_Code.ExDir + "/Saves", "*.wvs", SearchOption.TopDirectoryOnly))
                Save_Names.Add(Path.GetFileNameWithoutExtension(Files));
            if (Save_Names.Count == 0)
            {
                await DisplayActionSheet("プロジェクトを選択してください。", "キャンセル", null, new[] { "!プロジェクトが存在しません!" });
                Save_Names.Clear();
            }
            else
            {
                IsPageOpen = true;
                string Ex = ".wvs";
                Sub_Code.Select_Files_Window.Window_Show("Voice_Create_Load", Sub_Code.ExDir + "/Saves", Ex, true, false);
                await Navigation.PushModalAsync(Sub_Code.Select_Files_Window);
            }
        }
        private async void Save_B_Click(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("セーブ", "プロジェクト名を指定してください。", "決定", "キャンセル", null, -1, null, Project_Name);
            if (result != null)
            {
                if (!Sub_Code.IsSafePath(result, true))
                {
                    DependencyService.Get<IMessage>().Message_Alert("エラー:使用できない文字が含まれています。", Message_Length.Short);
                    return;
                }
                else if (File.Exists(Sub_Code.ExDir + "/Saves/" + result + ".wvs"))
                {
                    bool result_01 = await DisplayAlert("セーブデータを上書きしますか?", null, "はい", "いいえ");
                    if (!result_01)
                        return;
                }
                if (!Directory.Exists(Sub_Code.ExDir + "/Saves"))
                    _ = Directory.CreateDirectory(Sub_Code.ExDir + "/Saves");
                WVS_Save Save = new WVS_Save();
                Save.Add_Sound(Voice_One_Full_Name, WVS_File, 0);
                Save.Add_Sound(Voice_Two_Full_Name, WVS_File, 1);
                Save.Add_Sound(Voice_Three_Full_Name, WVS_File, 2);
                WVS_File.Dispose();
                Save.Create(Sub_Code.ExDir + "/Saves/" + result + ".wvs", result, false, false, false, new List<SE_Type_List>());
                Save.Dispose();
                Voice_Load_From_File(Sub_Code.ExDir + "/Saves/" + result + ".wvs");
                Sub_Code.Delete_Voice_Mods();
                Sub_Code.Show_Message("セーブしました。");
            }
        }
        public void Voice_Load_From_File(string WVS_File)
        {
            try
            {
                //音声を配置
                Init_Voice_Type();
                if (WVS_Load.IsFullWVSFile(WVS_File))
                {
                    _ = this.WVS_File.WVS_Load_File(WVS_File, Voice_One_Full_Name, Voice_Two_Full_Name, Voice_Three_Full_Name);
                    for (int Number = 0; Number < Voice_One_Full_Name.Count; Number++)
                        Voice_One_Types[Number].Count += Voice_One_Full_Name[Number].Count;
                    for (int Number = 0; Number < Voice_Two_Full_Name.Count; Number++)
                        Voice_Two_Types[Number].Count+= Voice_Two_Full_Name[Number].Count;
                    for (int Number = 0; Number < Voice_Three_Full_Name.Count; Number++)
                        Voice_Three_Types[Number].Count += Voice_Three_Full_Name[Number].Count;
                    Project_Name = this.WVS_File.Project_Name;
                }
                else
                {
                    this.WVS_File.Dispose();
                    string line;
                    StreamReader file = Sub_Code.File_Decrypt_To_Stream(WVS_File, "SRTTbacon_Create_Voice_Save");
                    string Name_All = file.ReadLine();
                    if (Name_All.Contains("|"))
                    {
                        string Mode_Name = Name_All.Substring(Name_All.LastIndexOf('|'));
                        if (Mode_Name == "|IsNotChangeProjectNameMode=true")
                        {
                            string Name_Only = Name_All.Substring(0, Name_All.LastIndexOf('|'));
                            Project_Name = Name_Only;
                        }
                        else
                            Project_Name = Name_All;
                    }
                    else
                        Project_Name = Name_All;
                    while ((line = file.ReadLine()) != null)
                    {
                        int Number = int.Parse(line.Substring(0, line.IndexOf('|')));
                        string File_Path = line.Substring(line.IndexOf('|') + 1);
                        if (Number < 34)
                        {
                            Voice_One_Full_Name[Number].Add(new Voice_Sound_List(File_Path));
                            Voice_One_Types[Number].Count++;
                        }
                        else if (Number < 50)
                        {
                            Voice_Two_Full_Name[Number - 34].Add(new Voice_Sound_List(File_Path));
                            Voice_Two_Types[Number - 34].Count++;
                        }
                        else
                        {
                            Voice_Three_Full_Name[Number - 50].Add(new Voice_Sound_List(File_Path));
                            Voice_Three_Types[Number - 50].Count++;
                        }
                    }
                    file.Close();
                }
                Set_Item_Type();
            }
            catch
            {
                Sub_Code.Show_Message("エラー:セーブデータが破損しています。");
                Init_Voice_Type();
                Set_Item_Type();
            }
        }
        private void Voice_Type_Back_B_Click(object sender, EventArgs e)
        {
            if (Voice_Page > 0)
            {
                Voice_Page--;
                Set_Item_Type();
            }
        }
        private void Voice_Type_Next_B_Click(object sender, EventArgs e)
        {
            if (Voice_Page < 2)
            {
                Voice_Page++;
                Set_Item_Type();
            }
        }
        private async void Sound_Add_B_Click(object sender, EventArgs e)
        {
            if (Voice_Type_L.SelectedItem == null)
            {
                Sub_Code.Show_Message("イベント名が選択されていません。");
                return;
            }
            if (Sub_Code.IsUseSelectPage && !IsPageOpen && Sub_Code.HasSystemPermission)
            {
                IsPageOpen = true;
                string Ex = ".aac|.mp3|.wav|.ogg|.aiff|.flac|.m4a|.mp4";
                Sub_Code.Select_Files_Window.Window_Show("Voice_Create", "", Ex);
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
                    if (!Directory.Exists(Sub_Code.ExDir + "/Temp/Voice_Mods"))
                        Directory.CreateDirectory(Sub_Code.ExDir + "/Temp/Voice_Mods");
                    int Voice_Type_Index = ((Voice_Type_List)Voice_Type_L.SelectedItem).Index;
                    List<string> Already_Music_List = new List<string>();
                    if (Voice_Page == 0)
                        Already_Music_List = Voice_One_Full_Name[Voice_Type_Index].Select(x => x.Name_Text).ToList();
                    else if (Voice_Page == 1)
                        Already_Music_List = Voice_Two_Full_Name[Voice_Type_Index].Select(x => x.Name_Text).ToList();
                    else if (Voice_Page == 2)
                        Already_Music_List = Voice_Three_Full_Name[Voice_Type_Index].Select(x => x.Name_Text).ToList();
                    List<Voice_Sound_List> Match = new List<Voice_Sound_List>();
                    if (Voice_Page == 0)
                        for (int Number_01 = 0; Number_01 < Voice_One_Full_Name.Count; Number_01++)
                            foreach (Voice_Sound_List File_Type in Voice_One_Full_Name[Number_01])
                                Match.Add(File_Type);
                    bool IsExist = false;
                    foreach (FileResult file_result in result)
                    {
                        string Name_Only = Path.GetFileName(file_result.FullPath);
                        if (Already_Music_List.Contains(Name_Only))
                        {
                            IsExist = true;
                            continue;
                        }
                        Voice_Sound_List Source = null;
                        foreach (Voice_Sound_List File_Now in Match)
                        {
                            if (File_Now.Full_Name_Source != null && Path.GetFileName(File_Now.Full_Name_Source) == Name_Only)
                            {
                                if (Sub_Code.File_Equal(File_Now.Full_Name, file_result.FullPath))
                                {
                                    Source = File_Now;
                                    break;
                                }
                            }
                        }
                        if (Source != null)
                        {
                            if (Voice_Page == 0)
                            {
                                Voice_One_Full_Name[Voice_Type_Index].Add(new Voice_Sound_List(Source.Full_Name, Source.Full_Name_Source));
                                Voice_One_Types[Voice_Type_Index].Count++;
                            }
                            else if (Voice_Page == 1)
                            {
                                Voice_Two_Full_Name[Voice_Type_Index].Add(new Voice_Sound_List(Source.Full_Name, Source.Full_Name_Source));
                                Voice_Two_Types[Voice_Type_Index].Count++;
                            }
                            else if (Voice_Page == 2)
                            {
                                Voice_Three_Full_Name[Voice_Type_Index].Add(new Voice_Sound_List(Source.Full_Name, Source.Full_Name_Source));
                                Voice_Three_Types[Voice_Type_Index].Count++;
                            }
                        }
                        else
                        {
                            int Random_ID = Sub_Code.r.Next(1000000, 9999999);
                            File.Copy(file_result.FullPath, Sub_Code.ExDir + "/Temp/Voice_Mods/" + Random_ID + Path.GetExtension(file_result.FullPath), true);
                            if (Voice_Page == 0)
                            {
                                Voice_One_Full_Name[Voice_Type_Index].Add(new Voice_Sound_List(Sub_Code.ExDir + "/Temp/Voice_Mods/" + Random_ID + Path.GetExtension(file_result.FullPath), file_result.FullPath));
                                Voice_One_Types[Voice_Type_Index].Count++;
                            }
                            else if (Voice_Page == 1)
                            {
                                Voice_Two_Full_Name[Voice_Type_Index].Add(new Voice_Sound_List(Sub_Code.ExDir + "/Temp/Voice_Mods/" + Random_ID + Path.GetExtension(file_result.FullPath), file_result.FullPath));
                                Voice_Two_Types[Voice_Type_Index].Count++;
                            }
                            else if (Voice_Page == 2)
                            {
                                Voice_Three_Full_Name[Voice_Type_Index].Add(new Voice_Sound_List(Sub_Code.ExDir + "/Temp/Voice_Mods/" + Random_ID + Path.GetExtension(file_result.FullPath), file_result.FullPath));
                                Voice_Three_Types[Voice_Type_Index].Count++;
                            }
                        }
                    }
                    if (IsExist)
                        Sub_Code.Show_Message("既に追加されているファイルが存在します。");
                    Set_Item_Type();
                    Set_Voice_Type_Index(Voice_Type_Index);
                    Set_Item_Sound();
                    string Cache_Dir = Path.GetDirectoryName(Sub_Code.ExDir) + "/cache";
                    if (Directory.Exists(Cache_Dir))
                        Directory.Delete(Cache_Dir, true);
                    IsForceColorMode = true;
                }
            }
        }
        private void Sound_Delete_B_Click(object sender, EventArgs e)
        {
            if (Voice_Type_L.SelectedItem == null)
            {
                Sub_Code.Show_Message("イベント名が選択されていません。");
                return;
            }
            if (Sound_File_L.SelectedItem == null)
            {
                Sub_Code.Show_Message("削除したいサウンドを選択してください。");
                return;
            }
            int Remove_Index = -1;
            int Voice_Type_Index = ((Voice_Type_List)Voice_Type_L.SelectedItem).Index;
            Voice_Sound_List Temp = (Voice_Sound_List)Sound_File_L.SelectedItem;
            if (Voice_Page == 0)
                Remove_Index = Voice_One_Full_Name[Voice_Type_Index].IndexOf(Temp);
            else if (Voice_Page == 1)
                Remove_Index = Voice_Two_Full_Name[Voice_Type_Index].IndexOf(Temp);
            else if (Voice_Page == 2)
                Remove_Index = Voice_Three_Full_Name[Voice_Type_Index].IndexOf(Temp);
            if (Remove_Index == -1)
            {
                Sub_Code.Show_Message("不明なエラーが発生しました。");
                return;
            }
            if (Voice_Page == 0)
            {
                Voice_One_Full_Name[Voice_Type_Index].RemoveAt(Remove_Index);
                Voice_One_Types[Voice_Type_Index].Count--;
            }
            else if (Voice_Page == 1)
            {
                Voice_Two_Full_Name[Voice_Type_Index].RemoveAt(Remove_Index);
                Voice_Two_Types[Voice_Type_Index].Count--;
            }
            else if (Voice_Page == 2)
            {
                Voice_Three_Full_Name[Voice_Type_Index].RemoveAt(Remove_Index);
                Voice_Three_Types[Voice_Type_Index].Count--;
            }
            WVS_File.Delete_Sound(Voice_Page, Voice_Type_Index, Remove_Index);
            Set_Item_Type();
            Set_Voice_Type_Index(Voice_Type_Index);
            Set_Item_Sound();
            Delete_Sound_File();
            IsForceColorMode = true;
        }
        private void Voice_Type_ViewCell_Tapped(object sender, EventArgs e)
        {
            if (IsForceColorMode)
                Clear_Voice_Type_Color();
            else if (Voice_Type_LastCell != null)
                Voice_Type_LastCell.View.BackgroundColor = Color.Transparent;
            ViewCell viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#82bfc8");
                Voice_Type_LastCell = viewCell;
            }
            Set_Item_Sound();
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
        private void Set_Voice_Type_Index(int Index)
        {
            if (Voice_Page == 0 && Voice_One_Types.Count - 1 >= Index)
                Voice_Type_L.SelectedItem = Voice_One_Types[Index];
            else if (Voice_Page == 1 && Voice_Two_Types.Count - 1 >= Index)
                Voice_Type_L.SelectedItem = Voice_Two_Types[Index];
            else if (Voice_Page == 2 && Voice_Three_Types.Count - 1 >= Index)
                Voice_Type_L.SelectedItem = Voice_Three_Types[Index];
            else
                return;
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = Voice_Type_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(Voice_Type_L);
                int Count = 0;
                foreach (ViewCell cell in cells as ITemplatedItemsList<Cell>)
                {
                    cell.View.BackgroundColor = cell.BindingContext != null && Count == Index ? Color.FromHex("#82bfc8") : Color.Transparent;
                    Count++;
                }
            }
        }
        private void Clear_Voice_Type_Color()
        {
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = Voice_Type_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(Voice_Type_L);
                foreach (ViewCell cell in cells as ITemplatedItemsList<Cell>)
                    if (cell.BindingContext != null)
                        cell.View.BackgroundColor = Color.Transparent;
            }
            IsForceColorMode = false;
        }
        private async void Create_B_Click(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            IsPageOpen = true;
            Create_Project_Window.Set_Project_Name(Project_Name);
            await Navigation.PushAsync(Create_Project_Window);
        }
        private async void Clear_B_Click(object sender, EventArgs e)
        {
            bool Result = await DisplayAlert("確認", "内容をクリアしますか?", "はい", "いいえ");
            if (Result)
            {
                Init_Voice_Type();
                Set_Item_Type();
                WVS_File.Dispose();
                Project_Name = "";
                Sub_Code.Delete_Voice_Mods();
                Sub_Code.Show_Message("クリアしました。");
            }
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            IsPageOpen = false;
        }
        public void Selected_Files()
        {
            int Voice_Type_Index = ((Voice_Type_List)Voice_Type_L.SelectedItem).Index;
            bool IsExist = false;
            foreach (string file_result in Sub_Code.Select_Files_Window.Get_Select_Files())
            {
                if (Voice_Page == 0 && !Voice_One_Full_Name[Voice_Type_Index].Select(x => x.Full_Name).Contains(file_result))
                {
                    Voice_One_Full_Name[Voice_Type_Index].Add(new Voice_Sound_List(file_result));
                    Voice_One_Types[Voice_Type_Index].Count++;
                }
                else if (Voice_Page == 1 && !Voice_Two_Full_Name[Voice_Type_Index].Select(x => x.Full_Name).Contains(file_result))
                {
                    Voice_Two_Full_Name[Voice_Type_Index].Add(new Voice_Sound_List(file_result));
                    Voice_Two_Types[Voice_Type_Index].Count++;
                }
                else if (Voice_Page == 1 && !Voice_Three_Full_Name[Voice_Type_Index].Select(x => x.Full_Name).Contains(file_result))
                {
                    Voice_Three_Full_Name[Voice_Type_Index].Add(new Voice_Sound_List(file_result));
                    Voice_Three_Types[Voice_Type_Index].Count++;
                }
                else
                    IsExist = true;
            }
            if (IsExist)
                Sub_Code.Show_Message("既に追加されているファイルが存在します。");
            Set_Item_Type();
            Set_Voice_Type_Index(Voice_Type_Index);
            Set_Item_Sound();
            IsForceColorMode = true;
        }
        public void Load_File()
        {
            foreach (string file_result in Sub_Code.Select_Files_Window.Get_Select_Files())
                Voice_Load_From_File(file_result);
            Sub_Code.Delete_Voice_Mods();
            Sub_Code.Show_Message("ロードしました。");
        }
        private void Delete_Sound_File()
        {
            if (Directory.Exists(Sub_Code.ExDir + "/Temp/Voice_Mods"))
            {
                List<string> Music_Files = new List<string>();
                foreach (List<Voice_Sound_List> a in Voice_One_Full_Name)
                    foreach (Voice_Sound_List b in a)
                        if (!Music_Files.Contains(Path.GetFileName(b.Full_Name)))
                            Music_Files.Add(Path.GetFileName(b.Full_Name));
                foreach (List<Voice_Sound_List> a in Voice_Two_Full_Name)
                    foreach (Voice_Sound_List b in a)
                        if (!Music_Files.Contains(Path.GetFileName(b.Full_Name)))
                            Music_Files.Add(Path.GetFileName(b.Full_Name));
                foreach (List<Voice_Sound_List> a in Voice_Three_Full_Name)
                    foreach (Voice_Sound_List b in a)
                        if (!Music_Files.Contains(Path.GetFileName(b.Full_Name)))
                            Music_Files.Add(Path.GetFileName(b.Full_Name));
                foreach (string File_Now in Directory.GetFiles(Sub_Code.ExDir + "/Temp/Voice_Mods", "*.*", SearchOption.TopDirectoryOnly))
                    if (!Music_Files.Contains(Path.GetFileName(File_Now)))
                        File.Delete(File_Now);
                Music_Files.Clear();
            }
        }
    }
}