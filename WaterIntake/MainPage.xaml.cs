using Microsoft.Maui.Storage;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace WaterIntake
{
    public partial class MainPage : ContentPage
    {
        int _goal;
        int _todayIntake = 0;

        int quick1;
        int quick2;

        public MainPage()
        {
            InitializeComponent();

            // ✅ Load saved daily goal or default to 2000 ML
            _goal = Preferences.Get("DailyGoal", 2000);

            // ✅ Show goal in Entry when app opens
            GoalEntry.Text = _goal.ToString();

            // ✅ Load saved quick log values
            quick1 = Preferences.Get("quick1", 200);
            quick2 = Preferences.Get("quick2", 500);

            Quick1Btn.Text = $"{quick1} ML";
            Quick2Btn.Text = $"{quick2} ML";

            UpdateUI();
        }

        void UpdateUI()
        {
            PercentLabel.Text =
                $"{Math.Round((double)_todayIntake / _goal * 100)}%";

            ProgressTextLabel.Text =
                $"{_todayIntake} / {_goal} ML";

            UpdateProgressCircle();
        }

        void UpdateProgressCircle()
        {
            ProgressCanvas.InvalidateSurface();
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear();

            float width = e.Info.Width;
            float height = e.Info.Height;

            float centerX = width / 2f;
            float centerY = height / 2f;
            float radius = Math.Min(width, height) / 2f - 20;

            float strokeWidth = 20;

            // Background grey ring
            using var backgroundPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.LightGray,
                StrokeWidth = strokeWidth,
                IsAntialias = true
            };

            // Progress blue ring
            using var progressPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#3A7DFF"),
                StrokeWidth = strokeWidth,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            canvas.DrawCircle(centerX, centerY, radius, backgroundPaint);

            float progress = (float)_todayIntake / _goal;
            progress = Math.Clamp(progress, 0, 1);
            float sweepAngle = 360f * progress;

            using var path = new SKPath();
            path.AddArc(new SKRect(centerX - radius, centerY - radius,
                                   centerX + radius, centerY + radius),
                                   -90, sweepAngle);

            canvas.DrawPath(path, progressPaint);
        }

        // ✅ Update goal + save permanently
        void OnUpdateGoalClicked(object sender, EventArgs e)
        {
            if (int.TryParse(GoalEntry.Text, out int g))
            {
                _goal = g;

                // ✅ Save globally so RecordPage + future sessions stay updated
                Preferences.Set("DailyGoal", g);

                UpdateUI();
            }
        }

        void OnAddQuick1Clicked(object sender, EventArgs e)
        {
            _todayIntake += quick1;
            UpdateUI();
        }

        void OnAddQuick2Clicked(object sender, EventArgs e)
        {
            _todayIntake += quick2;
            UpdateUI();
        }

        void OnAddCustomClicked(object sender, EventArgs e)
        {
            if (int.TryParse(CustomEntry.Text, out int v))
            {
                _todayIntake += v;
                CustomEntry.Text = "";
                UpdateUI();
            }
        }

        void OnResetClicked(object sender, EventArgs e)
        {
            _todayIntake = 0;
            UpdateUI();
        }

        // ✅ Save intake + goal to Firebase
        async void OnSaveClicked(object sender, EventArgs e)
        {
            var firebase = new FirebaseHelper();

            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            await firebase.SaveDailyRecord(date, time, _todayIntake, _goal);

            await DisplayAlert("Saved", "Your water intake has been saved to Firebase!", "OK");
        }

        private async void OnEditQuickLogClicked(object sender, EventArgs e)
        {
            string first = await DisplayPromptAsync("Edit Quick Log 1",
                "Enter new amount:", keyboard: Keyboard.Numeric);

            string second = await DisplayPromptAsync("Edit Quick Log 2",
                "Enter new amount:", keyboard: Keyboard.Numeric);

            if (int.TryParse(first, out int new1) &&
                int.TryParse(second, out int new2))
            {
                quick1 = new1;
                quick2 = new2;

                Preferences.Set("quick1", new1);
                Preferences.Set("quick2", new2);

                Quick1Btn.Text = $"{new1} ML";
                Quick2Btn.Text = $"{new2} ML";
            }
        }

        async void OnMenuClicked(object sender, EventArgs e)
        {
            string choice = await DisplayActionSheet("Menu", "Cancel", null,
                                                     "Home", "Record", "Information");

            if (choice == "Home")
                await Shell.Current.GoToAsync("//MainPage");
            else if (choice == "Record")
                await Shell.Current.GoToAsync("//RecordPage");
            else if (choice == "Information")
                await Shell.Current.GoToAsync("//InformationPage");
        }
    }
}
