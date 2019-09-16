using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TravelLogger.Models.Data;

namespace TravelLogger.Controllers
{
    [Authorize]
    public class LogController : Controller
    {
        private readonly TravelLoggerContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LogController(TravelLoggerContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Landing()
        {
            return View(await GetActiveLog());
        }

        [HttpPost]
        public async Task<IActionResult> Arrive(string comment)
        {
            comment = comment?.Trim();
            if (string.Empty == comment) comment = null;

            TravelLog activeLog = await GetActiveLog();
            if (activeLog == null)
            {
                return BadRequest();
            }

            activeLog.TimeOfArrival = DateTime.Now.ToUniversalTime();
            activeLog.Comment = comment;
            _context.Update(activeLog);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Depart()
        {
            TravelLog log = await GetActiveLog();
            if (log != null)
            {
                return Conflict();
            }

            // Start a new TravelLog
            _context.Add(
                new TravelLog
                {
                    User = await _userManager.GetUserAsync(HttpContext.User),
                    TimeOfDeparture = DateTime.Now.ToUniversalTime()
                }
            );
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Discard()
        {
            TravelLog log = await GetActiveLog();
            if (log != null)
            {
                _context.Remove(log);
                _context.SaveChanges();
            }

            return Ok();
        }

        private async Task<TravelLog> GetActiveLog()
        {
            IdentityUser activeUser = await _userManager.GetUserAsync(HttpContext.User);
            return _context.TravelLogs
                .Where(log => log.User == activeUser)
                .FirstOrDefault(log => log.TimeOfDeparture != null && log.TimeOfArrival == null);
        }
    }
}
