using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieGO.Data;
using MovieGO.Models;

namespace MovieGO.Controllers
{
    public class ScreeningsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScreeningsController(ApplicationDbContext context)
        {
            _context = context;
        }
        private async Task HidePastScreenings()
        {
            var pastScreenings = await _context.Screenings
                .Where(s => s.ScreeningDate < DateTime.Now.AddHours(-24) && !s.IsHidden)
                .ToListAsync();

            foreach (var screening in pastScreenings)
            {
                screening.IsHidden = true;
            }

            await _context.SaveChangesAsync();
        }

        // GET: Screenings
        public async Task<IActionResult> Index(string? city, string? movieTitle, DateTime? screeningDate)
        {
            await HidePastScreenings();

            var screeningsQuery = _context.Screenings
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .Include(s => s.Movie)
                    .ThenInclude(m => m.Category)
                .Where(s => !s.IsHidden)
                .AsQueryable();

            if (!string.IsNullOrEmpty(city))
            {
                screeningsQuery = screeningsQuery.Where(s => s.Hall.Cinema.City == city);
            }
            if (!string.IsNullOrEmpty(movieTitle))
            {
                screeningsQuery = screeningsQuery.Where(s => s.Movie.Title.Contains(movieTitle));
            }
            if (screeningDate.HasValue)
            {
                screeningsQuery = screeningsQuery.Where(s => s.ScreeningDate.Date == screeningDate.Value.Date);
            }

            screeningsQuery = screeningsQuery
                .OrderBy(s => s.ScreeningDate); 

            ViewData["Cities"] = await _context.Cinemas
                .Select(c => c.City)
                .Distinct()
                .ToListAsync();
            ViewData["Movies"] = await _context.Movies
                .Select(m => m.Title)
                .Distinct()
                .ToListAsync();

            ViewData["City"] = city;
            ViewData["MovieTitle"] = movieTitle;
            ViewData["ScreeningDate"] = screeningDate?.ToString("yyyy-MM-dd");

            return View(await screeningsQuery.ToListAsync());
        }


        // GET: Screenings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var screening = await _context.Screenings
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (screening == null)
            {
                return NotFound();
            }

            return View(screening);
        }

        // GET: Screenings/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            ViewData["HallId"] = new SelectList(
                   _context.Halls.Include(h => h.Cinema)
                  .Select(h => new { Id = h.Id, CinemaNameDisplay = $"{h.Cinema.Name} - {h.Name}" })
                  .ToList(),
                    "Id",
                    "CinemaNameDisplay"
            );
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");
            return View();
        }

        // POST: Screenings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,ScreeningDate,HallId,MovieId")] Screening screening)
        {
            if (ModelState.IsValid)
            {
                _context.Add(screening);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Id", screening.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Id", screening.MovieId);
            return View(screening);
        }

        // GET: Screenings/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var screening = await _context.Screenings.FindAsync(id);
            if (screening == null || screening.IsHidden)
            {
                return NotFound();
            }

            ViewData["HallId"] = new SelectList(
                _context.Halls.Include(h => h.Cinema)
                    .Select(h => new { Id = h.Id, CinemaNameDisplay = $"{h.Cinema.Name} - {h.Name}" })
                    .ToList(),
                "Id",
                "CinemaNameDisplay"
            );
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");
            return View(screening);
        }

        // POST: Screenings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ScreeningDate,HallId,MovieId")] Screening screening)
        {
            if (id != screening.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(screening);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScreeningExists(screening.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Id", screening.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Id", screening.MovieId);
            return View(screening);
        }

        // GET: Screenings/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var screening = await _context.Screenings
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (screening == null)
            {
                return NotFound();
            }

            return View(screening);
        }

        // POST: Screenings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var screening = await _context.Screenings.FindAsync(id);
            if (screening != null)
            {
                _context.Screenings.Remove(screening);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScreeningExists(int id)
        {
            return _context.Screenings.Any(e => e.Id == id);
        }
    }
}
