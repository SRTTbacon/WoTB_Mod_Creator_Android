using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using WoTB_Mod_Creater.Class;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.All_Page.Mod_Distribution
{
    public class Mod_Upload_List
    {
        public string Full_Name { get; private set; }
        public string Name_Text => Path.GetFileName(Full_Name);
        public Mod_Upload_List(string Full_Name)
        {
            this.Full_Name = Full_Name;
        }
    }
    public partial class Mod_Upload : ContentPage
    {
        private readonly List<Mod_Upload_List> Mod_Files = new List<Mod_Upload_List>();
        private SFTP_Client SFTPClient = null;
        private ViewCell Mod_File_L_LastCell = null;
        private bool IsPageOpen = false;
        private bool IsMessageShowing = false;
        private bool IsUploading = false;
        public Mod_Upload()
        {
            InitializeComponent();
            Add_B.Clicked += Add_B_Clicked;
            Remove_B.Clicked += Remove_B_Clicked;
            Upload_B.Clicked += Upload_B_Clicked;
            Password_C.CheckedChanged += Password_C_CheckedChanged;
            Password_T.IsVisible = false;
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
        private void Remove_B_Clicked(object sender, EventArgs e)
        {
            if (IsUploading)
                return;
            if (Mod_File_L.SelectedItem == null)
            {
                Message_Feed_Out("削除する項目が選択されていません。");
                return;
            }
            Mod_Upload_List Item = (Mod_Upload_List)Mod_File_L.SelectedItem;
            Mod_Files.Remove(Item);
            Update_File_List();
        }
        private async void Add_B_Clicked(object sender, EventArgs e)
        {
            if (!IsPageOpen && !IsUploading)
            {
                IsPageOpen = true;
                Sub_Code.Select_Files_Window.Window_Show("Mod_Upload", true);
                await Navigation.PushModalAsync(Sub_Code.Select_Files_Window);
            }
        }
        private async void Upload_B_Clicked(object sender, EventArgs e)
        {
            if (IsUploading)
                return;
            if (Mod_Files.Count == 0)
            {
                Message_Feed_Out("最低1つはファイルが必要です。");
                return;
            }
            if (Mod_Name_T.Text == "")
            {
                Message_Feed_Out("Mod名が指定されていません。");
                return;
            }
            try
            {
                Directory.CreateDirectory(Sub_Code.ExDir + "/" + Mod_Name_T.Text);
                Directory.Delete(Sub_Code.ExDir + "/" + Mod_Name_T.Text);
                if (Mod_Name_T.Text.Contains("/") || Mod_Name_T.Text.Contains("\\"))
                {
                    Message_Feed_Out("Mod名に不適切な文字が含まれています。");
                    return;
                }
            }
            catch
            {
                Message_Feed_Out("Mod名に不適切な文字が含まれています。");
                return;
            }
            if (Mod_Name_T.Text.CountOf("  ") > 0)
            {
                Message_Feed_Out("Mod名に空白を2つ続けて付けることはできません。");
                return;
            }
            if (Mod_Name_T.Text == "Backup")
            {
                Message_Feed_Out("そのMod名は別の目的に使用されています。");
                return;
            }
            if (Sub_Code.Main_Server.SFTPClient.Directory_Exist("/WoTB_Voice_Mod/Mods/" + Mod_Name_T.Text))
            {
                Message_Feed_Out("同名のModが既に存在します。");
                return;
            }
            try
            {
                IsUploading = true;
                Message_T.Text = "準備しています...";
                await Task.Delay(50);
                //Modの情報をXMLファイルに書き込む
                XDocument xml = new XDocument();
                XElement datas = new XElement("Mod_Upload_Config",
                new XElement("IsBGMMode", BGM_C.IsChecked),
                new XElement("IsPassword", Password_C.IsChecked),
                new XElement("IsEnableR18", R18_C.IsChecked),
                new XElement("UserName", Sub_Code.UserName),
                new XElement("Explanation", Explanation_T.Text),
                new XElement("Password", Password_T.Text));
                xml.Add(datas);
                xml.Save(Sub_Code.ExDir + "/Temp_Create_Mod.dat");
                //Mod情報をアップロード
                SFTPClient = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                //サーバーにフォルダを作成
                SFTPClient.Directory_Create("/WoTB_Voice_Mod/Mods/" + Mod_Name_T.Text);
                SFTPClient.Directory_Create("/WoTB_Voice_Mod/Mods/" + Mod_Name_T.Text + "/Files");
                SFTPClient.UploadFile(Sub_Code.ExDir + "/Temp_Create_Mod.dat", "/WoTB_Voice_Mod/Mods/" + Mod_Name_T.Text + "/Configs.dat");
                File.Delete(Sub_Code.ExDir + "/Temp_Create_Mod.dat");
                await Task.Delay(50);
                //Mod本体をアップロード
                string File_Path = "";
                foreach (string Upload_File in Mod_Files.Select(h => h.Full_Name))
                    File_Path += "|" + Upload_File;
                SFTPClient.File_Append("/WoTB_Voice_Mod/Mods/Mod_Names_Wwise.dat", Mod_Name_T.Text + "\n");
                SFTPClient.Close();
                Sub_Code.Main_Server.TCP_Server.Send("Response|Mod_Upload|" + Sub_Code.UserName + "|" + Mod_Name_T.Text + File_Path);
                Sub_Code.Main_Server.TCP_Server.Send("Message|" + Sub_Code.UserName + "(Android)->Mod名:" + Mod_Name_T.Text + "を公開しました。");
                Message_Feed_Out("Modを公開しました。");
            }
            catch (Exception e1)
            {
                Message_Feed_Out("エラーが発生しました。");
                Sub_Code.Error_Log_Write(e1.Message);
            }
            IsUploading = false;
        }
        private void Password_C_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Password_T.IsVisible = e.Value;
        }
        private void Mod_File_L_Tapped(object sender, EventArgs e)
        {
            if (Mod_File_L_LastCell != null)
                Mod_File_L_LastCell.View.BackgroundColor = Color.Transparent;
            ViewCell viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#82bfc8");
                Mod_File_L_LastCell = viewCell;
            }
        }
        private void Mod_File_L_No_Select()
        {
            Mod_File_L.SelectedItem = null;
            Mod_File_L_LastCell = null;
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = Mod_File_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(Mod_File_L);
                foreach (ViewCell cell in cells as ITemplatedItemsList<Cell>)
                    if (cell.BindingContext != null)
                        cell.View.BackgroundColor = Color.Transparent;
            }
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            IsPageOpen = false;
        }
        public void Selected_Files()
        {
            bool IsExist = false;
            foreach (string file_result in Sub_Code.Select_Files_Window.Get_Select_Files())
            {
                string Name_Only = Path.GetFileName(file_result);
                if (!Mod_Files.Select(h => h.Name_Text).Contains(Name_Only))
                    Mod_Files.Add(new Mod_Upload_List(file_result));
                else
                    IsExist = true;
            }
            if (IsExist)
                Message_Feed_Out("既に追加されているファイルが存在します。");
            Update_File_List();
        }
        private void Update_File_List()
        {
            Mod_File_L.ItemsSource = null;
            Mod_File_L.ItemsSource = Mod_Files;
            Mod_File_L_No_Select();
        }
        private void OnBGMModClicked(object sender, EventArgs e)
        {
            BGM_C.IsChecked = !BGM_C.IsChecked;
        }
        private void OnR18Clicked(object sender, EventArgs e)
        {
            R18_C.IsChecked = !R18_C.IsChecked;
        }
        private void OnPasswordClicked(object sender, EventArgs e)
        {
            Password_C.IsChecked = !Password_C.IsChecked;
        }
    }
}