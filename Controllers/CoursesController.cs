using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Edusync.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Edusync.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CoursesController : Controller
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(SchoolManagementDbContext context, ILogger<CoursesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("User {UserId} accessed the course index page.", User?.Identity?.Name);
            return View(await _context.Courses.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                _logger.LogWarning("Course with ID {Id} not found for details view by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} viewed details of course with ID {Id}.", User?.Identity?.Name, id);
            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            _logger.LogInformation("User {UserId} accessed create course page.", User?.Identity?.Name);
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Code,Credits")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Course created with ID {Id} by User {UserId}.", course.Id, User?.Identity?.Name);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Failed to create course due to invalid model state by User {UserId}.", User?.Identity?.Name);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                _logger.LogWarning("Course with ID {Id} not found for edit by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} is editing course with ID {Id}.", User?.Identity?.Name, id);
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,Credits")] Course course)
        {
            if (id != course.Id)
            {
                _logger.LogWarning("Course ID mismatch in edit action by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Course with ID {Id} updated by User {UserId}.", id, User?.Identity?.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        _logger.LogError("Concurrency error: Course with ID {Id} no longer exists during update by User {UserId}.", id, User?.Identity?.Name);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogCritical("Unexpected error occurred during course update for ID {Id} by User {UserId}.", id, User?.Identity?.Name);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Invalid model state for editing course with ID {Id} by User {UserId}.", id, User?.Identity?.Name);
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                _logger.LogWarning("Course with ID {Id} not found for deletion by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} accessed delete confirmation for course with ID {Id}.", User?.Identity?.Name, id);
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                _logger.LogInformation("Course with ID {Id} deleted by User {UserId}.", id, User?.Identity?.Name);
            }
            else
            {
                _logger.LogWarning("Attempted to delete nonexistent course with ID {Id} by User {UserId}.", id, User?.Identity?.Name);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        internal bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
