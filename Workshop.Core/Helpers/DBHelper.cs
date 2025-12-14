
using System.Data;

namespace Workshop.Core.Helpers
{
    class DBHelper
    {
        public static bool IsDBSetNullOrEmpty(DataSet dataSet)
        {
            if (dataSet == null || dataSet.Tables.Count == 0)
            {
                return true; // DataSet is null or has no tables
            }
            foreach (DataTable table in dataSet.Tables)
            {
                if (table.Rows.Count > 0)
                {
                    return false; // At least one table has rows
                }
            }
            return true; // All tables are empty
        }
    }
}
