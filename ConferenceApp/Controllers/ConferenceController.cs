using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ConferenceApp.Data;
using ConferenceApp.Models;

namespace ConferenceApp.Controllers
{
    public class ConferenceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConferenceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Conference
        public async Task<IActionResult> Index()
        {
            return View(await _context.Conferences.ToListAsync());
        }

        // GET: Conference/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conference = await _context.Conferences
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conference == null)
            {
                return NotFound();
            }


            var ConferenceVersions = await _context.ConferenceVersions.Where(x => x.ConferenceId == conference.Id).ToListAsync();
            var ConferenceVersionsName = new List<object>();
            foreach (var ConferenceVersion in ConferenceVersions)
            {
                ConferenceVersionsName.Add(ConferenceVersion.Number);
            }

            var FeedbackCategories = await _context.FeedbackCategories.ToListAsync();

            var Assistance = new List<object>();
            var CategoryAverageLists = new List<object>();
            var FeedbackCategoryName = new List<object>();
            foreach (var Category in FeedbackCategories)
            {
                FeedbackCategoryName.Add(Category.Name);
            }


            foreach (var conferenceVersion in ConferenceVersions)
            {
                var events = await _context.Events.Where(x => x.ConferenceVersionId == conferenceVersion.Id).ToListAsync();
                var ConferenceVersionAssistance = await _context.Roles.Where(r => r.Name == "attendant" && events.Any(e => e.Id == r.EventId)).CountAsync();
                Assistance.Add(ConferenceVersionAssistance.ToString());

                var Feedbacks = await _context.Feedbacks.Where(f => events.Any(e => e.Id == f.EventId)).ToListAsync();

                var FeedbackAveragePerCategory = new List<object>();

                foreach (var Category in FeedbackCategories)
                {
                    var FeedbacksScopesOfEventAndCategory = await _context.FeedbackScopes.Where(fs => Feedbacks.Any(f => fs.FeedbackId == f.Id && fs.FeedbackCategoryId == Category.Id)).ToListAsync();

                    if (FeedbacksScopesOfEventAndCategory.Count() >= 1)
                    {
                        var avg = FeedbacksScopesOfEventAndCategory.Average(f => f.Grade);
                        FeedbackAveragePerCategory.Add(TruncateFunction(avg, 2).ToString());
                    }
                    else
                    {
                        FeedbackAveragePerCategory.Add("No hay");
                    }
                }
                CategoryAverageLists.Add(FeedbackAveragePerCategory);
            }

            ViewBag.ConferenceVersionsName = ConferenceVersionsName;
            ViewBag.FeedbackCategoryName = FeedbackCategoryName;
            ViewBag.Assitance = Assistance;
            ViewBag.CategoryAverageLists = CategoryAverageLists;

            var tagsCategoryAverageLists = new List<object>();
            var tags = await _context.Tags.ToListAsync();

            foreach (var t in tags)
            {
                var temp = new List<object>();
                foreach (var cv in ConferenceVersions)
                {
                    var eventTags = await _context.EventTags.ToListAsync();
                    var events = await _context.Events.Where(x => x.ConferenceVersionId == cv.Id && eventTags.Any(et => et.EventId == x.Id && et.TagId == t.Id)).ToListAsync();
                    var feedbacks = await _context.Feedbacks.Where(f => events.Any(e => e.Id == f.EventId)).ToListAsync();

                    var averagePerCategoryPerTag = new List<object>();

                    foreach (var category in FeedbackCategories)
                    {
                        var scopes = await _context.FeedbackScopes.Where(fs => feedbacks.Any(f => fs.FeedbackId == f.Id && fs.FeedbackCategoryId == category.Id)).ToListAsync();

                        if (scopes.Any())
                        {
                            var avg = scopes.Average(f => f.Grade);
                            averagePerCategoryPerTag.Add(TruncateFunction(avg, 2).ToString());
                        }
                        else
                        {
                            averagePerCategoryPerTag.Add("No hay");
                        }
                    }
                    temp.Add(averagePerCategoryPerTag);
                }
                tagsCategoryAverageLists.Add(temp);
            }

            ViewBag.TagsCategoryAverageLists = tagsCategoryAverageLists;

            var tagNames = new List<object>();
            foreach (var t in tags)
            {
                tagNames.Add(t.Name);
            }

            ViewBag.TagNames = tagNames;

            return View(conference);
        }

        // GET: Conference/Create
        public IActionResult Create()
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.currentUserId = currentUserId;
            return View();
        }

        // POST: Conference/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,OrganizerId")] Conference conference)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                conference.OrganizerId = currentUserId;
                _context.Add(conference);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(conference);
        }

        // GET: Conference/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conference = await _context.Conferences.FindAsync(id);
            if (conference == null)
            {
                return NotFound();
            }
            return View(conference);
        }

        // POST: Conference/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Conference conference)
        {
            if (id != conference.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(conference);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConferenceExists(conference.Id))
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
            return View(conference);
        }

        // GET: Conference/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conference = await _context.Conferences
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conference == null)
            {
                return NotFound();
            }

            return View(conference);
        }

        // POST: Conference/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conference = await _context.Conferences.FindAsync(id);
            _context.Conferences.Remove(conference);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConferenceExists(int id)
        {
            return _context.Conferences.Any(e => e.Id == id);
        }

        public double TruncateFunction(double number, int digits)
        {
            var stepper = (double)(Math.Pow(10.0, (double)digits));
            var temp = (int)(stepper * number);
            return (double)temp / stepper;
        }
    }
}
