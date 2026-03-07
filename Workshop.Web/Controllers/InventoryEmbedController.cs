using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Encodings.Web;
using System.Text.Json;
using Workshop.Core.DTOs;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    public class InventoryEmbedController : BaseController
    {
        private readonly ERPApiClient _erpApiClient;
        private readonly WorkshopApiClient _apiClient;

        public InventoryEmbedController(WorkshopApiClient apiClient, ERPApiClient erpApiClient, IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            _apiClient = apiClient;
            _erpApiClient = erpApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmbedUrl(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith("/"))
                return BadRequest("Invalid returnUrl");

            if (returnUrl.StartsWith("//") || returnUrl.Contains("://"))
                return BadRequest("Invalid returnUrl");

            // user from session
            var userJson = HttpContext.Session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userJson))
                return Unauthorized("No user session");

            var sessionUser = JsonSerializer.Deserialize<UserDTO>(userJson);
            if (sessionUser == null)
                return Unauthorized("Invalid user session");

            int userId = sessionUser.UserID;

            //  user from ERP
            var erpUser = await _erpApiClient.GetUserInfoById(userId);
            if (erpUser == null)
                return Unauthorized("User not found");

            string email = erpUser.Email ?? "";
            string phone = erpUser.PhoneNo ?? "";

            // secrets + hash
            string prefix = _configuration["EmbedAuth:PrefixPassword"] ?? "";
            string suffix = _configuration["EmbedAuth:SuffixPassword"] ?? "";
            string contactHash = EmbedHashService.Generate(email, phone, userId, prefix, suffix);

            // build iframe src
            string inventoryBase = _configuration["ApiSettings:InventoryUrl"];
            string iframeSrc =
                $"{inventoryBase}/Authentication/EmbedVerify" +
                $"?userId={userId}" +
                $"&contactHash={contactHash}" +
                $"&returnUrl={UrlEncoder.Default.Encode(returnUrl)}";

            return Json(new { iframeSrc });
        }
    }
}
