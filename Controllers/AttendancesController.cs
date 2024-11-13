using System;
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
    public class AttendancesController : Controller
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<AttendancesController> _logger;

        public AttendancesController(SchoolManagementDbContext context, ILogger<AttendancesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Attendances
        [Authorize]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("User {UserId} accessed the attendance index page.", User?.Identity?.Name);
            var attendances = _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .ThenInclude(c => c.Course);
            return View(await attendances.ToListAsync());
        }

        // GET: Attendances/Create
        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            _logger.LogInformation("User {UserId} accessed the create attendance page.", User?.Identity?.Name);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName");
            ViewData["ClassId"] = new SelectList(_context.Classes.Include(c => c.Course), "Id", "Course.Name");
            return View();
        }

        // POST: Attendances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,ClassId,Date,Status")] Attendance attendance)
        {
            // Validate input to prevent over-posting attacks
            if (attendance.StudentId <= 0 || attendance.ClassId <= 0 || string.IsNullOrWhiteSpace(attendance.Status))
            {
                _logger.LogWarning("Invalid input received in Create action by User {UserId}.", User?.Identity?.Name);
                ModelState.AddModelError(string.Empty, "Invalid input values provided.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(attendance);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Attendance created for StudentId {StudentId} on {Date} by User {UserId}.", attendance.StudentId, attendance.Date, User?.Identity?.Name);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Failed to create attendance record due to invalid model state by User {UserId}.", User?.Identity?.Name);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", attendance.StudentId);
            ViewData["ClassId"] = new SelectList(_context.Classes.Include(c => c.Course), "Id", "Course.Name", attendance.ClassId);
            return View(attendance);
        }

        // GET: Attendances/Edit/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
            {
                _logger.LogWarning("Attendance record with ID {Id} not found for edit by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} is editing attendance record with ID {Id}.", User?.Identity?.Name, id);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", attendance.StudentId);
            ViewData["ClassId"] = new SelectList(_context.Classes.Include(c => c.Course), "Id", "Course.Name", attendance.ClassId);
            return View(attendance);
        }

        // POST: Attendances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentId,ClassId,Date,Status")] Attendance attendance)
        {
            if (id != attendance.Id)
            {
                _logger.LogWarning("Attendance ID mismatch in edit action by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            if (attendance.StudentId <= 0 || attendance.ClassId <= 0 || string.IsNullOrWhiteSpace(attendance.Status))
            {
                _logger.LogWarning("Invalid input detected in Edit action for attendance ID {Id} by User {UserId}.", id, User?.Identity?.Name);
                ModelState.AddModelError(string.Empty, "Invalid input values provided.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attendance);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Attendance record with ID {Id} updated by User {UserId}.", id, User?.Identity?.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceExists(attendance.Id))
                    {
                        _logger.LogError("Concurrency error: Attendance record with ID {Id} no longer exists during update by User {UserId}.", id, User?.Identity?.Name);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogCritical("Unexpected error occurred during attendance record update for ID {Id} by User {UserId}.", id, User?.Identity?.Name);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Invalid model state for editing attendance record with ID {Id} by User {UserId}.", id, User?.Identity?.Name);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", attendance.StudentId);
            ViewData["ClassId"] = new SelectList(_context.Classes.Include(c => c.Course), "Id", "Course.Name", attendance.ClassId);
            return View(attendance);
        }

        // GET: Attendances/Delete/5
        [Authorize(Roles = "Admin, Teacher")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendance == null)
            {
                _logger.LogWarning("Attendance record with ID {Id} not found for deletion by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} accessed delete confirmation for attendance record with ID {Id}.", User?.Identity?.Name, id);
            return View(attendance);
        }

        // POST: Attendances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance != null)
            {
                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Attendance record with ID {Id} deleted by User {UserId}.", id, User?.Identity?.Name);
            }
            else
            {
                _logger.LogWarning("Attempted to delete nonexistent attendance record with ID {Id} by User {UserId}.", id, User?.Identity?.Name);
            }
            return RedirectToAction(nameof(Index));
        }

        // ViewAttendance
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ViewOnly()
        {
            _logger.LogInformation("User {UserId} viewed attendance records.", User?.Identity?.Name);
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .ThenInclude(c => c.Course)
                .ToListAsync();

            return View(attendances);
        }

        private bool AttendanceExists(int id)
        {
            return _context.Attendances.Any(e => e.Id == id);
        }
    }
}
