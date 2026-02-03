namespace WaterIntake;

public partial class Splash : ContentPage
{
    public Splash()
    {
        InitializeComponent();
        NavigateToMain();
    }

    private async void NavigateToMain()
    {
        await Task.Delay(2000); // 2 seconds

        Application.Current.MainPage = new AppShell();
    }
}
