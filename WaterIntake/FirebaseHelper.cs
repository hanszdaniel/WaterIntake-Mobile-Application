using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WaterIntake
{
    public class FirebaseHelper
    {
        FirebaseClient firebase;

        public FirebaseHelper()
        {
            firebase = new FirebaseClient(
                "https://waterintakeapp-b69f5-default-rtdb.asia-southeast1.firebasedatabase.app/"
            );
        }

        // ============================================================
        // SAVE RECORD
        // ============================================================
        public async Task SaveDailyRecord(string date, string time, int intake, int goal)
        {
            await firebase
                .Child("DailyRecords")
                .Child(date)
                .PostAsync(new RecordFirebase
                {
                    time = time,
                    intake = intake,
                    goal = goal
                });
        }

        // ============================================================
        // LOAD ALL RECORDS WITH FIREBASE KEY
        // ============================================================
        public async Task<List<RecordItem>> GetAllRecords()
        {
            var list = new List<RecordItem>();

            // Load all date groups
            var days = await firebase.Child("DailyRecords").OnceAsync<object>();

            foreach (var day in days)
            {
                string date = day.Key;

                // Load entries for each date
                var entries = await firebase
                    .Child("DailyRecords")
                    .Child(date)
                    .OnceAsync<RecordFirebase>();

                foreach (var entry in entries)
                {
                    var obj = entry.Object;

                    string timeValue = string.IsNullOrEmpty(obj.time) ? "--:--" : obj.time;
                    string goalDisplay = obj.goal > 0 ? $"{obj.goal} ML" : "—";

                    list.Add(new RecordItem
                    {
                        Key = entry.Key,               // 🔥 FIREBASE KEY
                        Date = date,
                        Time = timeValue,
                        Intake = obj.intake,
                        IntakeDisplay = $"{obj.intake} ML",
                        Goal = goalDisplay
                    });
                }
            }

            return list;
        }

        // ============================================================
        // DELETE A SPECIFIC RECORD
        // ============================================================
        public async Task DeleteRecord(string date, string key)
        {
            await firebase
                .Child("DailyRecords")
                .Child(date)
                .Child(key)
                .DeleteAsync();
        }

        // ============================================================
        // UPDATE A SPECIFIC RECORD
        // ============================================================
        public async Task UpdateRecord(string date, string key, string time, int intake, int goal)
        {
            await firebase
                .Child("DailyRecords")
                .Child(date)
                .Child(key)
                .PutAsync(new RecordFirebase
                {
                    time = time,
                    intake = intake,
                    goal = goal
                });
        }

        // ============================================================
        // FIREBASE MODEL
        // ============================================================
        public class RecordFirebase
        {
            public string time { get; set; }
            public int intake { get; set; }
            public int goal { get; set; }
        }
    }
}
