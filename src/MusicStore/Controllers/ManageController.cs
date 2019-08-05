using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStore.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        public ManageController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuthenticationSchemeProvider schemeProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _schemeProvider = schemeProvider;
        }

        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            ViewBag.StatusMessage =
          message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
          : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
          : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
          : message == ManageMessageId.Error ? "An error has occurred."
          : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
          : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
          : "";

            var user = await GetCurrentUserAsync();

            var model = new IndexViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
            };
            return View(model);
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

    }
}
