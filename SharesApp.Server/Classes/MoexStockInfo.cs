namespace Stock_Analysis_Web_App.Classes
{
    public class MoexStockInfo
    {
        public string? SecId;
        public string? Name;
        public string? Isin;
        public ulong? IssueSize;
        public DateOnly? IssueDate;
        public int? ListLevel;
    }
}
