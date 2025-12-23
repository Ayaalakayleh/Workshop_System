using Workshop.Core.DTOs;
using Workshop.Core.DTOs.WorkshopDTOs;

namespace Workshop.Infrastructure.Repositories
{

    public class ExternalWorkshopInvoiceRepository : IExternalWorkshopInvoiceRepository
    {
        private readonly Database _database;

        public ExternalWorkshopInvoiceRepository(Database database)
        {
            _database = database;
        }

        public async Task<List<WorkShopDefinitionDTO>> GetWorkshopDetailsAsync(WorkShopFilterDTO filter)
        {
            var parameters = new
            {
                WorkshopId = filter.Id ?? (object)DBNull.Value,
                FromDate = filter.FromDate ?? (object)DBNull.Value,
                ToDate = filter.ToDate ?? (object)DBNull.Value
            };

            var result = await _database.ExecuteGetAllStoredProcedure<WorkShopDefinitionDTO>(
                "M_ExternalWorkshopInvoice_GetWorkshop",
                parameters);

            // Apply language-based name computation
            if (result != null)
            {
                foreach (var workshop in result)
                {
                    workshop.Name = filter.Language == "en" ? workshop.PrimaryName : workshop.SecondaryName;
                }
            }

            return result?.ToList() ?? new List<WorkShopDefinitionDTO>();
        }

        public async Task<List<ExternalWorkshopInvoiceDetailsDTO>> GetInvoiceDetailsAsync(ExternalWorkshopInvoiceDetailsFilterDTO filter)
        {
            var parameters = new
            {
                WorkshopId = filter.WorkshopId,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                ExternalWorkshopId = filter.ExternalWorkshopId
            };

            var result = await _database.ExecuteGetAllStoredProcedure<ExternalWorkshopInvoiceDetailsDTO>(
                "M_ExternalWorkshopInvoiceDetails_GetDetails",
                parameters);

            // Apply computed WorkOrderTitle
            if (result != null)
            {
                foreach (var detail in result)
                {
                    detail.WorkOrderTitle = $"{detail.BranchId}{detail.WorkOrderNo:D3}";
                }
            }

            return result?.ToList() ?? new List<ExternalWorkshopInvoiceDetailsDTO>();
        }
		public async Task<IEnumerable<WorkshopInvoice>> M_WorkshopInvoice_GetWorkshop(DateTime? fromDate, DateTime? toDate, int? customerId, int? vehicleId, int? projectId, int? companyId)
		{
			var parameters = new
			{
				FromDate = fromDate,
				ToDate = toDate,
				CustomerId = customerId ?? (object)DBNull.Value,
				VehicleId = vehicleId ?? (object)DBNull.Value,
				ProjectId = projectId ?? (object)DBNull.Value,
				CompanyId = companyId ?? (object)DBNull.Value
			};

			return await _database.ExecuteGetAllStoredProcedure<WorkshopInvoice>(
				"M_WorkshopInvoice_GetWorkshop",
				parameters
			);
		}
	}
}