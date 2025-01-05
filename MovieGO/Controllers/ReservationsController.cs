using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieGO.Data;
using MovieGO.Models;

namespace MovieGO.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservations
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservations = await _context.Reservations
                .Where(r => User.IsInRole("Administrator") || r.UserId == userId)
                .Include(r => r.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(r => r.Screening)
                    .ThenInclude(s => s.Hall)
                        .ThenInclude(h => h.Cinema)
                .Include(r => r.User)
                .OrderBy(r => r.Screening.ScreeningDate)
                .ToListAsync();

            return View(reservations);
        }

        // GET: Reservations/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Screening)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create(int screeningId)
        {
            var screening = _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .Include(s => s.Reservations)
                .FirstOrDefault(s => s.Id == screeningId);

            if (screening == null || screening.ScreeningDate < DateTime.Now)
            {
                ViewBag.ErrorMessage = "Seans nie istnieje lub już się odbył.";
                return View(new Reservation());
            }

            var reservation = new Reservation
            {
                Screening = screening,
                ScreeningId = screening.Id
            };

            return View(reservation);
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ScreeningId,Row,Column")] Reservation reservation)
        {
            reservation.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var screening = await _context.Screenings.FindAsync(reservation.ScreeningId);
            if (screening == null || screening.ScreeningDate < DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "Nie można dokonać rezerwacji na seans, który już się odbył lub jest w trakcie.");
                return View(reservation);
            }

            if (ModelState.IsValid)
            {
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(reservation);
        }

        // GET: Reservations/Edit/5
        [Authorize]
        public IActionResult Edit(int id)
        {
            var reservation = _context.Reservations
                .Include(r => r.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(r => r.Screening)
                    .ThenInclude(s => s.Hall)
                        .ThenInclude(h => h.Cinema)
                .Include(r => r.User)
                .Include(r => r.Screening)
                    .ThenInclude(s => s.Reservations)
                .FirstOrDefault(r => r.Id == id);

            if (reservation == null || reservation.Screening == null)
            {
                ViewBag.ErrorMessage = "Rezerwacja nie istnieje.";
                return View(new Reservation());
            }

            if (reservation.Screening.ScreeningDate < DateTime.Now)
            {
                ViewBag.ErrorMessage = "Nie można edytować rezerwacji na seans, który już się odbył.";
                return View(new Reservation());
            }

            ViewBag.HallRowCount = reservation.Screening.Hall.RowCount;
            ViewBag.HallColumnCount = reservation.Screening.Hall.ColumnCount;
            ViewBag.ExistingReservations = reservation.Screening.Reservations
                .Where(r => r.Id != reservation.Id)
                .ToList();

            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ScreeningId,Row,Column")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound("Rezerwacja nie istnieje.");
            }

            var existingReservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Screening)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingReservation == null || (!User.IsInRole("Administrator") && existingReservation.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Forbid();
            }

            var screening = await _context.Screenings.FirstOrDefaultAsync(s => s.Id == reservation.ScreeningId);

            if (screening == null || screening.ScreeningDate < DateTime.Now)
            {
                ViewBag.ErrorMessage = "Nie można edytować rezerwacji na seans, który już się odbył.";
                return View(reservation);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingReservation.Row = reservation.Row;
                    existingReservation.Column = reservation.Column;
                    _context.Update(existingReservation);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Reservations.Any(e => e.Id == reservation.Id))
                    {
                        return NotFound("Rezerwacja nie istnieje.");
                    }
                    throw;
                }
            }

            ViewBag.ErrorMessage = "Nie udało się zapisać zmian. Spróbuj ponownie.";
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Screening)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null || (!User.IsInRole("Administrator") && reservation.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return Forbid();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
