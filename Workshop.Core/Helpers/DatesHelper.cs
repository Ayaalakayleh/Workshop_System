using System.Text;

namespace Workshop.Core.Helpers
{
    public static class DatesHelper
    {
        #region DOW
        // Convert boolean day properties to DOW string
        public static string ConvertToDOW(bool sun, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat)
        {
            var dowBuilder = new StringBuilder();

            if (sun) dowBuilder.Append("SUN,");
            if (mon) dowBuilder.Append("MON,");
            if (tue) dowBuilder.Append("TUE,");
            if (wed) dowBuilder.Append("WED,");
            if (thu) dowBuilder.Append("THU,");
            if (fri) dowBuilder.Append("FRI,");
            if (sat) dowBuilder.Append("SAT,");

            // Remove trailing comma
            if (dowBuilder.Length > 0)
                dowBuilder.Length--;

            return dowBuilder.ToString();
        }

        // Convert DOW string to boolean properties
        public static (bool Sun, bool Mon, bool Tue, bool Wed, bool Thu, bool Fri, bool Sat) ConvertFromDOW(string dow)
        {
            if (string.IsNullOrEmpty(dow))
                return (false, false, false, false, false, false, false);

            var days = dow.Split(',');

            return (
                days.Contains("SUN"),
                days.Contains("MON"),
                days.Contains("TUE"),
                days.Contains("WED"),
                days.Contains("THU"),
                days.Contains("FRI"),
                days.Contains("SAT")
            );
        }

        // Convert to abbreviated format (e.g., "SUN,MON,TUE")
        public static string ToAbbreviatedString(bool sun, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat)
        {
            return ConvertToDOW(sun, mon, tue, wed, thu, fri, sat);
        }

        // Convert to full name format (e.g., "Sunday,Monday,Tuesday")
        public static string ToFullNameString(bool sun, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat)
        {
            var fullNameBuilder = new StringBuilder();

            if (sun) fullNameBuilder.Append("Sunday,");
            if (mon) fullNameBuilder.Append("Monday,");
            if (tue) fullNameBuilder.Append("Tuesday,");
            if (wed) fullNameBuilder.Append("Wednesday,");
            if (thu) fullNameBuilder.Append("Thursday,");
            if (fri) fullNameBuilder.Append("Friday,");
            if (sat) fullNameBuilder.Append("Saturday,");

            if (fullNameBuilder.Length > 0)
                fullNameBuilder.Length--;

            return fullNameBuilder.ToString();
        }

        // Convert to visual representation (e.g., "● ● ● ○ ○ ○ ○")
        public static string ToVisualRepresentation(bool sun, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat)
        {
            return $"{(sun ? "●" : "○")} {(mon ? "●" : "○")} {(tue ? "●" : "○")} {(wed ? "●" : "○")} {(thu ? "●" : "○")} {(fri ? "●" : "○")} {(sat ? "●" : "○")}";
        }

        // Get number of selected days
        public static int GetSelectedDaysCount(bool sun, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat)
        {
            int count = 0;
            if (sun) count++;
            if (mon) count++;
            if (tue) count++;
            if (wed) count++;
            if (thu) count++;
            if (fri) count++;
            if (sat) count++;
            return count;
        }

        // Check if all days are selected
        public static bool AreAllDaysSelected(bool sun, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat)
        {
            return sun && mon && tue && wed && thu && fri && sat;
        }

        // Check if no days are selected
        public static bool AreNoDaysSelected(bool sun, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat)
        {
            return !sun && !mon && !tue && !wed && !thu && !fri && !sat;
        }

        // Get list of selected day names
        public static List<string> GetSelectedDayNames(bool sun, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat, bool abbreviated = true)
        {
            var selectedDays = new List<string>();

            if (sun) selectedDays.Add(abbreviated ? "SUN" : "Sunday");
            if (mon) selectedDays.Add(abbreviated ? "MON" : "Monday");
            if (tue) selectedDays.Add(abbreviated ? "TUE" : "Tuesday");
            if (wed) selectedDays.Add(abbreviated ? "WED" : "Wednesday");
            if (thu) selectedDays.Add(abbreviated ? "THU" : "Thursday");
            if (fri) selectedDays.Add(abbreviated ? "FRI" : "Friday");
            if (sat) selectedDays.Add(abbreviated ? "SAT" : "Saturday");

            return selectedDays;
        }
        #endregion
    }
}