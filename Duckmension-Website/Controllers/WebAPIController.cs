// WebAPIController.cs
// Provides JSON endpoints consumed by the Duckmension VS game over HTTP.
//
//   POST /api/game/login           { "Username": "Alice", "Password": "secret" }
//                                  -> { "ok": true,  "username": "Alice", "cookies": 42 }
//                                  -> { "ok": false, "error": "Invalid credentials" }
//
//   POST /api/game/update-cookies  { "Username": "Alice", "CookiesToAdd": 5 }
//                                  -> { "ok": true, "totalCookies": 47 }
//
//   GET  /api/game/user-info?username=Alice
//                                  -> { "ok": true, "username": "Alice", "cookies": 47 }
//
// Uses plain IdentityUser (matching the rest of the project) and reads/writes
// cookies via UserProfile.CookieCount, exactly as the rest of the site does.

using Duckmension_Website.Data;
using Duckmension_Website.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Duckmension_Website.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class WebAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public WebAPIController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ── POST /api/game/login ─────────────────────────────────────────────
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] GameLoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { ok = false, error = "Username and password are required" });

            var user = await _userManager.FindByNameAsync(req.Username);
            if (user == null)
                return Unauthorized(new { ok = false, error = "Invalid credentials" });

            // Verify password using the same Identity hasher the website uses
            var result = _userManager.PasswordHasher.VerifyHashedPassword(
                user, user.PasswordHash!, req.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { ok = false, error = "Invalid credentials" });

            // Look up the player's cookie count from their UserProfile
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            int cookies = profile?.Cookies ?? 0;

            return Ok(new { ok = true, username = user.UserName, cookies });
        }

        // ── POST /api/game/update-cookies ────────────────────────────────────
        [HttpPost("update-cookies")]
        public async Task<IActionResult> UpdateCookies([FromBody] GameUpdateCookiesRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || req.CookiesToAdd < 0)
                return BadRequest(new { ok = false, error = "Invalid request" });

            var user = await _userManager.FindByNameAsync(req.Username);
            if (user == null)
                return NotFound(new { ok = false, error = "User not found" });

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                // Create a profile on the fly if somehow missing (matches UserController behaviour)
                profile = new UserProfile
                {
                    UserId = user.Id,
                    Cookies = 0,
                    CurrentlyWornHat = 0,
                    OwnedHats = new List<int>()
                };
                _db.UserProfiles.Add(profile);
            }

            profile.Cookies += req.CookiesToAdd;
            await _db.SaveChangesAsync();

            return Ok(new { ok = true, totalCookies = profile.Cookies });
        }

        // ── GET /api/game/user-info?username=Alice ───────────────────────────
        [HttpGet("user-info")]
        public async Task<IActionResult> UserInfo([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { ok = false, error = "Username is required" });

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound(new { ok = false, error = "User not found" });

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            int cookies = profile?.Cookies ?? 0;

            return Ok(new { ok = true, username = user.UserName, cookies });
        }
    }

    // ── Request DTOs ─────────────────────────────────────────────────────────
    // Prefixed with "Game" to avoid clashing with any other LoginRequest
    // that Razor Pages / Identity scaffolding may already define.

    public class GameLoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class GameUpdateCookiesRequest
    {
        public string Username { get; set; } = "";
        public int CookiesToAdd { get; set; }
    }
}