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
    public class OrganisersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public OrganisersController(RaceDayDbContext context) { _context = context; }
        private int CurrentUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrganiser(int id)
        {
            var o = await _context.Organisers.Include(x => x.User).FirstOrDefaultAsync(x => x.OrganiserID == id);
            if (o == null) return NotFound(new { error = "Organiser not found." });
            return Ok(o);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateOrganiser(int id, [FromBody] Organiser updated)
        {
            var o = await _context.Organisers.FirstOrDefaultAsync(x => x.OrganiserID == id);
            if (o == null) return NotFound();
            if (o.UserID != CurrentUserId()) return Forbid();
            o.OrganisationName = updated.OrganisationName ?? o.OrganisationName;
            o.ContactPerson = updated.ContactPerson ?? o.ContactPerson;
            o.PhoneNumber = updated.PhoneNumber ?? o.PhoneNumber;
            o.Website = updated.Website ?? o.Website;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated.", o.OrganiserID });
        }

        [AllowAnonymous]
        [HttpGet("{id}/events")]
        public async Task<IActionResult> GetEvents(int id)
        {
            var events = await _context.Events.Where(e => e.OrganiserID == id).ToListAsync();
            return Ok(events);
        }
    }
}
