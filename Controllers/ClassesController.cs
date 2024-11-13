using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Edusync.Data;
using Edusync.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Edusync.Controllers
{
    public class ClassesController : Controller
    {
        private readonly SchoolManagementDbContext _context;
        private readonly INotyfService _notyfService;
        private readonly ILogger<ClassesController> _logger;

        public ClassesController(SchoolManagementDbContext context, INotyfService notyfService, ILogger<ClassesController> logger)
        {
            _context = context;
            _notyfService = notyfService;
            _logger = logger;
        }

        // GET: Classes
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("User {UserId} accessed the classes index page.", User?.Identity?.Name);
            var classes = _context.Classes.Include(q => q.Course).Include(q => q.Teachers);
            return View(await classes.ToListAsync());
        }

        // GET: Classes/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Teachers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@class == null)
            {
                _logger.LogWarning("Class with ID {Id} not found for details view by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} viewed details of class ID {Id}.", User?.Identity?.Name, id);
            return View(@class);
        }

        // GET: Classes/Create
        [Authorize(Roles = "Admin, Teacher")]
        public IActionResult Create()
        {
            _logger.LogInformation("User {UserId} accessed the create class page.", User?.Identity?.Name);
            CreateSelectLists();
            return View();
        }

        // POST: Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TeachersId,CourseId,Time")] Class @class)
        {
            if (@class.CourseId == null || @class.TeachersId == null)
            {
                _logger.LogWarning("Create action received invalid input by User {UserId}.", User?.Identity?.Name);
                ModelState.AddModelError(string.Empty, "Invalid input values for creating a class.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(@class);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Class created successfully by User {UserId}.", User?.Identity?.Name);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Failed to create class due to invalid model state by User {UserId}.", User?.Identity?.Name);
            CreateSelectLists();
            return View(@class);
        }

        // GET: Classes/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null)
            {
                _logger.LogWarning("Class with ID {Id} not found for editing by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} accessed edit page for class ID {Id}.", User?.Identity?.Name, id);
            CreateSelectLists();
            return View(@class);
        }

        // POST: Classes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeachersId,CourseId,Time")] Class @class)
        {
            if (id != @class.Id)
            {
                _logger.LogWarning("Class ID mismatch during edit action by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            if (@class.CourseId == null || @class.TeachersId == null)
            {
                _logger.LogWarning("Edit action received invalid input by User {UserId}.", User?.Identity?.Name);
                ModelState.AddModelError(string.Empty, "Invalid input values for editing a class.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Class with ID {Id} updated by User {UserId}.", id, User?.Identity?.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.Id))
                    {
                        _logger.LogError("Concurrency error: Class with ID {Id} does not exist during update by User {UserId}.", id, User?.Identity?.Name);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogCritical("Unexpected error occurred during class update for ID {Id} by User {UserId}.", id, User?.Identity?.Name);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Invalid model state for editing class with ID {Id} by User {UserId}.", id, User?.Identity?.Name);
            CreateSelectLists();
            return View(@class);
        }

        // GET: Classes/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete action requested without ID by User {UserId}.", User?.Identity?.Name);
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Teachers)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                _logger.LogWarning("Class with ID {Id} not found for deletion by User {UserId}.", id, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} accessed delete confirmation for class ID {Id}.", User?.Identity?.Name, id);
            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class != null)
            {
                _context.Classes.Remove(@class);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Class with ID {Id} deleted by User {UserId}.", id, User?.Identity?.Name);
            }
            else
            {
                _logger.LogWarning("Attempted to delete nonexistent class with ID {Id} by User {UserId}.", id, User?.Identity?.Name);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ManageEnrollments(int classId)
        {
            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Teachers)
                .Include(q => q.Enrollments)
                    .ThenInclude(q => q.Students)
                .FirstOrDefaultAsync(m => m.Id == classId);

            if (@class == null)
            {
                _logger.LogWarning("Class with ID {classId} not found for enrollment management by User {UserId}.", classId, User?.Identity?.Name);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} is managing enrollments for class ID {classId}.", User?.Identity?.Name, classId);
            
            var students = await _context.Students.ToListAsync();

            var model = new ClassEnrollmentViewModel
            {
                Class = new ClassViewModel
                {
                    Id = @class.Id,
                    CourseName = $"{@class.Course.Code} - {@class.Course.Name}",
                    TeacherName = $"{@class.Teachers.FirstName} {@class.Teachers.LastName}",
                    Time = @class.Time.ToString()
                },
                Students = students.Select(stu => new StudentEnrollmentViewModel
                {
                    Id = stu.Id,
                    FirstName = stu.FirstName,
                    LastName = stu.LastName,
                    IsEnrolled = @class.Enrollments.Any(q => q.StudentsId == stu.Id)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EnrollStudent(int classId, int studentId, bool shouldEnroll)
        {
            if (shouldEnroll)
            {
                var enrollment = new Enrollment { ClassId = classId, StudentsId = studentId };
                await _context.AddAsync(enrollment);
                _notyfService.Success("Student enrolled successfully.");
                _logger.LogInformation("Student ID {studentId} enrolled in class ID {classId} by User {UserId}.", studentId, classId, User?.Identity?.Name);
            }
            else
            {
                var enrollment = await _context.Enrollments.FirstOrDefaultAsync(
                    q => q.ClassId == classId && q.StudentsId == studentId);

                if (enrollment != null)
                {
                    _context.Remove(enrollment);
                    _notyfService.Warning("Student unenrolled successfully.");
                    _logger.LogInformation("Student ID {studentId} unenrolled from class ID {classId} by User {UserId}.", studentId, classId, User?.Identity?.Name);
                }
                else
                {
                    _logger.LogWarning("Attempted to unenroll nonexistent enrollment for student ID {studentId} from class {classId} by User {UserId}.", studentId, classId, User?.Identity?.Name);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageEnrollments), new { classId });
        }

        [Authorize(Roles = "Admin, Teacher, Student")]
        public async Task<IActionResult> ViewOnly()
        {
            _logger.LogInformation("User {UserId} accessed the view-only classes page.", User?.Identity?.Name);
            var classes = _context.Classes.Include(q => q.Course).Include(q => q.Teachers);
            return View(await classes.ToListAsync());
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }

        private void CreateSelectLists()
        {
            var courses = _context.Courses.Select(q => new
            {
                CourseName = $"{q.Code} - {q.Name} ({q.Credits} Credits)",
                q.Id
            });

            ViewData["CourseId"] = new SelectList(courses, "Id", "CourseName");

            var teachers = _context.Teachers.Select(q => new
            {
                Fullname = $"{q.FirstName} {q.LastName}",
                q.Id
            });
            ViewData["TeachersId"] = new SelectList(teachers, "Id", "Fullname");
        }
    }
}
