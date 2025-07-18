using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string EmailId { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime TimeStamp { get; set; }

    public string? ProfilePhoto { get; set; }

    public string? Providers { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
