using Workshop.Core.DTOs;
using Workshop.Core.DTOs.General;
using Workshop.Web.Models;

namespace Workshop.Web.Services
{
    public class WorkflowEmailService 
    {
        private readonly ERPApiClient _erpApiClient;
        private readonly WorkshopApiClient _apiClient;
        private readonly InventoryApiClient _inventoryApiClient;
        private readonly VehicleApiClient _vehicleApis;
        private readonly EmailSender _emailSender;
        private readonly ILogger<WorkflowEmailService> _logger;

        public WorkflowEmailService(
            ERPApiClient erpApiClient,
            WorkshopApiClient apiClient,
            InventoryApiClient inventoryApiClient,
            VehicleApiClient vehicleApis,
            EmailSender emailSender,
            ILogger<WorkflowEmailService> logger)
        {
            _erpApiClient = erpApiClient;
            _apiClient = apiClient;
            _inventoryApiClient = inventoryApiClient;
            _vehicleApis = vehicleApis;
            _emailSender = emailSender;
            _logger = logger;
        }


        public async Task<bool> SendAsync(WorkflowEmailRequest request, StateResponse state)
        {
            try
            {
                if (request.MasterId == Guid.Empty || state == null)
                    return false;

                var ctx = await _apiClient.WIP_GetItemsById(request.WipId, request.WipItemId, request.KeyId)
                          ?? new CreateItemDTO();

                if (ctx.ItemId > 0)
                {
                    var item = await _inventoryApiClient.GetItemByIdAsync(ctx.ItemId);
                    if (item != null)
                    {
                        ctx.Code ??= item.Code;
                        ctx.Name ??= request.Lang == "en" ? item.PrimaryName : item.SecondaryName;
                    }
                }

                var company = await _erpApiClient.GetCompanyById(request.CompanyId);
                if (company == null)
                    return false;

                if (state.UsersContactInformation == null || state.UsersContactInformation.Count == 0)
                    return false;

                if (state.NotificationType != 2 && state.NotificationType != 3)
                    return false;

                string primaryBody = "";
                string secondaryBody = "";
                string body = "";
                string subject = "Workflow Action - إجراء سير العمل";

                switch (request.Action)
                {
                    case 1: // approve
                        primaryBody =
                            $"<h3>Price approval completed for WIP #{ctx.WIPId}, action is required.</h3>" +
                            $"<p><strong>Item Code:</strong> {ctx.Code ?? "-"}</p>" +
                            $"<p><strong>Item Name:</strong> {ctx.Name ?? "-"}</p>" +
                            $"<p><strong>Qty:</strong> {ctx.RequestQuantity:0.##}</p>" +
                            $"<p><strong>Price:</strong> {(ctx.SalePrice?.ToString("0.##") ?? "-")}</p>";

                        secondaryBody =
                            $"<h3 style='text-align:right'>تمت الموافقة على اعتماد السعر للطلب #{ctx.WIPId} ، يرجى اتخاذ إجراء.</h3>" +
                            $"<p style='text-align:right'><strong>رمز الصنف:</strong> {ctx.Code ?? "-"}</p>" +
                            $"<p style='text-align:right'><strong>اسم الصنف:</strong> {ctx.Name ?? "-"}</p>" +
                            $"<p style='text-align:right'><strong>الكمية:</strong> {ctx.RequestQuantity:0.##}</p>" +
                            $"<p style='text-align:right'><strong>السعر:</strong> {(ctx.SalePrice?.ToString("0.##") ?? "-")}</p>";
                        break;

                    case 2: // reject
                        primaryBody =
                            $"<h3>Price approval rejected for WIP #{ctx.WIPId}, action is required.</h3>" +
                            $"<p><strong>Item Code:</strong> {ctx.Code ?? "-"}</p>" +
                            $"<p><strong>Item Name:</strong> {ctx.Name ?? "-"}</p>" +
                            $"<p><strong>Qty:</strong> {ctx.RequestQuantity:0.##}</p>" +
                            $"<p><strong>Price:</strong> {(ctx.SalePrice?.ToString("0.##") ?? "-")}</p>";

                        secondaryBody =
                            $"<h3 style='text-align:right'>تم رفض اعتماد السعر للطلب #{ctx.WIPId} ، يرجى اتخاذ إجراء.</h3>" +
                            $"<p style='text-align:right'><strong>رمز الصنف:</strong> {ctx.Code ?? "-"}</p>" +
                            $"<p style='text-align:right'><strong>اسم الصنف:</strong> {ctx.Name ?? "-"}</p>" +
                            $"<p style='text-align:right'><strong>الكمية:</strong> {ctx.RequestQuantity:0.##}</p>" +
                            $"<p style='text-align:right'><strong>السعر:</strong> {(ctx.SalePrice?.ToString("0.##") ?? "-")}</p>";
                        break;

                    case 3: // review
                        primaryBody =
                            $"<h3>Price approval reviewed for WIP #{ctx.WIPId} , action is required.</h3>" +
                            $"<p><strong>Item Code:</strong> {ctx.Code ?? "-"}</p>" +
                            $"<p><strong>Item Name:</strong> {ctx.Name ?? "-"}</p>" +
                            $"<p><strong>Qty:</strong> {ctx.RequestQuantity:0.##}</p>" +
                            $"<p><strong>Price:</strong> {(ctx.SalePrice?.ToString("0.##") ?? "-")}</p>";

                        secondaryBody =
                            $"<h3 style='text-align:right'>تمت مراجعة اعتماد السعر للطلب #{ctx.WIPId} ، يرجى اتخاذ إجراء.</h3>" +
                            $"<p style='text-align:right'><strong>رمز الصنف:</strong> {ctx.Code ?? "-"}</p>" +
                            $"<p style='text-align:right'><strong>اسم الصنف:</strong> {ctx.Name ?? "-"}</p>" +
                            $"<p style='text-align:right'><strong>الكمية:</strong> {ctx.RequestQuantity:0.##}</p>" +
                            $"<p style='text-align:right'><strong>السعر:</strong> {(ctx.SalePrice?.ToString("0.##") ?? "-")}</p>";
                        break;

                    default: // new
                        primaryBody =
                            $"<h3>New price approval request created for WIP #{ctx.WIPId}, action is required.</h3>" +
                            $"<p><strong>Item Code:</strong> {ctx.Code ?? "-"}</p>" +
                            $"<p><strong>Item Name:</strong> {ctx.Name ?? "-"}</p>" +
                            $"<p><strong>Qty:</strong> {ctx.RequestQuantity:0.##}</p>" +
                            $"<p><strong>Price:</strong> {(ctx.SalePrice?.ToString("0.##") ?? "-")}</p>";

                        secondaryBody =
                            $"<h3 style='text-align:right'>تم إنشاء طلب اعتماد سعر جديد للطلب #{ctx.WIPId} ، يرجى اتخاذ إجراء.</h3>" +
                            $"<p style='text-align:right'><strong>رمز الصنف:</strong> {ctx.Code ?? "-"}</p>" +
                            $"<p style='text-align:right'><strong>اسم الصنف:</strong> {ctx.Name ?? "-"}</p>" +
                            $"<p style='text-align:right'><strong>الكمية:</strong> {ctx.RequestQuantity:0.##}</p>" +
                            $"<p style='text-align:right'><strong>السعر:</strong> {(ctx.SalePrice?.ToString("0.##") ?? "-")}</p>";
                        break;
                }

                body = primaryBody + secondaryBody;

                var allSent = true;

                foreach (var user in state.UsersContactInformation)
                {
                    if (string.IsNullOrWhiteSpace(user.Email))
                        continue;

                    var mail = new Mail
                    {
                        To = user.Email,
                        Subject = subject,
                        Body = body
                    };

                    var sent = await _emailSender.SendAsync(mail, company);
                    if (!sent)
                        allSent = false;
                }

                return allSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send workflow email. MasterId: {MasterId}", request.MasterId);
                return false;
            }
        }
        //public async Task<bool> SendAsync(WorkflowEmailRequest request, StateResponse state)
        //{
        //    try
        //    {
        //        if (request.MasterId == Guid.Empty || state == null)
        //            return false;

