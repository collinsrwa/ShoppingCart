using SharedModels.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BETOnlineShopv1._0.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class UserController : Controller
    {
        UserManager<IdentityUser> _userManager;
        SignInManager<IdentityUser> _signInManager;
        HttpClientHandler _httpClient = new HttpClientHandler();
        static readonly HttpClient client = new HttpClient();
        private readonly string _apiBaseUrl;
        private IConfiguration _configuration;
        public UserController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpClient.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            _configuration = configuration;
            _apiBaseUrl = _configuration["ApiSettings:BaseUrl"]; 
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Register user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    user.UserName = user.Email;
                    var result = await _userManager.CreateAsync(user, user.Password);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction("Index", "Home", new { area = "Customer" });
                    }
                    foreach (var err in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, err.Description);
                    }
                }

                return View();
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    user.UserName = user.Email;
                    TokenResponse tResp = new TokenResponse();

                        StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                        using (var response = await client.PostAsync(_apiBaseUrl+"User/login", content))
                        {
                            string apiRes = await response.Content.ReadAsStringAsync();
                            tResp = JsonConvert.DeserializeObject<TokenResponse>(apiRes);
                        }
                        if (tResp.token != null)
                        {
                            HttpContext.Session.SetString("JWToken", tResp.token);
                        }

                    var result = await _signInManager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home", new { area = "Customer" });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt");
                    }
                }
                return View();

            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
        public IActionResult LogOut()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
                return View();
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
        public IActionResult DoNotLogout()
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home", new { area = "Customer" });

            }

            return View();
        }

    }
}
