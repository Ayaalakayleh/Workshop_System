using DocumentFormat.OpenXml.Office2010.Excel;
﻿using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
﻿using System.Net;
using System.Net;
﻿using System.Net;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.ExternalWorkshopExp;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopDTOs;
using Workshop.Core.DTOs.WorkshopMovement;
using static System.Runtime.InteropServices.JavaScript.JSType;
//using Newtonsoft.Json;

namespace Workshop.Web.Services
{
    public class WorkshopApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public WorkshopApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        #region Technician
        public async Task<IEnumerable<TechnicianDTO>?> GetAllTechniciansAsync(FilterTechnicianDTO oFilter)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<TechnicianDTO>>($"api/DTechnician/GetAll?workshopId={oFilter.WorkshopId}&Name={oFilter.Name}&Email={oFilter.Email}&PageNumber={oFilter.PageNumber}");
        }
        public async Task<IEnumerable<TechnicianDTO>?> GetAllPINTechniciansAsync(FilterTechnicianDTO oFilter)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<TechnicianDTO>>($"api/DTechnician/GetAllPIN?workshopId={oFilter.WorkshopId}&Name={oFilter.Name}&Email={oFilter.Email}&PageNumber={oFilter.PageNumber}");
        }
        public async Task<IEnumerable<TechnicianDTO>?> GetTechniciansDDL(int BranchId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<TechnicianDTO>>($"api/DTechnician/GetDDL?Id={BranchId}");
        }

        public async Task<TechnicianDTO?> GetTechnicianByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<TechnicianDTO>($"api/DTechnician/GetById/{id}");
        }

        public async Task<int?> AddTechnicianAsync(CreateDTechnicianDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/DTechnician/Add", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<bool> UpdateTechnicianAsync(UpdateDTechnicianDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/DTechnician/Update", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTechnicianAsync(DeleteDTechnicianDto dto)
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/DTechnician/Delete")
            {
                Content = JsonContent.Create(dto)
            });
            return response.IsSuccessStatusCode;
        }
        public async Task<IEnumerable<TechnicianAvailabilityDTO>> GetAvailableTechniciansAsync(DateTime date, decimal duration, int branchId)
        {
            // 👇 Force invariant formatting for decimal and date
            string url = string.Format(
                CultureInfo.InvariantCulture,
                "api/DTechnician/GetAvailableTechniciansAsync?date={0:O}&duration={1}&branchId={2}",
                date,
                duration,
                branchId
            );

            return await _httpClient.GetFromJsonAsync<IEnumerable<TechnicianAvailabilityDTO>>(url);
        }



        #endregion

        #region LookupDetails
        public async Task<IEnumerable<LookupDetailsDTO>?> GetAllLookupDetailsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<LookupDetailsDTO>>("api/Lookup/Details/GetAll");
        }

        public async Task<IEnumerable<LookupDetailsDTO>?> GetAllLookupDetailsByHeaderIdAsync(int headerId, int CompanyId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<LookupDetailsDTO>>($"api/Lookup/Details/GetByHeaderId?headerId={headerId}&CompanyId={CompanyId}");
        }

        public async Task<LookupDetailsDTO?> GetLookupDetailByIdAsync(int Id, int headerId, int CompanyId)
        {
            return await _httpClient.GetFromJsonAsync<LookupDetailsDTO>($"api/Lookup/Details/GetById?Id={Id}&HeaderId={headerId}&CompanyId={CompanyId}");
        }

        public async Task<int?> AddLookupDetailAsync(CreateLookupDetailsDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Lookup/Details/Add", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<int?> UpdateLookupDetailAsync(UpdateLookupDetailsDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/Lookup/Details/Update", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("Updated") ? result["Updated"] : null;
        }

        public async Task<bool> DeleteLookupDetailAsync(DeleteLookupDetailsDTO dto)
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "api/Lookup/Details/Delete")
            {
                Content = JsonContent.Create(dto)
            });
            return response.IsSuccessStatusCode;
        }

        #endregion

        #region LookupHeader
        public async Task<IEnumerable<LookupHeaderDTO>?> GetAllLookupHeadersAsync(string language, int CompanyId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<LookupHeaderDTO>>($"api/Lookup/GetAll?language={language}&CompanyId={CompanyId}");
        }

        public async Task<LookupHeaderDTO?> GetLookupHeaderByIdAsync(int id, int CompanyId)
        {
            return await _httpClient.GetFromJsonAsync<LookupHeaderDTO>($"api/Lookup/Header/GetById?Id={id}&CompanyId={CompanyId}");
        }
        #endregion

        #region TechnicianWorkSchedule
        public async Task<IEnumerable<TechnicianWorkScheduleDTO>?> GetAllTechnicianWorkSchedulesAsync(FilterTechnicianWorkScheduleDTO oFilter)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<TechnicianWorkScheduleDTO>>($"api/TechnicianWorkSchedule/GetAll?workshopId={oFilter.WorkshopId}&Name={oFilter.Name}&Date={oFilter.Date}&language={oFilter.lang}&PageNumber={oFilter.PageNumber}");
        }

        public async Task<TechnicianWorkScheduleDTO?> GetTechnicianWorkScheduleByIdAsync(int id, string language)
        {
            return await _httpClient.GetFromJsonAsync<TechnicianWorkScheduleDTO>($"api/TechnicianWorkSchedule/GetById/{id}?language={language}");
        }

        public async Task<int?> AddTechnicianWorkScheduleAsync(CreateTechnicianWorkScheduleDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/TechnicianWorkSchedule/Add", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<int?> UpdateTechnicianWorkScheduleAsync(UpdateTechnicianWorkScheduleDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/TechnicianWorkSchedule/Update", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("Updated") ? result["Updated"] : null;
        }

        public async Task<int?> DeleteTechnicianWorkScheduleAsync(DeleteTechnicianWorkScheduleDTO dto)
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "api/TechnicianWorkSchedule/Delete")
            {
                Content = JsonContent.Create(dto)
            });
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("Id") ? result["Id"] : null;
        }

        public async Task<IEnumerable<int>?> GetTechniciansFromWorkSchedulesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<int>>($"api/TechnicianWorkSchedule/GetTechnicians");
        }
        #endregion
        #region DRTSCODE
        //public async Task<IEnumerable<RTSCodeDTO>?> GetAllDRTSCodesAsync(FilterRTSCodeDTO oFilter)
        //{
        //	return await _httpClient.GetFromJsonAsync<IEnumerable<RTSCodeDTO>>($"api/DRTSCode/GetAll?Name={oFilter.Name}&Code={oFilter.Code}&PageNumber={oFilter.PageNumber}");
        //}

        //public async Task<RTSCodeDTO?> GetDRTSCodeByIdAsync(int id)
        //{
        //	return await _httpClient.GetFromJsonAsync<RTSCodeDTO>($"api/DRTSCode/GetById/{id}");
        //}

        //public async Task<int?> AddDRTSCodeAsync(CreateRTSCodeDTO dto)
        //{
        //	var response = await _httpClient.PostAsJsonAsync($"api/DRTSCode/Add", dto);
        //	if (!response.IsSuccessStatusCode)
        //	{
        //		var errorContent = await response.Content.ReadAsStringAsync();
        //		Console.WriteLine($"Error Status: {response.StatusCode}");
        //		Console.WriteLine($"Error Content: {errorContent}");
        //		return null;
        //	}
        //	var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        //	return result != null && result.ContainsKey("id") ? result["id"] : null;
        //}

        //public async Task<bool> UpdateDRTSCodeAsync(UpdateRTSCodeDTO dto)
        //{
        //	var response = await _httpClient.PutAsJsonAsync($"api/DRTSCode/Update", dto);
        //	return response.IsSuccessStatusCode;
        //}
        #endregion

        #region RTS Code
        public async Task<IEnumerable<RTSCodeDTO>?> GetAllRTSCodesAsync(FilterRTSCodeDTO oFilter)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RTSCodeDTO>>($"api/RTSCode/GetAll?Name={oFilter.Name}&Code={oFilter.Code}&PageNumber={oFilter.PageNumber}");
        }

        public async Task<RTSCodeDTO?> GetRTSCodeByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<RTSCodeDTO>($"api/RTSCode/GetById/{id}");
        }

        public async Task<int?> AddRTSCodeAsync(CreateRTSCodeDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/RTSCode/Add", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<int?> UpdateRTSCodeAsync(UpdateRTSCodeDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/RTSCode/Update", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("Updated") ? result["Updated"] : null;
        }

        public async Task<int> DeleteRTSCodeAsync(DeleteRTSCodeDTO dto)
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"api/RTSCode/Delete")
            {
                Content = JsonContent.Create(dto)
            });
            var result = await response.Content.ReadFromJsonAsync<int>();
            return result;
        }
        public async Task<IEnumerable<RTSCodeDTO>?> GetAllRTSCodesDDLAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RTSCodeDTO>>($"api/RTSCode/DDL");
        }
        public async Task<bool> IsRTSCodeExists(string code, int companyId, int? excludeId = null)
        {
            var url = $"api/RTSCode/isCodeExists?code={Uri.EscapeDataString(code)}&companyId={companyId}";

            if (excludeId.HasValue)
            {
                url += $"&excludeId={excludeId.Value}";
            }

            return await _httpClient.GetFromJsonAsync<bool>(url);
        }


        #endregion

        #region Workshop

        public async Task<IEnumerable<WorkshopListDTO>?> WorkshopGetAllPageAsync(WorkShopFilterDTO filter)
        {
            try
            {
                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

                // Basic Filters
                if (filter.Id.HasValue)
                    queryString.Add("Id", filter.Id.Value.ToString());
                if (filter.ParentId.HasValue)
                    queryString.Add("ParentId", filter.ParentId.Value.ToString());
                if (!string.IsNullOrEmpty(filter.PrimaryName))
                    queryString.Add("PrimaryName", filter.PrimaryName);
                if (!string.IsNullOrEmpty(filter.SecondaryName))
                    queryString.Add("SecondaryName", filter.SecondaryName);

                // Location Filters
                if (filter.CityId.HasValue)
                    queryString.Add("CityId", filter.CityId.Value.ToString());
                if (!string.IsNullOrEmpty(filter.Email))
                    queryString.Add("Email", filter.Email);
                if (!string.IsNullOrEmpty(filter.Phone))
                    queryString.Add("Phone", filter.Phone);
                if (!string.IsNullOrEmpty(filter.PrimaryAddress))
                    queryString.Add("PrimaryAddress", filter.PrimaryAddress);
                if (!string.IsNullOrEmpty(filter.SecondaryAddress))
                    queryString.Add("SecondaryAddress", filter.SecondaryAddress);

                // Company Filter (non-nullable)
                queryString.Add("CompanyId", filter.CompanyId.ToString());

                // Pagination and Utility
                queryString.Add("Page", filter.Page.ToString());
                queryString.Add("RowsOfPage", filter.RowsOfPage.ToString());

                //var uriBuilder = new UriBuilder($"api/Workshops/GetAll")
                //{

                //    Query = queryString.ToString(),

                //};

                var url = $"api/Workshops/GetAllWorkshopsPage?" + queryString.ToString();
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();


                return await response.Content.ReadFromJsonAsync<IEnumerable<WorkshopListDTO>>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<IEnumerable<WorkShopDefinitionDTO>?> WorkshopGetAllAsync(int companyId, int? branchId = null, int? cityId = null, string? lang = null)
        {
            try
            {
                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
                queryString.Add("companyId", companyId.ToString());
                queryString.Add("branchId", branchId.ToString());

                if (cityId.HasValue)
                    queryString.Add("cityId", cityId.Value.ToString());

                if (!string.IsNullOrEmpty(lang))
                    queryString.Add("lang", lang);

                var url = $"api/Workshops/GetAll?" + queryString.ToString();
                return await _httpClient.GetFromJsonAsync<IEnumerable<WorkShopDefinitionDTO>>(url);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<WorkShopDefinitionDTO?> GetWorkshopByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<WorkShopDefinitionDTO>($"api/Workshops/GetById/{id}");
        }

        public async Task<int?> AddAsync(CreateWorkShopDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Workshops/Add", dto);

            if (!response.IsSuccessStatusCode) return null;
            //var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> UpdateAsync(UpdateWorkShopDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Workshops/Update", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(DeleteWorkShopDTO dto)
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/Workshops/Delete")
            {
                Content = JsonContent.Create(dto)
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<ParentWorkshopSimpleDTO>?> GetAllSimpleParentsWorkshop(int companyId, string language = "en")
        {
            Console.WriteLine($"api/Workshops/GetAllSimpleParentsWorkshop?companyId={companyId}&language={language}");
            return await _httpClient.GetFromJsonAsync<IEnumerable<ParentWorkshopSimpleDTO>>($"api/Workshops/GetAllSimpleParentsWorkshop?companyId={companyId}&language={language}");
        }
        public async Task AddRTSFranchisesAsync(int rtsId, IEnumerable<int> makeIds)
        {
            var payload = new { RtsId = rtsId, MakeIds = makeIds };
            await _httpClient.PostAsJsonAsync("api/Workshops/RTSFranchise/AddRange", payload);
        }

        public async Task AddRTSVehicleClassesAsync(int rtsId, IEnumerable<int> vehicleClassIds)
        {
            var payload = new { RtsId = rtsId, VehicleClassIds = vehicleClassIds };
            await _httpClient.PostAsJsonAsync("api/Workshops/RTSVehicleClass/AddRange", payload);
        }

        public async Task DeleteRTSFranchisesByRtsIdAsync(int rtsId)
        {
            await _httpClient.DeleteAsync($"api/Workshops/RTSFranchise/DeleteByRtsId/{rtsId}");
        }

        public async Task DeleteRTSVehicleClassesByRtsIdAsync(int rtsId)
        {
            await _httpClient.DeleteAsync($"api/Workshops/RTSVehicleClass/DeleteByRtsId/{rtsId}");
        }
        #endregion

        #region Labour Rate
        public async Task<IEnumerable<LabourRateDTO>?> GetAllLabourRatesAsync(FilterLabourRateDTO oFilter)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<LabourRateDTO>>($"api/LabourRate/GetAll?Name={oFilter.Name}&Language={oFilter.lang}&PageNumber={oFilter.PageNumber}");
        }

        public async Task<LabourRateDTO?> GetLabourRateByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<LabourRateDTO>($"api/LabourRate/GetById/{id}");
        }

        //public async Task<LabourRateDTO?> GetByGroupIdAsync(int id)
        //{
        //    return await _httpClient.GetFromJsonAsync<LabourRateDTO>($"api/LabourRate/GetByGroupId/{id}");
        //}

        public async Task<int?> AddLabourRateAsync(CreateLabourRateDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/LabourRate/Add", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<int?> UpdateLabourRateAsync(UpdateLabourRateDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/LabourRate/Update", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("Updated") ? result["Updated"] : null;
        }

        public async Task<bool> DeleteLabourRateAsync(DeleteLabourRateDTO dto)
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "api/LabourRate/Delete")
            {
                Content = JsonContent.Create(dto)
            });
            return response.IsSuccessStatusCode;
        }
        #endregion

        #region Menu
        public async Task<IEnumerable<MenuDTO>?> GetAllMenusAsync(FilterMenuDTO oFilter)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<MenuDTO>>($"api/Menu/GetAll?GroupCode={oFilter.GroupCode}&Name={oFilter.Name}&PageNumber={oFilter.PageNumber}");
        }
        public async Task<IEnumerable<MenuDTO>?> GetAllMenuDDL()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<MenuDTO>>($"api/Menu/GetAllMenuDDL");
        }


        public async Task<MenuDTO?> GetMenuByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<MenuDTO>($"api/Menu/GetById/{id}");
        }

        public async Task<IEnumerable<MenuGroupDTO>> GetMenuItemsByIdAsync(int id)
        {
            var uri = $"api/Menu/GetMenuItemsById/{id}";
            var resp = await _httpClient.GetAsync(uri);

            string content = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                var msg = $"API error. Status: {(int)resp.StatusCode} {resp.ReasonPhrase}. Content: {content}";
                throw new HttpRequestException(msg);
            }

            try
            {

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };

                var items = JsonSerializer.Deserialize<IEnumerable<MenuGroupDTO>>(content, options);
                return items ?? Enumerable.Empty<MenuGroupDTO>();
            }
            catch (Exception ex)
            {
                throw new Exception("Deserialization failed: " + ex.Message + "\nResponse content: " + content, ex);
            }
        }



        public async Task<int?> AddMenuAsync(CreateMenuDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Menu/Add", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<int?> UpdateMenuAsync(UpdateMenuDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/Menu/Update", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("Updated") ? result["Updated"] : null;
        }

        public async Task<bool> DeleteMenuAsync(DeleteMenuDTO dto)
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "api/Menu/Delete")
            {
                Content = JsonContent.Create(dto)
            });
            return response.IsSuccessStatusCode;
        }
        #endregion

        #region External Workshop Expenses

        public async Task<IEnumerable<MExternalWorkshopExpDTO>?> GetExternalWorkshopExpAsync(ExternalWorkshopExpFilterDTO filter)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ExternalWorkshopExp/GetExternalWorkshopExp", filter);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<IEnumerable<MExternalWorkshopExpDTO>>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> InsertExternalWorkshopExpAsync(CreateExternalWorkshopExpDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/ExternalWorkshopExp/ExternalWorkshopExpInsert", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<DExternalWorkshopExpDTO>?> GetExternalWorkshopExpDetailsByIdAsync(int headerId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<DExternalWorkshopExpDTO>>($"api/ExternalWorkshopExp/ExternalWorkshopExpGetDetailsById/{headerId}");
        }

        public async Task<MExternalWorkshopExpDTO?> GetExternalWorkshopExpByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<MExternalWorkshopExpDTO>($"api/ExternalWorkshopExp/ExternalWorkshopExpGetById/{id}");
        }

        public async Task<bool> UpdateExternalWorkshopExpDetailsAsync(List<DExternalWorkshopExpDTO> prData)
        {
            var response = await _httpClient.PutAsJsonAsync("api/ExternalWorkshopExp/ExternalWorkshopExpDetailsUpdate", prData);
            return response.IsSuccessStatusCode;
        }

        #endregion

        #region Excel Mapping

        public async Task<IEnumerable<MExcelMappingDTO>?> GetExcelMappingAsync(ExcelMappingFilterDTO filter)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ExternalWorkshopExp/ExcelMappingGet", filter);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<IEnumerable<MExcelMappingDTO>>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> InsertExcelMappingAsync(CreateExcelMappingDTO dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ExternalWorkshopExp/ExcelMappingInsert", dto);
                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {

                return false;
            }
        }

        public async Task<bool> UpdateExcelMappingAsync(UpdateExcelMappingDTO dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/ExternalWorkshopExp/ExcelMappingUpdate", dto);

                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {

                return false;
            }
        }

        public async Task<IEnumerable<ExcelMappingColumnDTO>?> GetExcelMappingColumnsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ExcelMappingColumnDTO>>("api/ExternalWorkshopExp/ExcelMappingGetColumns");
        }

        public async Task<IEnumerable<DExcelMappingDTO>?> GetExcelMappingDetailsByIdAsync(int? id, int? workshopId)
        {
            try
            {
                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

                if (id.HasValue)
                    queryString.Add("id", id.Value.ToString());
                if (workshopId.HasValue)
                    queryString.Add("workshopId", workshopId.Value.ToString());

                var url = $"api/ExternalWorkshopExp/ExcelMappingGetDetailsById?" + queryString.ToString();
                return await _httpClient.GetFromJsonAsync<IEnumerable<DExcelMappingDTO>>(url);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<IEnumerable<object>?> GetPayableReportAsync(int month, int year, int companyId)
        {
            try
            {
                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
                queryString.Add("month", month.ToString());
                queryString.Add("year", year.ToString());
                queryString.Add("companyId", companyId.ToString());

                var url = $"api/ExternalWorkshopExp/GetPayableReport?" + queryString.ToString();
                return await _httpClient.GetFromJsonAsync<IEnumerable<object>>(url);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<IEnumerable<ExternalWorkshopExpReportDTO>?> GetExternalWorkshopExpReportAsync(ExternalWorkshopExpReportFilterDTO filter)
        {
            try
            {
                // Using POST with body
                var response = await _httpClient.PostAsJsonAsync("api/ExternalWorkshopExp/ExternalWorkshopExpReport", filter);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<IEnumerable<ExternalWorkshopExpReportDTO>>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        #endregion

        #region Workshop Movment

        //MovementIn
        public async Task<bool> WorkshopInvoiceInsertAsync(MovementInvoice invoice)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/WorkshopMovement/WorkshopInvoiceInsert", invoice);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                    return result?.IsSuccess ?? false;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Handle exception
                throw;
            }
        }
        public async Task<int> DExternalWorkshopInvoiceInsertAsync(MovementInvoice invoice)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/WorkshopMovement/DExternalWorkshopInvoiceInsert", invoice);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<VehicleMovementStatusDTO> CheckVehicleMovementStatusAsync(int vehicleId)
        {
            var response = await _httpClient.GetAsync($"api/workshopmovement/CheckVehicleMovementStatus/{vehicleId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<VehicleMovementStatusDTO>();
        }

        public async Task<VehicleMovement> InsertVehicleMovementAsync(VehicleMovement movement)
        {
            var response = await _httpClient.PostAsJsonAsync("api/workshopmovement/InsertVehicleMovement", movement);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<VehicleMovement>();
        }

        public async Task InsertMWorkshopMovementStrikes(VehicleMovementStrike vehicleMovementStrike)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/WorkshopMovement/InsertMWorkshopMovementStrikes", vehicleMovementStrike);

            response.EnsureSuccessStatusCode();
        }

        public async Task InsertMovementDocumentAsync(VehicleMovementDocument movmentDoc)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WorkshopMovement/InsertMovementDocument", movmentDoc);
            response.EnsureSuccessStatusCode();
        }


        public async Task<IEnumerable<VehicleChecklistLookup>> GetVehicleChecklistLookup()
        {
            var response = await _httpClient.GetAsync($"api/workshopmovement/GetVehicleChecklistLookup");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<VehicleChecklistLookup>>();
        }
        public async Task<IEnumerable<TyreChecklistLookup>> GetTyreChecklistLookup()
        {
            var response = await _httpClient.GetAsync($"api/workshopmovement/GetTyreChecklistLookup");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<TyreChecklistLookup>>();
        }

        //Movements

        public async Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovementAsync(WorkshopMovementFilter filter)
        {
            try
            {

                var response = await _httpClient.PostAsJsonAsync("api/WorkshopMovement/GetAllDWorkshopVehicleMovement", filter);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<VehicleMovement>>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovementDDL(WorkshopMovementFilter filter)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/WorkshopMovement/GetAllDWorkshopVehicleMovementDDL", filter);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<VehicleMovement>>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<VehicleMovement> GetVehicleMovementByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/WorkshopMovement/GetVehicleMovementById/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<VehicleMovement>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<VehicleMovementDocument>> GetMovementDocumentsAsync(int movementId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/WorkshopMovement/GetMovementDocuments/{movementId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<VehicleMovementDocument>>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<IEnumerable<VehicleChecklist>> GetVehicleChecklistByMovementId(int? movementId)
        {
            var response = await _httpClient.PostAsJsonAsync("api/workshopmovement/GetVehicleChecklistByMovementId", movementId);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync <IEnumerable<VehicleChecklist>>();
        }

        public async Task<IEnumerable<TyreChecklist>> GetTyresChecklistByMovementId(int? movementId)
        {
            var response = await _httpClient.PostAsJsonAsync("api/workshopmovement/GetTyresChecklistByMovementId", movementId);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<TyreChecklist>>();
        }
        public async Task<int> InsertVehicleChecklist(VehicleChecklist vehicleChecklist)
        {
            var response = await _httpClient.PostAsJsonAsync("api/workshopmovement/InsertVehicleChecklist", vehicleChecklist);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }
        public async Task<int> InsertTyreChecklist(TyreChecklist tyreChecklist)
        {
            var response = await _httpClient.PostAsJsonAsync("api/workshopmovement/InsertTyreChecklist", tyreChecklist);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }
        public async Task<int> UpdateVehicleChecklist(VehicleChecklist vehicleChecklist)
        {
            var response = await _httpClient.PostAsJsonAsync("api/workshopmovement/UpdateVehicleChecklist", vehicleChecklist);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }
        public async Task<int> UpdateTyreChecklist(VehicleChecklist vehicleChecklist)
        {
            var response = await _httpClient.PostAsJsonAsync("api/workshopmovement/UpdateTyreChecklist", vehicleChecklist);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<List<MovementInvoice>> GetWorkshopInvoiceByMovementId(int movementId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/WorkshopMovement/GetWorkshopInvoiceByMovementId/{movementId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<MovementInvoice>>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<VehicleMovement> GetLastVehicleMovementByVehicleIdAsync(int vehicleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/WorkshopMovement/GetLastVehicleMovementByVehicleId/{vehicleId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<VehicleMovement>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<VehicleMovement> GetLastMovementOutByWorkOrderId(int workorder)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/WorkshopMovement/VehicleMovement_GetLastMovementOutByWorkOrderId?WorkOrderId={workorder}");
                response.EnsureSuccessStatusCode();
                
                return response != null ? await response.Content.ReadFromJsonAsync<VehicleMovement>() : new VehicleMovement();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<string> GetVehicleMovementStrikeAsync(int movementId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/WorkshopMovement/GetVehicleMovementStrike/{movementId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //MovementOut
        public async Task UpdateVehicleMovementStatusAync(int workshopId, Guid masterId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/WorkshopMovement/UpdateVehicleMovementStatus?workShopId={workshopId}&masterId={masterId}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<List<MovementInvoice>> GetWorkshopInvoiceByWorkOrderId(int workOrderId)
        {
            var response = await _httpClient.GetAsync($"api/workshopmovement/GetWorkshopInvoiceByWorkOrderId/{workOrderId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<MovementInvoice>>();
        }
        #endregion

        #region WorkOrder
        // generate GetMWorkOrders method
        public async Task<MWorkOrderDTO?> GetMWorkOrderByID(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/MWorkOrder/GetMWorkOrderByID/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<MWorkOrderDTO>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<MWorkOrderDTO>?> GetMWorkOrdersAsync(WorkOrderFilterDTO filter)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/MWorkOrder/GetMWorkOrders", filter);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<MWorkOrderDTO>>();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<MWorkOrderDTO> InsertMWorkOrderAsync(MWorkOrderDTO workOrder)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/MWorkOrder/InsertMWorkOrder", workOrder);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<MWorkOrderDTO>();
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Handle exception
                throw;
            }
        }

        public async Task<MWorkOrderDTO> UpdateMWorkOrderAsync(MWorkOrderDTO workOrder)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/MWorkOrder/UpdateMWorkOrder", workOrder);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<MWorkOrderDTO>();
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Handle exception
                throw;
            }
        }

        public async Task<bool> DeleteMWorkOrderAsync(int id)
        {
            try
            {
                // Using HTTP DELETE (recommended)
                var response = await _httpClient.DeleteAsync($"api/MWorkOrder/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                    return result?.IsSuccess ?? false;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Handle exception
                throw;
            }
        }
        
        public async Task UpdateWorkOrderKMAsync(int workOrderId, decimal receivedKM)
        {
            var response = await _httpClient.PutAsync($"api/MWorkOrder/UpdateWorkOrderKM?workOrderId={workOrderId}&receivedKM={receivedKM}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateWorkOrderStatusAsync(int workOrderId, int statusId, decimal totalCost = 0)
        {
            var response = await _httpClient.PutAsync($"api/MWorkOrder/UpdateWorkOrderStatus?workOrderId={workOrderId}&statusId={statusId}&totalCost={totalCost}", null);
            response.EnsureSuccessStatusCode();         
        }

        public async Task<bool> UpdateMAccidentStatusAsync(InsuranceClaimHistory insuranceClaimHistory)
        {
            var response = await _httpClient.PutAsJsonAsync("api/MWorkOrder/UpdateMAccidentStatus", insuranceClaimHistory);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result?.IsSuccess ?? false;
        }

        public async Task<MWorkOrderDTO> GetMWorkOrderByMasterId(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/MWorkOrder/GetMWorkOrderByMasterId/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MWorkOrderDTO>();
        }

        public async Task UpdateWorkOrderInvoicingStatusAsync(int workOrderId)
        {
            var response = await _httpClient.PutAsync($"api/MWorkOrder/UpdateWorkOrderInvoicingStatus?workOrderId={workOrderId}", null);
            response.EnsureSuccessStatusCode();
        }
        

        public async Task FixWorkOrderAsync(int workOrderId, bool isFix)
        {
            var response = await _httpClient.PutAsync($"api/MWorkOrder/FixWorkOrder?workOrderId={workOrderId}&isFix={isFix}", null);
            response.EnsureSuccessStatusCode();
        }


        #endregion

        #region Shift

        // Get all shifts with filtering and pagination
        public async Task<List<ShiftListItemDTO>> ShiftGetAllAsync(ShiftFilterDTO filter)
        {
            try
            {
                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
                queryString.Add("CompanyId", filter.CompanyId.ToString());

                if (!string.IsNullOrEmpty(filter.Name))
                    queryString.Add("Name", filter.Name);
                if (!string.IsNullOrEmpty(filter.Code))
                    queryString.Add("Code", filter.Code);
                if (filter.Page.HasValue)
                    queryString.Add("Page", filter.Page.Value.ToString());
                if (filter.PageSize.HasValue)
                    queryString.Add("PageSize", filter.PageSize.Value.ToString());

                var url = $"api/Shifts/ShiftGetAll?" + queryString.ToString();
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<ShiftListItemDTO>>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid request parameters");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving shifts: {ex.Message}");
            }
        }

        // Get shift by ID
        public async Task<ShiftDTO> ShiftGetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Shifts/ShiftGetById/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new KeyNotFoundException("Shift not found");

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ShiftDTO>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid shift ID");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Shift not found");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving shift {id}: {ex.Message}");
            }
        }

        // Create new shift
        public async Task<int> ShiftCreateAsync(CreateShiftDTO shift)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Shifts/ShiftCreate", shift);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    throw new ArgumentException(errorContent?.Message ?? "Invalid request");
                }

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
                return result != null && result.ContainsKey("id") ? result["id"] : 0;

            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid shift data");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating shift: {ex.Message}");
            }
        }

        // Update existing shift
        public async Task<ApiResponse<bool>> ShiftUpdateAsync(UpdateShiftDTO shift)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/Shifts/ShiftUpdate", shift);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    throw new ArgumentException(errorContent?.Message ?? "Invalid request");
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new KeyNotFoundException("Shift not found");

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid shift data");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Shift not found");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating shift {shift.Id}: {ex.Message}");
            }
        }

        // Delete shift
        public async Task<ApiResponse<bool>> ShiftDeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Shifts/ShiftDelete/{id}");

                if (response.StatusCode == HttpStatusCode.BadRequest)
                    throw new ArgumentException("Invalid shift ID");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new KeyNotFoundException("Shift not found");

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid shift ID");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Shift not found");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting shift {id}: {ex.Message}");
            }
        }

        public async Task<IEnumerable<ShiftDTO>?> GetAllShiftsAsync()
        {
            var response = await _httpClient.GetAsync($"api/Shifts/GetAllShiftsDDL");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ShiftDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        }

        #endregion

        #region AllowedTime

        // Get all allowed times with filtering and pagination
        public async Task<List<AllowedTimeListItemDTO>> AllowedTimeGetAllAsync(AllowedTimeFilterDTO filter)
        {
            try
            {
                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

                if (filter.Make.HasValue)
                    queryString.Add("Make", filter.Make.Value.ToString());
                if (filter.Model.HasValue)
                    queryString.Add("Model", filter.Model.Value.ToString());
                if (filter.Year.HasValue)
                    queryString.Add("Year", filter.Year.Value.ToString());
                if (!string.IsNullOrEmpty(filter.Engine))
                    queryString.Add("Engine", filter.Engine);
                if (filter.RTSCode.HasValue)
                    queryString.Add("RTSCode", filter.RTSCode.Value.ToString());
                if (filter.Page.HasValue)
                    queryString.Add("Page", filter.Page.Value.ToString());
                if (filter.PageSize.HasValue)
                    queryString.Add("PageSize", filter.PageSize.Value.ToString());

                var url = $"api/AllowedTimes/AllowedTimeGetAll?" + queryString.ToString();
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<AllowedTimeListItemDTO>>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid request parameters");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving allowed times: {ex.Message}");
            }
        }

        // Get allowed time by ID
        public async Task<AllowedTimeDTO> AllowedTimeGetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/AllowedTimes/AllowedTimeGetById/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new KeyNotFoundException("Allowed time not found");

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<AllowedTimeDTO>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid allowed time ID");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Allowed time not found");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving allowed time {id}: {ex.Message}");
            }
        }

        // Create new allowed time
        public async Task<int> AllowedTimeCreateAsync(CreateAllowedTimeDTO allowedTime)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/AllowedTimes/AllowedTimeCreate", allowedTime);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    throw new ArgumentException(errorContent?.Message ?? "Invalid request");
                }

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
                return result != null && result.ContainsKey("id") ? result["id"] : 0;

            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid allowed time data");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating allowed time: {ex.Message}");
            }
        }

        // Update existing allowed time
        public async Task<ApiResponse<bool>> AllowedTimeUpdateAsync(UpdateAllowedTimeDTO allowedTime)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/AllowedTimes/AllowedTimeUpdate", allowedTime);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    throw new ArgumentException(errorContent?.Message ?? "Invalid request");
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new KeyNotFoundException("Allowed time not found");

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid allowed time data");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Allowed time not found");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating allowed time {allowedTime.Id}: {ex.Message}");
            }
        }

        // Delete allowed time
        public async Task<ApiResponse<bool>> AllowedTimeDeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/AllowedTimes/AllowedTimeDelete/{id}");

                if (response.StatusCode == HttpStatusCode.BadRequest)
                    throw new ArgumentException("Invalid allowed time ID");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new KeyNotFoundException("Allowed time not found");

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid allowed time ID");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Allowed time not found");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting allowed time {id}: {ex.Message}");
            }
        }

        #endregion

        #region ExternalWorkshopInvoice

        public async Task<List<WorkShopDefinitionDTO>?> GetExternalWorkshopInvoiceWorkshopDetailsAsync(WorkShopFilterDTO filter)
        {
            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            if (filter.Id.HasValue)
                queryString.Add("Id", filter.Id.Value.ToString());
            if (filter.FromDate.HasValue)
                queryString.Add("FromDate", filter.FromDate.Value.ToString());
            if (filter.ToDate.HasValue)
                queryString.Add("ToDate", filter.ToDate.Value.ToString());

            var url = $"api/ExternalWorkshopInvoice/GetWorkshopDetails?" + queryString.ToString();
            return await _httpClient.GetFromJsonAsync<List<WorkShopDefinitionDTO>>(url);
        }

        public async Task<List<ExternalWorkshopInvoiceDetailsDTO>?> GetExternalWorkshopInvoiceDetailsAsync(ExternalWorkshopInvoiceDetailsFilterDTO filter)
        {
            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            if (filter.WorkshopId.HasValue)
                queryString.Add("WorkshopId", filter.WorkshopId.Value.ToString());
            if (filter.ExternalWorkshopId.HasValue)
                queryString.Add("ExternalWorkshopId", filter.ExternalWorkshopId.Value.ToString());
            if (filter.FromDate.HasValue)
                queryString.Add("FromDate", filter.FromDate.Value.ToString());
            if (filter.ToDate.HasValue)
                queryString.Add("ToDate", filter.ToDate.Value.ToString());


            var url = $"api/ExternalWorkshopInvoice/GetInvoiceDetails?" + queryString.ToString();
            return await _httpClient.GetFromJsonAsync<List<ExternalWorkshopInvoiceDetailsDTO>>(url);
        }

        public async Task<List<ExternalWorkshopInvoiceDetailsDTO>?> GetInvoiceDetailsByWIPIdAsync(int? WIPId)
        {
            var url = $"api/ExternalWorkshopInvoice/GetInvoiceDetailsByWIPId?WIPId={WIPId}";

            var response = await _httpClient.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API failed: {(int)response.StatusCode} {response.ReasonPhrase} | Body: {body}");

            return await response.Content.ReadFromJsonAsync<List<ExternalWorkshopInvoiceDetailsDTO>>();
        }


        #endregion

        #region MaintenanceCard

        public async Task InsertDMaintenanceCardAsync(MaintenanceCardDTO maintenanceCard)
        {
            var response = await _httpClient.PostAsJsonAsync("api/MaintenanceCard/InsertDMaintenanceCard", maintenanceCard);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteDMaintenanceCardAsync(int movementId)
        {
            var response = await _httpClient.DeleteAsync($"api/MaintenanceCard/DeleteDMaintenanceCard?movementId={movementId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateDMaintenanceCardAsync(MaintenanceCardDTO maintenanceCard)
        {
            var response = await _httpClient.PostAsJsonAsync("api/MaintenanceCard/UpdateDMaintenanceCard", maintenanceCard);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<MaintenanceCardDTO>> GetDMaintenanceCardsByMovementIdAsync(int movementId)
        {
            var response = await _httpClient.GetAsync($"api/MaintenanceCard/GetDMaintenanceCardsByMovementId?MovementId={movementId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<MaintenanceCardDTO>>();
        }

        public async Task<List<MaintenanceCardDTO>> GetDMaintenanceCardsByMasterIdAsync(Guid masterId)
        {
            var response = await _httpClient.GetAsync($"api/MaintenanceCard/GetDMaintenanceCardsByMasterId?MasterId={masterId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<MaintenanceCardDTO>>();
        }
        #endregion

        #region  team
        public async Task<List<GetTeamDTO>> GetAllTeamsAsync(FilterTeamDTO oFilterTeamDTO)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Team/GetAllTeams", oFilterTeamDTO);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<GetTeamDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<PagedTeamResultDTO> GetAllTeamsPagedAsync(FilterTeamDTO oFilterTeamDTO)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Team/GetAllTeamsPaged", oFilterTeamDTO);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PagedTeamResultDTO>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<List<GetTeamDTO>> GetAllTeamsDDLAsync()
        {

            var response = await _httpClient.GetAsync("api/Team/GetAllTeamsDDL");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<GetTeamDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<int> AddTeamAsync(AddTeamDTO team)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Team/AddTeam", team);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<int> UpdateTeamAsync(UpdateTeamDTO team)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Team/UpdateTeam", team);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<int> DeleteTeamAsync(int teamID)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Team/DeleteTeam", teamID);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<IEnumerable<GetTeamDTO>> GetTeamByIDAsync(int teamID)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Team/GetTeam", teamID);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<GetTeamDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<GetTeamDTO>();
        }

        public async Task<List<GetTeamDTO>> SearchTeams(GetTeamDTO getTeamDTO)
        {

            var response = await _httpClient.PostAsJsonAsync($"api/Team/SearchTeam", getTeamDTO);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<GetTeamDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }



        #endregion

        #region  service reminder
        public async Task<int> AddServiceReminderAsync(CreateServiceReminderDTO createServiceReminderDTO)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/ServiceReminder/AddReminder", createServiceReminderDTO);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<int> UpdateServiceReminderAsync(UpdateServiceReminderDTO updateServiceReminderDTO)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/ServiceReminder/UpdateReminder", updateServiceReminderDTO);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<GetServiceReminderDTO> GetServiceReminderByIdAsync(int Id)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/ServiceReminder/GetReminder", Id);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GetServiceReminderDTO>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<List<GetServiceReminderDTO>> GetAllServiceRemindersAsync(GetServiceReminderDTO getServiceReminderDTO)
        {
            var response = await _httpClient.PostAsJsonAsync("api/ServiceReminder/GetAllReminder", getServiceReminderDTO);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return new List<GetServiceReminderDTO>();

            var result = JsonSerializer.Deserialize<List<GetServiceReminderDTO>>(
                content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result ?? new List<GetServiceReminderDTO>();
        }

        public async Task<bool> DeleteServiceRemindersAsync(int Id)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/ServiceReminder/DeleteReminder", Id);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<List<ServiceReminderDue>> GetDueServiceReminders()
        {
            var response = await _httpClient.GetAsync($"api/ServiceReminder/GetDueServiceReminders");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return new List<ServiceReminderDue>();

            var result = JsonSerializer.Deserialize<List<ServiceReminderDue>>(
                content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result ?? new List<ServiceReminderDue>();
        }

        public async Task<List<ReminderStatus>> GetServiceRemindersStatus()
        {
            var response = await _httpClient.GetAsync($"api/ServiceReminder/GetServiceRemindersStatus");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return new List<ReminderStatus>();

            var result = JsonSerializer.Deserialize<List<ReminderStatus>>(
                content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result ?? new List<ReminderStatus>();
        }

        public async Task<int> UpdateServiceScheduleByDamageId(ServiceScheduleModel serviceSchedule)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/ServiceReminder/UpdateServiceScheduleByDamageId", serviceSchedule);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<int>(
                content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }

        #endregion
        #region PriceMatrix
        public async Task<List<GetPriceMatrixDTO>> GetAllPrices(PriceMatrixFilter filter)
        {

            var response = await _httpClient.PostAsJsonAsync($"api/PriceMatrix/GetAll", filter);


            //await response.Content.ReadFromJsonAsync<List<GetPriceMatrixDTO>> ();
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<GetPriceMatrixDTO>>();

        }

        public async Task<int?> AddPricesAsync(CreatePriceMatrixDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/PriceMatrix/Add", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<int>();
            return result;
        }

        public async Task<bool> UpdatePricesAsync(UpdatePriceMatrixDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/PriceMatrix/Update", dto);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePricesAsync(PriceMatrixDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/PriceMatrix/Delete", dto.Id);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
        public async Task<GetPriceMatrixDTO> GetPrice(GetPriceMatrixDTO filter)
        {

            var response = await _httpClient.PostAsJsonAsync($"api/PriceMatrix/Get", filter);
            response.EnsureSuccessStatusCode();
            var val = await response.Content.ReadFromJsonAsync<GetPriceMatrixDTO>();
            return val;
        }

        public async Task<PagedPriceMatrixResultDTO> GetAllPricesPaged(PriceMatrixFilter filter)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/PriceMatrix/GetPaged", filter);

                // If not success, try to read content for diagnositics but don't throw
                if (!response.IsSuccessStatusCode)
                {
                    var text = await response.Content.ReadAsStringAsync();
                    Console.Error.WriteLine($"GetAllPricesPaged: API returned {(int)response.StatusCode} - {response.ReasonPhrase}. Content: {text}");
                    return new PagedPriceMatrixResultDTO
                    {
                        Items = new List<GetPriceMatrixDTO>(),
                        TotalRecords = 0,
                        TotalPages = 1,
                        CurrentPage = filter.PageNumber ?? 1,
                        PageSize = filter.PageSize ?? 25
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<PagedPriceMatrixResultDTO>();
                if (result == null)
                {
                    return new PagedPriceMatrixResultDTO
                    {
                        Items = new List<GetPriceMatrixDTO>(),
                        TotalRecords = 0,
                        TotalPages = 1,
                        CurrentPage = filter.PageNumber ?? 1,
                        PageSize = filter.PageSize ?? 25
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"GetAllPricesPaged exception: {ex}");
                return new PagedPriceMatrixResultDTO
                {
                    Items = new List<GetPriceMatrixDTO>(),
                    TotalRecords = 0,
                    TotalPages = 1,
                    CurrentPage = filter.PageNumber ?? 1,
                    PageSize = filter.PageSize ?? 25
                };
            }
        }
        #endregion

        #region JobCard
        public async Task<JobCardDTO?> GetJobCardByMasterId(Guid id)
        {
            try{
                var response = await _httpClient.GetAsync($"api/JobCard/GetJobCardByMasterId?id={id}");
            response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<JobCardDTO?>();
            } catch (Exception e)
            {
                return null;
            }
        }
        #endregion
        #region ServiceReminders

        public async Task UpdateServiceScheduleByWorkOrderIdAsync(ServiceScheduleDTO serviceSchedule)
        {
            try
            {

                var response = await _httpClient.PutAsJsonAsync("api/ServiceReminders/update-service-schedule", serviceSchedule);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error updating service schedule: {ex.Message}");
            }
        }
        #endregion

        #region WIP
        public async Task<IEnumerable<WIPDTO>?> GetAllWIPsAsync(FilterWIPDTO oFilter)
        {
            //return await _httpClient.GetFromJsonAsync<IEnumerable<WIPDTO>>($"api/WIP/GetAll?PageNumber={oFilter.PageNumber}");
            var response = await _httpClient.PostAsJsonAsync("api/WIP/GetAll", oFilter);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<WIPDTO>>();
            return result != null ? result : null;
        }

        public async Task<IEnumerable<CreateWIPServiceDTO>?> GetAllInternalLabourLineAsync(int WIPId)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/GetAllInternalLabourLineAsync", WIPId);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<CreateWIPServiceDTO>>();
            return result != null ? result : null;
        }


        public async Task<IEnumerable<CreateItemDTO>?> GetAllInternalPartsLineAsync(int WIPId)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/GetAllInternalPartsLineAsync", WIPId);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<CreateItemDTO>>();
            return result != null ? result : null;
        }


        public async Task<WIPDTO?> GetWIPByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<WIPDTO>($"api/WIP/GetById/{id}");
        }

        public async Task<AccountDTO?> WIP_GetAccountById(int id)
        {
            return await _httpClient.GetFromJsonAsync<AccountDTO>($"api/WIP/GetAccountInfo/{id}");
        }

        public async Task<int?> AddWIPAsync(CreateWIPDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/Add", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<int?> InsertWIPAccount(AccountDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/InsertWIPAccount", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<int?> WIPSCheduleInsert(WIPSChedule dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/WIPSCheduleInsert", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<int?> GeneralRequest(GeneralRequest dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/GeneralRequest", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<int?> UpdateWIPAsync(UpdateWIPDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/WIP/Update", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Raw JSON Response: " + raw);
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("updated") ? result["updated"] : null;
        }
        public async Task<int?> UpdateWIPStatus(UpdateWIPStatusDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/WIP/UpdateWIPStatus", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Raw JSON Response: " + raw);
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("updated") ? result["updated"] : null;
        }

        public async Task<bool> DeleteWIPAsync(DeleteWIPDTO dto)
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "api/WIP/Delete")
            {
                Content = JsonContent.Create(dto)
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<RTSCodeDTO?>> GetAllServicesWithTimeAsync(RTSWithTimeDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/GetAllServices", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<RTSCodeDTO>>(); 
            return result != null ? result : null;
        }

        public async Task<IEnumerable<MenuDTO?>> GetAllMenus()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<MenuDTO>>("api/WIP/GetAllMenus");
           
        }

        public async Task<IEnumerable<CreateItemDTO?>> WIP_GetItemsById(int id, string lang)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CreateItemDTO>>($"api/WIP/GetWIPItems?Id={id}&lang={lang}");

        }

        public async Task<IEnumerable<CreateWIPServiceDTO?>> WIP_GetServicesById(int id, string lang="en")
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CreateWIPServiceDTO>>($"api/WIP/GetWIPServices?Id={id}&lang={lang}");
        }

        public async Task<WIPSChedule?> WIP_SChedule_Get(int RTSId, int WIPId, int KeyId)
        {
            return await _httpClient.GetFromJsonAsync<WIPSChedule>($"api/WIP/WIPSCheduleGet?RTSId={RTSId}&WIPId={WIPId}&KeyId={KeyId}");
        }

        public async Task<IEnumerable<WIPSChedule>> WIP_SChedule_GetAll()
        {

            return await _httpClient.GetFromJsonAsync<IEnumerable<WIPSChedule>>($"api/WIP/WIPSCheduleGetAll");
        }
        public async Task<int?> UpdateServiceStatus(UpdateService dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/WIP/UpdateServiceStatus", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("updated") ? result["updated"] : null;
        }

        public async Task<int?> UpdatePartStatus(UpdatePartStatus dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/WIP/UpdatePartStatus", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("updated") ? result["updated"] : null;
        }
        
        public async Task<IEnumerable<ReturnItems>?> GetReturnParts(int WIPId=0)  
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ReturnItems>>("api/WIP/GetReturnParts?WIPId={WIPId}");

        }

        public async Task<int?> InsertWIPVehicleDetails(VehicleTabDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/InsertWIPVehicleDetails", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<VehicleTabDTO?> WIP_GetVehicleDetailsById(int id)
        {
            return await _httpClient.GetFromJsonAsync<VehicleTabDTO>($"api/WIP/GetVehicleDetailsById/{id}");
        }

        public async Task<int?> UpdateWIPOptions(WIPOptionsDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/WIP/UpdateWIPOptions", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("Updated") ? result["Updated"] : null;
        }
        public async Task<WIPOptionsDTO?> WIP_GetOptionsById(int id)
        {
            return await _httpClient.GetFromJsonAsync<WIPOptionsDTO>($"api/WIP/GetOptionsById/{id}");
        }

        public async Task<IEnumerable<M_WIPServiceHistoryDTO>> GetWIPServiceHistory(int VehicleId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<M_WIPServiceHistoryDTO>>($"api/WIP/GetWIPServiceHistory?VehicleId={VehicleId}");
        }

        public async Task<decimal?> GetLabourRate(LabourRateFilterDTO dto)
        {
            //return await _httpClient.GetFromJsonAsync<LabourRateDTO>("api/WIP/GetLabourRate", dto);
            var response = await _httpClient.PostAsJsonAsync("api/WIP/GetLabourRate", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Raw JSON: " + raw);
            var result = await response.Content.ReadFromJsonAsync<decimal?>();
            return result;
            //var result = await response.Content.ReadFromJsonAsync<Dictionary<string, decimal>>();
            //return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<IEnumerable<WIPServiceHistoryDetails_Parts>> WIPServiceHistoryDetails_GetPartsByWIPId(int Id)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WIPServiceHistoryDetails_Parts>>($"api/WIP/WIPServiceHistoryDetailsGetPartsByWIPId?Id={Id}");
        }

        public async Task<IEnumerable<WIPServiceHistoryDetails_Labour>> WIPServiceHistoryDetails_GetLaboursByWIPId(int Id)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WIPServiceHistoryDetails_Labour>>($"api/WIP/WIPServiceHistoryDetailsGetLaboursByWIPId?Id={Id}");
        }

        public async Task<IEnumerable<WIPServiceHistoryDetails_Parts>> WIPServiceHistoryDetails_GetParts()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WIPServiceHistoryDetails_Parts>>($"api/WIP/WIPServiceHistoryDetailsGetParts");
        }

        public async Task<IEnumerable<WIPServiceHistoryDetails_Labour>> WIPServiceHistoryDetails_GetLabours()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WIPServiceHistoryDetails_Labour>>($"api/WIP/WIPServiceHistoryDetailsGetLabours");
        }

        public async Task<IEnumerable< WIPDTO?>> GetWIPDDL()
        {
           /* var response = await _httpClient.GetAsync("api/WIP/WIPGetDDL");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("Updated") ? result["Updated"] : null;
           */
            return await _httpClient.GetFromJsonAsync<IEnumerable<WIPDTO>>($"api/WIP/WIPGetDDL");
        }

        public async Task<int?> WIP_Close(int Id, int ClosedBy)
        {
            CloseWIPDTO dto = new CloseWIPDTO()
            {
                WIPId= Id,
                ClosedBy = ClosedBy
            };

            var response = await _httpClient.PutAsJsonAsync("api/WIP/WIPClose", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Raw JSON Response: " + raw);
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("updated") ? result["updated"] : null;
        }

        public async Task<int?> WIP_Validation(int WIPId)
        {
            var response = await _httpClient.PostAsync($"api/WIP/WIPValidation?WIPId={WIPId}", null);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var raw = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Raw JSON Response: " + raw);
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }
        // create method to call this api
        //[HttpPost("GetWIPServicesByMovementId")]
        //public async Task<IActionResult> GetWIPServicesByMovementIdAsync(int movementId)
        //{
        //    var result = await _service.GetWIPServicesByMovementIdAsync(movementId);
        //    return Ok(result);
        //}

        public async Task<IEnumerable<CreateWIPServiceDTO?>> GetWIPServicesByMovementIdAsync(int movementId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CreateWIPServiceDTO>>($"api/WIP/GetWIPServicesByMovementId?movementId={movementId}");
        }

        public async Task TransferMaintenanceMovement(int movementId, int workshopId, Guid masterId, string reason)
        {
            var response = await _httpClient.PostAsync($"api/WIP/TransferMaintenanceMovement?movementId={movementId}&workshopId={workshopId}&masterId={masterId}&reason={reason}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<int> UpdateWIPServicesIsExternalAsync(string ids)
        {
            var response = await _httpClient.PostAsync($"api/WIP/UpdateWIPServicesIsExternal?Ids={ids}", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<int> UpdateWIPServicesIsFixedAsync(string ids)
        {
            var response = await _httpClient.PostAsync($"api/WIP/UpdateWIPServicesIsFixed?Ids={ids}", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<List<VehicleMovement>> GetAllVehicleTransferMovementAsync(int? vehicleId, int? page, int workshopId)
        {
            var response = await _httpClient.GetAsync($"api/WIP/GetAllVehicleTransferMovement?vehicleId={vehicleId}&page={page}&workshopId={workshopId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<VehicleMovement>>();
        }

        public async Task<CheckWIPCountDTO> GetWIPByVehicleId(int? vehicleId)
        {
            return await _httpClient.GetFromJsonAsync<CheckWIPCountDTO>($"api/WIP/GetWIPByVehicleId?vehicleId={vehicleId}");

        }


        public async Task<int> DeleteService(DeleteServiceDTO dto)
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"api/WIP/DeleteService")
            {
                Content = JsonContent.Create(dto)
            });
            return response.IsSuccessStatusCode ? 1 : 0;
        }

        public async Task<int> WIP_DeleteItems(int WIPId, int Id)
        {
            
            var response = await _httpClient.DeleteAsync($"api/WIP/WIP_DeleteItems?WIPId={WIPId}&Id={Id}");

            return response.IsSuccessStatusCode ? 1 : 0;
        }

        public async Task<int?> UpdateIssueIdToWIP(UpdateIssueIdDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/WIP/UpdateIssueId", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }
        public async Task<int?> UpdatePartStatusForSingleItem(UpdateSinglePartStatusDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/UpdatePartStatusForSingleItem", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<int>();
            return result != null ? result : null;
        }

        public async Task<IEnumerable<WIPDTO>> GetWIPByMovementId(string movementIds)
        {

            return await _httpClient.GetFromJsonAsync<IEnumerable<WIPDTO>>($"api/WIP/GetWIPByMovementIds?movementIds={movementIds}");
        }

        public async Task<int?> InsertWIPInvoice(CreateWIPInvoiceDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/InsertWIPInvoice", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("id") ? result["id"] : null;
        }

        public async Task<IEnumerable<WIPInvoiceDTO?>> WIPInvoiceGetById(int? id, int? TransactionMasterId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WIPInvoiceDTO>>($"api/WIP/WIPInvoiceGetById?Id={id}&TransactionMasterId={TransactionMasterId}");
        }
        public async Task<IEnumerable<WipInvoiceDetailDTO>> WipInvoiceByHeaderId(int headerId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<WipInvoiceDetailDTO>>($"api/WIP/WipInvoiceByHeaderId?headerId={headerId}");

        }
        public async Task<int> UpdateWIPServicesExternalAndFixStatus(List<Models.WipServiceFixDto> services)
        {
            var response = await _httpClient.PostAsJsonAsync("api/WIP/UpdateWIPServicesExternalAndFixStatus", services);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return 0;
            }

            var result = await response.Content.ReadFromJsonAsync<int>();
            return result;
        }
        #endregion


        #region Recall
        public async Task<IEnumerable<RecallDTO>?> GetAllRecallAsync(FilterRecallDTO oFilter)
		{
			var response =  await _httpClient.PostAsJsonAsync<FilterRecallDTO>($"api/Recall/GetAll", oFilter);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<RecallDTO>?>();
            return result;
        }
        public async Task<IEnumerable<RecallDTO>?> GetAllRecallsDDLAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RecallDTO>?>($"api/Recall/GetAllDDL");
        }

        public async Task<RecallDTO?> GetRecallByIdAsync(int id)
		{
			return await _httpClient.GetFromJsonAsync<RecallDTO>($"api/Recall/GetById/{id}");
		}

		public async Task<int?> AddRecallAsync(CreateRecallDTO dto)
		{
			var response = await _httpClient.PostAsJsonAsync("api/Recall/Add", dto);
			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"Error Status: {response.StatusCode}");
				Console.WriteLine($"Error Content: {errorContent}");
				return null;
			}
			var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
			return result != null && result.ContainsKey("id") ? result["id"] : null;
		}

		public async Task<int?> UpdateRecallAsync(UpdateRecallDTO dto)
		{
			var response = await _httpClient.PutAsJsonAsync("api/Recall/Update", dto);
			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"Error Status: {response.StatusCode}");
				Console.WriteLine($"Error Content: {errorContent}");
				return null;
			}
			var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
			return result != null && result.ContainsKey("Updated") ? result["Updated"] : null;
		}

		public async Task<int> DeleteRecallAsync(DeleteRecallDTO dto)
		{

            var response = await _httpClient.PostAsJsonAsync("api/Recall/Delete", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return 0;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return result != null && result.ContainsKey("deleted") ? result["deleted"] : 0;
        }
        public async Task<ActiveRecallsByChassisResponseDto?> GetActiveRecallsByChassis(string chassisNo)
        {
            return await _httpClient.GetFromJsonAsync<ActiveRecallsByChassisResponseDto>($"api/Recall/GetActiveRecallsByChassis/{chassisNo}");
        }
        public async Task<int> UpdateRecallVehicleStatusAsync(string chassisNo, int statusId)
        {
            var url = $"api/Recall/UpdateRecallVehicleStatus?chassisNo={Uri.EscapeDataString(chassisNo)}&statusId={statusId}";
            var response = await _httpClient.PutAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return 0;
            }

            var result = await response.Content.ReadFromJsonAsync<int>();
            return result;

        }
        public async Task<List<ActiveRecallsByChassisResponseDto>?> GetActiveRecallsByChassisBulkAsync(List<string> chassisList)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Recall/GetActiveRecallsByChassisBulk",  chassisList);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<List<ActiveRecallsByChassisResponseDto>>();

            return result;
        }



        #endregion

        #region Clocking

        public async Task<IEnumerable<ClockingDTO>?> GetClocksAsync()
        {
            return (await _httpClient.GetFromJsonAsync<IEnumerable<ClockingDTO>>($"api/Clocking/Get"));
        }

        public async Task<int?> InsertClock(ClockingDTO dTO)
        {
            var response = (await _httpClient.PostAsJsonAsync<ClockingDTO>($"api/Clocking/Insert", dTO));
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<int>();
            return result;

        }

        public async Task<int?> UpdateClock(ClockingDTO dTO)
        {
            var response = (await _httpClient.PostAsJsonAsync<ClockingDTO>($"api/Clocking/Update", dTO));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<int>();
            return result;

        }

        public async Task<int?> DeleteClock(int id)
        {
            var response = (await _httpClient.PostAsJsonAsync<int>($"api/Clocking/Delete", id));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<int>();
            return result;

        }

        public async Task<int?> InsertClockBreak(ClockingBreakDTO dTO)
        {
            var response = (await _httpClient.PostAsJsonAsync<ClockingBreakDTO>($"api/Clocking/InsertClockBreak", dTO));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<int>();
            return result;
            
        }
        public async Task<IEnumerable<TechnicianScheduleDTO>> GetTechniciansSchedule(DateTime Date, DateTime? DateTo, int branchId)
        {
            var url = $"api/WorkshopLoading/GetTechniciansSchedule?Date={Date}";

            if (DateTo.HasValue)
                url += $"&DateTo={DateTo}";
            url += $"&BranchId={branchId}";
            var obj = await _httpClient.GetFromJsonAsync<IEnumerable<TechnicianScheduleDTO>>(url);
            return obj;
        }

        public async Task<IEnumerable<TechnicianAvailabiltyDTO>> Get_TechnicianAvailabilty()
        {

            var obj = await _httpClient.GetFromJsonAsync<IEnumerable<TechnicianAvailabiltyDTO>>($"api/WorkshopLoading/GetTechnicianAvailabilty");
            return obj;
        }
        
        public async Task<IEnumerable<ClockingDTO>?> GetClocksHistoryAsync()
        {
            return (await _httpClient.GetFromJsonAsync<IEnumerable<ClockingDTO>>($"api/Clocking/GetClocksHistory"));
        }

        public async Task<IEnumerable<TechniciansNameDTO>> GetTechniciansName(int? Id)
        {
            var obj = await _httpClient.GetFromJsonAsync<IEnumerable<TechniciansNameDTO>>($"api/WorkshopLoading/GetAllTechnicians?Id={Id}");
            return obj;
        }


        public async Task<int?> UpdateClockBreak(ClockingBreakDTO dTO)
        {
            var response = (await _httpClient.PostAsJsonAsync<ClockingBreakDTO>($"api/Clocking/UpdateClockBreak", dTO));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<int>();
            return result;

        }
        
        public async Task<ClockingDTO?> GetClockById(int Id)
        {
            var response = (await _httpClient.PostAsJsonAsync<int>($"api/Clocking/GetClockById", Id));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<ClockingDTO>();
            return result;

        }

        public async Task<ClockingBreakDTO?> GetLastBreakByClockID(int Id)
        {
            var response = (await _httpClient.PostAsJsonAsync<int>($"api/Clocking/GetLastBreakByClockID", Id));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<ClockingBreakDTO>();
            return result;

        }
        public async Task<List<ClockingBreakDTO>?> GetAllClocksBreaksDDL()
        {
            return (await _httpClient.GetFromJsonAsync<List<ClockingBreakDTO>>($"api/Clocking/GetAllClocksBreaksDDL")); 

        }

        public async Task<List<ClockingDTO>?> GetAllClocksPaged(ClockingFilterDTO filterDTO)
        {
            var response = (await _httpClient.PostAsJsonAsync<ClockingFilterDTO>($"api/Clocking/GetAllPaged", filterDTO));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<List<ClockingDTO>>();
            return result;

        }
        public async Task<List<ClockingBreakDTO>?> GetAllBreaksPaged(ClockingFilterDTO filterDTO)
        {
            var response = (await _httpClient.PostAsJsonAsync<ClockingFilterDTO>($"api/Clocking/GetAllBreaksPaged", filterDTO));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<List<ClockingBreakDTO>>();
            return result;

        }
        public async Task<List<ClockingBreakDTO>?> GetBreaksByClockID(int ClockID)
        {
            var response = (await _httpClient.PostAsJsonAsync<int>($"api/Clocking/GetBreaksByClockID", ClockID));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<List<ClockingBreakDTO>>();
            return result;

        }

         public async Task<List<GetClockingFilter>?> GetClockingFilter()
        {
            var response = (await _httpClient.GetFromJsonAsync<List<GetClockingFilter>?>($"api/Clocking/GetClockingFilter"));
            return response;
        }
        #endregion


        #region WorkshopLoading

        public async Task<IEnumerable<TechnicianScheduleDTO>> GetTechniciansSchedule(DateTime Date, int branchId)
        {
            var dateString = Date.ToString("o", CultureInfo.InvariantCulture);
            var obj = await _httpClient.GetFromJsonAsync<IEnumerable<TechnicianScheduleDTO>>($"api/WorkshopLoading/GetTechniciansSchedule?Date={Uri.EscapeDataString(dateString)}&branchId={branchId}");
            return obj;
        }

        public async Task<IEnumerable<GroupedServicesDTO>> GetGroupedServices(int Id)
        {

            var obj = await _httpClient.GetFromJsonAsync<IEnumerable<GroupedServicesDTO>>($"api/WorkshopLoading/GetGroupedServices?Id={Id}");
            return obj;
        }

        #endregion

        #region Reservation
        public async Task<IEnumerable<ReservationListItemDTO>> GetAllReservationsAsync(ReservationFilterDTO reservationFilter)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Reservation/GetAllReservationsAsync", reservationFilter);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<ReservationListItemDTO>>();
            return result ?? Enumerable.Empty<ReservationListItemDTO>();
        }

        public async Task<int> InsertReservationAsync(ReservationDTO reservationDTO)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Reservation/InsertReservation", reservationDTO);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var result = json.GetProperty("reservationId").GetInt32();
            return result;
        }
        public async Task<int> UpdatedReservationStatusAsync(ReservationStatusUpdateDTO reservationStatusUpdateDTO)
        {
            var response = await _httpClient.PutAsJsonAsync("api/Reservation/UpdatedReservationStatus", reservationStatusUpdateDTO);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<int>();

            return result;
        }
        public async Task<IEnumerable<ReservationListItemDTO>>GetAllActiveReservationsFilteredAsync(DateTime? dateFrom, DateTime? dateTo)
        {
            var dateFromStr = dateFrom?.ToString("o", CultureInfo.InvariantCulture);
            var dateToStr = dateTo?.ToString("o", CultureInfo.InvariantCulture);

            var url =
                $"api/Reservation/GetAllActiveReservationsFilteredAsync" +
                $"?dateFrom={Uri.EscapeDataString(dateFromStr ?? string.Empty)}" +
                $"&dateTo={Uri.EscapeDataString(dateToStr ?? string.Empty)}";

            return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationListItemDTO>>(url);
        }
        public async Task<IEnumerable<ReservationListItemDTO>> GetReservationsByIdsAsync(string ids)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<ReservationListItemDTO>>($"api/Reservation/GetReservationsByIdsAsync?ids={ids}");

            return response;
        }
        public async Task<int> CheckIfVehicleHasActiveReservation(int vehicleId)
        {
            var response = await _httpClient.GetFromJsonAsync<int>($"api/Reservation/VehicleHasActiveReservation?vehicleId={vehicleId}");
            return response;
        }
        public async Task<int> UpdateReservation(ReservationDTO reservationDTO)
        {
            var response = await _httpClient.PutAsJsonAsync("api/Reservation/UpdateReservation", reservationDTO);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var result = json.GetProperty("reservationId").GetInt32();
            return result;
        }

        #endregion

        #region AccountDefinition

        public async Task<AccountDefinitionDTO?> GetAccountDefinitionGetAsync(int companyId)
        {
            return await _httpClient.GetFromJsonAsync<AccountDefinitionDTO>($"api/AccountDefinition/Get/{companyId}");
        }

        public async Task<int?> AddAccountDefinitionAsync(AccountDefinitionDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/AccountDefinition/Add", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            if (result == null) return null;
            if (result.ContainsKey("Id")) return result["Id"];
            if (result.ContainsKey("id")) return result["id"];
            return null;
        }

        public async Task<int?> UpdateAccountDefinitionAsync(AccountDefinitionDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/AccountDefinition/Update", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return null;
            }
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            if (result == null) return null;
            if (result.ContainsKey("Updated")) return result["Updated"];
            if (result.ContainsKey("updated")) return result["updated"];
            return null;
        }

        #endregion
    }
}
