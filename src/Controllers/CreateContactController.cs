using Auth0Mvc.DAL;
using Auth0Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Auth0Mvc.Helpers;

namespace Auth0Mvc.Controllers;

public class CreateContactController(ContactsService contactsService) : Controller
{
    private readonly ContactsService _contactsService = contactsService;

    // GET: CreateContactController
    public ActionResult Index()
    {
        return View();
    }

    // GET: CreateContactController/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: CreateContactController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IFormCollection collection)
    {
        try
        {
            var ownerId = User?.GetUserId();

            var displayName = collection[nameof(Contact.DisplayName)];
            var userId = collection[nameof(Contact.UserId)];

            await _contactsService.AddContactToListAsync(ownerId, new Contact 
            { 
                DisplayName = displayName,
                UserId = userId
            });

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }
}
