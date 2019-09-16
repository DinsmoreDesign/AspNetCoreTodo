using System;
using System.Threading.Tasks;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreTodo.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ITodoItemService _todoItemService;
        private readonly UserManager<IdentityUser> _userManager;
        
        public TodoController(ITodoItemService todoItemService, UserManager<IdentityUser> userManager)
        {
            _todoItemService = todoItemService;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var items = await _todoItemService.GetIncompleteItemsAsync(currentUser);
            
            var model = new TodoViewModel()
            {
                Items = items
            };

            return View(model);
        }

        [HttpPost]
        [Route("AddItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(TodoItem newItem)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var successful = await _todoItemService.AddItemAsync(newItem, currentUser);

            if (!successful)
            {
                return BadRequest("Could not add item.");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("MarkDone")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDone(Guid id)
        {
            if (id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var successful = await _todoItemService.MarkDoneAsync(id, currentUser);

            if (!successful)
            {
                return BadRequest("Could not mark item as done.");
            }

            return RedirectToAction("Index");
        }
    }
}