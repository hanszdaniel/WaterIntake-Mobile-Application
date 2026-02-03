using Microsoft.Maui.ApplicationModel;

namespace WaterIntake
{
    public partial class InformationPage : ContentPage
    {
        public string AppVersion { get; set; }
        public string Developer { get; set; }

        public InformationPage()
        {
            InitializeComponent();

            AppVersion = AppInfo.Current.VersionString;
            Developer = "Hansz Daniel bin Suphian";

            BindingContext = this;
        }

        // ✅ Menu Pop-Up Navigation (same as other pages)
        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(
                "Menu",     // title
                "Cancel",   // cancel button
                null,       // destructive button
                "Home",
                "Record",
                "Information"
            );

            switch (action)
            {
                case "Home":
                    await Navigation.PushAsync(new MainPage());
                    break;

                case "Record":
                    await Navigation.PushAsync(new RecordPage());
                    break;

                case "Information":
                    // Already here — do nothing
                    break;
            }
        }
    }
}
