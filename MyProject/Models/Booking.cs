using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Booking
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int NoOfPerson { get; set; }

    public long Phone { get; set; }

    public string Mail { get; set; } = null!;

    public DateTime DateTime { get; set; }

    public string Message { get; set; } = null!;

    public Guid UserId { get; set; }
}
