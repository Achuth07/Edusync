using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Edusync.Data;
using Microsoft.AspNetCore.Authorization;

namespace Edusync.Controllers
{
    public class AttendancesController : Controller
    {
        private readonly SchoolManagementDbContext _context;

        public AttendancesController(SchoolManagementDbContext context)
        {
            _context = context;
        }

        // GET: Attendances
        [Authorize]
        public async Task<IActionResult> Index()
        {
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
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName");
            ViewData["ClassId"] = new SelectList(_context.Classes.Include(c => c.Course), "Id", "Course.Name");
            return View();
        }

        // POST: Attendances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,ClassId,Date,Status")] Attendance attendance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(attendance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
                return NotFound();
            }

            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
            {
                return NotFound();
            }
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
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attendance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceExists(attendance.Id))
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
                return NotFound();
            }

            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Class)
                .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendance == null)
            {
                return NotFound();
            }

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
            }
            return RedirectToAction(nameof(Index));
        }

        //ViewAttendance
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ViewOnly()
        {
            var attendances = await _context.Attendances
                .Include(a => a.Student)  // To get student details
                .Include(a => a.Class)    // To get class details
                .ThenInclude(c => c.Course)  // To get course details (if needed)
                .ToListAsync();

            return View(attendances);
        }

        private bool AttendanceExists(int id)
        {
            return _context.Attendances.Any(e => e.Id == id);
        }
    }
}
