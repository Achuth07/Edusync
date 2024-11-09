using Edusync.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Edusync.Controllers
{
    public class GradesController : Controller
    {
        private readonly SchoolManagementDbContext _context;

        public GradesController(SchoolManagementDbContext context)
        {
            _context = context;
        }

        // GET: Grades/Index
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Include the related Student and Class data
            var grades = await _context.Grades
                .Include(g => g.Student) // Eager load the Student entity
                .Include(g => g.Class) // Eager load the Class entity
                    .ThenInclude(c => c.Course) // Ensure the Course related to Class is loaded
                .ToListAsync();

            return View(grades);
        }


        // GET: Grades/Create
        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
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
                // Handle case where there's no data, redirect or show an error message
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
            if (ModelState.IsValid)
            {
                _context.Add(grade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Class)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null)
            {
                return NotFound();
            }

            // Populate the SelectList for the dropdowns
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
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradeExists(grade.Id))
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

            // Repopulate the dropdowns in case of failure
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
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Class)
                .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (grade == null)
            {
                return NotFound();
            }

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
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        //View Only Grades Page
        //[AllowAnonymous]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ViewOnly()
        {
            var grades = await _context.Grades
                .Include(g => g.Student)   // To load student details
                .Include(g => g.Class)     // To load class details
                .ThenInclude(c => c.Course)  // To load course details (if needed)
                .ToListAsync();

            return View(grades);
        }

        private bool GradeExists(int id)
        {
            return _context.Grades.Any(e => e.Id == id);
        }
    }
}
