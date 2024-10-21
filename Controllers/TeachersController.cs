using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Edusync.Data;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace Edusync.Controllers
{
    public class TeachersController : Controller
    {
        private readonly SchoolManagementDbContext _context;
        private readonly INotyfService _notyfService;

        public TeachersController(SchoolManagementDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Teachers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Teachers.ToListAsync());
        }

        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName")] Teacher teacher)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id))
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
            return View(teacher);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

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
                    return NotFound();
                }

                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                // Returning success response for AJAX
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                // Checking for foreign key constraint error
                if (ex.InnerException != null && ex.InnerException.Message.Contains("The DELETE statement conflicted with the REFERENCE constraint"))
                {
                    // Trigger Notyf error on the server-side
                    _notyfService.Error("Teacher has been assigned Classes in the timetable. Please remove teacher from the timetable before deleting teacher records.");

                    // Return error response to AJAX
                    return Json(new { success = false, message = "Teacher has been assigned Classes in the timetable. Please remove teacher from the timetable before deleting teacher records." });
                }
                else
                {
                    // Return a generic error message
                    return Json(new { success = false, message = "An error occurred while deleting the teacher." });
                }
            }
        }



        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }
    }
}
