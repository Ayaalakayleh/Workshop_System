using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopMovement;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IWorkshopMovementRepository
    {
        Task<int> WorkshopInvoiceInsertAsync(MovementInvoice invoice);
        Task<int> DExternalWorkshopInvoiceInsertAsync(MovementInvoice invoice);
        Task<VehicleMovementStatusDTO> CheckVehicleMovementStatusAsync(int vehicleId);
        Task<VehicleMovement> InsertVehicleMovementAsync(VehicleMovement movement);
        Task InsertMWorkshopMovementStrikesAsync(int movementId, string strikes);
        Task InsertMovementDocumentAsync(VehicleMovementDocument movmentDoc);
        Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovement(WorkshopMovementFilter filter);
        Task<VehicleMovement> GetVehicleMovementByIdAsync(int movementId);
        Task<List<VehicleMovementDocument>> GetMovementDocumentsAsync(int movementId);
        Task<List<MovementInvoice>> GetWorkshopInvoiceByMovementIdAsync(int movementId);
        Task<VehicleMovement> GetLastVehicleMovementByVehicleIdAsync(int vehicleId);
        Task<string> GetVehicleMovementStrikeAsync(int movementId);
        Task UpdateVehicleMovementStatusAync(int workshopId, Guid masterId);
        Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovementDDL(WorkshopMovementFilter filter);
		Task<VehicleMovementDTO> WorkshopMovement_GetFirstAgreementMovementByVehicleId(int vehicleId);
		Task<string> MovementStrikes_GetAsync(int movementId);
		Task UpdateMovementReplacementAsync(int movementId);
		Task<VehicleMovement> VehicleMovement_GetLastMovementOutByWorkOrderIdAsync(int workOrderId);
        Task<IEnumerable<VehicleChecklistLookup>> GetVehicleChecklistLookup();
        Task<IEnumerable<VehicleChecklistLookup>> GetTyresChecklistLookup();
        Task<IEnumerable<VehicleChecklist>> GetVehicleChecklistByMovementId(int? movementId);
        Task<IEnumerable<TyreChecklist>> GetTyresChecklistByMovementId(int? movementId);
        Task<int> InsertVehicleChecklist(VehicleChecklist vehicleChecklist);
        Task<int> InsertTyreChecklist(TyreChecklist tyreChecklist);
        Task<int> UpdateVehicleChecklist(VehicleChecklist vehicleChecklist);
        Task<int> UpdateTyreChecklist(TyreChecklist tyreChecklist);
        Task<List<MovementInvoice>> GetWorkshopInvoiceByWorkOrderId(int workOrderId);

    }
}
