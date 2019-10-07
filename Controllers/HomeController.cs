using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Login_Reg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Login_Reg.Controllers
{
    public class HomeController : Controller
    {

        private LoginContext dbContext;

        public HomeController(LoginContext context)
        {
            dbContext = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            if(HttpContext.Session.GetInt32("curUser") != null) {
                return RedirectToAction("Success");
            }
            return View();
        }

        [HttpPost]
        [Route("/register")]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid) {
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                dbContext.Add(user);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("curUser", user.UserId);
                return RedirectToAction("Success");
            }
            return View("Index");
        }

        [HttpPost]
        [Route("/process_login")]
        public IActionResult ProcessLogin(LoginUser userSubmission) {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("Email", "There is no user with this email address!");
                    return View("Login");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    ModelState.AddModelError("Password", "Incorrect Password!");
                    return View("Login");
                }
                HttpContext.Session.SetInt32("curUser", userInDb.UserId);
                return RedirectToAction("Success");
            }
            return View("Login");
        }

        [HttpGet]
        [Route("/login")]
        public IActionResult Login()
        {
            if(HttpContext.Session.GetInt32("curUser") != null) {
                return RedirectToAction("Success");
            }
            return View();
        }

        [HttpGet]
        [Route("/success")]
        public IActionResult Success() {
            if(HttpContext.Session.GetInt32("curUser") == null) {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        [Route("/logout")]
        public IActionResult Logout() {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
