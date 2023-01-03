using System;
using System.IO;
using WoTB_Mod_Creater.Class;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.All_Page
{
    public partial class Music_Player_Setting_Page : ContentPage
    {
        public delegate void CheckBoxEventHandler<T>(T args);
        public event CheckBoxEventHandler<bool> ChangeLPFEnable;
        public event CheckBoxEventHandler<bool> ChangeHPFEnable;
        public event CheckBoxEventHandler<bool> ChangeECHOEnable;
        public bool IsLPFChanged = false;
        public bool IsHPFChanged = false;
        public bool IsECHOChanged = false;
        public bool IsLPFEnable = false;
        public bool IsHPFEnable = false;
        public bool IsECHOEnable = false;
        public double LPF_Value = 0;
        public double HPF_Value = 0;
        public double ECHO_Delay_Value = 0;
        public double ECHO_Power_Original_Value = 0;
        public double ECHO_Power_ECHO_Value = 0;
        public double ECHO_Length_Value = 0;
        public bool IsLoaded = false;
        public Music_Player_Setting_Page()
        {
            InitializeComponent();
            LPF_S.ValueChanged += LPF_S_ValueChanged;
            HPF_S.ValueChanged += HPF_S_ValueChanged;
            ECHO_Delay_S.ValueChanged += ECHO_Delay_S_ValueChanged;
            ECHO_Power_Original_S.ValueChanged += ECHO_Power_Original_S_ValueChanged;
            ECHO_Power_ECHO_S.ValueChanged += ECHO_Power_ECHO_S_ValueChanged;
            ECHO_Length_S.ValueChanged += ECHO_Length_S_ValueChanged;
            LPF_Enable_C.CheckedChanged += LPF_Enable_C_ChanckedChanged;
            HPF_Enable_C.CheckedChanged += HPF_Enable_C_ChanckedChanged;
            ECHO_Enable_C.CheckedChanged += ECHO_Enable_C_ChackedChanged;
        }
        private void LPF_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            IsLPFChanged = true;
            LPF_Value = e.NewValue;
            LPF_T.Text = "Low Pass Filter:" + (int)LPF_S.Value;
        }
        private void HPF_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            IsHPFChanged = true;
            HPF_Value = e.NewValue;
            HPF_T.Text = "High Pass Filter:" + (int)HPF_S.Value;
        }
        private void ECHO_Delay_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            IsECHOChanged = true;
            ECHO_Delay_Value = e.NewValue;
            ECHO_Delay_T.Text = "エコー(遅延):" + Math.Round(ECHO_Delay_S.Value, 1, MidpointRounding.AwayFromZero) + "秒";
        }
        private void ECHO_Power_Original_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            IsECHOChanged = true;
            ECHO_Power_Original_Value = e.NewValue;
            ECHO_Power_Original_T.Text = "エコー(元音量):" + (int)ECHO_Power_Original_S.Value;
        }
        private void ECHO_Power_ECHO_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            IsECHOChanged = true;
            ECHO_Power_ECHO_Value = e.NewValue;
            ECHO_Power_ECHO_T.Text = "エコー音量:" + (int)ECHO_Power_ECHO_S.Value;
        }
        private void ECHO_Length_S_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            IsECHOChanged = true;
            ECHO_Length_Value = e.NewValue;
            ECHO_Length_T.Text = "エコー(長さ):" + (int)ECHO_Length_S.Value;
        }
        private void LPF_Enable_C_ChanckedChanged(object sender, CheckedChangedEventArgs e)
        {
            IsLPFEnable = e.Value;
            ChangeLPFEnable(e.Value);
        }
        private void HPF_Enable_C_ChanckedChanged(object sender, CheckedChangedEventArgs e)
        {
            IsHPFEnable = e.Value;
            ChangeHPFEnable(e.Value);
        }
        private void ECHO_Enable_C_ChackedChanged(object sender, CheckedChangedEventArgs e)
        {
            IsECHOEnable = e.Value;
            ChangeECHOEnable(e.Value);
        }
        public void Configs_Load()
        {
            if (IsLoaded)
                return;
            if (File.Exists(Sub_Code.ExDir + "/Configs/Music_Player_Setting.dat"))
            {
                try
                {
                    Sub_Code.File_Decrypt_To_File(Sub_Code.ExDir + "/Configs/Music_Player_Setting.dat", Sub_Code.ExDir + "/Configs/Music_Player_Setting.tmp", "ISCREAM_SRTTbacon_Cry", false);
                    StreamReader str = new StreamReader(Sub_Code.ExDir + "/Configs/Music_Player_Setting.tmp");
                    LPF_S.Value = double.Parse(str.ReadLine());
                    HPF_S.Value = double.Parse(str.ReadLine());
                    ECHO_Delay_S.Value = double.Parse(str.ReadLine());
                    ECHO_Power_Original_S.Value = double.Parse(str.ReadLine());
                    ECHO_Power_ECHO_S.Value = double.Parse(str.ReadLine());
                    ECHO_Length_S.Value = double.Parse(str.ReadLine());
                    ECHO_Enable_C.IsChecked = bool.Parse(str.ReadLine());
                    str.Close();
                    File.Delete(Sub_Code.ExDir + "/Configs/Music_Player_Setting.tmp");
                    IsLoaded = true;
                }
                catch (Exception e1)
                {
                    Sub_Code.Error_Log_Write(e1.Message);
                }
            }
            if (!IsLoaded)
            {
                ECHO_Delay_S.Value = 0.3;
                ECHO_Power_Original_S.Value = 85;
                ECHO_Power_ECHO_S.Value = 30;
                ECHO_Length_S.Value = 45;
                LPF_Enable_C.IsChecked = false;
                HPF_Enable_C.IsChecked = false;
            }
            IsLoaded = true;
        }
        private void Configs_Save()
        {
            try
            {
                StreamWriter stw = File.CreateText(Sub_Code.ExDir + "/Configs/Music_Player_Setting.tmp");
                stw.WriteLine(LPF_S.Value);
                stw.WriteLine(HPF_S.Value);
                stw.WriteLine(ECHO_Delay_S.Value);
                stw.WriteLine(ECHO_Power_Original_S.Value);
                stw.WriteLine(ECHO_Power_ECHO_S.Value);
                stw.WriteLine(ECHO_Length_S.Value);
                stw.Write(ECHO_Enable_C.IsChecked);
                stw.Close();
                Sub_Code.File_Encrypt(Sub_Code.ExDir + "/Configs/Music_Player_Setting.tmp", Sub_Code.ExDir + "/Configs/Music_Player_Setting.dat", "ISCREAM_SRTTbacon_Cry", true);
            }
            catch (Exception e)
            {
                Sub_Code.Error_Log_Write(e.Message);
            }
        }
        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            Configs_Save();
        }
    }
}