using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NotesProject.Data;
using NotesProject.Models;
using NotesProject.ViewModels;

namespace NotesProject.Controllers
{
    [Authorize] // this makes sure the user is logged in
    public class NotesController : Controller
    {
        private readonly AppDbContext _context;

        // returns the logged in User's ID for the identification and filtering of notes for that user
        private string GetUserId()
        {
            return HttpContext.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        }

        public NotesController(AppDbContext context)
        {
            _context = context;
        }

        // Returns the list of active notes for logged in user
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allNotes = await _context.Notes.Where(x => x.UserId == GetUserId() && x.IsActive).OrderByDescending(x => x.CreatedOn).ToListAsync();
            return View(allNotes);
        }

        // returns the view (UI page) for create operation
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // handles the post request for create page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNoteViewModel note)
        {
            if (ModelState.IsValid)
            {
                Note n = new Note
                {
                    UserId = GetUserId(),
                    IsActive = true,
                    Title = note.Title,
                    Content = note.Content,
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now,
                    LastAccessedOn = DateTime.Now
                };
                _context.Add(n);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
           
            return View(note);
        }


        // returns the view (UI page) for edit operation
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            // update the access date before returning view
            note.LastAccessedOn = DateTime.Now;
            _context.Update(note);
            await _context.SaveChangesAsync();

            return View(new UpdateNoteViewModel
            {
                Id = note.Id,
                Content = note.Content,
                Title = note.Title,
            });
        }

        // handles the post for edit operation

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateNoteViewModel note)
        {
            if (id != note.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // i try to fetcht he note by id from db, if it does not exist then throw error, otherwise update the data
                    Note noteFromDb = await _context.Notes.FindAsync(note.Id);
                    if(noteFromDb == null)
                    {
                        return NotFound();
                    }

                    noteFromDb.Title = note.Title;
                    noteFromDb.Content = note.Content;
                    noteFromDb.ModifiedOn = DateTime.Now;

                    _context.Update(noteFromDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteExists(note.Id))
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
         
            return View(note);
        }

        // fetches a note by ID for delete view
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == GetUserId());
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // delete operation handler
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(x => x.Id== id && x.UserId == GetUserId());
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(RecycleBin));
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> MoveToRecycleBin(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == GetUserId());
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        [HttpPost, ActionName("MoveToRecycleBin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToRecycleBinConfirm(int id)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId());
            note.IsActive = false;
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> RecycleBin()
        {
            var allNonActiveNotes = await _context.Notes.Where(x => x.UserId == GetUserId() && x.IsActive == false).OrderByDescending(x => x.ModifiedOn).ToListAsync();
            return View(allNonActiveNotes);
        }

        [HttpPost, ActionName("Recycle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recycle(int id)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == GetUserId());
            if(note != null)
            {
                note.IsActive = true;
                _context.Notes.Update(note);
                await _context.SaveChangesAsync();
            }
           
            return RedirectToAction(nameof(Index));
        }

    }
}
