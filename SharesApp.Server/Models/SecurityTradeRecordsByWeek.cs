using System;
using System.Collections.Generic;

namespace SharesApp.Server.Models;

public partial class SecurityTradeRecordsByWeek
{
    public int SecurityTradeRecordId { get; set; }

    public int SecurityInfoId { get; set; }

    public DateOnly DateOfTrade { get; set; }

    public int NumberOfTrades { get; set; }

    public double Value { get; set; }

    public double Open { get; set; }

    public double Low { get; set; }

    public double High { get; set; }

    public double Close { get; set; }

    public virtual SecurityInfo SecurityInfo { get; set; } = null!;
}
