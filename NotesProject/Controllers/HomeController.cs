using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotesProject.Data;
using NotesProject.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NotesProject.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            this._context = context;
        }

        private string GetUserId()
        {
            return HttpContext.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        }

        public async Task<IActionResult> Index()
        {
            var recentlyAccessedNotes = await _context.Notes.Where(x => x.UserId == GetUserId() && x.IsActive).OrderByDescending(x => x.LastAccessedOn).Take(4).ToListAsync();
            return View(recentlyAccessedNotes);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
