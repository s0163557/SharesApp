using System;
using System.Collections.Generic;

namespace SharesApp.Server.Models;

public partial class Inflation
{
    public int InflationId { get; set; }

    public DateOnly DateOfRecord { get; set; }

    public decimal? KeyRate { get; set; }

    public decimal InflationValue { get; set; }
}
