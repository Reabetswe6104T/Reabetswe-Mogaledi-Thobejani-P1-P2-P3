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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public CategoriesController(RaceDayDbContext context) { _context = context; }
        private int CurrentUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

        [AllowAnonymous]
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var categories = await _context.Categories.Where(c => c.EventID == eventId).ToListAsync();
            return Ok(categories);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var c = await _context.Categories.Include(x => x.Event).FirstOrDefaultAsync(x => x.CategoryID == id);
            if (c == null) return NotFound();
            return Ok(c);
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create([FromBody] Category newCategory)
        {
            var ev = await _context.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventID == newCategory.EventID);
            if (ev == null) return NotFound(new { error = "Event not found." });
            if (ev.Organiser == null || ev.Organiser.UserID != CurrentUserId()) return Forbid();
            newCategory.CurrentParticipants = 0;
            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategory), new { id = newCategory.CategoryID }, new { message = "Category created.", newCategory.CategoryID });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] Category updated)
        {
            var c = await _context.Categories.Include(x => x.Event).ThenInclude(e => e!.Organiser).FirstOrDefaultAsync(x => x.CategoryID == id);
            if (c == null) return NotFound();
            if (c.Event?.Organiser == null || c.Event.Organiser.UserID != CurrentUserId()) return Forbid();
            c.CategoryName = updated.CategoryName ?? c.CategoryName;
            c.Distance = updated.Distance ?? c.Distance;
            c.EntryFee = updated.EntryFee ?? c.EntryFee;
            c.MaxParticipants = updated.MaxParticipants ?? c.MaxParticipants;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category updated." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.Categories.Include(x => x.Event).ThenInclude(e => e!.Organiser).Include(x => x.Entries).FirstOrDefaultAsync(x => x.CategoryID == id);
            if (c == null) return NotFound();
            if (c.Event?.Organiser == null || c.Event.Organiser.UserID != CurrentUserId()) return Forbid();
            if (c.Entries.Any(e => e.Status != "Cancelled")) return BadRequest(new { error = "Cannot delete with active entries." });
            _context.Categories.Remove(c);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category deleted." });
        }
    }
}