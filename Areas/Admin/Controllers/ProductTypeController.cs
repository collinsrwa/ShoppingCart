using BETOnlineShopv1._0.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace BETOnlineShopv1._0.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductTypeController : Controller
    {
        private ApplicationDbContext _db;
        HttpClientHandler _httpClient = new HttpClientHandler();
        static readonly HttpClient client = new HttpClient();

        public ProductTypeController(ApplicationDbContext db)
        {
            _db = db;
            _httpClient.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                List<ProductType> shopitemtypes = new List<ProductType>();
                    using (var response = await client.GetAsync("https://localhost:44368/api/ProductType/getalltypes"))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopitemtypes = JsonConvert.DeserializeObject<List<ProductType>>(apiRes);
                    }
                var data = shopitemtypes;
                return View(data);
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Process failed");
            }
        }
        //Get Action Method
        public ActionResult Create()
        {
            return View();
        }
        //Post Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductType productType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ProductType shopitemtype = new ProductType();
                        //Use token to access the API
                        var userToken = HttpContext.Session.GetString("JWToken");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
                        StringContent content = new StringContent(JsonConvert.SerializeObject(productType), Encoding.UTF8, "application/json");
                        using (var response = await client.PostAsync("https://localhost:44368/api/ProductType/addproducttype", content))
                        {
                            string apiRes = await response.Content.ReadAsStringAsync();
                            shopitemtype = JsonConvert.DeserializeObject<ProductType>(apiRes);
                        }
                    TempData["save"] = "Product type has been saved";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {

                throw new OperationCanceledException("Process failed");
            }
            return View(productType);
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
                ProductType shopitemtype = new ProductType();
                    var userToken = HttpContext.Session.GetString("JWToken");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
                    using (var response = await client.GetAsync("https://localhost:44368/api/ProductType/" + Id))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopitemtype = JsonConvert.DeserializeObject<ProductType>(apiRes);
                    }
                var productType = shopitemtype;
                if (productType != null)
                {
                    return View(productType);
                }
                else { return NotFound("Product type with Id: " + Id + "was not found"); }
            }
            catch (Exception)
            {

                throw new OperationCanceledException("Process failed");
            }
        }
        //Post Edit Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductType productType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ProductType shopitemtype = new ProductType();
                        var userToken = HttpContext.Session.GetString("JWToken");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
                        StringContent content = new StringContent(JsonConvert.SerializeObject(productType), Encoding.UTF8, "application/json");
                        using (var response = await client.PutAsync("https://localhost:44368/api/ProductType/updateproducttype", content))
                        {
                            string apiRes = await response.Content.ReadAsStringAsync();
                            shopitemtype = JsonConvert.DeserializeObject<ProductType>(apiRes);
                        }
                    TempData["save"] = "Product type has been updated";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {

                throw new OperationCanceledException("Process failed");
            }
            return View(productType);
        }
        //Get Details Method
        public async Task<ActionResult> Details(int? Id)
        {
            try
            {
                if (Id == null)
                {
                    return NotFound("Product type with Id: " + Id + "was not found");
                }
                ProductType shopitemtype = new ProductType();
                    using (var response = await client.GetAsync("https://localhost:44368/api/ProductType/" + Id))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopitemtype = JsonConvert.DeserializeObject<ProductType>(apiRes);
                    }
                var productType = shopitemtype;
                if (productType != null)
                {
                    return View(productType);
                }
                else { return NotFound("Product type with Id: " + Id + "was not found"); }
            }
            catch (Exception)
            {

                throw new OperationCanceledException("Process failed");
            }
        }
        //Post Details Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(ProductType productType)
        {
            return RedirectToAction(nameof(Index));
        }
        //Get Delete Method
        public async Task<ActionResult> Delete(int? Id)
        {
            try
            {
                if (Id == null)
                {
                    return NotFound("Product type with Id: " + Id + "was not found");
                }
                ProductType shopitemtype = new ProductType();
                    var userToken = HttpContext.Session.GetString("JWToken");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
                    using (var response = await client.GetAsync("https://localhost:44368/api/ProductType/" + Id))
                    {
                        string apiRes = await response.Content.ReadAsStringAsync();
                        shopitemtype = JsonConvert.DeserializeObject<ProductType>(apiRes);
                    }
                var productType = shopitemtype;
                if (productType != null)
                {
                    return View(productType);
                }
                else { return NotFound("Product type with Id: " + Id + "was not found"); }
            }
            catch (Exception)
            {

                throw new OperationCanceledException("Process could not be completed");
            }
        }
        //Post Edit Action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ProductType productType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (productType.Id == 0)
                    {
                        return NotFound("Product type with Id: " + productType.Id + "was not found");
                    }
                    ProductType shopitemtype = new ProductType();
                        var userToken = HttpContext.Session.GetString("JWToken");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
                        using (var response = await client.DeleteAsync("https://localhost:44368/api/ProductType/" + productType.Id))
                        {
                            string apiRes = await response.Content.ReadAsStringAsync();
                        }
                    TempData["save"] = "Product type has been deleted";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {

                    throw new OperationCanceledException("Delete failed");
                } 
            }
            return View(productType);
        }
    }
}
