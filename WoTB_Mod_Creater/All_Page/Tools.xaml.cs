using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WoTB_Mod_Creater.Class;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.All_Page
{
    public partial class Tools : ContentPage
    {
        private SFTP_Client SFTPClient = null;
        private bool IsPageOpen = false;
        private bool IsMessageShowing = false;
        public Tools()
        {
            InitializeComponent();
            DVPL_UnPack_B.Clicked += DVPL_UnPack_B_Clicked;
            DVPL_Pack_B.Clicked += DVPL_Pack_B_Clicked;
            Input_Image_B.Clicked += Input_Image_B_Clicked;
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
        private async void DVPL_UnPack_B_Clicked(object sender, EventArgs e)
        {
            if (Sub_Code.IsUseSelectPage && !IsPageOpen && Sub_Code.HasSystemPermission)
            {
                IsPageOpen = true;
                Sub_Code.Select_Files_Window.Window_Show("DVPL_Unpack", "", ".dvpl");
                await Navigation.PushModalAsync(Sub_Code.Select_Files_Window);
            }
            else
            {
                PickOptions options = new PickOptions
                {
                    PickerTitle = "ファイルを選択してください。"
                };
                IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(options);
                if (result != null)
                {
                    bool HasError = false;
                    if (!Directory.Exists(Sub_Code.ExDir + "/DVPL"))
                        Directory.CreateDirectory(Sub_Code.ExDir + "/DVPL");
                    int Count = 0;
                    foreach (FileResult file_result in result)
                    {
                        if (Path.GetExtension(file_result.FileName) == ".dvpl")
                        {
                            try
                            {
                                Stream File_Stream = await file_result.OpenReadAsync();
                                byte[] Buffer = new byte[File_Stream.Length];
                                File_Stream.Read(Buffer, 0, Buffer.Length);
                                DVPL.DVPL_UnPack(Buffer, Sub_Code.ExDir + "/DVPL/" + Path.GetFileNameWithoutExtension(file_result.FullPath));
                                File_Stream.Close();
                                Count++;
                            }
                            catch (Exception e1)
                            {
                                Sub_Code.Error_Log_Write(e1.Message);
                                HasError = true;
                            }
                        }
                    }
                    if (Count == 0)
                        Message_Feed_Out(".dvplファイルを選択してください。");
                    else
                    {
                        string Message_01 = Count + "個のファイルのdvplを解除しました。";
                        if (HasError)
                            Message_01 += "(エラーあり)";
                        Message_01 += "\n/DVPLフォルダに該当するファイルが生成されています。";
                        Message_Feed_Out(Message_01);
                    }
                }
            }
        }
        public void UnPack_Selected_Files()
        {
            List<string> Selected_Files = Sub_Code.Select_Files_Window.Get_Select_Files();
            if (Selected_Files.Count == 0)
                return;
            int Count = 0;
            bool HasError = false;
            foreach (string file_result in Selected_Files)
            {
                if (Path.GetExtension(file_result) == ".dvpl")
                {
                    try
                    {
                        Stream File_Stream = File.OpenRead(file_result);
                        byte[] Buffer = new byte[File_Stream.Length];
                        File_Stream.Read(Buffer, 0, Buffer.Length);
                        DVPL.DVPL_UnPack(Buffer, Sub_Code.ExDir + "/DVPL/" + Path.GetFileNameWithoutExtension(file_result));
                        File_Stream.Close();
                        Count++;
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                        HasError = true;
                    }
                }
            }
            string Message_01 = Count + "個のファイルのdvplを解除しました。";
            if (HasError)
                Message_01 += "(エラーあり)";
            Message_01 += "\n/DVPLフォルダに該当するファイルが生成されています。";
            Message_Feed_Out(Message_01);
        }
        private async void DVPL_Pack_B_Clicked(object sender, EventArgs e)
        {
            if (Sub_Code.IsUseSelectPage && !IsPageOpen && Sub_Code.HasSystemPermission)
            {
                IsPageOpen = true;
                Sub_Code.Select_Files_Window.Window_Show("DVPL_Pack");
                await Navigation.PushModalAsync(Sub_Code.Select_Files_Window);
            }
            else
            {
                PickOptions options = new PickOptions
                {
                    PickerTitle = "ファイルを選択してください。"
                };
                IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(options);
                if (result != null)
                {
                    bool HasError = false;
                    if (!Directory.Exists(Sub_Code.ExDir + "/DVPL"))
                        Directory.CreateDirectory(Sub_Code.ExDir + "/DVPL");
                    int Count = 0;
                    foreach (FileResult file_result in result)
                    {
                        try
                        {
                            Stream File_Stream = await file_result.OpenReadAsync();
                            byte[] Buffer = new byte[File_Stream.Length];
                            File_Stream.Read(Buffer, 0, Buffer.Length);
                            DVPL.DVPL_Pack(Buffer, Sub_Code.ExDir + "/DVPL/" + file_result.FileName + ".dvpl", file_result.FileName.Contains(".tex.dvpl"));
                            File_Stream.Close();
                            Count++;
                        }
                        catch (Exception e1)
                        {
                            Sub_Code.Error_Log_Write(e1.Message);
                            HasError = true;
                        }
                    }
                    string Message_01 = Count + "個のファイルをdvpl化しました。";
                    if (HasError)
                        Message_01 += "(エラーあり)";
                    Message_01 += "\n/DVPLフォルダに該当するファイルが生成されています。";
                    Message_Feed_Out(Message_01);
                }
            }
        }
        public void Pack_Selected_Files()
        {
            List<string> Selected_Files = Sub_Code.Select_Files_Window.Get_Select_Files();
            if (Selected_Files.Count == 0)
                return;
            int Count = 0;
            bool HasError = false;
            foreach (string file_result in Selected_Files)
            {
                try
                {
                    Stream File_Stream = File.OpenRead(file_result);
                    byte[] Buffer = new byte[File_Stream.Length];
                    File_Stream.Read(Buffer, 0, Buffer.Length);
                    DVPL.DVPL_Pack(Buffer, Sub_Code.ExDir + "/DVPL/" + file_result + ".dvpl", file_result.EndsWith(".tex.dvpl"));
                    File_Stream.Close();
                    Count++;
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                    HasError = true;
                }
            }
            string Message_01 = Count + "個のファイルをdvpl化しました。";
            if (HasError)
                Message_01 += "(エラーあり)";
            Message_01 += "\n/DVPLフォルダに該当するファイルが生成されています。";
            Message_Feed_Out(Message_01);
        }
        private async void Input_Image_B_Clicked(object sender, EventArgs e)
        {
            if (Sub_Code.IsUseSelectPage && !IsPageOpen && Sub_Code.HasSystemPermission)
            {
                IsPageOpen = true;
                string Ex = ".png|.jpg|.bmp|.webp";
                Sub_Code.Select_Files_Window.Window_Show("Input_Image", "", Ex);
                await Navigation.PushModalAsync(Sub_Code.Select_Files_Window);
            }
            else
            {
                PickOptions options = new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "ファイルを選択してください。",
                };
                FileResult result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    try
                    {
                        string ID_Name = WwiseHash.HashString(result.FullPath) + Path.GetExtension(result.FileName);
                        Stream File_Stream = await result.OpenReadAsync();
                        byte[] Buffer = new byte[File_Stream.Length];
                        File_Stream.Read(Buffer, 0, Buffer.Length);
                        IsMessageShowing = false;
                        await Task.Delay(100);
                        Message_T.Text = "画像ファイルを送信しています...";
                        SFTPClient = new SFTP_Client(SRTTbacon_Server.IP, SRTTbacon_Server.Name, SRTTbacon_Server.Password, SRTTbacon_Server.SFTP_Port);
                        await Task.Delay(50);
                        if (!SFTPClient.Directory_Exist("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Images"))
                        {
                            _ = SFTPClient.Directory_Create("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName);
                            _ = SFTPClient.Directory_Create("/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Images");
                        }
                        SFTPClient.SFTP_Server.UploadFile(File_Stream, "/WoTB_Voice_Mod/Android/Users/" + Sub_Code.UserName + "/Images/" + ID_Name);
                        Sub_Code.Main_Server.TCP_Server.Send("AI_Images|" + Sub_Code.UserName + "|" + ID_Name);
                    }
                    catch (Exception e1)
                    {
                        Sub_Code.Error_Log_Write(e1.Message);
                    }
                    Message_Feed_Out("");
                }
            }
        }
    }
}