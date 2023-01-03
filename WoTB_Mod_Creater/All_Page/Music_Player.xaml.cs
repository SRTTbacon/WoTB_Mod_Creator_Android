using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using System.Reflection;
using System.Linq;
using WoTB_Mod_Creater.Class;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WoTB_Mod_Creater.All_Page
{
    public class Music_Type_List
    {
        public string File_Full { get; set; }
        public string File_Full_Source = null;
        public string Name_Text => File_Full_Source != null ? Path.GetFileName(File_Full_Source) : Path.GetFileName(File_Full);
        public bool IsPlayed = false;
        public Color Name_Color => IsPlayed ? Color.FromHex("#BF6C6C6C") : Color.Aqua;
        public Music_Type_List(string Full_Name, string File_Full_Source = null)
        {
            File_Full = Full_Name;
            this.File_Full_Source = File_Full_Source;
        }
    }
    public partial class Music_Player : ContentPage
    {
        private readonly List<List<Music_Type_List>> Music_List = new List<List<Music_Type_List>>();
        private readonly List<string> Already_Played_Path = new List<string>();
        private ViewCell Music_List_LastCell = null;
        private readonly BASS_BFX_BQF LPF_Setting = new BASS_BFX_BQF(BASSBFXBQF.BASS_BFX_BQF_LOWPASS, 500f, 0f, 0.707f, 0f, 0f, BASSFXChan.BASS_BFX_CHANALL);
        private readonly BASS_BFX_BQF HPF_Setting = new BASS_BFX_BQF(BASSBFXBQF.BASS_BFX_BQF_HIGHPASS, 1000f, 0f, 0.707f, 0f, 0f, BASSFXChan.BASS_BFX_CHANALL);
        private readonly BASS_BFX_ECHO4 ECHO_Setting = new BASS_BFX_ECHO4(0, 0, 0, 0, true, BASSFXChan.BASS_BFX_CHANALL);
        private string Playing_Music_Name_Now = "";
        private int Stream = 0;
        private int Stream_LPF = 0;
        private int Stream_HPF = 0;
        private int Stream_ECHO = 0;
        private int Music_Page = 0;
        private double Start_Time = 0;
        private double End_Time = 0;
        private float Music_Frequency = 44100f;
        private bool IsPageOpen = false;
        private bool IsNotMusicChange = false;
        private bool IsEnded = false;
        private bool IsSyncPitch_And_Speed = false;
        private bool IsPaused = false;
        private bool IsLocationChanging = false;
        private bool IsPlayingMouseDown = false;
        private bool IsAddMode = false;
        private SYNCPROC IsMusicEnd;
        private readonly Music_Player_Setting_Page Music_Player_Setting_Window = new Music_Player_Setting_Page();
        public Music_Player()
        {
            InitializeComponent();
            for (int Number = 0; Number < 9; Number++)
                Music_List.Add(new List<Music_Type_List>());
            Music_L.ItemTapped += delegate (object sender, ItemTappedEventArgs e)
            {
                Music_L_SelectionChanged(e, true);
            };
            Music_Page_Back_B.Clicked += Music_Page_Back_B_Click;
            Music_Page_Next_B.Clicked += Music_Page_Next_B_Click;
            Music_Pause_B.Clicked += Music_Pause_B_Click;
            Music_Play_B.Clicked += Music_Play_B_Click;
            Music_Add_B.Clicked += Music_Add_B_Click;
            Music_Delete_B.Clicked += Music_Delete_B_Click;
            Music_Minus_B.Clicked += Music_Minus_B_Click;
            Music_Plus_B.Clicked += Music_Plus_B_Click;
            Reset_B.Clicked += Reset_B_Clicked;
            Setting_B.Clicked += Setting_B_Click;
            Clear_B.Clicked += Clear_B_Clicked;
            Loop_C.CheckedChanged += Loop_C_CheckedChanged;
            Random_C.CheckedChanged += Random_C_CheckedChanged;
            Mode_C.CheckedChanged += Mode_C_CheckedChanged;
            Volume_S.ValueChanged += Volume_S_ValueChanged;
            Volume_S.DragCompleted += delegate
            {
                Configs_Save();
            };
            Location_S.DragStarted += Location_S_DragStarted;
            Location_S.DragCompleted += Location_S_DragCompleted;
            Location_S.ValueChanged += Location_S_ValueChanged;
            Pitch_S.ValueChanged += Pitch_S_ValueChanged;
            Speed_S.ValueChanged += Speed_S_ValueChanged;
            Pitch_Speed_S.ValueChanged += Pitch_Speed_S_ValueChanged;
            Volume_S.Value = 50;
            Pitch_Speed_S.Value = 50;
        }
        private async void Position_Change()
        {
            double nextFrame = Environment.TickCount;
            float period = 1000f / 30f;
            while (true)
            {
                //FPSを上回っていたらスキップ
                int tickCount = Environment.TickCount;
                if (tickCount < nextFrame)
                {
                    if (nextFrame - tickCount > 1)
                        await Task.Delay((int)(nextFrame - tickCount));
                    continue;
                }
                bool IsPlaying = Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING;
                if (IsPlaying)
                {
                    Bass.BASS_ChannelUpdate(Stream, 325);
                    if (Start_Time != -1 && Location_S.Value >= End_Time)
                    {
                        Music_Pos_Change(Start_Time, true);
                        Set_Position_Slider();
                    }
                    else if (Start_Time != -1 && Location_S.Value < Start_Time)
                    {
                        Music_Pos_Change(Start_Time, true);
                        Set_Position_Slider();
                    }
                }
                if (IsVisible)
                {
                    if (IsPlaying && !IsLocationChanging)
                    {
                        Set_Position_Slider();
                        TimeSpan Time = TimeSpan.FromSeconds(Location_S.Value);
                        string Minutes = Time.Minutes.ToString();
                        string Seconds = Time.Seconds.ToString();
                        if (Time.Minutes < 10)
                            Minutes = "0" + Time.Minutes;
                        if (Time.Seconds < 10)
                            Seconds = "0" + Time.Seconds;
                        Location_T.Text = Minutes + ":" + Seconds;
                    }
                    if (Stream_LPF != 0 && Music_Player_Setting_Window.IsVisible)
                    {
                        if (Music_Player_Setting_Window.IsLPFChanged && Music_Player_Setting_Window.IsLPFEnable)
                        {
                            LPF_Setting.fCenter = 500f + 4000f * (1 - (float)Music_Player_Setting_Window.LPF_Value / 100f);
                            Bass.BASS_FXSetParameters(Stream_LPF, LPF_Setting);
                            Music_Player_Setting_Window.IsLPFChanged = false;
                        }
                        if (Music_Player_Setting_Window.IsHPFChanged && Music_Player_Setting_Window.IsHPFEnable)
                        {
                            HPF_Setting.fCenter = 100f + 4000f * (float)Music_Player_Setting_Window.HPF_Value / 100f;
                            Bass.BASS_FXSetParameters(Stream_HPF, HPF_Setting);
                            Music_Player_Setting_Window.IsHPFChanged = false;
                        }
                        if (Music_Player_Setting_Window.IsECHOChanged && Music_Player_Setting_Window.IsECHOEnable)
                        {
                            ECHO_Setting.fDelay = (float)Music_Player_Setting_Window.ECHO_Delay_Value;
                            ECHO_Setting.fDryMix = (float)Music_Player_Setting_Window.ECHO_Power_Original_Value / 100f;
                            ECHO_Setting.fWetMix = (float)Music_Player_Setting_Window.ECHO_Power_ECHO_Value / 100f;
                            ECHO_Setting.fFeedback = (float)Music_Player_Setting_Window.ECHO_Length_Value / 100f;
                            Bass.BASS_FXSetParameters(Stream_ECHO, ECHO_Setting);
                            Music_Player_Setting_Window.IsECHOChanged = false;
                        }
                    }
                }
                if (IsEnded)
                {
                    int Index = Get_Select_Index();
                    if (Loop_C.IsChecked)
                    {
                        Bass.BASS_ChannelStop(Stream);
                        Bass.BASS_ChannelPlay(Stream, true);
                    }
                    else if (Random_C.IsChecked)
                    {
                        if (Music_List[Music_Page].Count == 1)
                            Bass.BASS_ChannelSetPosition(Stream, 0);
                        else
                        {
                            Random r = new Random();
                            if (Already_Played_Path.Count >= Music_List[Music_Page].Count)
                            {
                                Already_Played_Path.Clear();
                                foreach (Music_Type_List Info in Music_List[Music_Page])
                                    Info.IsPlayed = false;
                                Music_List_Sort();
                            }
                            else
                                Music_List[Music_Page][Index].IsPlayed = true;
                            while (true)
                            {
                                int r2 = r.Next(0, Music_List[Music_Page].Count);
                                if (!Already_Played_Path.Contains(Music_List[Music_Page][r2].File_Full))
                                {
                                    Set_Music_Index(r2);
                                    Already_Played_Path.Add(Music_List[Music_Page][r2].File_Full);
                                    Music_L_SelectionChanged(new ItemTappedEventArgs(Music_List[Music_Page], Music_List[Music_Page][r2], r2));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Bass.BASS_ChannelStop(Stream);
                        Bass.BASS_StreamFree(Stream);
                        Music_L.SelectedItem = null;
                        Music_List_Sort();
                        Playing_Music_Name_Now = "";
                    }
                }
                IsEnded = false;
                //次のフレーム時間を計算
                if (Environment.TickCount >= nextFrame + period)
                {
                    nextFrame += period;
                    continue;
                }
                nextFrame += period;
            }
        }
        private async void EndSync(int handle, int channel, int data, IntPtr user)
        {
            if (!IsEnded)
            {
                await Task.Delay(500);
                IsEnded = true;
            }
        }
        private void Music_List_Tapped(object sender, EventArgs e)
        {
            if (Music_List_LastCell != null)
                Music_List_LastCell.View.BackgroundColor = Color.Transparent;
            ViewCell viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#82bfc8");
                Music_List_LastCell = viewCell;
            }
        }
        private int Get_Select_Index()
        {
            if (Music_L.SelectedItem == null)
                return -1;
            Music_Type_List Temp = (Music_Type_List)Music_L.SelectedItem;
            for (int Number = 0; Number < Music_List[Music_Page].Count; Number++)
                if (Music_List[Music_Page][Number].Name_Text == Temp.Name_Text)
                    return Number;
            return -1;
        }
        private void Set_Music_Index(int Index)
        {
            Music_L.ItemsSource = null;
            Music_L.ItemsSource = Music_List[Music_Page];
            Music_L.SelectedItem = Music_List[Music_Page][Index];
            OnPropertyChanged("SelectedItem");
            IEnumerable<PropertyInfo> pInfos = Music_L.GetType().GetRuntimeProperties();
            PropertyInfo templatedItems = pInfos.FirstOrDefault(info => info.Name == "TemplatedItems");
            if (templatedItems != null)
            {
                object cells = templatedItems.GetValue(Music_L);
                int Count = 0;
                foreach (ViewCell cell in (cells as ITemplatedItemsList<Cell>).Cast<ViewCell>())
                {
                    cell.View.BackgroundColor = cell.BindingContext != null && Count == Index ? Color.FromHex("#82bfc8") : Color.Transparent;
                    Count++;
                }
            }
        }
        private void Set_Music_FileName(string File_Full)
        {
            int Index = -1;
            for (int Number = 0; Number < Music_List[Music_Page].Count; Number++)
            {
                if (Music_List[Music_Page][Number].File_Full == File_Full)
                {
                    Index = Number;
                    break;
                }
            }
            if (Index != -1)
            {
                Music_L.SelectedItem = Music_List[Music_Page][Index];
                Set_Music_Index(Index);
            }
        }
        private void Music_L_SelectionChanged(ItemTappedEventArgs e, bool IsClear = false)
        {
            if (e.Item != null && !IsNotMusicChange)
            {
                Music_Type_List Temp = (Music_Type_List)Music_L.SelectedItem;
                if (!File.Exists(Temp.File_Full))
                {
                    Sub_Code.Show_Message("ファイルが存在しません。リストから削除されます。");
                    Sub_Code.Error_Log_Write("次のファイルが見つかりませんでした。->" + Temp.File_Full);
                    List_Remove_Index(e.ItemIndex);
                    Playing_Music_Name_Now = "";
                    return;
                }
                if (Playing_Music_Name_Now == Music_List[Music_Page][e.ItemIndex].File_Full)
                    return;
                if (IsClear)
                {
                    Already_Played_Path.Clear();
                    Set_Music_Index(e.ItemIndex);
                    foreach (Music_Type_List Info in Music_List[Music_Page])
                        Info.IsPlayed = false;
                    Already_Played_Path.Add(Music_List[Music_Page][e.ItemIndex].File_Full);
                }
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_FXReset(Stream_LPF);
                Bass.BASS_FXReset(Stream_HPF);
                Bass.BASS_FXReset(Stream_ECHO);
                Bass.BASS_StreamFree(Stream);
                Location_S.Value = 0;
                Playing_Music_Name_Now = Music_List[Music_Page][e.ItemIndex].File_Full;
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 175);
                int StreamHandle = Bass.BASS_StreamCreateFile(Music_List[Music_Page][e.ItemIndex].File_Full, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_LOOP | BASSFlag.BASS_STREAM_PRESCAN);
                Stream = BassFx.BASS_FX_TempoCreate(StreamHandle, BASSFlag.BASS_FX_FREESOURCE);
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 500);
                IsMusicEnd = new SYNCPROC(EndSync);
                Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref Music_Frequency);
                Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, IsMusicEnd, IntPtr.Zero);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
                Stream_LPF = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_BQF, 2);
                Stream_HPF = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_BQF, 1);
                HPF_Setting.fCenter = 1000f;
                if (Music_Player_Setting_Window.IsLPFEnable)
                {
                    LPF_Setting.fCenter = 500 + 4000f * (1 - (float)Music_Player_Setting_Window.LPF_Value / 100.0f);
                    Bass.BASS_FXSetParameters(Stream_LPF, LPF_Setting);
                }
                if (Music_Player_Setting_Window.IsHPFEnable)
                {
                    HPF_Setting.fCenter = 1000 + 4000f * (float)Music_Player_Setting_Window.HPF_Value / 100.0f;
                    Bass.BASS_FXSetParameters(Stream_HPF, HPF_Setting);
                }
                ECHO_Setting.fWetMix = 0;
                if (Music_Player_Setting_Window.IsECHOEnable)
                {
                    Stream_ECHO = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_ECHO4, 0);
                    ECHO_Setting.fDelay = (float)Music_Player_Setting_Window.ECHO_Delay_Value;
                    ECHO_Setting.fDryMix = (float)Music_Player_Setting_Window.ECHO_Power_Original_Value / 100f;
                    ECHO_Setting.fWetMix = (float)Music_Player_Setting_Window.ECHO_Power_ECHO_Value / 100f;
                    ECHO_Setting.fFeedback = (float)Music_Player_Setting_Window.ECHO_Length_Value / 100f;
                    Bass.BASS_FXSetParameters(Stream_ECHO, ECHO_Setting);
                }
                if (IsSyncPitch_And_Speed)
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Music_Frequency * (float)(Pitch_Speed_S.Value / 50));
                else
                {
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)Pitch_S.Value);
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, (float)Speed_S.Value);
                }
                Location_S.Maximum = Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTES));
                Start_Time = 0;
                End_Time = Location_S.Maximum;
                Loop_Time_T.Text = "再生時間:" + (int)Start_Time + "～" + (int)End_Time;
                IsPaused = false;
                Bass.BASS_ChannelPlay(Stream, true);
            }
            else if (!IsAddMode)
            {
                Location_S.Value = 0;
                Location_S.Maximum = 1;
                Location_T.Text = "00:00";
            }
        }
        private void List_Remove_Index(int Index)
        {
            Music_L.SelectedItem = null;
            Music_List[Music_Page].RemoveAt(Index);
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            Music_List_Sort();
            Already_Played_Path.Clear();
            Music_List_Save();
        }
        private void Music_List_Sort()
        {
            IsNotMusicChange = true;
            string List_Now = "";
            if (Music_L.SelectedItem != null)
                List_Now = ((Music_Type_List)Music_L.SelectedItem).File_Full;
            Music_List[Music_Page] = Music_List[Music_Page].OrderBy(x => Path.GetExtension(x.File_Full)).ThenBy(x => x.Name_Text).ToList();
            Music_L.ItemsSource = null;
            Music_L.SelectedItem = null;
            Music_L.ItemsSource = Music_List[Music_Page];
            if (List_Now != "")
            {
                int Index = -1;
                for (int Number = 0; Number < Music_List[Music_Page].Count; Number++)
                {
                    if (Music_List[Music_Page][Number].File_Full == List_Now)
                    {
                        Index = Number;
                        break;
                    }
                }
                if (Index != -1)
                    Set_Music_Index(Index);
            }
            IsNotMusicChange = false;
        }
        private void Music_List_Save()
        {
            StreamWriter stw = File.CreateText(Sub_Code.ExDir + "/Configs/Temp_Music_Player_List.dat");
            for (int Number = 0; Number < Music_List.Count; Number++)
                foreach (Music_Type_List Now in Music_List[Number])
                {
                    if (Now.File_Full_Source == null)
                        stw.WriteLine(Number + "|" + Now.File_Full + "|null");
                    else
                        stw.WriteLine(Number + "|" + Now.File_Full + "|" + Now.File_Full_Source);
                }
            stw.Close();
            Sub_Code.File_Encrypt(Sub_Code.ExDir + "/Configs/Temp_Music_Player_List.dat", Sub_Code.ExDir + "/Configs/Music_Player_List.dat", "SRTTbacon_Music_List_Save", true);
        }
        private void Configs_Save()
        {
            try
            {
                int Number_01 = 0;
                if (Music_List[Music_Page].Count == 0)
                {
                    for (int Number_02 = 0; Number_02 < Music_List.Count; Number_02++)
                        if (Music_List[Number_02].Count > 0)
                            Number_01 = Number_02;
                }
                else
                    Number_01 = Music_Page;
                StreamWriter stw = File.CreateText(Sub_Code.ExDir + "/Configs/Music_Player.tmp");
                stw.WriteLine(Loop_C.IsChecked);
                stw.WriteLine(Random_C.IsChecked);
                stw.WriteLine(Volume_S.Value);
                stw.WriteLine(Mode_C.IsChecked);
                stw.Write(Number_01);
                stw.Close();
                Sub_Code.File_Encrypt(Sub_Code.ExDir + "/Configs/Music_Player.tmp", Sub_Code.ExDir + "/Configs/Music_Player.conf", "Music_Player_Configs_Save", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        private void Configs_Load()
        {
            if (File.Exists(Sub_Code.ExDir + "/Configs/Music_Player_List.dat") && Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED)
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Sub_Code.ExDir + "/Configs/Music_Player_List.dat", "SRTTbacon_Music_List_Save");
                    for (int Number = 0; Number < 9; Number++)
                        Music_List[Number].Clear();
                    string line;
                    bool IsOK = true;
                    while ((line = str.ReadLine()) != null)
                    {
                        if (line != "" && line.Contains("|"))
                        {
                            if (line.CountOf("|") >= 2)
                            {
                                int Index = int.Parse(line.Substring(0, line.IndexOf('|')));
                                string First = line.Substring(line.IndexOf('|') + 1);
                                string File_Path = First.Substring(0, First.IndexOf('|'));
                                string Name = First.Substring(First.IndexOf('|') + 1);
                                string Name_Only = null;
                                if (Path.GetFileName(Name) != "null")
                                    Name_Only = Name;
                                if (File.Exists(File_Path))
                                    Music_List[Index].Add(new Music_Type_List(File_Path, Name_Only));
                            }
                            else
                                IsOK = false;
                        }
                    }
                    str.Close();
                    Music_List_Sort();
                    Already_Played_Path.Clear();
                    if (!IsOK)
                        Sub_Code.Show_Message("V0.2で指定したサウンドリストは初期化されます。");
                }
                catch (Exception e1)
                {
                    File.Delete(Sub_Code.ExDir + "/Configs/Music_Player_List.dat");
                    for (int Number = 0; Number < 9; Number++)
                        Music_List[Number].Clear();
                    Music_L.SelectedItem = null;
                    Music_L.ItemsSource = null;
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
            if (File.Exists(Sub_Code.ExDir + "/Configs/Music_Player.conf"))
            {
                try
                {
                    StreamReader str = Sub_Code.File_Decrypt_To_Stream(Sub_Code.ExDir + "/Configs/Music_Player.conf", "Music_Player_Configs_Save");
                    Loop_C.IsChecked = bool.Parse(str.ReadLine());
                    Random_C.IsChecked = bool.Parse(str.ReadLine());
                    Volume_S.Value = double.Parse(str.ReadLine());
                    Mode_C.IsChecked = bool.Parse(str.ReadLine());
                    Music_List_Change(int.Parse(str.ReadLine()));
                    str.Close();
                }
                catch (Exception e1)
                {
                    File.Delete(Sub_Code.ExDir + "/Configs/Music_Player.conf");
                    Loop_C.IsChecked = false;
                    Random_C.IsChecked = false;
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
        }
        private void Music_Pos_Change(double Pos, bool IsBassPosChange)
        {
            if (IsBusy)
                return;
            if (IsBassPosChange)
                Bass.BASS_ChannelSetPosition(Stream, Pos);
            TimeSpan Time = TimeSpan.FromSeconds(Pos);
            string Minutes = Time.Minutes.ToString();
            string Seconds = Time.Seconds.ToString();
            if (Time.Minutes < 10)
                Minutes = "0" + Time.Minutes;
            if (Time.Seconds < 10)
                Seconds = "0" + Time.Seconds;
            Location_T.Text = Minutes + ":" + Seconds;
        }
        private void Set_Position_Slider()
        {
            long position = Bass.BASS_ChannelGetPosition(Stream);
            Location_S.Value = Bass.BASS_ChannelBytes2Seconds(Stream, position);
        }
        private async void Setting_B_Click(object sender, EventArgs e)
        {
            if (IsPageOpen)
                return;
            IsPageOpen = true;
            await Navigation.PushModalAsync(Music_Player_Setting_Window);
        }
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            if (!Music_Player_Setting_Window.IsLoaded)
            {
                Configs_Load();
                Position_Change();
                Music_Player_Setting_Window.ChangeLPFEnable += delegate (bool IsEnable)
                {
                    if (IsEnable)
                    {
                        LPF_Setting.fCenter = 500f + 4000f * (1 - (float)Music_Player_Setting_Window.LPF_Value / 100f);
                        Bass.BASS_FXSetParameters(Stream_LPF, LPF_Setting);
                    }
                    else
                    {
                        Bass.BASS_ChannelRemoveFX(Stream, Stream_LPF);
                        Stream_LPF = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_BQF, 2);
                    }
                };
                Music_Player_Setting_Window.ChangeHPFEnable += delegate (bool IsEnable)
                {
                    if (IsEnable)
                    {
                        HPF_Setting.fCenter = 100f + 4000f * (float)Music_Player_Setting_Window.HPF_Value / 100f;
                        Bass.BASS_FXSetParameters(Stream_HPF, HPF_Setting);
                    }
                    else
                    {
                        Bass.BASS_ChannelRemoveFX(Stream, Stream_HPF);
                        Stream_HPF = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_BQF, 1);
                    }
                };
                Music_Player_Setting_Window.ChangeECHOEnable += delegate (bool IsEnable)
                {
                    if (IsEnable)
                    {
                        Stream_ECHO = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_BFX_ECHO4, 0);
                        ECHO_Setting.fDelay = (float)Music_Player_Setting_Window.ECHO_Delay_Value;
                        ECHO_Setting.fDryMix = (float)Music_Player_Setting_Window.ECHO_Power_Original_Value / 100f;
                        ECHO_Setting.fWetMix = (float)Music_Player_Setting_Window.ECHO_Power_ECHO_Value / 100f;
                        ECHO_Setting.fFeedback = (float)Music_Player_Setting_Window.ECHO_Length_Value / 100f;
                        Bass.BASS_FXSetParameters(Stream_ECHO, ECHO_Setting);
                    }
                    else
                        Bass.BASS_ChannelRemoveFX(Stream, Stream_ECHO);
                };
                Music_Player_Setting_Window.Configs_Load();
            }
            IsPageOpen = false;
        }
        private void Loop_C_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
                Random_C.IsChecked = !e.Value;
            Configs_Save();
        }
        private void Random_C_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
                Loop_C.IsChecked = !e.Value;
            Configs_Save();
        }
        private void Mode_C_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            IsSyncPitch_And_Speed = e.Value;
            if (e.Value)
            {
                Pitch_T.IsVisible = false;
                Pitch_S.IsVisible = false;
                Speed_T.IsVisible = false;
                Speed_S.IsVisible = false;
                Pitch_Speed_T.IsVisible = true;
                Pitch_Speed_S.IsVisible = true;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Music_Frequency * (float)(Pitch_Speed_S.Value / 50));
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, 0f);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, 0f);
            }
            else
            {
                Pitch_T.IsVisible = true;
                Pitch_S.IsVisible = true;
                Speed_T.IsVisible = true;
                Speed_S.IsVisible = true;
                Pitch_Speed_T.IsVisible = false;
                Pitch_Speed_S.IsVisible = false;
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Music_Frequency);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)Pitch_S.Value);
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, (float)Speed_S.Value);
            }
            Configs_Save();
        }
        private async void Music_Add_B_Click(object sender, EventArgs e)
        {
            if (Sub_Code.IsUseSelectPage && !IsPageOpen && Sub_Code.HasSystemPermission)
            {
                IsPageOpen = true;
                string Ex = ".aac|.mp3|.wav|.ogg|.aiff|.flac|.m4a|.mp4";
                Sub_Code.Select_Files_Window.Window_Show("Music_Player", "", Ex);
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
                    int Index = Get_Select_Index();
                    Music_Type_List Temp = null;
                    if (Index != -1)
                        Temp = Music_List[Music_Page][Index];
                    IsAddMode = true;
                    bool IsExist = false;
                    if (!Directory.Exists(Sub_Code.ExDir + "/Temp/Music"))
                        Directory.CreateDirectory(Sub_Code.ExDir + "/Temp/Music");
                    List<string> Already_Music_List = Music_List[Music_Page].Select(x => x.Name_Text).ToList();
                    List<Music_Type_List> Match = new List<Music_Type_List>();
                    for (int Number_01 = 0; Number_01 < Music_List.Count; Number_01++)
                        for (int Number_02 = 0; Number_02 < Music_List[Number_01].Count; Number_02++)
                            Match.Add(Music_List[Number_01][Number_02]);
                    foreach (FileResult file_result in result)
                    {
                        string Name_Only = Path.GetFileName(file_result.FullPath);
                        if (Already_Music_List.Contains(Name_Only))
                        {
                            IsExist = true;
                            continue;
                        }
                        Music_Type_List Source = null;
                        foreach (Music_Type_List File_Now in Match)
                        {
                            if (File_Now.File_Full_Source != null && Path.GetFileName(File_Now.File_Full_Source) == Name_Only)
                            {
                                if (Sub_Code.File_Equal(File_Now.File_Full, file_result.FullPath))
                                {
                                    Source = File_Now;
                                    break;
                                }
                            }
                        }
                        if (Source != null)
                            Music_List[Music_Page].Add(new Music_Type_List(Source.File_Full, Source.File_Full_Source));
                        else
                        {
                            int Random_ID = Sub_Code.r.Next(1000000, 9999999);
                            File.Copy(file_result.FullPath, Sub_Code.ExDir + "/Temp/Music/" + Random_ID + Path.GetExtension(file_result.FullPath), true);
                            Music_List[Music_Page].Add(new Music_Type_List(Sub_Code.ExDir + "/Temp/Music/" + Random_ID + Path.GetExtension(file_result.FullPath), file_result.FullPath));
                        }
                    }
                    if (IsExist)
                        Sub_Code.Show_Message("既に追加されているファイルが存在します。");
                    Already_Music_List.Clear();
                    Match.Clear();
                    Music_List_Sort();
                    if (Temp != null)
                        Set_Music_FileName(Temp.File_Full);
                    Music_List_Save();
                    string Cache_Dir = Path.GetDirectoryName(Sub_Code.ExDir) + "/cache";
                    if (Directory.Exists(Cache_Dir))
                        Directory.Delete(Cache_Dir, true);
                    IsAddMode = false;
                }
            }
        }
        private async void Music_Delete_B_Click(object sender, EventArgs e)
        {
            if (Music_L.SelectedItem == null)
                return;
            bool Result = await DisplayAlert("選択した項目を削除しますか?", null, "はい", "いいえ");
            if (Result)
            {
                Music_Type_List Temp = (Music_Type_List)Music_L.SelectedItem;
                Music_List[Music_Page].Remove(Temp);
                Music_L.SelectedItem = null;
                Bass.BASS_ChannelStop(Stream);
                Bass.BASS_StreamFree(Stream);
                Loop_Time_T.Text = "再生時間:0～0";
                Already_Played_Path.Clear();
                foreach (Music_Type_List Info in Music_List[Music_Page])
                    Info.IsPlayed = false;
                Music_List_Sort();
                Music_List_Save();
                Delete_Music_File();
            }
        }
        private void Music_Minus_B_Click(object sender, EventArgs e)
        {
            if (Location_S.Value <= 5)
                Location_S.Value = 0;
            else
                Location_S.Value -= 5;
            Music_Pos_Change(Location_S.Value, true);
        }
        private void Music_Plus_B_Click(object sender, EventArgs e)
        {
            if (Location_S.Value + 5 >= Location_S.Maximum)
                Location_S.Value = Location_S.Maximum;
            else
                Location_S.Value += 5;
            Music_Pos_Change(Location_S.Value, true);
        }
        private void Music_Play_B_Click(object sender, EventArgs e)
        {
            Play_Volume_Animation();
        }
        private void Music_Pause_B_Click(object sender, EventArgs e)
        {
            Pause_Volume_Animation(false);
        }
        private void Music_Page_Back_B_Click(object sender, EventArgs e)
        {
            if (Music_Page > 0)
                Music_List_Change(Music_Page - 1);
        }
        private void Music_Page_Next_B_Click(object sender, EventArgs e)
        {
            if (Music_Page < 8)
                Music_List_Change(Music_Page + 1);
        }
        private void Reset_B_Clicked(object sender, EventArgs e)
        {
            Speed_S.Value = 0;
            Pitch_S.Value = 0;
            Pitch_Speed_S.Value = 50;
        }
        async void Play_Volume_Animation(float Feed_Time = 30f)
        {
            IsPaused = false;
            if (Playing_Music_Name_Now == "")
                return;
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
        public async void Pause_Volume_Animation(bool IsStop, float Feed_Time = 30f)
        {
            IsPaused = true;
            if (Playing_Music_Name_Now == "")
                return;
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
                    Loop_Time_T.Text = "再生時間:0～0";
                    Music_L.SelectedItem = null;
                }
                else if (IsPaused)
                    Bass.BASS_ChannelPause(Stream);
            }
        }
        private void Volume_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Volume_T.Text = "音量:" + (int)e.NewValue;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, (float)Volume_S.Value / 100);
        }
        private void Pitch_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Pitch_T.Text = "音程:" + (Math.Floor(Pitch_S.Value * 10) / 10).ToString();
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)Pitch_S.Value);
        }
        private void Speed_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Speed_T.Text = "速度:" + (Math.Floor(Speed_S.Value * 10) / 10).ToString();
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO, (float)Speed_S.Value);
        }
        private void Pitch_Speed_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Pitch_Speed_T.Text = "音程と速度:" + (int)Pitch_Speed_S.Value;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, Music_Frequency * (float)(Pitch_Speed_S.Value / 50));
            Bass.BASS_ChannelUpdate(Stream, 50);
        }
        private void Location_S_DragStarted(object sender, EventArgs e)
        {
            IsLocationChanging = true;
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                IsPlayingMouseDown = true;
                Pause_Volume_Animation(false, 10);
            }
        }
        private void Location_S_DragCompleted(object sender, EventArgs e)
        {
            IsLocationChanging = false;
            Bass.BASS_ChannelSetPosition(Stream, Location_S.Value);
            if (IsPlayingMouseDown)
            {
                IsPaused = false;
                Play_Volume_Animation(10);
                IsPlayingMouseDown = false;
            }
        }
        private void Location_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (IsLocationChanging)
                Music_Pos_Change(Location_S.Value, false);
        }
        private void Music_List_Change(int Index)
        {
            if (Music_Page == Index)
                return;
            Already_Played_Path.Clear();
            Music_Page = Index;
            Music_Page_T.Text = "音楽リスト:" + (Music_Page + 1);
            Music_L.SelectedItem = null;
            Pause_Volume_Animation(true, 10f);
            Music_List_Sort();
            Configs_Save();
            Playing_Music_Name_Now = "";
        }
        private async void Clear_B_Clicked(object sender, EventArgs e)
        {
            if (Music_List[Music_Page].Count > 0)
            {
                bool IsOK = await DisplayAlert("ページ内の項目を削除しますか?", null, "はい", "いいえ");
                if (IsOK)
                {
                    Music_List[Music_Page].Clear();
                    Music_L.ItemsSource = null;
                    Music_L.SelectedItem = null;
                    Pause_Volume_Animation(true, 10f);
                    Playing_Music_Name_Now = "";
                    Music_List_Save();
                    Delete_Music_File();
                }
            }
        }
        public void Selected_Files()
        {
            int Index = Get_Select_Index();
            Music_Type_List Temp = null;
            if (Index != -1)
                Temp = Music_List[Music_Page][Index];
            IsAddMode = true;
            bool IsExist = false;
            List<string> Already_Music_List = Music_List[Music_Page].Select(x => x.File_Full).ToList();
            foreach (string file_result in Sub_Code.Select_Files_Window.Get_Select_Files())
            {
                if (Already_Music_List.Contains(file_result))
                {
                    IsExist = true;
                    continue;
                }
                Music_List[Music_Page].Add(new Music_Type_List(file_result));
            }
            if (IsExist)
                Sub_Code.Show_Message("既に追加されているファイルが存在します。");
            Music_List_Sort();
            if (Temp != null)
                Set_Music_FileName(Temp.File_Full);
            Music_List_Save();
            IsAddMode = false;
        }
        private void Delete_Music_File()
        {
            if (Directory.Exists(Sub_Code.ExDir + "/Temp/Music"))
            {
                List<string> Music_Files = new List<string>();
                foreach (List<Music_Type_List> a in Music_List)
                    foreach (Music_Type_List b in a)
                        if (!Music_Files.Contains(Path.GetFileName(b.File_Full)))
                            Music_Files.Add(Path.GetFileName(b.File_Full));
                foreach (string File_Now in Directory.GetFiles(Sub_Code.ExDir + "/Temp/Music", "*.*", SearchOption.TopDirectoryOnly))
                    if (!Music_Files.Contains(Path.GetFileName(File_Now)))
                        File.Delete(File_Now);
                Music_Files.Clear();
            }
        }
    }
}