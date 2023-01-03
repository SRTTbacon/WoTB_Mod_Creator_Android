using System;
using WoTB_Mod_Creater.Class;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WoTB_Mod_Creater.All_Page
{
    public partial class Credit_Page : ContentPage
    {
        public Credit_Page()
        {
            InitializeComponent();
            Version_T.Text = "現在のバージョン : V" + SRTTbacon_Server.Version;
        }
        private async void Open_Discord_Link(object sender, EventArgs e)
        {
            bool Test = await DisplayAlert("確認", "Discordサーバーへのリンクを開きますか?", "はい", "いいえ");
            if (Test)
            {
                await Browser.OpenAsync("https://discord.gg/bSupFP2eN3", new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Show,
                    PreferredToolbarColor = Color.AliceBlue,
                    PreferredControlColor = Color.Violet
                });
            }
        }
    }
}