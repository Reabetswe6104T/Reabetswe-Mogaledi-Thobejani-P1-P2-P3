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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public ResultsController(RaceDayDbContext context) { _context = context; }
        private int CurrentUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
        private string CurrentRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            var r = await _context.Results.Include(x => x.Entry).ThenInclude(e => e!.Participant).Include(x => x.Entry).ThenInclude(e => e!.Event).FirstOrDefaultAsync(x => x.ResultID == id);
            if (r == null) return NotFound();
            if (CurrentRole() != "Organiser" && r.Entry?.Participant?.UserID != CurrentUserId()) return Forbid();
            return Ok(r);
        }

        [AllowAnonymous]
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var results = await _context.Results.Include(r => r.Entry).ThenInclude(e => e!.Participant).Where(r => r.Entry!.EventID == eventId).OrderBy(r => r.Position).ToListAsync();
            return Ok(results);
        }

        [AllowAnonymous]
        [HttpGet("leaderboard/{eventId}")]
        public async Task<IActionResult> Leaderboard(int eventId, [FromQuery] int limit = 10)
        {
            var results = await _context.Results.Include(r => r.Entry).ThenInclude(e => e!.Participant).Where(r => r.Entry!.EventID == eventId && r.Status == "Finished" && r.Position != null).OrderBy(r => r.Position).Take(limit).ToListAsync();
            return Ok(results);
        }

        [Authorize]
        [HttpGet("entry/{entryId}")]
        public async Task<IActionResult> GetByEntry(int entryId)
        {
            var r = await _context.Results.Include(x => x.Entry).ThenInclude(e => e!.Participant).FirstOrDefaultAsync(x => x.EntryID == entryId);
            if (r == null) return NotFound();
            if (CurrentRole() != "Organiser" && r.Entry?.Participant?.UserID != CurrentUserId()) return Forbid();
            return Ok(r);
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create([FromBody] Result newResult)
        {
            var entry = await _context.Entries.Include(e => e.Event).ThenInclude(e => e!.Organiser).FirstOrDefaultAsync(e => e.EntryID == newResult.EntryID);
            if (entry == null) return NotFound();
            if (entry.Event?.Organiser == null || entry.Event.Organiser.UserID != CurrentUserId()) return Forbid();
            if (await _context.Results.AnyAsync(r => r.EntryID == newResult.EntryID)) return BadRequest(new { error = "Result already exists." });
            _context.Results.Add(newResult);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetResult), new { id = newResult.ResultID }, new { message = "Result created.", newResult.ResultID });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] Result updated)
        {
            var r = await _context.Results.Include(x => x.Entry).ThenInclude(e => e!.Event).ThenInclude(ev => ev!.Organiser).FirstOrDefaultAsync(x => x.ResultID == id);
            if (r == null) return NotFound();
            if (r.Entry?.Event?.Organiser == null || r.Entry.Event.Organiser.UserID != CurrentUserId()) return Forbid();
            r.FinishTime = updated.FinishTime ?? r.FinishTime;
            r.Position = updated.Position ?? r.Position;
            r.Pace = updated.Pace ?? r.Pace;
            r.Status = updated.Status ?? r.Status;
            r.CertificateURL = updated.CertificateURL ?? r.CertificateURL;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Result updated." });
        }
    }
}
