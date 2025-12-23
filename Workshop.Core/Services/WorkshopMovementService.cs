using System.Data;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopMovement;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
	public class WorkshopMovementService : IWorkshopMovementService
	{
		private readonly IWorkshopMovementRepository _workshopMovementRepository;

		public WorkshopMovementService(IWorkshopMovementRepository workshopMovementRepository)
		{
			_workshopMovementRepository = workshopMovementRepository;
		}

		#region MovementIn
		public async Task<bool> WorkshopInvoiceInsertAsync(MovementInvoice invoice)
		{
			try
			{

				var result = await _workshopMovementRepository.WorkshopInvoiceInsertAsync(invoice);
				return result > 0;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<int> DExternalWorkshopInvoiceInsertAsync(MovementInvoice invoice)
		{
			return await _workshopMovementRepository.DExternalWorkshopInvoiceInsertAsync(invoice);
		}

		public async Task<VehicleMovementStatusDTO> CheckVehicleMovementStatusAsync(int vehicleId)
		{
			return await _workshopMovementRepository.CheckVehicleMovementStatusAsync(vehicleId);
		}

		public async Task<VehicleMovement> InsertVehicleMovementAsync(VehicleMovement movement)
		{
			return await _workshopMovementRepository.InsertVehicleMovementAsync(movement);
		}

		public async Task InsertMWorkshopMovementStrikesAsync(int movementId, string strikes)
		{
			await _workshopMovementRepository.InsertMWorkshopMovementStrikesAsync(movementId, strikes);
		}

		public async Task InsertMovementDocumentAsync(VehicleMovementDocument movmentDoc)
		{
			await _workshopMovementRepository.InsertMovementDocumentAsync(movmentDoc);
		}

        public async Task<IEnumerable<VehicleChecklistLookup>> GetVehicleChecklistLookup()
		{
			return await _workshopMovementRepository.GetVehicleChecklistLookup();
		}
        public async Task<IEnumerable<VehicleChecklistLookup>> GetTyresChecklistLookup()
		{
            return await _workshopMovementRepository.GetTyresChecklistLookup();
        }
        public async Task<IEnumerable<VehicleChecklist>> GetVehicleChecklistByMovementId(int? movementId)
		{
            return await _workshopMovementRepository.GetVehicleChecklistByMovementId(movementId);
        }
        public async Task<IEnumerable<TyreChecklist>> GetTyresChecklistByMovementId(int? movementId)
		{
            return await _workshopMovementRepository.GetTyresChecklistByMovementId(movementId);
        }
        public async Task<int> InsertVehicleChecklist(VehicleChecklist vehicleChecklist)
		{
            return await _workshopMovementRepository.InsertVehicleChecklist(vehicleChecklist);
        }
        public async Task<int> InsertTyreChecklist(TyreChecklist tyreChecklist)
		{
            return await _workshopMovementRepository.InsertTyreChecklist(tyreChecklist);
        }
        public async Task<int> UpdateVehicleChecklist(VehicleChecklist vehicleChecklist)
		{
            return await _workshopMovementRepository.UpdateVehicleChecklist(vehicleChecklist);
        }
        public async Task<int> UpdateTyreChecklist(TyreChecklist tyreChecklist)
		{
            return await _workshopMovementRepository.UpdateTyreChecklist(tyreChecklist);
        }
        #endregion

        #region Movements
        public async Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovement(WorkshopMovementFilter filter)
		{
			return await _workshopMovementRepository.GetAllDWorkshopVehicleMovement(filter);
		}

		public async Task<VehicleMovement> GetVehicleMovementByIdAsync(int movementId)
		{
			return await _workshopMovementRepository.GetVehicleMovementByIdAsync(movementId);
		}

		public async Task<List<VehicleMovementDocument>> GetMovementDocumentsAsync(int movementId)
		{
			return await _workshopMovementRepository.GetMovementDocumentsAsync(movementId);
		}

		public async Task<List<MovementInvoice>> GetWorkshopInvoiceByMovementIdAsync(int movementId)
		{
			return await _workshopMovementRepository.GetWorkshopInvoiceByMovementIdAsync(movementId);
		}

		public async Task<VehicleMovement> GetLastVehicleMovementByVehicleIdAsync(int vehicleId)
		{
			return await _workshopMovementRepository.GetLastVehicleMovementByVehicleIdAsync(vehicleId);
		}

		public async Task<string> GetVehicleMovementStrikeAsync(int movementId)
		{
			return await _workshopMovementRepository.GetVehicleMovementStrikeAsync(movementId);
		}

		public async Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovementDDL(WorkshopMovementFilter filter)
		{
			return await _workshopMovementRepository.GetAllDWorkshopVehicleMovementDDL(filter);
		}
		public async Task<VehicleMovementDTO> WorkshopMovement_GetFirstAgreementMovementByVehicleId(int vehicleId)
		{
			try
			{
				return await _workshopMovementRepository.WorkshopMovement_GetFirstAgreementMovementByVehicleId(vehicleId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in WorkshopMovement_GetFirstAgreementMovementByVehicleId", ex);
			}
		}
		public async Task<string> MovementStrikes_GetAsync(int movementId)
		{
			try
			{
				return await _workshopMovementRepository.MovementStrikes_GetAsync(movementId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in MovementStrikes_GetAsync", ex);
			}
		}
		public async Task UpdateMovementReplacementAsync(int movementId)
		{
			try
			{
				await _workshopMovementRepository.UpdateMovementReplacementAsync(movementId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in UpdateMovementReplacementAsync", ex);
			}
		}
		public async Task<VehicleMovement> VehicleMovement_GetLastMovementOutByWorkOrderIdAsync(int workOrderId)
		{
			try
			{
				return await _workshopMovementRepository.VehicleMovement_GetLastMovementOutByWorkOrderIdAsync(workOrderId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in VehicleMovement_GetLastMovementOutByWorkOrderIdAsync", ex);
			}
		}

		#endregion

		#region MovementOut


		public async Task UpdateVehicleMovementStatusAync(int workshopId, Guid masterId)
		{
			await _workshopMovementRepository.UpdateVehicleMovementStatusAync(workshopId, masterId);
		}
		#endregion
	}
}