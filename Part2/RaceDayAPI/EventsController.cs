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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public EventsController(RaceDayDbContext context) { _context = context; }
        private int CurrentUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events.Include(e => e.Organiser).Where(e => e.IsActive).ToListAsync();
            return Ok(events);
        }

        [AllowAnonymous]
        [HttpGet("upcoming")]
        public async Task<IActionResult> Upcoming()
        {
            var upcoming = await _context.Events.Where(e => e.EventDate > DateTime.UtcNow && e.IsActive).OrderBy(e => e.EventDate).ToListAsync();
            return Ok(upcoming);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var e = await _context.Events.Include(x => x.Organiser).Include(x => x.Categories).FirstOrDefaultAsync(x => x.EventID == id);
            if (e == null) return NotFound(new { error = "Event not found." });
            return Ok(e);
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create([FromBody] Event newEvent)
        {
            if (newEvent.RegistrationDeadline >= newEvent.EventDate) return BadRequest(new { error = "Deadline must be before event date." });
            if (newEvent.EventDate < DateTime.UtcNow) return BadRequest(new { error = "Event date cannot be in the past." });
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId());
            if (organiser == null) return Forbid();
            newEvent.OrganiserID = organiser.OrganiserID;
            newEvent.CurrentParticipants = 0;
            newEvent.IsActive = true;
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventID }, new { message = "Event created.", newEvent.EventID });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] Event updated)
        {
            var e = await _context.Events.Include(x => x.Organiser).FirstOrDefaultAsync(x => x.EventID == id);
            if (e == null) return NotFound();
            if (e.Organiser == null || e.Organiser.UserID != CurrentUserId()) return Forbid();
            e.EventName = updated.EventName ?? e.EventName;
            e.Description = updated.Description ?? e.Description;
            e.MaxParticipants = updated.MaxParticipants ?? e.MaxParticipants;
            e.EntryFee = updated.EntryFee ?? e.EntryFee;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated.", e.EventID });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _context.Events.Include(x => x.Organiser).FirstOrDefaultAsync(x => x.EventID == id);
            if (e == null) return NotFound();
            if (e.Organiser == null || e.Organiser.UserID != CurrentUserId()) return Forbid();
            e.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event deleted." });
        }
    }
}
