using Microsoft.AspNetCore.Mvc;
using zuHause.ViewModels.MemberViewModel;

namespace zuHause.ViewComponents
{
    public class MemberApplicationCardViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(MemberApplicationCardInputViewModel model)
        {
            var record = model.Record;
            var statusLogs = record.StatusLogs;
            var latestStatus = statusLogs.LastOrDefault()?.StatusCode ?? "";
            string appType = record.ApplicationType ?? "RENTAL"; // "RENTAL" or "HOUSE_VIEWING"



            // 若最後狀態為 REJECTED，改用婉拒流程
            bool isRejected = latestStatus == "REJECTED";
            var steps = isRejected
                ? ApplicationStepsMap["REJECTED_FLOW"]
                : ApplicationStepsMap.GetValueOrDefault(appType) ?? new();

            int currentStepIndex = steps.IndexOf(latestStatus);
            int activeStep = currentStepIndex >= 0 ? currentStepIndex + 1 : 0;

            // 加進 ViewData 傳入 View
            ViewData["Steps"] = steps;
            ViewData["ActiveStep"] = activeStep;

            return View(model);
        }

        // 這建議放在 ViewComponent 或 Helper 類別
        private static readonly Dictionary<string, List<string>> ApplicationStepsMap = new()
        {
            ["HOUSE_VIEWING"] = new() // 看房
            {
                "APPLIED",       // 已申請
                "PENDING",       // 待審核
                "APPROVED",      // 已同意申請
                "VIEWING_SCHEDULED",       // 看房日
                "VIEWING_COMPLETED"         // 看房已完成
            },
            ["RENTAL"] = new() // 租賃
            {
                "APPLIED",       // 已申請
                "PENDING",       // 待審核
                "APPROVED",      // 已同意申請
                "SIGNING",       // 合約簽署中
                "CONTRACTED"         // 合約已完成
            },
            ["REJECTED_FLOW"] = new() // 看房/租賃被拒絕
            {
                "APPLIED",
                "PENDING",
                "REJECTED"
            }
        };

    }
}
