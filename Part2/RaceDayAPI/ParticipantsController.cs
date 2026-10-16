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
    public class ParticipantsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId()
        {
            return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
        }

        private string CurrentRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetParticipant(int id)
        {
            var p = await _context.Participants.Include(x => x.User).FirstOrDefaultAsync(x => x.ParticipantID == id);
            if (p == null) return NotFound(new { error = "Participant not found." });
            if (CurrentRole() != "Organiser" && p.UserID != CurrentUserId()) return Forbid();
            return Ok(p);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParticipant(int id, [FromBody] Participant updated)
        {
            var p = await _context.Participants.FirstOrDefaultAsync(x => x.ParticipantID == id);
            if (p == null) return NotFound(new { error = "Participant not found." });
            if (CurrentRole() != "Organiser" && p.UserID != CurrentUserId()) return Forbid();
            p.FullName = updated.FullName ?? p.FullName;
            p.PhoneNumber = updated.PhoneNumber ?? p.PhoneNumber;
           
            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated.", p.ParticipantID });
        }

        [HttpGet("{id}/entries")]
        public async Task<IActionResult> GetEntries(int id)
        {
            var p = await _context.Participants.FirstOrDefaultAsync(x => x.ParticipantID == id);
            if (p == null) return NotFound();
            if (CurrentRole() != "Organiser" && p.UserID != CurrentUserId()) return Forbid();
            var entries = await _context.Entries.Where(e => e.ParticipantID == id).ToListAsync();
            return Ok(entries);
        }

        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResults(int id)
        {
            var p = await _context.Participants.FirstOrDefaultAsync(x => x.ParticipantID == id);
            if (p == null) return NotFound();
            if (CurrentRole() != "Organiser" && p.UserID != CurrentUserId()) return Forbid();
            var results = await _context.Results.Include(r => r.Entry).Where(r => r.Entry!.ParticipantID == id).ToListAsync();
            return Ok(results);
        }
    }
}
