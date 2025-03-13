using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Review
{
    public int Id { get; set; }

    public DateTime DateTime { get; set; }

    public int Rating { get; set; }

    public string Review1 { get; set; } = null!;

    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
