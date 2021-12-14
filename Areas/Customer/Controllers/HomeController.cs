using BETOnlineShopv1._0.Data;
using SharedModels.Models;
using BETOnlineShopv1._0.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using Newtonsoft.Json;
using System.Net.Http;
using BETOnlineShopv1._0.Areas.Customer.Controllers;

namespace BETOnlineShopv1._0.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        HttpClientHandler _httpClient = new HttpClientHandler();
        UserManager<IdentityUser> _userManager;
        SignInManager<IdentityUser> _signInManager;
        static readonly HttpClient client = new HttpClient();
        public HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpClient.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        }

        public async Task<ActionResult> Index(int? page)
        {
            try
            {
                var userToken = HttpContext.Session.GetString("JWToken");
                if ((userToken ==null|| userToken=="")&&( _signInManager.IsSignedIn(User)==true)) 
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
                List<Product> shopitems = new List<Product>();
                    using (var response = await client.GetAsync("https://localhost:44368/api/Product/getallproducts"))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopitems = JsonConvert.DeserializeObject<List<Product>>(apiRes);
                    }
                var products = shopitems;//_db.Products.Include(c => c.productType).ToList();
                if (products != null)
                {
                    var allproducts = new List<Product>();
                    foreach (var product in products)
                    {
                        if (product.Image == null)
                        {
                            product.Image = "Images/noImage.png";
                        }
                        allproducts.Add(product);
                    }
                    products = allproducts;
                }
                return View(products.ToList().ToPagedList(page ?? 1, 3));

            }
            catch (Exception)
            {

                throw new OperationCanceledException("Operation failed");
            }
        }
        public async Task<ActionResult> Details(int? Id)
        {
            try
            {
                if (Id == null)
                {
                    return NotFound("Product type with Id: " + Id + "was not found");
                }
                Product shopItem = new Product();
                List<ProductType> itemTypes = new List<ProductType>();
                    using (var response = await client.GetAsync("https://localhost:44368/api/Product/" + Id))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopItem = JsonConvert.DeserializeObject<Product>(apiRes);
                    }
                    using (var response = await client.GetAsync("https://localhost:44368/api/ProductType/getalltypes"))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        itemTypes = JsonConvert.DeserializeObject<List<ProductType>>(apiRes);
                    }
                var product = shopItem; 
                if (product.Image == null)
                {
                    product.Image = "Images/noImage.png";
                }
                if (product != null)
                {
                    product.Quantity = 1;
                    ViewData["ProdTypeId"] = new SelectList(itemTypes.ToList(), "Id", "ProdType");
                    return View(product);
                }
                else { return NotFound("Product type with Id: " + Id + "was not found"); }
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
        //Post Details Action Method
        [HttpPost]
        [ActionName("Details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductDetails(Product prod)
        {
            try
            {
                if (prod.Id == 0)
                {
                    return NotFound("Product was not found");
                }
                List<Product> products = new List<Product>();
                Product shopItem = new Product();
                List<ProductType> itemTypes = new List<ProductType>();
                    using (var response = await client.GetAsync("https://localhost:44368/api/Product/" + prod.Id))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopItem = JsonConvert.DeserializeObject<Product>(apiRes);
                    }
                    using (var response = await client.GetAsync("https://localhost:44368/api/ProductType/getalltypes"))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        itemTypes = JsonConvert.DeserializeObject<List<ProductType>>(apiRes);
                    }
                var product = shopItem;
                if (product.Image == null)
                {
                    product.Image = "Images/noImage.png";
                }
                if (product != null)
                {
                    product.Quantity = prod.Quantity;
                    ViewData["ProdTypeId"] = new SelectList(itemTypes.ToList(), "Id", "ProdType");
                    products = HttpContext.Session.Get<List<Product>>("products");
                    if (products == null)
                    {
                        products = new List<Product>();
                    }
                    products.Add(product);
                    HttpContext.Session.Set("products", products);
                    return RedirectToAction(nameof(Index));
                }
                else { return NotFound("Product type with Id: " + prod.Id + "was not found"); }
            }
            catch (Exception)
            {

                throw new OperationCanceledException("Operation failed");
            }
        }
        
        [ActionName("Remove")]
        public IActionResult RemoveFromCart(int? Id)
        {
            try
            {
                List<Product> products = HttpContext.Session.Get<List<Product>>("products");
                Product product = new Product();
                if (Id == null)
                {
                    return NotFound();
                }
                if (products != null) { product = products.FirstOrDefault(x => x.Id == Id); }
                if (product == null)
                {
                    return NotFound("Product type with Id: " + Id + "was not found");
                }
                else
                {
                    if (products != null)
                    {
                        products.Remove(product);
                        HttpContext.Session.Set("products", products);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
        [HttpPost]
        public IActionResult Remove(int? Id)
        {
            try
            {
                List<Product> products = HttpContext.Session.Get<List<Product>>("products");
                Product product = new Product();
                if (Id == null)
                {
                    return NotFound();
                }
                if (products != null) { product = products.FirstOrDefault(x => x.Id == Id); }
                if (product == null)
                {
                    return NotFound("Product type with Id: " + Id + "was not found");
                }
                else
                {
                    if (products != null)
                    {
                        products.Remove(product);
                        HttpContext.Session.Set("products", products);
                    }
                }
                return RedirectToAction(nameof(Index));

            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
        public IActionResult CatDetails()
        {
            try
            {
                List<Product> products = HttpContext.Session.Get<List<Product>>("products");
                if (products == null)
                {
                    products = new List<Product>();
                }
                return View(products);
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
    }
}
