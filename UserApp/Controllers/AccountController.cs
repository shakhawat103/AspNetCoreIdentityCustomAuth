
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserApp.Models;
using UserApp.ViewModels;

namespace UserApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> RoleManager;
        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, RoleManager<IdentityRole> RoleManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.RoleManager = RoleManager;
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
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users user = new Users
                {
                   
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.Name,
                    NormalizedUserName = model.Email.ToUpper(),
                    NormalizedEmail = model.Email.ToUpper() 

                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var roleExist= await RoleManager.RoleExistsAsync("User");
                    if (!roleExist)
                    {
                        var role= new IdentityRole("User");
                        await RoleManager.CreateAsync(role);
                    }
                    await userManager.AddToRoleAsync(user, "User");
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Login", "Account");
                }
               
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
               
            }

            return View(model);

        }



        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user= await userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Here you would typically generate a token and send it via email.
                    // For simplicity, we will just redirect to ChangePassword view.
                    return RedirectToAction("ChangePassword", "Account", new {username= user.Email});
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email not found");
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult ChangePassword(string username)
        {
            if(string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }

            return View(new ChangePasswordViewModel { Email=username});
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid) 
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result= await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        var addPassResult= await userManager.AddPasswordAsync(user, model.NewPassword);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found");
                    return View(model);
                }

            
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Something wrong.....");
                return View(model);
            }
            return View(model);
        }


        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult LoginIndex()
        {
            return View();
        }
    }
}
