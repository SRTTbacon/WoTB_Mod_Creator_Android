using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using WoTB_Mod_Creater.Class;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.All_Page
{
    public partial class Other_Mods_Setting : ContentPage
    {
        private ViewCell Sound_File_LastCell = null;
        private int Type_Index = -1;
        private int Stream;
        private bool IsLoaded = false;
        private bool IsEnded = false;
        private bool IsPaused = false;
        private bool IsLocationChanging = false;
        private bool IsMessageShowing = false;
        private List<Other_File_Type> Before_Data = null;
        private SYNCPROC IsMusicEnd;
        public Other_Mods_Setting()
        {
            InitializeComponent();
            Volume_S.Value = 100;
            Location_S.DragStarted += Location_S_DragStart;
            Location_S.DragCompleted += Location_S_DragCompleted;
            Feed_In_C.CheckedChanged += Feed_In_C_CheckedChanged;
            Volume_S.ValueChanged += Volume_S_ValueChanged;
            Volume_S.DragCompleted += Volume_S_DragCompleted;
            Feed_Help_B.Clicked += Feed_Help_B_Clicked;
            Pause_B.Clicked += Pause_B_Clicked;
            Play_B.Clicked += Play_B_Clicked;
            Minus_B.Clicked += Minus_B_Clicked;
            Plus_B.Clicked += Plus_B_Clicked;
            Time_Start_B.Clicked += Time_Start_B_Clicked;
            Time_End_B.Clicked += Time_End_B_Clicked;
            Time_Clear_B.Clicked += Time_Clear_B_Clicked;
            Cancel_B.Clicked += Cancel_B_Clicked;
            OK_B.Clicked += OK_B_Clicked;
        }
        protected override bool OnBackButtonPressed()
        {
            Sound_File_L.SelectedItem = null;
            Sound_File_L.ItemsSource = null;
            if (Sound_File_LastCell != null)
                Sound_File_LastCell.View.BackgroundColor = Color.Transparent;
            if (Before_Data != null)
            {
                Sub_Code.Other_Create_Window.Other_List[0][Type_Index].Files = new List<Other_File_Type>(Before_Data);
                Before_Data.Clear();
                Before_Data = null;
            }
            Pause_Volume_Animation(true, 10f);
            return base.OnBackButtonPressed();
        }
        //曲の位置を更新(30FPSで動作)
        private async void Position_Change()
        {
            double nextFrame = Environment.TickCount;
            float period = 1000f / 30f;
            while (IsVisible)
            {
                int tickCount = Environment.TickCount;
                if (tickCount < nextFrame)
                {
                    if (nextFrame - tickCount > 1)
                        await Task.Delay((int)(nextFrame - tickCount));
                    continue;
                }
                //曲が終わったら開始位置に戻る
                if (IsEnded)
                {
                    int Index = Get_Select_Index();
                    IsPaused = true;
                    if (Index != -1 && Sound_File_L.SelectedItem != null)
                    {
                        Location_S.Value = Sub_Code.Other_Create_Window.Other_List[0][Type_Index].Files[Index].Music_Time.Start_Time;
                        Music_Pos_Change(Location_S.Value, true);
                        Bass.BASS_ChannelPause(Stream);
                    }
                    IsEnded = false;
                }
                //曲が再生中だったら
                if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING && !IsLocationChanging)
                {
                    int Index = Get_Select_Index();
                    long position = Bass.BASS_ChannelGetPosition(Stream);
                    Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
                    if (Index != -1)
                    {
                        double End_Time = Sub_Code.Other_Create_Window.Other_List[0][Type_Index].Files[Index].Music_Time.End_Time;
                        if (End_Time != 0 && Location_S.Value >= End_Time)
                        {
                            Music_Pos_Change(Sub_Code.Other_Create_Window.Other_List[0][Type_Index].Files[Index].Music_Time.Start_Time, true);
                            long position2 = Bass.BASS_ChannelGetPosition(Stream);
                            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                        }
                        else if (Location_S.Value < Sub_Code.Other_Create_Window.Other_List[0][Type_Index].Files[Index].Music_Time.Start_Time)
                        {
                            Music_Pos_Change(Sub_Code.Other_Create_Window.Other_List[0][Type_Index].Files[Index].Music_Time.Start_Time, true);
                            long position2 = Bass.BASS_ChannelGetPosition(Stream);
                            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position2);
                        }
                    }
                    //テキストボックスに曲の現在時間を表示
                    //例:00:05 : 01:21など
                    TimeSpan Time = TimeSpan.FromSeconds(Location_S.Value);
                    string Minutes = Time.Minutes.ToString();
                    string Seconds = Time.Seconds.ToString();
                    if (Time.Minutes < 10)
                        Minutes = "0" + Time.Minutes;
                    if (Time.Seconds < 10)
                        Seconds = "0" + Time.Seconds;
                    Location_T.Text = Minutes + ":" + Seconds;
                }
                else if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED && !IsLocationChanging && !IsPaused)
                {
                    Location_S.Value = 0;
                    Location_T.Text = "00:00";
                }
                IsEnded = false;
                if (Environment.TickCount >= nextFrame + period)
                {
                    nextFrame += period;
                    continue;
                }
                nextFrame += period;
            }
        }
        //曲が終わったら呼ばれる
        async void EndSync(int handle, int channel, int data, IntPtr user)
        {
            if (!IsEnded)
            {
                await Task.Delay(500);
                IsEnded = true;
            }
        }
        //メッセージを表示
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
        //ウィンドウを表示させる際、必ず実行させる
        public void Window_Show(int Type_Index)
        {
            this.Type_Index = Type_Index;
            Sound_File_L.ItemsSource = null;
            Sound_File_L.ItemsSource = Sub_Code.Other_Create_Window.Other_List[0][Type_Index].Files;
            Sound_File_L.SelectedItem = null;
            if (!IsLoaded && File.Exists(Sub_Code.ExDir + "/Configs/Other_Mods_Setting.conf"))
            {
                IsLoaded = true;
                StreamReader str = Sub_Code.File_Decrypt_To_Stream(Sub_Code.ExDir + "/Configs/Other_Mods_Setting.conf", "SRTTbacon_Android_Mod_Creater");
                Volume_S.Value = double.Parse(str.ReadLine());
                str.Close();
            }
            Before_Data = new List<Other_File_Type>();
            foreach (Other_File_Type Temp in Sub_Code.Other_Create_Window.Other_List[0][Type_Index].Files)
                Before_Data.Add(Temp.Clone());
            AiForms.Effects.Border.SetColor(Sound_File_L, Color.Aqua);
            AiForms.Effects.Border.SetRadius(Sound_File_L, 3);
            AiForms.Effects.Border.SetWidth(Sound_File_L, 1);
            Position_Change();
        }
        //設定を保存(音量のみ)
        private void Configs_Save()
        {
            StreamWriter stw = File.CreateText(Sub_Code.ExDir + "/Configs/Other_Mods_Setting.tmp");
            stw.Write(Volume_S.Value);
            stw.Close();
            Sub_Code.File_Encrypt(Sub_Code.ExDir + "/Configs/Other_Mods_Setting.tmp", Sub_Code.ExDir + "/Configs/Other_Mods_Setting.conf", "SRTTbacon_Android_Mod_Creater", true);
        }
        private async void Sound_List_Tapped(object sender, EventArgs e)
        {
            if (Sound_File_LastCell != null)
                Sound_File_LastCell.View.BackgroundColor = Color.Transparent;
            ViewCell viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#9982bfc8");
                Sound_File_LastCell = viewCell;
                Other_File_Type Temp = (Other_File_Type)Sound_File_L.SelectedItem;
                if (!File.Exists(Temp.Full_Path) && Temp.Full_Path.Contains("/"))
                {
                    Message_Feed_Out("ファイルが存在しませんでした。");
                    return;
                }
                if (Location_S.Maximum != 1)
                {
                    IsPaused = true;
                    float Volume_Now = 1f;
                    Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
                    float Volume_Minus = Volume_Now / 10f;
                    while (Volume_Now > 0f && IsPaused)
                    {
                        Volume_Now -= Volume_Minus;
                        if (Volume_Now < 0f)
                            Volume_Now = 0f;
                        Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                        await Task.Delay(1000 / 60);
                    }
                    if (Volume_Now <= 0f)
                    {
                        Bass.BASS_ChannelStop(Stream);
                        Bass.BASS_StreamFree(Stream);
                        Location_S.Value = 0;
                        Location_S.Maximum = 1;
                        Location_T.Text = "00:00";
                        Play_Time_T.Text = "再生時間:0～0";
                    }
                }
                Sub_Code.Delete_Temp_Files();
                string Sound_File;
                if (!Temp.Full_Path.Contains("/"))
                {
                    if (!Directory.Exists(Sub_Code.ExDir + "/Temp"))
                        Directory.CreateDirectory(Sub_Code.ExDir + "/Temp");
                    File.WriteAllBytes(Sub_Code.ExDir + "/Temp/Temp_" + Path.GetFileName(Temp.Full_Path), Sub_Code.Other_Create_Window.WMS_File.Get_Sound_Bytes(0, Type_Index, Get_Select_Index()));
                    Sound_File = Sub_Code.ExDir + "/Temp/Temp_" + Path.GetFileName(Temp.Full_Path);
                }
                else
                    Sound_File = Temp.Full_Path;
                int StreamHandle = Bass.BASS_StreamCreateFile(Sound_File, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP | BASSFlag.BASS_STREAM_PRESCAN);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                IsMusicEnd = new SYNCPROC(EndSync);
                Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
                Location_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
                double End_Time = Temp.Music_Time.End_Time;
                if (End_Time == 0)
                    End_Time = Location_S.Maximum;
                Play_Time_T.Text = "再生時間:" + (int)Temp.Music_Time.Start_Time + "～" + (int)End_Time;
                Feed_In_C.IsChecked = Temp.Music_Feed_In;
                IsPaused = true;
            }
        }
        private int Get_Select_Index()
        {
            if (Sound_File_L.SelectedItem == null)
                return -1;
            return Sub_Code.Other_Create_Window.Other_List[0][Type_Index].Files.Select(h => h.Full_Path).ToList().IndexOf(((Other_File_Type)Sound_File_L.SelectedItem).Full_Path);
        }
        private void Music_Pos_Change(double Position, bool IsBassPosChange)
        {
            if (IsBusy)
                return;
            if (IsBassPosChange)
                Bass.BASS_ChannelSetPosition(Stream, Position);
            TimeSpan Time = TimeSpan.FromSeconds(Position);
            string Minutes = Time.Minutes.ToString();
            string Seconds = Time.Seconds.ToString();
            if (Time.Minutes < 10)
                Minutes = "0" + Time.Minutes;
            if (Time.Seconds < 10)
                Seconds = "0" + Time.Seconds;
            Location_T.Text = Minutes + ":" + Seconds;
        }
        //再生位置を変更(スライダー)
        private void Location_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (IsLocationChanging)
                Music_Pos_Change(Location_S.Value, false);
        }
        //再生位置のスライダーを押したら
        private void Location_S_DragStart(object sender, EventArgs e)
        {
            if (IsBusy)
                return;
            IsLocationChanging = true;
            Bass.BASS_ChannelPause(Stream);
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0f);
        }
        //再生位置のスライダーを離したら
        private void Location_S_DragCompleted(object sender, EventArgs e)
        {
            IsLocationChanging = false;
            Bass.BASS_ChannelSetPosition(Stream, Location_S.Value);
            if (!IsPaused)
            {
                Bass.BASS_ChannelPlay(Stream, false);
                Play_Volume_Animation();
            }
        }
        private void Volume_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Volume_T.Text = "音量:" + (int)e.NewValue;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
        }
        private void Volume_S_DragCompleted(object sender, EventArgs e)
        {
            Configs_Save();
        }
        private async void Play_Volume_Animation(float Feed_Time = 15f)
        {
            IsPaused = false;
            Bass.BASS_ChannelPlay(Stream, false);
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Plus = (float)(Volume_S.Value / 100) / Feed_Time;
            while (Volume_Now < (float)(Volume_S.Value / 100) && !IsPaused)
            {
                Volume_Now += Volume_Plus;
                if (Volume_Now > 1f)
                    Volume_Now = 1f;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
        }
        //フェードアウトしながら一時停止または停止
        private async void Pause_Volume_Animation(bool IsStop, float Feed_Time = 15f)
        {
            IsPaused = true;
            float Volume_Now = 1f;
            Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, ref Volume_Now);
            float Volume_Minus = Volume_Now / Feed_Time;
            while (Volume_Now > 0f && IsPaused)
            {
                Volume_Now -= Volume_Minus;
                if (Volume_Now < 0f)
                    Volume_Now = 0f;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume_Now);
                await Task.Delay(1000 / 60);
            }
            if (Volume_Now <= 0f)
            {
                if (IsStop)
                {
                    Bass.BASS_ChannelStop(Stream);
                    Bass.BASS_StreamFree(Stream);
                    Location_S.Value = 0;
                    Location_S.Maximum = 1;
                    Location_T.Text = "00:00";
                    Play_Time_T.Text = "再生時間:0～0";
                    Sub_Code.Delete_Temp_Files();
                }
                else
                    Bass.BASS_ChannelPause(Stream);
            }
        }
        private void Pause_B_Clicked(object sender ,EventArgs e)
        {
            if (!IsPaused)
                Pause_Volume_Animation(false);
        }
        private void Play_B_Clicked(object sender, EventArgs e)
        {
            if (IsPaused)
                Play_Volume_Animation();
        }
        private void Minus_B_Clicked(object sender, EventArgs e)
        {
            if (Location_S.Value <= 5)
                Location_S.Value = 0;
            else
                Location_S.Value -= 5;
            Music_Pos_Change(Location_S.Value, true);
        }
        private void Plus_B_Clicked(object sender, EventArgs e)
        {
            if (Location_S.Value + 5 >= Location_S.Maximum)
                Location_S.Value = Location_S.Maximum;
            else
                Location_S.Value += 5;
            Music_Pos_Change(Location_S.Value, true);
        }
        private void Feed_Help_B_Clicked(object sender, EventArgs e)
        {
            Message_Feed_Out("WoTB内で、サウンドがフェードインしながら再生するようになります。");
        }
        private void Time_Start_B_Clicked(object sender, EventArgs e)
        {
            if (Sound_File_L.SelectedItem == null)
                return;
            Other_File_Type Temp = (Other_File_Type)Sound_File_L.SelectedItem;
            Temp.Music_Time.Start_Time = Location_S.Value;
            double End_Time = Temp.Music_Time.End_Time;
            if (End_Time != 0 && Temp.Music_Time.Start_Time > End_Time)
            {
                Temp.Music_Time.End_Time = 0;
                Play_Time_T.Text = "再生時間:" + (int)Temp.Music_Time.Start_Time + "～" + (int)Location_S.Maximum;
                Message_Feed_Out("開始時間が終了時間より大きかったため、終了時間を最大にします。");
            }
            else if (End_Time != 0)
                Play_Time_T.Text = "再生時間:" + (int)Temp.Music_Time.Start_Time + "～" + (int)End_Time;
            else
                Play_Time_T.Text = "再生時間:" + (int)Temp.Music_Time.Start_Time + "～" + (int)Location_S.Maximum;
        }
        private void Time_End_B_Clicked(object sender, EventArgs e)
        {
            if (Sound_File_L.SelectedItem == null)
                return;
            Other_File_Type Temp = (Other_File_Type)Sound_File_L.SelectedItem;
            Temp.Music_Time.End_Time = Location_S.Value;
            if (Temp.Music_Time.End_Time < Temp.Music_Time.Start_Time)
            {
                Temp.Music_Time.Start_Time = 0;
                Message_Feed_Out("終了時間が開始時間より小さかったため、開始時間を0秒にします。");
            }
            Play_Time_T.Text = "再生時間:" + (int)Temp.Music_Time.Start_Time + "～" + (int)Temp.Music_Time.End_Time;
        }
        private void Time_Clear_B_Clicked(object sender, EventArgs e)
        {
            if (Sound_File_L.SelectedItem == null)
                return;
            Other_File_Type Temp = (Other_File_Type)Sound_File_L.SelectedItem;
            Temp.Music_Time.Start_Time = 0;
            Temp.Music_Time.End_Time = 0;
            Play_Time_T.Text = "再生時間:0～" + (int)Location_S.Maximum;
        }
        private void Cancel_B_Clicked(object sender, EventArgs e)
        {
            Sound_File_L.SelectedItem = null;
            Sound_File_L.ItemsSource = null;
            if (Sound_File_LastCell != null)
                Sound_File_LastCell.View.BackgroundColor = Color.Transparent;
            if (Before_Data != null)
            {
                Sub_Code.Other_Create_Window.Other_List[0][Type_Index].Files = new List<Other_File_Type>(Before_Data);
                Before_Data.Clear();
                Before_Data = null;
            }
            Pause_Volume_Animation(true, 10f);
            Navigation.PopModalAsync();
        }
        private void OK_B_Clicked(object sender, EventArgs e)
        {
            Sound_File_L.SelectedItem = null;
            Sound_File_L.ItemsSource = null;
            if (Sound_File_LastCell != null)
                Sound_File_LastCell.View.BackgroundColor = Color.Transparent;
            if (Before_Data != null)
            {
                Before_Data.Clear();
                Before_Data = null;
            }
            Pause_Volume_Animation(true, 10f);
            Navigation.PopModalAsync();
        }
        private void Feed_In_C_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (Sound_File_L.SelectedItem != null)
                ((Other_File_Type)Sound_File_L.SelectedItem).Music_Feed_In = e.Value;
        }
    }
}