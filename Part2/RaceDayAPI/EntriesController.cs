using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.Security.Claims;
namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EntriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public EntriesController(RaceDayDbContext context) { _context = context; }
        private int CurrentUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
        private string CurrentRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntry(int id)
        {
            var e = await _context.Entries.Include(x => x.Participant).Include(x => x.Event).Include(x => x.Category).FirstOrDefaultAsync(x => x.EntryID == id);
            if (e == null) return NotFound();
            if (CurrentRole() != "Organiser" && e.Participant?.UserID != CurrentUserId()) return Forbid();
            return Ok(e);
        }

        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Create([FromBody] Entry newEntry)
        {
            var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId());
            if (participant == null) return Forbid();
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventID == newEntry.EventID);
            if (ev == null) return NotFound(new { error = "Event not found." });
            if (ev.RegistrationDeadline < DateTime.UtcNow) return BadRequest(new { error = "Deadline passed." });
            var cat = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == newEntry.CategoryID);
            if (cat == null || cat.EventID != newEntry.EventID) return BadRequest(new { error = "Invalid category." });
            if (await _context.Entries.AnyAsync(e => e.ParticipantID == participant.ParticipantID && e.EventID == newEntry.EventID && e.Status != "Cancelled"))
                return BadRequest(new { error = "Already enrolled." });
            if (ev.MaxParticipants.HasValue && ev.CurrentParticipants >= ev.MaxParticipants) return BadRequest(new { error = "Event full." });
            newEntry.ParticipantID = participant.ParticipantID;
            newEntry.Status = "Pending";
            newEntry.PaymentStatus = "Pending";
            var lastBib = await _context.Entries.Where(e => e.BibNumber != null).MaxAsync(e => (int?)e.BibNumber) ?? 1000;
            newEntry.BibNumber = lastBib + 1;
            _context.Entries.Add(newEntry);
            ev.CurrentParticipants++;
            cat.CurrentParticipants++;
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEntry), new { id = newEntry.EntryID }, new { message = "Enrolled.", newEntry.EntryID, newEntry.BibNumber });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Entry updated)
        {
            var e = await _context.Entries.Include(x => x.Participant).FirstOrDefaultAsync(x => x.EntryID == id);
            if (e == null) return NotFound();
            if (CurrentRole() != "Organiser" && e.Participant?.UserID != CurrentUserId()) return Forbid();
            if (e.Status == "Confirmed" && CurrentRole() != "Organiser") return BadRequest(new { error = "Cannot update confirmed entry." });
            e.TShirtSize = updated.TShirtSize ?? e.TShirtSize;
            e.DietaryRequirements = updated.DietaryRequirements ?? e.DietaryRequirements;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated." });
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var e = await _context.Entries.Include(x => x.Participant).Include(x => x.Event).Include(x => x.Category).FirstOrDefaultAsync(x => x.EntryID == id);
            if (e == null) return NotFound();
            if (CurrentRole() != "Organiser" && e.Participant?.UserID != CurrentUserId()) return Forbid();
            if (e.Status == "Cancelled") return BadRequest(new { error = "Already cancelled." });
            e.Status = "Cancelled";
            if (e.Event != null) e.Event.CurrentParticipants--;
            if (e.Category != null) e.Category.CurrentParticipants--;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cancelled." });
        }

        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEntries(int eventId)
        {
            var ev = await _context.Events.Include(x => x.Organiser).FirstOrDefaultAsync(x => x.EventID == eventId);
            if (ev == null) return NotFound();
            if (ev.Organiser == null || ev.Organiser.UserID != CurrentUserId()) return Forbid();
            var entries = await _context.Entries.Where(e => e.EventID == eventId).Include(e => e.Participant).Include(e => e.Category).ToListAsync();
            return Ok(entries);
        }

        [HttpPut("{id}/payment")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] Entry paymentUpdate)
        {
            var e = await _context.Entries.Include(x => x.Participant).FirstOrDefaultAsync(x => x.EntryID == id);
            if (e == null) return NotFound();
            if (e.Participant?.UserID != CurrentUserId()) return Forbid();
            if (string.IsNullOrEmpty(paymentUpdate.PaymentReference)) return BadRequest(new { error = "Payment reference required." });
            e.PaymentReference = paymentUpdate.PaymentReference;
            e.PaymentStatus = paymentUpdate.PaymentStatus ?? "Paid";
            e.PaymentDate = DateTime.UtcNow;
            e.EntryFeePaid = paymentUpdate.EntryFeePaid ?? e.EntryFeePaid;
            e.Status = "Confirmed";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Payment updated." });
        }
    }
}
