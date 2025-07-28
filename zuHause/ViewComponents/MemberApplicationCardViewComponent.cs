using Microsoft.AspNetCore.Mvc;
using zuHause.ViewModels.MemberViewModel;
using zuHause.Configs;

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
                ? ApplicationFlowConfig.ApplicationStepsMap["REJECTED_FLOW"]
                : ApplicationFlowConfig.ApplicationStepsMap.GetValueOrDefault(appType) ?? new();

            int currentStepIndex = steps.IndexOf(latestStatus);
            int activeStep = currentStepIndex >= 0 ? currentStepIndex + 1 : 0;

            // 加進 ViewData 傳入 View
            ViewData["Steps"] = steps;
            ViewData["ActiveStep"] = activeStep;
            ViewData["StepsName"] = latestStatus;

            return View(model);
        }


    }
}
