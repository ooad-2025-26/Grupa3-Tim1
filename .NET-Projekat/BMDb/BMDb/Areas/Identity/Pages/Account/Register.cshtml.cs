// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BMDb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace BMDb.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Osoba> _signInManager;
        private readonly UserManager<Osoba> _userManager;
        private readonly IUserStore<Osoba> _userStore;
        private readonly IUserEmailStore<Osoba> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private const string DefaultRoleName = "Korisnik";
        private const string DefaultRoleId = "3";

        public RegisterModel(
            UserManager<Osoba> userManager,
            IUserStore<Osoba> userStore,
            SignInManager<Osoba> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [Display(Name = "Ime")]
            public string Ime { get; set; }

            [Required]
            [Display(Name = "Prezime")]
            public string Prezime { get; set; }

            [Required]
            [Display(Name = "Nadimak")]
            public string Nadimak { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/Home/Glavna");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Home/Glavna");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                try
                {
                    var defaultRole = await _roleManager.FindByNameAsync(DefaultRoleName);
                    if (defaultRole == null || defaultRole.Id != DefaultRoleId)
                    {
                        _logger.LogError("Registration failed because the default role {RoleName} with id {RoleId} was not found.", DefaultRoleName, DefaultRoleId);
                        ModelState.AddModelError(string.Empty, "Registracija trenutno nije moguća jer osnovna korisnička rola nije pravilno podešena.");
                        return RedirectToRegisterPopup(returnUrl);
                    }

                    var user = CreateUser();
                    user.Ime = Input.Ime;
                    user.Prezime = Input.Prezime;
                    user.Nadimak = Input.Nadimak;
                    user.Avatar = "~/images/uploads/obicna.png";
                    user.DatumRegistracije = DateTime.UtcNow;
                    user.NotifikacijeUkljucene = true;
                    user.StatusOsobe = BMDb.Models.OsobaStatus.Aktivan;
                    user.BrojRecenzija = 0;

                    await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                    var result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        var roleResult = await _userManager.AddToRoleAsync(user, DefaultRoleName);
                        var roleRelationSaved = roleResult.Succeeded &&
                            await _context.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == DefaultRoleId);

                        if (!roleRelationSaved)
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }

                            if (!roleResult.Errors.Any())
                            {
                                ModelState.AddModelError(string.Empty, "Registracija nije dovršena jer korisnička rola nije spremljena.");
                            }

                            await _userManager.DeleteAsync(user);
                            return RedirectToRegisterPopup(returnUrl);
                        }

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Registration failed.");
                    ModelState.AddModelError(string.Empty, "Registracija trenutno nije uspjela. Pokušajte ponovo.");
                }
            }

            return RedirectToRegisterPopup(returnUrl);
        }

        private IActionResult RedirectToRegisterPopup(string returnUrl)
        {
            var errors = ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Uneseni podaci nisu validni." : x.ErrorMessage)
                .Distinct()
                .ToList();

            if (!errors.Any())
            {
                errors.Add("Registracija trenutno nije uspjela. Provjerite unesene podatke.");
            }

            TempData["ShowRegisterPopup"] = "true";
            TempData["RegisterErrors"] = JsonSerializer.Serialize(errors);
            TempData["RegisterInput.Ime"] = Input?.Ime ?? string.Empty;
            TempData["RegisterInput.Prezime"] = Input?.Prezime ?? string.Empty;
            TempData["RegisterInput.Nadimak"] = Input?.Nadimak ?? string.Empty;
            TempData["RegisterInput.Email"] = Input?.Email ?? string.Empty;

            return LocalRedirect(GetSafeReturnUrl(returnUrl));
        }

        private string GetSafeReturnUrl(string returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/Home/Glavna");
        }

        private Osoba CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Osoba>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Osoba)}'. " +
                    $"Ensure that '{nameof(Osoba)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<Osoba> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Osoba>)_userStore;
        }
    }
}
