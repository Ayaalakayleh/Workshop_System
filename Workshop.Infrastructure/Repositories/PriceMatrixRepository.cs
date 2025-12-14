using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class PriceMatrixRepository : IPriceMatrixRepository
    {

        private readonly DapperContext _context;
        public PriceMatrixRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(CreatePriceMatrixDTO createPriceMatrixDTO)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Applies", createPriceMatrixDTO.AppliesTo);
            parameters.Add("@FK_AccountId", createPriceMatrixDTO.AccountId);
            //parameters.Add("@FK_AccountNumber", createPriceMatrixDTO.AccountNumber);
            parameters.Add("@BasisId", createPriceMatrixDTO.BasisId);
            parameters.Add("@Rate", createPriceMatrixDTO.RatePerHour);
            parameters.Add("@Markup", createPriceMatrixDTO.Markup);
            parameters.Add("@AccountType", createPriceMatrixDTO.AccountType);
            parameters.Add("@Name", createPriceMatrixDTO.Name);
            parameters.Add("@CreatedBy", createPriceMatrixDTO.CreatedBy);


            // Create a DataTable with a single 'Value' column
            DataTable matchValuesTable = new DataTable();
            DataTable customersValueTable = new DataTable();
            matchValuesTable.Columns.Add("Value", typeof(int));
            customersValueTable.Columns.Add("Value", typeof(int));

            // Populate the DataTable with values from the list
            foreach (var value in createPriceMatrixDTO.MatchValue)
            {
                matchValuesTable.Rows.Add(value);
            }

            foreach (var value in createPriceMatrixDTO.Customers)
            {
                customersValueTable.Rows.Add(value);
            }

            parameters.Add("@MatchValues", matchValuesTable.AsTableValuedParameter("dbo.MatchValuesType"));
            parameters.Add("@Customers", customersValueTable.AsTableValuedParameter("dbo.CustomerValuesType"));


            var result = await connection.ExecuteAsync(
                              "PriceMatrix_Add",
                              parameters,
                              commandType: CommandType.StoredProcedure
                            );
            return result;



        }

        public async Task<int> UpdateAsync(UpdatePriceMatrixDTO createPriceMatrixDTO)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", createPriceMatrixDTO.Id);
            parameters.Add("@Applies", createPriceMatrixDTO.AppliesTo);
            parameters.Add("@FK_AccountId", createPriceMatrixDTO.AccountId);
            //parameters.Add("@FK_AccountNumber", createPriceMatrixDTO.AccountNumber);
            parameters.Add("@BasisId", createPriceMatrixDTO.BasisId);
            parameters.Add("@Rate", createPriceMatrixDTO.RatePerHour);
            parameters.Add("@Markup", createPriceMatrixDTO.Markup);
            parameters.Add("@Name", createPriceMatrixDTO.Name);
            parameters.Add("@AccountType", createPriceMatrixDTO.AccountType);
            parameters.Add("@UpdatedBy", createPriceMatrixDTO.UpdatedBy);


            // Create a DataTable with a single 'Value' column
            DataTable matchValuesTable = new DataTable();
            matchValuesTable.Columns.Add("Value", typeof(int));

            // Populate the DataTable with values from the list
            foreach (var value in createPriceMatrixDTO.MatchValue)
            {
                matchValuesTable.Rows.Add(value);
            }

            parameters.Add("@MatchValues", matchValuesTable.AsTableValuedParameter("dbo.MatchValuesType"));

            DataTable customerValuesTable = new DataTable();
            customerValuesTable.Columns.Add("Value", typeof(int));
            foreach (var value in createPriceMatrixDTO.Customers)
            {
                customerValuesTable.Rows.Add(value);
            }

            parameters.Add("@Customers",
                customerValuesTable.AsTableValuedParameter("dbo.CustomerValuesType"));


            var result = await connection.ExecuteAsync(
            "PriceMatrix_Update",
            parameters,
            commandType: CommandType.StoredProcedure
          );
            return result;


        }


        public async Task<List<GetPriceMatrixDTO>> GetAllAsync(PriceMatrixFilter getPriceMatrixDTO)
        {

            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Name", getPriceMatrixDTO.Name);
            parameters.Add("@Applies", getPriceMatrixDTO.AppliesTo);
            parameters.Add("@BasisId", getPriceMatrixDTO.Basis);

            var multi = await connection.QueryMultipleAsync(
            "PriceMatrix_GetAll",
            parameters,
            commandType: CommandType.StoredProcedure
            );

            var prices = (await multi.ReadAsync<GetPriceMatrixDTO>()).ToList();

            var matchValues = await multi.ReadAsync<(int HeaderId, int RefId)>();

            foreach (var price in prices)
            {
                price.MatchValue = matchValues
                    .Where(t => t.HeaderId == price.Id)
                    .Select(t => t.RefId)
                    .ToList();

            }

            return prices;

        }

        // New paged method
        public async Task<PagedPriceMatrixResultDTO> GetAllPagedAsync(PriceMatrixFilter filter)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Name", filter.Name ?? string.Empty, DbType.String);
            parameters.Add("@Applies", filter.AppliesTo ?? string.Empty, DbType.String);
            parameters.Add("@BasisId", filter.Basis ?? (object)DBNull.Value, DbType.Int32);
            parameters.Add("@PageNumber", filter.PageNumber ?? 1, DbType.Int32);
            parameters.Add("@PageSize", filter.PageSize ?? 25, DbType.Int32);

            using var multi = await connection.QueryMultipleAsync(
                "PriceMatrix_GetAllPaged",
                parameters,
                commandType: CommandType.StoredProcedure);

            //var items = (await multi.ReadAsync<GetPriceMatrixDTO>()).ToList();

            int totalRecords = 0;
            int pageSize = filter.PageSize ?? 25;
            int currentPage = filter.PageNumber ?? 1;

            var prices = (await multi.ReadAsync<GetPriceMatrixDTO>()).ToList();

            var totalRecordsResult = await multi.ReadAsync<int>();
            totalRecords = totalRecordsResult.FirstOrDefault();

            var matchValues = await multi.ReadAsync<(int HeaderId, int RefId)>();

            foreach (var price in prices)
            {
                price.MatchValue = matchValues
                    .Where(t => t.HeaderId == price.Id)
                    .Select(t => t.RefId)
                    .ToList();

            }

            // Fallback: if stored procedure didn't return total records, estimate
            if (totalRecords == 0)
            {
                if (prices.Count == pageSize)
                {
                    // Likely more records exist
                    totalRecords = currentPage * pageSize + 1;
                }
                else
                {
                    // This is the last page
                    totalRecords = (currentPage - 1) * pageSize + prices.Count;
                }
            }

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return new PagedPriceMatrixResultDTO
            {
                Items = prices,
                TotalRecords = totalRecords,
                TotalPages = totalPages > 0 ? totalPages : 1,
                CurrentPage = currentPage,
                PageSize = pageSize
            };
        }


        public async Task<GetPriceMatrixDTO> GetAsync(GetPriceMatrixDTO getPriceMatrixDTO)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", getPriceMatrixDTO.Id);

            var multi = await connection.QueryMultipleAsync(
            "PriceMatrix_Get",
            parameters,
            commandType: CommandType.StoredProcedure
            );

            var prices = (GetPriceMatrixDTO)(await multi.ReadAsync<GetPriceMatrixDTO>()).First<GetPriceMatrixDTO>();
            var matchValues = await multi.ReadAsync<(int HeaderId, int RefId)>();
            var customerValues = await multi.ReadAsync<(int HeaderId, int RefId)>();

            prices.MatchValue = matchValues
                .Where(t => t.HeaderId == prices.Id)
                .Select(t => t.RefId)
                .ToList();

            prices.Customers = customerValues
                .Where(t => t.HeaderId == prices.Id)
                .Select(t => t.RefId)
                .ToList();
            return prices;
        }

        public async Task<int> DeleteAsync(int Id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", Id);

            var result = await connection.ExecuteAsync(
                "PriceMatrix_Delete",
                parameters,
                commandType: CommandType.StoredProcedure
              );
            return result;
        }
    }
}
