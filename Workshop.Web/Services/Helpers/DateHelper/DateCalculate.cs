namespace Workshop.Web.Services.Helpers.DateHelper
{
    public static class DateCalculate
    {
        public static string GetDateDifference(DateTime date1, DateTime date2, string lang = "en")
        {
            TimeSpan difference = date1.Date - date2.Date;

            int yearsDifference = Math.Abs(difference.Days / 360);
            int monthsDifference = Math.Abs((difference.Days % 360) / 30);
            int weeksDifference = Math.Abs((difference.Days % 360) % 30 / 7);
            int daysDifference = Math.Abs((difference.Days % 360) % 7);

            bool isArabic = lang.ToLower() == "ar";

            if (yearsDifference > 0)
                return isArabic
                    ? $"{yearsDifference} {(yearsDifference == 1 ? "سنة" : "سنوات")}"
                    : $"{yearsDifference} {(yearsDifference == 1 ? "Year" : "Years")}";

            if (monthsDifference > 0)
                return isArabic
                    ? $"{monthsDifference} {(monthsDifference == 1 ? "شهر" : "أشهر")}"
                    : $"{monthsDifference} {(monthsDifference == 1 ? "Month" : "Months")}";

            if (weeksDifference > 0)
                return isArabic
                    ? $"{weeksDifference} {(weeksDifference == 1 ? "أسبوع" : "أسابيع")}"
                    : $"{weeksDifference} {(weeksDifference == 1 ? "Week" : "Weeks")}";

            if (daysDifference > 0)
                return isArabic
                    ? $"{daysDifference} {(daysDifference == 1 ? "يوم" : "أيام")}"
                    : $"{daysDifference} {(daysDifference == 1 ? "Day" : "Days")}";

            return isArabic ? "اليوم" : "Today";
        }



        public static string GetDecimalDifference(decimal value1, decimal value2, string lang = "en")
        {
            decimal dif = value2 - value1;
            bool isOverdue = value2 > value1;

            string unit;
            string status;

            switch (lang.ToLower())
            {
                case "ar":
                    unit = "كم";
                    status = isOverdue ? "متأخر" : "متبقي";
                    break;

                default: // English
                    unit = "km";
                    status = isOverdue ? "Overdue" : "From now";
                    break;
            }

            return $"{Math.Abs(dif)} {unit} {status}";
        }

    }
}
