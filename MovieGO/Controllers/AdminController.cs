using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieGO.Data;
using MovieGO.Models;

namespace MovieGO.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Index
        public async Task<IActionResult> Index(int? cinemaId)
        {
            // Pobranie seansów
            var screeningsQuery = _context.Screenings
                .Include(s => s.Hall)
                .ThenInclude(h => h.Cinema)
                .Include(s => s.Reservations)
                .Include(s => s.Movie)
                .AsQueryable();

            if (cinemaId.HasValue)
            {
                screeningsQuery = screeningsQuery.Where(s => s.Hall.CinemaId == cinemaId);
            }

            var screenings = await screeningsQuery.ToListAsync();

            // Statystyki
            var totalReservedSeats = screenings.Sum(s => s.Reservations.Count);
            var totalSeats = screenings.Sum(s => s.Hall.RowCount * s.Hall.ColumnCount);

            var mostPopularMovie = screenings
                .GroupBy(s => s.Movie)
                .OrderByDescending(g => g.Sum(s => s.Reservations.Count))
                .Select(g => new
                {
                    Movie = g.Key,
                    Reservations = g.Sum(s => s.Reservations.Count)
                })
                .FirstOrDefault();

            var mostPopularCinema = screenings
                .GroupBy(s => s.Hall.Cinema)
                .OrderByDescending(g => g.Sum(s => s.Reservations.Count))
                .Select(g => g.Key)
                .FirstOrDefault();

            // Dane do widoku
            ViewData["TotalReservedSeats"] = totalReservedSeats;
            ViewData["TotalSeats"] = totalSeats;
            ViewData["MostPopularMovie"] = mostPopularMovie?.Movie?.Title ?? "Brak";
            ViewData["MostPopularMovieReservations"] = mostPopularMovie?.Reservations ?? 0;
            ViewData["MostPopularCinema"] = mostPopularCinema?.Name ?? "Brak / Nie dotyczy";
            ViewData["Cinemas"] = new SelectList(await _context.Cinemas.ToListAsync(), "Id", "Name");

            return View();
        }

        // Użytkownicy - lista
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "Brak roli";
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            ViewBag.AllRoles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string email, string password, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError(string.Empty, "Wszystkie pola są wymagane.");
                ViewBag.AllRoles = new SelectList(_roleManager.Roles, "Name", "Name");
                return View();
            }

            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.AllRoles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View();
        }

        // GET: Admin/EditUser
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.AllRoles = new SelectList(roles, "Name", "Name");
            ViewBag.UserRoles = userRoles;

            return View(user);
        }

        // POST: Admin/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!string.IsNullOrEmpty(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Users));
            }

            ModelState.AddModelError(string.Empty, "Nie udało się usunąć użytkownika.");
            return RedirectToAction(nameof(Users));
        }

        // Role - lista
        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // GET: Admin/CreateRole
        public IActionResult CreateRole()
        {
            return View();
        }

        // POST: Admin/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
            return RedirectToAction(nameof(Roles));
        }

        // GET: Admin/EditRole
        public async Task<IActionResult> EditRole(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null || role.Name == "Administrator") return NotFound();

            return View(role);
        }

        // POST: Admin/EditRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string id, string newName)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null || role.Name == "Administrator") return NotFound();

            role.Name = newName;
            await _roleManager.UpdateAsync(role);

            return RedirectToAction(nameof(Roles));
        }

        // POST: Admin/DeleteRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null || role.Name == "Administrator") return NotFound();

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Roles));
            }

            ModelState.AddModelError(string.Empty, "Nie udało się usunąć roli.");
            return RedirectToAction(nameof(Roles));
        }

        //POST: Usunięcie roli

    }
}
