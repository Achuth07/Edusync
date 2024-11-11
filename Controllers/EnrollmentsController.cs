using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Edusync.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Edusync.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EnrollmentsController : Controller
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<EnrollmentsController> _logger;

        public EnrollmentsController(SchoolManagementDbContext context, ILogger<EnrollmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("User {UserId} accessed the enrollment index page.", User?.Identity?.Name);
            var schoolManagementDbContext = _context.Enrollments.Include(e => e.Class).Include(e => e.Students);
            return View(await schoolManagementDbContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Class)
                .Include(e => e.Students)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment with ID {Id} not found for details view by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} viewed details of enrollment with ID {Id}.", User?.Identity?.Name, id);
            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            _logger.LogInformation("User {UserId} accessed create enrollment page.", User?.Identity?.Name);
            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Id");
            ViewData["StudentsId"] = new SelectList(_context.Students, "Id", "Id");
            return View();
        }

        // POST: Enrollments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentsId,ClassId,Grade")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Enrollment created with ID {Id} by User {UserId}.", enrollment.Id, User?.Identity?.Name);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Failed to create enrollment due to invalid model state by User {UserId}.", User?.Identity?.Name);
            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Id", enrollment.ClassId);
            ViewData["StudentsId"] = new SelectList(_context.Students, "Id", "Id", enrollment.StudentsId);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment with ID {Id} not found for edit by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} is editing enrollment with ID {Id}.", User?.Identity?.Name, id);
            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Id", enrollment.ClassId);
            ViewData["StudentsId"] = new SelectList(_context.Students, "Id", "Id", enrollment.StudentsId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentsId,ClassId,Grade")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                _logger.LogWarning("Enrollment ID mismatch in edit action by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Enrollment with ID {Id} updated by User {UserId}.", id, User?.Identity?.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        _logger.LogError("Concurrency error: Enrollment with ID {Id} no longer exists during update by User {UserId}.", id, User?.Identity?.Name);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogCritical("Unexpected error occurred during enrollment update for ID {Id} by User {UserId}.", id, User?.Identity?.Name);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Invalid model state for editing enrollment with ID {Id} by User {UserId}.", id, User?.Identity?.Name);
            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Id", enrollment.ClassId);
            ViewData["StudentsId"] = new SelectList(_context.Students, "Id", "Id", enrollment.StudentsId);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Class)
                .Include(e => e.Students)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment with ID {Id} not found for deletion by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} accessed delete confirmation for enrollment with ID {Id}.", User?.Identity?.Name, id);
            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                _logger.LogInformation("Enrollment with ID {Id} deleted by User {UserId}.", id, User?.Identity?.Name);
            }
            else
            {
                _logger.LogWarning("Attempted to delete nonexistent enrollment with ID {Id} by User {UserId}.", id, User?.Identity?.Name);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
    }
}
