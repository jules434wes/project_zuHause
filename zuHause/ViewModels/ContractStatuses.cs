namespace zuHause.ViewModels
{

    //集中管理合約狀態active/signed 家具租借使用平率極高
    public class ContractStatuses
    {
            public const string Active = "active";
            public const string Signed = "signed";

            public static readonly string[] ValidStatuses = { Active, Signed };
     
    }
}
