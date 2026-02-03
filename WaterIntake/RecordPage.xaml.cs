using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Controls;      // ✅ Correct MAUI namespace
using System.Windows.Input;         // ✅ Keep this ONLY for ICommand

namespace WaterIntake
{
    public partial class RecordPage : ContentPage
    {
        public ObservableCollection<RecordItem> Records { get; set; }
        public ICommand RefreshCommand { get; }

        public RecordPage()
        {
            InitializeComponent();

            Records = new ObservableCollection<RecordItem>();
            RefreshCommand = new Command(async () => await LoadRecords());

            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadRecords();
        }

        private async Task LoadRecords()
        {
            try
            {
                Records.Clear();

                var firebase = new FirebaseHelper();
                var all = await firebase.GetAllRecords() ?? new List<RecordItem>();

                // Convert Date and Time into sortable DateTime
                foreach (var r in all)
                {
                    if (DateTime.TryParse(r.Time, out DateTime timeParsed))
                        r.SortTime = timeParsed;
                    else
                        r.SortTime = DateTime.MinValue;

                    if (DateTime.TryParseExact(r.Date, "dd-MM-yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateParsed))
                        r.SortDate = dateParsed;
                    else
                        r.SortDate = DateTime.MinValue;
                }

                // Sort newest first
                var sorted = all
                    .OrderByDescending(r => r.SortDate)
                    .ThenByDescending(r => r.SortTime)
                    .ToList();

                // For each record, calculate progress
                foreach (var record in sorted)
                {
                    int goal = 0;
                    bool hasGoal = false;

                    if (!string.IsNullOrWhiteSpace(record.Goal) && record.Goal != "—")
                        hasGoal = int.TryParse(record.Goal.Replace(" ML", ""), out goal);

                    string progress = "No Goal";
                    string color = "#888888";

                    if (hasGoal)
                    {
                        if (record.Intake >= goal)
                        {
                            progress = "Complete";
                            color = "#41A445";
                        }
                        else
                        {
                            progress = "In-Progress";
                            color = "#D98A2F";
                        }
                    }

                    Records.Add(new RecordItem
                    {
                        Key = record.Key,
                        Date = record.Date,
                        Time = record.Time,
                        Intake = record.Intake,
                        IntakeDisplay = $"{record.Intake} ML",
                        Goal = hasGoal ? $"{goal} ML" : "—",
                        Progress = progress,
                        ProgressColor = color,
                        SortDate = record.SortDate,
                        SortTime = record.SortTime
                    });
                }
            }
            finally
            {
                RecordsRefreshView.IsRefreshing = false;
            }
        }

        // ✏ EDIT RECORD
        private async void OnEditButtonClicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var record = btn.BindingContext as RecordItem;

            string newIntake = await DisplayPromptAsync(
                "Edit Intake",
                "Enter intake (ML):",
                initialValue: record.Intake.ToString(),
                keyboard: Microsoft.Maui.Keyboard.Numeric  // ✅ FIX
            );

            if (string.IsNullOrWhiteSpace(newIntake)) return;

            string newGoal = await DisplayPromptAsync(
                "Edit Goal",
                "Enter goal (ML):",
                initialValue: record.Goal.Replace(" ML", ""),
                keyboard: Microsoft.Maui.Keyboard.Numeric  // ✅ FIX
            );

            if (string.IsNullOrWhiteSpace(newGoal)) return;

            string newTime = await DisplayPromptAsync(
                "Edit Time",
                "Enter time (HH:mm:ss):",
                initialValue: record.Time
            );

            if (string.IsNullOrWhiteSpace(newTime)) return;

            var firebase = new FirebaseHelper();
            await firebase.UpdateRecord(record.Date, record.Key,
                                       newTime,
                                       int.Parse(newIntake),
                                       int.Parse(newGoal));

            await LoadRecords();
        }

        // 🗑 DELETE RECORD
        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var record = btn.BindingContext as RecordItem;

            bool confirm = await DisplayAlert(
                "Delete",
                $"Delete this record?\n{record.Date} {record.Time}",
                "Delete",
                "Cancel"
            );

            if (!confirm) return;

            var firebase = new FirebaseHelper();
            await firebase.DeleteRecord(record.Date, record.Key);

            await LoadRecords();
        }

        // Menu logic
        async void OnMenuClicked(object sender, EventArgs e)
        {
            string choice = await DisplayActionSheet(
                "Menu",
                "Cancel",
                null,
                "Home",
                "Record",
                "Information"
            );

            switch (choice)
            {
                case "Home":
                    await Shell.Current.GoToAsync("//MainPage");
                    break;

                case "Record":
                    await Shell.Current.GoToAsync("//RecordPage");
                    break;

                case "Information":
                    await Shell.Current.GoToAsync("//InformationPage");
                    break;
            }
        }
    }

    public class RecordItem
    {
        public string Key { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int Intake { get; set; }
        public string IntakeDisplay { get; set; }
        public string Goal { get; set; }
        public string Progress { get; set; }
        public string ProgressColor { get; set; }

        public DateTime SortDate { get; set; }
        public DateTime SortTime { get; set; }
    }
}
