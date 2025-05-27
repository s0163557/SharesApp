using System;
using System.Collections.Generic;

namespace SharesApp.Server.Models;

public partial class SecurityInfo
{
    public int SecurityInfoId { get; set; }

    public string SecurityId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Isin { get; set; }

    public long IssueSize { get; set; }

    public DateOnly IssueDate { get; set; }

    public int ListLevel { get; set; }

    public virtual ICollection<SecurityDividend> SecurityDividends { get; set; } = new List<SecurityDividend>();

    public virtual ICollection<SecurityTradeRecord> SecurityTradeRecords { get; set; } = new List<SecurityTradeRecord>();

    public virtual ICollection<SecurityTradeRecordsByMonth> SecurityTradeRecordsByMonths { get; set; } = new List<SecurityTradeRecordsByMonth>();

    public virtual ICollection<SecurityTradeRecordsByWeek> SecurityTradeRecordsByWeeks { get; set; } = new List<SecurityTradeRecordsByWeek>();
}
