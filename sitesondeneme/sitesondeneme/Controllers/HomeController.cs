using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using sitesondeneme.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.IO; // 🔵 dosya işlemleri için gerekli
using System.Threading.Tasks; // 🔵 async methodlar için gerekli

namespace sitesondeneme.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;
            return View();
        }

        public IActionResult Products()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;

            // Veritabanından ürünleri çek
            var products = _context.Products.ToList();

            return View(products);  // Ürün listesini view'e gönder
        }


        public IActionResult ProductDetails()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;
            return View();
        }

        public IActionResult Cart()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;
            return View();
        }

        [HttpGet]
        public IActionResult Account()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewData["Username"] = username;
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("account");
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre yanlış.");
                return View("account");
            }

            HttpContext.Session.SetString("Username", user.Username);
            return RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            return RedirectToAction("Index");
        }

        // 🔵 Ürün ekleme sayfasını GET ile göster
        [HttpGet]
        public IActionResult AddProduct()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return RedirectToAction("Account"); // 🔐 Giriş yapmamışsa yönlendir
            }

            ViewData["Username"] = username;
            return View();
        }

        // 🔵 Ürün ekleme işlemi (POST)
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile ImageFile)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Account");
            }

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                product.ImagePath = "/images/" + fileName;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Products");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
