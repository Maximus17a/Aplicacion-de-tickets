using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AplicacionDeTickets.Models.ViewModels;
using AplicacionDeTickets.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AplicacionDeTickets.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: /Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userService.AuthenticateAsync(model.Email, model.Password);

                if (user != null)
                {
                    // Crear claims para la identidad del usuario
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.ID_Usuario),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.GivenName, $"{user.Nombre} {user.Primer_Apellido}"),
                        new Claim(ClaimTypes.Role, user.Rol_Usuario)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTime.UtcNow.AddDays(30) // Configura según tus necesidades
                        });

                    _logger.LogInformation($"Usuario {user.Email} inició sesión correctamente");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Dashboard", "Home");
                }

                ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                _logger.LogWarning($"Intento fallido de inicio de sesión para {model.Email}");
            }

            return View(model);
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = await _userService.RegisterAsync(model);

                if (success)
                {
                    _logger.LogInformation($"Usuario {model.Email} registrado exitosamente");
                    TempData["SuccessMessage"] = "Usuario registrado exitosamente. Ahora puedes iniciar sesión.";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, message);
                _logger.LogWarning($"Error en registro para {model.Email}: {message}");
            }

            return View(model);
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}