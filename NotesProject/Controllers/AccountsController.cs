using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotesProject.Models;
using NotesProject.ViewModels;
using System.Threading.Tasks;

namespace NotesProject.Controllers
{
    public class AccountsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userByEmail = await _userManager.FindByEmailAsync(model.Email);
                if(userByEmail == null)
                {
                    ModelState.AddModelError("", "Invalid Username");
                    return View(model);
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(userByEmail, model.Password);
                if (!isPasswordValid)
                {
                    ModelState.AddModelError("", "Invalid Password");
                    return View(model);
                }

                await _signInManager.SignInAsync(userByEmail, true);
                return RedirectToAction("Index", "Home");

            }

            return View(model);
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task< IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Email
                };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var e in result.Errors)
                    {
                        ModelState.AddModelError("", e.Description);
                    }
                    return View(model);
                }

                await _signInManager.SignInAsync(user, true);
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
