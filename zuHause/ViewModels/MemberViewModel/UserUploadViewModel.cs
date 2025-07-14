namespace zuHause.ViewModels.MemberViewModel
{
    public class UserUploadViewModel
    {
        //public string UserUploadID { get; set; }
        public IFormFile? UploadFile { get; set; } // 上傳檔案
        public string? UploadTypeCode {get;set; } // 系統表類別
        public string? ModuleCode { get;set;} // 來源模組
        public string? CodeCategory { get;set;} // 系統表模組別
    }
}
