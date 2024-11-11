using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Edusync.Data;
using Edusync.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Edusync.Controllers
{
    public class StudentsController : Controller
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(SchoolManagementDbContext context, ILogger<StudentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Students
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin accessed the Students list.");
            return View(await _context.Students.ToListAsync());
        }

        // GET: Students/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Attempted to access Student details with a null ID.");
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {Id} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Admin accessed details for Student ID {Id}.", id);
            return View(student);
        }

        // GET: Students/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            _logger.LogInformation("Admin navigated to Create Student page.");
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,DateOfBirth")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin created a new Student with ID {Id}.", student.Id);
                return RedirectToAction(nameof(Index));
            }
            _logger.LogWarning("Failed to create Student due to model state errors.");
            return View(student);
        }

        // GET: Students/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Attempted to edit Student with a null ID.");
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {Id} not found for editing.", id);
                return NotFound();
            }

            _logger.LogInformation("Admin accessed Edit page for Student ID {Id}.", id);
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,DateOfBirth")] Student student)
        {
            if (id != student.Id)
            {
                _logger.LogWarning("Student ID mismatch for editing. Provided ID: {ProvidedId}, Student ID: {StudentId}.", id, student.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Admin updated Student with ID {Id}.", student.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        _logger.LogWarning("Student with ID {Id} not found during update attempt.", student.Id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("Concurrency error occurred while updating Student with ID {Id}.", student.Id);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Failed to update Student with ID {Id} due to model state errors.", student.Id);
            return View(student);
        }

        // GET: Students/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Attempted to delete Student with a null ID.");
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {Id} not found for deletion.", id);
                return NotFound();
            }

            _logger.LogInformation("Admin accessed Delete page for Student ID {Id}.", id);
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin deleted Student with ID {Id}.", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete Student with ID {Id}, but student not found.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AcademicProgress(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {Id} not found while accessing Academic Progress.", studentId);
                return NotFound();
            }

            var grades = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Class)
                .ThenInclude(c => c.Course)
                .Where(g => g.StudentId == studentId)
                .ToListAsync();

            _logger.LogInformation("Admin accessed Academic Progress for Student ID {Id}.", studentId);

            var model = new AcademicProgressViewModel
            {
                StudentId = studentId,
                StudentName = $"{student.FirstName} {student.LastName}",
                CourseGrades = grades.Select(g => new CourseGradeViewModel
                {
                    CourseName = g.Class.Course.Name,
                    AssessmentType = g.AssessmentType,
                    Score = g.Score,
                    DateRecorded = g.DateRecorded
                }).ToList()
            };

            return View(model);
        }

        [Authorize(Roles = "Student, Teacher")]
        public async Task<IActionResult> ViewOnly()
        {
            _logger.LogInformation("User with role Student or Teacher accessed the Students ViewOnly page.");
            var students = await _context.Students.ToListAsync();
            return View(students);
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
