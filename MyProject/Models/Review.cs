using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Review
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime DateTime { get; set; }

    public int Rating { get; set; }

    public string Review1 { get; set; } = null!;
}
