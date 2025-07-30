using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Models;
using MyProject.Services;
using System.Security.Claims;

[Authorize]
public class BookingController : Controller
{
    private readonly LaCaffeineContext _context;
    private readonly IEmailServices _emailService;
    private readonly TimeSpan _buffer = TimeSpan.FromMinutes(45); // ⏰

    public BookingController(LaCaffeineContext context, IEmailServices emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(BookingDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdStr, out Guid userId))
            return Unauthorized();

        // Booking time with 45 min buffer on each side
        DateTime start = dto.DateTime - _buffer;
        DateTime end = dto.DateTime + _buffer;

        var slotCount = await _context.Bookings
            .Where(b => b.DateTime >= start && b.DateTime <= end)
            .CountAsync();

        string statusMessage;

        if (slotCount >= 5)
        {
            statusMessage = "Your booking is in the waiting list due to full capacity.";
        }
        else
        {
            _context.Bookings.Add(new Booking
            {
                Name = dto.Name,
                NoOfPerson = dto.NoOfPerson,
                Phone = dto.Phone,
                Mail = dto.Mail,
                DateTime = dto.DateTime,
                Message = dto.Message ?? "",
                UserId = userId
            });

            await _context.SaveChangesAsync();
            statusMessage = "Your booking is confirmed.";
        }

        await _emailService.SendEmailAsync(dto.Mail, "Booking Status", statusMessage);
        ViewBag.Status = statusMessage;
        return RedirectToAction("Index", "Home");
    }
}
