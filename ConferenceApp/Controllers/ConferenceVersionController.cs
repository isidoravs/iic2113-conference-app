using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ConferenceApp.Data;
using ConferenceApp.Models;

namespace ConferenceApp.Controllers
{
    public class ConferenceVersionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConferenceVersionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ConferenceVersion
        public async Task<IActionResult> Index(int? conferenceId)
        {
            if (conferenceId == null) return View(await _context.ConferenceVersions.ToListAsync());
            ViewBag.conferenceId = conferenceId;
            return View(await _context.ConferenceVersions.Where(x => x.ConferenceId == conferenceId).ToListAsync());
        }

        // GET: ConferenceVersion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conferenceVersion = await _context.ConferenceVersions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conferenceVersion == null)
            {
                return NotFound();
            }
            
            var sponsorships = await _context.Sponsorships.Where(x => x.ConferenceVersionId == id).ToListAsync();
            var sponsors = new List<object>();
            foreach (var member in sponsorships)
            {
                var s = await _context.Sponsors.FindAsync(member.SponsorId);
                sponsors.Add(s.Name);
            }
            ViewBag.sponsors = sponsors;
            
            var events = await _context.Events.Where(x => x.ConferenceVersionId == id).ToListAsync();

            ViewBag.events = events;

            return View(conferenceVersion);
        }

        // GET: ConferenceVersion/Create
        public async Task<IActionResult> Create(int? conferenceId)
        {
            var conferences = conferenceId == null
                ? await _context.Conferences.ToListAsync()
                : await _context.Conferences.Where(x => x.Id == conferenceId).ToListAsync();
            ViewData["Conferences"] = new SelectList(conferences,"Id","Name");

            var eventCentres = await _context.EventCentres.ToListAsync();
            ViewData["eventCentres"] = new SelectList(eventCentres, "Id", "Name");
            return View();
        }

        // POST: ConferenceVersion/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Number,StartDate,EndDate,ConferenceId,EventCentreId")] ConferenceVersion conferenceVersion)
        {

            if (ModelState.IsValid)
            {
                _context.Add(conferenceVersion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(conferenceVersion);
        }

        // GET: ConferenceVersion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conferenceVersion = await _context.ConferenceVersions.FindAsync(id);
            if (conferenceVersion == null)
            {
                return NotFound();
            }
            var eventCentres = await _context.EventCentres.ToListAsync();
            ViewData["eventCentres"] = new SelectList(eventCentres, "Id", "Name");
            return View(conferenceVersion);
        }

        // POST: ConferenceVersion/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,StartDate,EndDate,ConferenceId,EventCentreId")] ConferenceVersion conferenceVersion)
        {

            if (id != conferenceVersion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(conferenceVersion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConferenceVersionExists(conferenceVersion.Id))
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
            return View(conferenceVersion);
        }

        // GET: ConferenceVersion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conferenceVersion = await _context.ConferenceVersions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conferenceVersion == null)
            {
                return NotFound();
            }

            return View(conferenceVersion);
        }

        // POST: ConferenceVersion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conferenceVersion = await _context.ConferenceVersions.FindAsync(id);
            _context.ConferenceVersions.Remove(conferenceVersion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConferenceVersionExists(int id)
        {
            return _context.ConferenceVersions.Any(e => e.Id == id);
        }
    }
}