        //        var ctx = await _apiClient.WIP_GetItemsById(request.WipId, request.WipItemId, request.KeyId)
        //                  ?? new CreateItemDTO();

        //        if (ctx.ItemId > 0)
        //        {
        //            var item = await _inventoryApiClient.GetItemByIdAsync(ctx.ItemId);
        //            if (item != null)
        //            {
        //                ctx.Code ??= item.Code;
        //                ctx.Name ??= request.Lang == "en" ? item.PrimaryName : item.SecondaryName;
        //            }
        //        }

        //        var emailTemplate = await _vehicleApis.GetEmailTemplate(14);
        //        if (emailTemplate == null)
        //            return false;

        //        var bodyTemplate = request.Lang == "ar" ? emailTemplate.SecondaryBody : emailTemplate.PrimaryBody;
        //        var subjectTemplate = request.Lang == "ar" ? emailTemplate.SecondarySubject : emailTemplate.PrimarySubject;

        //        if (string.IsNullOrWhiteSpace(bodyTemplate) || string.IsNullOrWhiteSpace(subjectTemplate))
        //            return false;

        //        var primaryTitle = request.Action switch
        //        {
        //            1 => "Price approval step completed — next action required.",
        //            2 => "Price approval rejected — action required.",
        //            3 => "Price approval reviewed — action required.",
        //            _ => "New price approval request — action required."
        //        };

