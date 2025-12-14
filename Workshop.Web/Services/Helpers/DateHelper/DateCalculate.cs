namespace Workshop.Web.Services.Helpers.DateHelper
{
    public static class DateCalculate
    {
        public static string getDateDifference(DateTime date1, DateTime date2)
        {
            TimeSpan difference = date1.Date - date2.Date;
            string result = "";
            int yearsDifference = Math.Abs(difference.Days / 360);
            int weeksDifference = Math.Abs((difference.Days % 360) % 30 / 7);
            int monthsDifference = Math.Abs((difference.Days % 360) / 30);
            int daysDifference = Math.Abs((difference.Days % 360) % 7);

            if (yearsDifference > 0)
                result += $"{yearsDifference} {(yearsDifference == 1 ? "Year" : "Years")} ";

            else if (monthsDifference > 0)
                result += $"{monthsDifference} {(monthsDifference == 1 ? "Month" : "Monthes")} ";

            else if (weeksDifference > 0)
                result += $"{weeksDifference} {(weeksDifference == 1 ?" Week" : "Weeks")} ";

            else if (daysDifference > 0)
                result += $"{daysDifference} {(daysDifference == 1 ? "Day" : "Days")} ";

            return result;
        }


        public static string getDecimalDifference(decimal value1, decimal value2)
        {
            decimal dif = value2 - value1;
            string agoOrFromNow = value2 > value1 ? "Overdue" : "fromnow";
            return $"{Math.Abs(dif)} {"km"} {agoOrFromNow}";
        }
    }
}
