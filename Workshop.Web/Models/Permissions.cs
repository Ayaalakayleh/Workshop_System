namespace Workshop.Web.Models
{
    public static class Permissions
    {
        #region AccountDefinitions
        public static class AccountDefinitions
        {
            public const int View = 1882;
            public const int Create = 1883;
        }
        #endregion

        #region TechnicianProfile
        public static class TechnicianProfile
        {
            public const int View = 1884;
            public const int Create = 1885;
            public const int Edit = 1886;
            public const int Delete = 1887;
        }
        #endregion

        #region LookupManagement
        public static class LookupManagement
        {
            public const int View = 1888;
            public const int Create = 1889;
            public const int Delete = 1890;
        }
        #endregion

        #region Recall
        public static class Recall
        {
            public const int View = 1891;
            public const int Create = 1892;
            public const int Edit = 1893;
            public const int Delete = 1894;
        }
        #endregion

        #region RTSCode
        public static class RTSCode
        {
            public const int View = 1895;
            public const int Create = 1896;
            public const int Edit = 1897;
            public const int Delete = 1898;
        }
        #endregion

        #region Menu
        public static class Menu
        {
            public const int View = 1899;
            public const int Create = 1900;
            public const int Edit = 1901;
            public const int Delete = 1902;
        }
        #endregion

        #region Teams
        public static class Teams
        {
            public const int View = 1903;
            public const int Create = 1904;
            public const int Edit = 1905;
            public const int Delete = 1906;
        }
        #endregion

        #region Shifts
        public static class Shifts
        {
            public const int View = 1907;
            public const int Create = 1908;
            public const int Edit = 1909;
            public const int Delete = 1910;
        }
        #endregion

        #region PriceMatrix
        public static class PriceMatrix
        {
            public const int View = 1911;
            public const int Create = 1912;
            public const int Delete = 1913;
            public const int Edit = 1914;
        }
        #endregion

        #region ServiceReminder
        public static class ServiceReminder
        {
            public const int View = 1915;
            public const int Edit = 1916;
            public const int Delete = 1917;
            public const int Create = 1918;
        }
        #endregion

        #region WorkshopDefinitions
        public static class WorkshopDefinitions
        {
            public const int View = 1919;
            public const int Create = 1920;
            public const int Edit = 1921;
        }
        #endregion

        #region VehicleDefinition
        public static class VehicleDefinition
        {
            public const int View = 1922;
            public const int Create = 1923;
            public const int Edit = 1924;
        }
        #endregion

        #region WorkshopLoading
        public static class WorkshopLoading
        {
            public const int View = 1925;
            public const int Create = 1926;
            public const int Edit = 1927;
        }
        #endregion

        #region WorkOrder
        public static class WorkOrder
        {
            public const int View = 1928;
            public const int Create = 1929;
            public const int Edit = 1930;
        }
        #endregion

        #region WIP
        public static class WIP
        {
            public const int View = 1931;
            public const int Create = 1932;
            public const int Edit = 1933;
            public const int AddRTS = 1934;
            public const int AddParts = 1935;
            public const int Approve = 1936;
            public const int Reject = 1937;
            public const int Issue = 1938;
            public const int UndoIssue = 1939;
            public const int AssignTech = 1940;
            public const int ReOpen = 1941;
            public const int Close = 1942;
            public const int Invoice = 1943;
            public const int Transfer = 1944;
            public const int AddDiscount = 1945;
        }
        #endregion

        #region TechnicianDashboard
        public static class TechnicianDashboard
        {
            public const int View = 1946;
            public const int Edit = 1947;
            public const int EditClock = 1948;
            public const int Clocking = 1949;
        }
        #endregion

        #region Reservations
        public static class Reservations
        {
            public const int View = 1950;
            public const int Create = 1951;
            public const int BackTime = 1952;
        }
        #endregion

        #region CollectionMapping
        public static class CollectionMapping
        {
            public const int View = 1953;
            public const int Create = 1954;
        }
        #endregion

        #region Collection
        public static class Collection
        {
            public const int View = 1955;
            public const int Create = 1956;
            public const int Edit = 1957;
        }
        #endregion

        #region ExternalWorkshopInvoice
        public static class ExternalWorkshopInvoice
        {
            public const int View = 1958;
            public const int Create = 1959;
        }
        #endregion

        #region ExternalVehicleTransferMovement
        public static class ExternalVehicleTransferMovement
        {
            public const int View = 1960;
            public const int Create = 1961;
        }
        #endregion

        #region MovementIn
        public static class MovementIn
        {
            public const int View = 1962;
            public const int Create = 1963;
            public const int BackTime = 1964;
        }
        #endregion

        #region Movements
        public static class Movements
        {
            public const int View = 1965;
            public const int Details = 1966;
            public const int MovementOutCreate = 1967;
        }
        #endregion

        #region StockCard
        public static class StockCard
        {
            public const int View = 1968;
            public const int Create = 1969;
            public const int Edit = 1970;
            public const int Delete = 1971;
        }
        #endregion
    }
}