        //        var secondaryTitle = request.Action switch
        //        {
        //            1 => "تمت خطوة اعتماد السعر — يوجد إجراء مطلوب للمرحلة التالية.",
        //            2 => "تم رفض اعتماد السعر — يرجى اتخاذ إجراء.",
        //            3 => "تمت مراجعة اعتماد السعر — يرجى اتخاذ إجراء.",
        //            _ => "طلب اعتماد سعر جديد — يرجى اتخاذ إجراء."
        //        };

        //        var subject = subjectTemplate
        //            .Replace("##WipId##", ctx.WIPId > 0 ? ctx.WIPId.ToString() : "-")
        //            .Replace("##WipItemId##", request.WipItemId.ToString())
        //            .Replace("##ItemCode##", ctx.Code ?? "-")
        //            .Replace("##ItemName##", ctx.Name ?? "-");

        //        var body = bodyTemplate
        //            .Replace("##PrimaryTitle##", primaryTitle)
        //            .Replace("##SecondaryTitle##", secondaryTitle)
        //            .Replace("##WipId##", ctx.WIPId > 0 ? ctx.WIPId.ToString() : "-")
        //            .Replace("##WipItemId##", request.WipItemId.ToString())
        //            .Replace("##ItemCode##", ctx.Code ?? "-")
        //            .Replace("##ItemName##", ctx.Name ?? "-")
        //            .Replace("##Qty##", ctx.Quantity.ToString("0.##"))
        //            .Replace("##Price##", ctx.SalePrice?.ToString("0.##") ?? "-")
        //            .Replace("##MasterId##", request.MasterId.ToString());

        //        var company = await _erpApiClient.GetCompanyById(request.CompanyId);
        //        if (company == null)
        //            return false;

        //        if (state.UsersContactInformation == null || state.UsersContactInformation.Count == 0)
        //            return false;

        //        if (state.NotificationType != 2 && state.NotificationType != 3)
        //            return false;

        //        var allSent = true;

        //        foreach (var user in state.UsersContactInformation)
        //        {
        //            if (string.IsNullOrWhiteSpace(user.Email))
        //                continue;

        //            var mail = new Mail
        //            {
        //                //To = user.Email,
        //                To = "alakayleh.aya@gmail.com",
        //                Subject = subject,
        //                Body = body
        //            };

        //            var sent = await _emailSender.SendAsync(mail, company);
        //            if (!sent)
        //                allSent = false;
        //        }

        //        return allSent;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to send workflow email. MasterId: {MasterId}", request.MasterId);
        //        return false;
        //    }
        //}


    }
}

