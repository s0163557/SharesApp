using System;
using System.Collections.Generic;

namespace SharesApp.Server.Models;

public partial class SecurityDividend
{
    public int SecurityDividendId { get; set; }

    public int SecurityInfoId { get; set; }

    public DateOnly? Registry { get; set; }

    public DateOnly DateOfPayment { get; set; }

    public DateOnly? Period { get; set; }

    public double Dividend { get; set; }

    public decimal? Income { get; set; }

    public virtual SecurityInfo SecurityInfo { get; set; } = null!;
}
