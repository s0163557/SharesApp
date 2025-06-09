using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharesApp.Server.Models;

public partial class SecurityDividend
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SecurityDividendId { get; set; }

    public int SecurityInfoId { get; set; }

    public DateOnly? Registry { get; set; }

    public DateOnly DateOfPayment { get; set; }

    public DateOnly? Period { get; set; }

    public double Dividend { get; set; }

    public decimal? Income { get; set; }

    public virtual SecurityInfo SecurityInfo { get; set; } = null!;
}
