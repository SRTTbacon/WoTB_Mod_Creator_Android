using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using WoTB_Mod_Creater.Class;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.All_Page
{
    public class Select_File_List : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Type_Mods_List Info { get; private set; }
        public string Name { get; private set; }
        public string Full_Path { get; private set; }
        public bool IsDirectory { get; private set; }
        public bool IsVisible => !IsDirectory;
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
                }
            }
        }
        public Color Name_Color => IsDirectory ? Color.Purple : Color.Aqua;
        public Select_File_List(string Name, bool IsDirectory, string Full_Path = "",Type_Mods_List Info = null)
        {
            this.Name = Name;
            this.IsDirectory = IsDirectory;
            this.Full_Path = Full_Path;
            this.Info = Info;
        }
    }
    public partial class Select_Files : ContentPage
    {
        public delegate void Select_File_EventHandler<T>(T args);
        public event Select_File_EventHandler<string> Selected;
        private readonly List<string> Search_Option = new List<string>();
        private readonly List<string> Back_Dirs = new List<string>();
        private readonly Dictionary<string, string> Page_Dirs = new Dictionary<string, string>();
        private List<Select_File_List> File_Dir_List = new List<Select_File_List>();
        private string Page_Name = "";
        private string Dir = "";
        private string Bottom_Dir = "";
        private int Mod_Page = 0;
        private bool IsLoaded = false;
        private bool IsMultiSelect = true;
        private bool IsModSelect = false;
        private bool IsSelectedMod = false;
        public Select_Files()
        {
            InitializeComponent();
            Dir_Back_B.Clicked += Dir_Back_B_Clicked;
            Select_Mod_Page_B.Clicked += Select_Mod_Page_B_Clicked;
            Cancel_B.Clicked += Cancel_B_Clicked;
            OK_B.Clicked += OK_B_Clicked;
            Files_L.ItemSelected += Files_L_ItemSelected;
        }
        public void Window_Show(string Page_Name, string Dir = "", string Search_Option = "", bool IsNoback = false, bool IsMultiSelect = true, string Bottom_Dir = "")
        {
            IsModSelect = false;
            Select_Mod_Page_B.IsVisible = false;
            Dispose();
            if (!IsLoaded)
            {
                IsLoaded = true;
                if (File.Exists(Sub_Code.ExDir + "/Configs/Select_Files_Dir.dat"))
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Sub_Code.ExDir + "/Configs/Select_Files_Dir.dat", "GuminoAme_Select_Files_Dir_Save");
                    string line;
                    while ((line = str.ReadLine()) != null)
                    {
                        if (line.Contains("|"))
                        {
                            string Page = line.Split('|')[0];
                            string Page_Dir = line.Split('|')[1];
                            Page_Dirs.Add(Page, Page_Dir);
                        }
                    }
                    str.Close();
                }
            }
            string Select_Dir = Dir;
            if (Search_Option != "")
            {
                string[] Options = Search_Option.Split('|');
                foreach (string Option in Options)
                    this.Search_Option.Add(Option);
            }
            if (IsNoback)
                Dir_Back_B.IsVisible = false;
            else
                Dir_Back_B.IsVisible = true;
            this.IsMultiSelect = IsMultiSelect;
            this.Page_Name = Page_Name;
            this.Bottom_Dir = Bottom_Dir;
            if (Dir == "" && Page_Dirs.ContainsKey(Page_Name))
                Select_Dir = Page_Dirs[Page_Name];
            Change_Dir(Select_Dir);
        }
        public void Window_Show(string Page_Name, bool IsModSelect)
        {
            this.IsModSelect = IsModSelect;
            if (IsModSelect)
            {
                Sub_Code.Created_Mods_Window.Init();
                Select_Mod_Page_B.IsVisible = true;
                this.Page_Name = Page_Name;
                Change_Mod_Type();
            }
            else
                Window_Show(Page_Name);
        }
        public List<string> Get_Select_Files()
        {
            List<string> Temp = new List<string>();
            foreach (Select_File_List Select_File in File_Dir_List)
            {
                if (!Select_File.IsDirectory && Select_File.IsChecked)
                {
                    if (IsModSelect)
                        Temp.Add(Select_File.Full_Path);
                    else
                        Temp.Add(Dir + "/" + Select_File.Name);
                }
            }
            return Temp;
        }
        protected override bool OnBackButtonPressed()
        {
            if (Back_Dirs.Count == 0)
            {
                if (!IsModSelect)
                    Save_Page_Dir();
                Dispose();
                return base.OnBackButtonPressed();
            }
            else
            {
                Change_Dir(Back_Dirs[Back_Dirs.Count - 1]);
                Back_Dirs.RemoveAt(Back_Dirs.Count - 1);
                return true;
            }
        }
        private void Files_L_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItemIndex == -1)
                return;
            Select_File_List Temp = File_Dir_List[e.SelectedItemIndex];
            if (!Temp.IsDirectory)
            {
                bool IsChecked = Temp.IsChecked;
                if (!IsMultiSelect)
                    foreach (Select_File_List a in File_Dir_List)
                        a.IsChecked = false;
                Temp.IsChecked = !IsChecked;
                Files_L.SelectedItem = null;
                OnPropertyChanged("SelectedItem");
                IEnumerable<PropertyInfo> pInfos = Files_L.GetType().GetRuntimeProperties();
                PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
                if (templatedItems != null)
                {
                    object cells = templatedItems.GetValue(Files_L);
                    foreach (ViewCell cell in cells as ITemplatedItemsList<Cell>)
                        cell.View.BackgroundColor = Color.Transparent;
                }
            }
            else
            {
                if (IsModSelect)
                    Change_Mod_Info(Temp.Info);
                else
                {
                    Back_Dirs.Add(Dir);
                    Change_Dir(Dir + "/" + Temp.Name);
                }
            }
        }
        private void Change_Dir(string Dir)
        {
            if (!string.IsNullOrWhiteSpace(Dir) && Directory.Exists(Dir))
                this.Dir = Dir;
            else
                this.Dir = "/storage/emulated/0";
            Files_L.SelectedItem = null;
            Files_L.ItemsSource = null;
            File_Dir_List.Clear();
            string[] Dirs = Directory.GetDirectories(this.Dir, "*", SearchOption.TopDirectoryOnly);
            foreach (string Dir_Now in Dirs)
                File_Dir_List.Add(new Select_File_List(Path.GetFileName(Dir_Now), true));
            IEnumerable<string> Files;
            if (Search_Option.Count > 0)
                Files = Directory.EnumerateFiles(this.Dir, "*.*", SearchOption.TopDirectoryOnly).Where(file => Search_Option.Any(pattern => file.ToLower().EndsWith(pattern)));
            else
                Files = Directory.GetFiles(this.Dir, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string File_Now in Files)
                File_Dir_List.Add(new Select_File_List(Path.GetFileName(File_Now), false));
            File_Dir_List = File_Dir_List.OrderBy(h => !h.IsDirectory).ThenBy(h => h.Name).ToList();
            Files_L.ItemsSource = File_Dir_List;
            Dir_T.Text = (this.Dir + "/").Replace("/storage/emurated/0", "");
        }
        private void Dir_Back_B_Clicked(object sender, EventArgs e)
        {
            if (IsModSelect)
            {
                if (IsSelectedMod)
                    Change_Mod_Type();
            }
            else
            {
                if (Dir != "/storage/emulated/0" && Dir != Bottom_Dir)
                {
                    string Next_Dir = Directory.GetParent(Dir).FullName;
                    Change_Dir(Next_Dir);
                    Back_Dirs.Add(Next_Dir);
                }
                else
                    Sub_Code.Show_Message("これより下の階層は閲覧できません。");
            }
        }
        private void Cancel_B_Clicked(object sender, EventArgs e)
        {
            Save_Page_Dir();
            Dispose();
            Navigation.PopModalAsync();
        }
        private void OK_B_Clicked(object sender, EventArgs e)
        {
            if (!IsModSelect)
                Save_Page_Dir();
            Navigation.PopModalAsync();
            Selected(Page_Name);
        }
        public void Dispose()
        {
            File_Dir_List.Clear();
            Search_Option.Clear();
            Back_Dirs.Clear();
            Files_L.SelectedItem = null;
            Files_L.ItemsSource = null;
            Dir = "";
            IsSelectedMod = false;
        }
        private void Change_Page_Dir()
        {
            if (Page_Dirs.ContainsKey(Page_Name))
                Page_Dirs[Page_Name] = Dir;
            else
                Page_Dirs.Add(Page_Name, Dir);
        }
        private void Save_Page_Dir()
        {
            Change_Page_Dir();
            StreamWriter stw = File.CreateText(Sub_Code.ExDir + "/Configs/Select_Files_Dir.tmp");
            foreach (string Key in Page_Dirs.Keys)
                stw.WriteLine(Key + "|" + Page_Dirs[Key]);
            stw.Close();
            Sub_Code.File_Encrypt(Sub_Code.ExDir + "/Configs/Select_Files_Dir.tmp", Sub_Code.ExDir + "/Configs/Select_Files_Dir.dat", "GuminoAme_Select_Files_Dir_Save", true);
        }
        private void Change_Mod_Type()
        {
            Files_L.SelectedItem = null;
            Files_L.ItemsSource = null;
            File_Dir_List.Clear();
            foreach (Type_Mods_List Info in Sub_Code.Created_Mods_Window.Type_Mods[Mod_Page])
            {
                if (Info.IsBuilt)
                    File_Dir_List.Add(new Select_File_List(Info.Name, true, "", Info));
            }
            IsSelectedMod = false;
            Files_L.ItemsSource = File_Dir_List;
        }
        private void Change_Mod_Info(Type_Mods_List Info)
        {
            string Project_Dir = "/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Voice_Projects/" + Info.ID;
            if (Info.ModType == 1)
                Project_Dir = "/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Other_Projects/" + Info.ID;
            Files_L.SelectedItem = null;
            Files_L.ItemsSource = null;
            File_Dir_List.Clear();
            foreach (string File_Result in Sub_Code.Main_Server.SFTPClient.GetFiles(Project_Dir, true, false))
                File_Dir_List.Add(new Select_File_List(Path.GetFileName(File_Result), false, File_Result));
            IsSelectedMod = true;
            Files_L.ItemsSource = File_Dir_List;
        }
        private async void Select_Mod_Page_B_Clicked(object sender, EventArgs e)
        {
            string[] Set_Types = { "音声Mod", "その他のサウンドMod" };
            string Name = await DisplayActionSheet("表示タイプを選択してください。", "キャンセル", null, Set_Types);
            if (Name == null || Name == "キャンセル")
                return;
            string Type_Name = "音声Mod";
            if (Name == "音声Mod")
                Mod_Page = 0;
            else if (Name == "その他のサウンドMod")
            {
                Mod_Page = 1;
                Type_Name = "その他";
            }
            Select_Mod_Page_B.Text = "表示タイプ:" + Type_Name;
            Change_Mod_Type();
        }
    }
}