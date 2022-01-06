using BETOnlineShopv1._0.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace BETOnlineShopv1._0.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private IWebHostEnvironment _he;
        HttpClientHandler _httpClient = new HttpClientHandler();
        static readonly HttpClient client = new HttpClient();
        private readonly string _apiBaseUrl;
        private IConfiguration _configuration;
        public ProductController(IWebHostEnvironment he, IConfiguration configuration)
        {
            _he = he;
            _httpClient.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            _configuration = configuration;
            _apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Product> shopitems = new List<Product>();
                    using (var response = await client.GetAsync(_apiBaseUrl + "Product/getallproducts"))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopitems = JsonConvert.DeserializeObject<List<Product>>(apiRes);
                    }
                var products = shopitems;
                return View(products);
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
        //Get Create Action Method
        public async Task<ActionResult> Create()
        {
            try
            {
                List<ProductType> itemTypes = new List<ProductType>();
                    using (var response = await client.GetAsync(_apiBaseUrl + "ProductType/getalltypes"))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        itemTypes = JsonConvert.DeserializeObject<List<ProductType>>(apiRes);
                    }
                ViewData["ProdTypeId"] = new SelectList(itemTypes.ToList(), "Id", "ProdType");
                return View();
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
        //Post Create Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile image)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<Product> shopitems = new List<Product>();
                    List<ProductType> itemTypes = new List<ProductType>();
                  
                        using (var response = await client.GetAsync(_apiBaseUrl + "Product/getallproducts"))
                        {
                            string apiRes = await response.Content.ReadAsStringAsync();
                            shopitems = JsonConvert.DeserializeObject<List<Product>>(apiRes);
                        }
                        using (var response = await client.GetAsync(_apiBaseUrl + "ProductType/getalltypes"))
                        {
                            string apiRes = await response.Content.ReadAsStringAsync();
                            itemTypes = JsonConvert.DeserializeObject<List<ProductType>>(apiRes);
                        }
                    var searchProduct = shopitems.FirstOrDefault(x => x.Name == product.Name);
                    if (searchProduct != null)
                    {
                        ViewBag.message = "Product already exists";
                        ViewData["ProdTypeId"] = new SelectList(itemTypes, "Id", "ProdType");
                        return View(searchProduct);
                    }
                    if (image != null)
                    {
                        var name = Path.Combine(_he.WebRootPath + "/Images", Path.GetFileName(image.FileName));
                        if (!System.IO.File.Exists(name))
                        {
                            await image.CopyToAsync(new FileStream(name, FileMode.Create));
                        }
                        product.Image = "Images/" + image.FileName;
                    }
                    if (image == null)
                    {
                        product.Image = "Images/noImage.png";
                    }
                    Product shopItem = new Product();
                        var userToken = HttpContext.Session.GetString("JWToken");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
                        StringContent content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
                        using (var response = await client.PostAsync(_apiBaseUrl + "Product/addproduct", content))
                        {
                            string apiRes = await response.Content.ReadAsStringAsync();
                            shopItem = JsonConvert.DeserializeObject<Product>(apiRes);
                        }
                    
                    product = shopItem;
                    TempData["save"] = "Product has been saved";
                    return RedirectToAction(nameof(Index));
                }
               
            }
            catch (Exception ex)
            {

                throw new OperationCanceledException("Operation failed");
            }
            return View(product);
        }
        //Get Edit Method
        public async Task<ActionResult> Edit(int? Id)
        {
            try
            {
                if (Id == null)
                {
                    return NotFound("Product type with Id: " + Id + "was not found");
                }
                Product shopItem = new Product();
                List<ProductType> itemTypes = new List<ProductType>();
                using (var httpClient = new HttpClient(_httpClient))
                {
                    using (var response = await httpClient.GetAsync(_apiBaseUrl + "Product/" + Id))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopItem = JsonConvert.DeserializeObject<Product>(apiRes);
                    }
                    using (var response = await httpClient.GetAsync(_apiBaseUrl + "ProductType/getalltypes"))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        itemTypes = JsonConvert.DeserializeObject<List<ProductType>>(apiRes);
                    }
                }
                var product = shopItem;
                if (product.Image == null)
                {
                    product.Image = "Images/noImage.png";
                }
                if (product != null)
                {
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
        //Post Edit Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile image)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (image != null)
                    {
                        var name = Path.Combine(_he.WebRootPath + "/Images", Path.GetFileName(image.FileName));
                        if (!System.IO.File.Exists(name))
                        {
                            await image.CopyToAsync(new FileStream(name, FileMode.Create));
                        }
                        product.Image = "Images/" + image.FileName;
                    }
                    if (image == null)
                    {
                        product.Image = "Images/noImage.png";
                    }
                    Product shopItem = new Product();
                        var userToken = HttpContext.Session.GetString("JWToken");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
                        StringContent content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
                        using (var response = await client.PutAsync(_apiBaseUrl + "Product/updateproduct", content))
                        {
                            string apiRes = await response.Content.ReadAsStringAsync();
                            shopItem = JsonConvert.DeserializeObject<Product>(apiRes);
                        }
                    shopItem = product;
                    TempData["save"] = "Product has been updated";
                    return RedirectToAction(nameof(Index));
                }
                return View(product);
            }
            catch (Exception)
            {

                throw new OperationCanceledException("Operation failed");
            }
        }
        //GET Details Method
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
                    using (var response = await client.GetAsync(_apiBaseUrl + "Product/" + Id))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopItem = JsonConvert.DeserializeObject<Product>(apiRes);
                    }
                    using (var response = await client.GetAsync(_apiBaseUrl + "ProductType/getalltypes"))
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
        [ValidateAntiForgeryToken]
        public IActionResult Details(Product product)
        {
            return RedirectToAction(nameof(Index));
        }
        //GET Details Method
        public async Task<ActionResult> Delete(int? Id)
        {
            try
            {
                if (Id == null)
                {
                    return NotFound("Product with Id: " + Id + "was not found");
                }
                Product shopItem = new Product();
                List<ProductType> itemTypes = new List<ProductType>();
                    using (var response = await client.GetAsync(_apiBaseUrl + "Product/" + Id))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopItem = JsonConvert.DeserializeObject<Product>(apiRes);
                    }
                    using (var response = await client.GetAsync(_apiBaseUrl + "ProductType/getalltypes"))
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
                    ViewData["ProdTypeId"] = new SelectList(itemTypes.ToList(), "Id", "ProdType");
                    return View(product);
                }
                else { return NotFound("Product with Id: " + Id + "was not found"); }
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
        //Post Details Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                        var userToken = HttpContext.Session.GetString("JWToken");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
                        using (var response = await client.DeleteAsync(_apiBaseUrl + "Product/" + product.Id))
                        {
                            string apiRes = await response.Content.ReadAsStringAsync();
                        }
                    TempData["save"] = "Product has been deleted";
                    return RedirectToAction(nameof(Index));
                }
                return View(product);
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
    }
}
