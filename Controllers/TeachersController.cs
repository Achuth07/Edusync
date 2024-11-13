using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Edusync.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Edusync.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeachersController : Controller
    {
        private readonly SchoolManagementDbContext _context;
        private readonly INotyfService _notyfService;
        private readonly ILogger<TeachersController> _logger;

        public TeachersController(SchoolManagementDbContext context, INotyfService notyfService, ILogger<TeachersController> logger)
        {
            _context = context;
            _notyfService = notyfService;
            _logger = logger;
        }

        // GET: Teachers
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin accessed the Teachers list.");
            return View(await _context.Teachers.ToListAsync());
        }

        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Attempted to access Teacher details with a null ID.");
                return NotFound();
            }

            var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher with ID {Id} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Admin accessed details for Teacher ID {Id}.", id);
            return View(teacher);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            _logger.LogInformation("Admin navigated to Create Teacher page.");
            return View();
        }

        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin created a new Teacher with ID {Id}.", teacher.Id);
                _notyfService.Success("Teacher created successfully.");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Failed to create Teacher due to model state errors.");
            _notyfService.Error("Failed to create Teacher. Please check the input values.");
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Attempted to edit Teacher with a null ID.");
                return NotFound();
            }

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher with ID {Id} not found for editing.", id);
                return NotFound();
            }

            _logger.LogInformation("Admin accessed Edit page for Teacher ID {Id}.", id);
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName")] Teacher teacher)
        {
            if (id != teacher.Id)
            {
                _logger.LogWarning("Teacher ID mismatch for editing. Provided ID: {ProvidedId}, Teacher ID: {TeacherId}.", id, teacher.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Admin updated Teacher with ID {Id}.", teacher.Id);
                    _notyfService.Success("Teacher updated successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id))
                    {
                        _logger.LogWarning("Teacher with ID {Id} not found during update attempt.", teacher.Id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("Concurrency error occurred while updating Teacher with ID {Id}.", teacher.Id);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Failed to update Teacher with ID {Id} due to model state errors.", teacher.Id);
            _notyfService.Error("Failed to update Teacher. Please check the input values.");
            return View(teacher);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Attempted to delete Teacher with a null ID.");
                return NotFound();
            }

            var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher with ID {Id} not found for deletion.", id);
                return NotFound();
            }

            _logger.LogInformation("Admin accessed Delete page for Teacher ID {Id}.", id);
            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var teacher = await _context.Teachers.FindAsync(id);
                if (teacher == null)
                {
                    _logger.LogWarning("Teacher with ID {Id} not found when attempting to delete.", id);
                    return NotFound();
                }

                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin deleted Teacher with ID {Id}.", id);
                _notyfService.Success("Teacher deleted successfully.");

                // Return JSON success response for AJAX
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                // Check for foreign key constraint error
                if (ex.InnerException != null && ex.InnerException.Message.Contains("REFERENCE constraint"))
                {
                    _notyfService.Error("Teacher has assigned classes. Remove teacher from timetable before deletion.");
                    _logger.LogWarning("Failed to delete Teacher ID {Id} due to foreign key constraint.", id);

                    // Return JSON error response for AJAX
                    return Json(new { success = false, message = "Teacher has assigned classes. Remove from timetable before deletion." });
                }
                else
                {
                    _logger.LogError("Unexpected error occurred while deleting Teacher with ID {Id}.", id);
                    // Return JSON generic error message
                    return Json(new { success = false, message = "An unexpected error occurred while deleting the teacher." });
                }
            }
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }
    }
}
