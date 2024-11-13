using Edusync.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Edusync.Controllers
{
    public class GradesController : Controller
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<GradesController> _logger;

        public GradesController(SchoolManagementDbContext context, ILogger<GradesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Grades/Index
        [Authorize]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("User {UserId} accessed the grades index page.", User?.Identity?.Name);

            var grades = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Class)
                .ThenInclude(c => c.Course)
                .ToListAsync();

            return View(grades);
        }

        // GET: Grades/Create
        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            _logger.LogInformation("User {UserId} accessed the create grade page.", User?.Identity?.Name);

            var students = _context.Students.Select(s => new 
            {
                s.Id,
                FullName = s.FirstName + " " + s.LastName
            }).ToList();

            var classes = _context.Classes.Select(c => new 
            {
                c.Id,
                CourseName = c.Course.Code + " - " + c.Course.Name
            }).ToList();

            if (!students.Any() || !classes.Any())
            {
                _logger.LogWarning("No students or classes available for creating a grade. Redirecting to index page.");
                TempData["ErrorMessage"] = "No Students or Classes available to create a grade!";
                return RedirectToAction("Index");
            }

            ViewBag.StudentId = new SelectList(students, "Id", "FullName");
            ViewBag.ClassId = new SelectList(classes, "Id", "CourseName");

            return View();
        }

        // POST: Grades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentId,ClassId,AssessmentType,Score,AcademicYear")] Grade grade)
        {
            // Server-side validation
            if (!await _context.Students.AnyAsync(s => s.Id == grade.StudentId))
            {
                ModelState.AddModelError("StudentId", "Selected student does not exist.");
                _logger.LogWarning("Invalid student ID {StudentId} provided by User {UserId}.", grade.StudentId, User?.Identity?.Name);
            }
            if (!await _context.Classes.AnyAsync(c => c.Id == grade.ClassId))
            {
                ModelState.AddModelError("ClassId", "Selected class does not exist.");
                _logger.LogWarning("Invalid class ID {ClassId} provided by User {UserId}.", grade.ClassId, User?.Identity?.Name);
            }

            if (ModelState.IsValid)
            {
                _context.Add(grade);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Grade created for student ID {StudentId} in class ID {ClassId} by User {UserId}.", grade.StudentId, grade.ClassId, User?.Identity?.Name);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Failed to create grade due to invalid model state by User {UserId}.", User?.Identity?.Name);
            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "Id", grade.ClassId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", grade.StudentId);
            return View(grade);
        }

        // GET: Grades/Edit/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Class)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null)
            {
                _logger.LogWarning("Grade with ID {Id} not found for edit by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} is editing grade with ID {Id}.", User?.Identity?.Name, id);

            ViewData["ClassId"] = new SelectList(_context.Classes.Select(c => new 
            {
                c.Id,
                CourseName = c.Course.Code + " - " + c.Course.Name
            }).ToList(), "Id", "CourseName", grade.ClassId);

            ViewData["StudentId"] = new SelectList(_context.Students.Select(s => new 
            {
                s.Id,
                FullName = s.FirstName + " " + s.LastName
            }).ToList(), "Id", "FullName", grade.StudentId);

            return View(grade);
        }

        // POST: Grades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentId,ClassId,AssessmentType,Score,AcademicYear")] Grade grade)
        {
            if (id != grade.Id)
            {
                _logger.LogWarning("Grade ID mismatch in edit action by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            // Server-side validation
            if (!await _context.Students.AnyAsync(s => s.Id == grade.StudentId))
            {
                ModelState.AddModelError("StudentId", "Selected student does not exist.");
                _logger.LogWarning("Invalid student ID {StudentId} provided by User {UserId} during edit.", grade.StudentId, User?.Identity?.Name);
            }
            if (!await _context.Classes.AnyAsync(c => c.Id == grade.ClassId))
            {
                ModelState.AddModelError("ClassId", "Selected class does not exist.");
                _logger.LogWarning("Invalid class ID {ClassId} provided by User {UserId} during edit.", grade.ClassId, User?.Identity?.Name);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grade);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Grade with ID {Id} updated by User {UserId}.", id, User?.Identity?.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradeExists(grade.Id))
                    {
                        _logger.LogError("Concurrency error: Grade with ID {Id} no longer exists during update by User {UserId}.", id, User?.Identity?.Name);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogCritical("Unexpected error occurred during grade update for ID {Id} by User {UserId}.", id, User?.Identity?.Name);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Invalid model state for editing grade with ID {Id} by User {UserId}.", id, User?.Identity?.Name);
            ViewData["ClassId"] = new SelectList(_context.Classes, "Id", "CourseName", grade.ClassId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", grade.StudentId);
            return View(grade);
        }

        // GET: Grades/Delete/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Grades == null)
            {
                _logger.LogWarning("Delete action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Class)
                .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (grade == null)
            {
                _logger.LogWarning("Grade with ID {Id} not found for deletion by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} accessed delete confirmation for grade with ID {Id}.", User?.Identity?.Name, id);
            return View(grade);
        }

        // POST: Grades/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade != null)
            {
                _context.Grades.Remove(grade);
                _logger.LogInformation("Grade with ID {Id} deleted by User {UserId}.", id, User?.Identity?.Name);
            }
            else
            {
                _logger.LogWarning("Attempted to delete nonexistent grade with ID {Id} by User {UserId}.", id, User?.Identity?.Name);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // View Only Grades Page
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ViewOnly()
        {
            _logger.LogInformation("User {UserId} accessed the grades view-only page.", User?.Identity?.Name);

            var grades = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Class)
                .ThenInclude(c => c.Course)
                .ToListAsync();

            return View(grades);
        }

        private bool GradeExists(int id)
        {
            return _context.Grades.Any(e => e.Id == id);
        }
    }
}
