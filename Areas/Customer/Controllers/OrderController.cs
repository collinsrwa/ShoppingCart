using BETOnlineShopv1._0.Data;
using SharedModels.Models;
using BETOnlineShopv1._0.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using SelectPdf;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Mail;
using System.Net.Http.Headers;

namespace BETOnlineShopv1._0.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        HttpClientHandler _httpClient = new HttpClientHandler();
        private IWebHostEnvironment _environment;
        private ICompositeViewEngine _compositeViewEngine;
        static readonly HttpClient client = new HttpClient();
        public OrderController(ApplicationDbContext db, ICompositeViewEngine compositeViewEngine, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _environment = environment;
            _compositeViewEngine = compositeViewEngine;
          _httpContextAccessor = httpContextAccessor;
          _httpClient.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        }
        public IActionResult Index()
        {
            return View();
        }
        //Check Get Method
        [Authorize]
        public IActionResult CheckOut()
        {
            return View();
        }
        //Checkout post action Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CheckOut(Order order)
        {
            try
            {
                //Use token to access the API
                var userToken = HttpContext.Session.GetString("JWToken");
                List<Product> products = HttpContext.Session.Get<List<Product>>("products");
                if (products != null)
                {
                    foreach (var product in products)
                    {
                        OrderDetails orderDetails = new OrderDetails();
                        orderDetails.ProductId = product.Id;
                        orderDetails.Quantity = product.Quantity;
                        order.orderDetails.Add(orderDetails);
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }

                    await CreatePdf();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
                StringContent content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
                using (var response = await client.PostAsync("https://localhost:44368/api/Order/addanorder", content))
                {
                    string apiRes = await response.Content.ReadAsStringAsync();

                }
                HttpContext.Session.Set("products", new List<Product>());
                return View();
            }
            catch (Exception)
            {

                throw new OperationCanceledException("Operation failed");
            }
        }
        /// <summary>
        /// Generate Order number for the current order
        /// </summary>
        /// <returns></returns>
        public string GetOrderNo()
        {
            int count = _db.Orders.ToList().Count()+1;
            return count.ToString("000");
        }  
        /// <summary>
        /// Generate PDF of the order and send as email
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="fileName"></param>
        public void SendMail(string toEmail, string fileName)
        {
            try
            {
                string MailBody = "Hi " + toEmail + " Please find attached summary of your order. Thank you";
                string fromEmail = "somemailaddress";
                string emailTitle = "Order Details";
                string mailSubject = "Order Details";
                string mailPassword = "somepassword";

                //Mail message
                MailMessage message = new MailMessage(new MailAddress(fromEmail, emailTitle), new MailAddress(toEmail));

                //Mail Content
                message.Subject = mailSubject;
                message.Body = MailBody;
                message.IsBodyHtml = true;

                //Attachment
                Attachment file = new Attachment(fileName);
                message.Attachments.Add(file);

                //Server Details
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                //Credentials
                smtp.UseDefaultCredentials = false;
                System.Net.NetworkCredential credential = new System.Net.NetworkCredential();
                credential.UserName = fromEmail;
                credential.Password = mailPassword;
                smtp.Credentials = credential;
                smtp.Send(message);
            }
            catch (Exception)
            {
                throw new OperationCanceledException("Operation failed");
            }
        }
        public async Task CreatePdf()
        {
            try
            {
                string path = Path.Combine(this._environment.WebRootPath, "Orders");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (var stringWriter = new StringWriter())
                {
                    var viewResult = _compositeViewEngine.FindView(ControllerContext, "CheckOut", true);
                    if (viewResult.View == null)
                    {
                        throw new ArgumentNullException("View not found");
                    }
                    var ViewDictionary = new ViewDataDictionary(ViewData);
                    var viewContext = new ViewContext(
                        ControllerContext,
                        viewResult.View,
                        ViewDictionary,
                        TempData,
                        stringWriter,
                        new HtmlHelperOptions()
                        );
                    await viewResult.View.RenderAsync(viewContext);
                    var htmlToPdf = new HtmlToPdf(1260,720);
                    htmlToPdf.Options.DrawBackground = true;
                    var pdf = htmlToPdf.ConvertHtmlString(stringWriter.ToString(), "https://localhost:44392/Customer/Order/CheckOut");
                    var pdfBytes = pdf.Save();
                    using (var streamWriter = new StreamWriter(path + "/" + GetOrderNo() + ".pdf"))
                    {
                        await streamWriter.BaseStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                    }
                    string fileToSend = path + "/" + GetOrderNo() + ".pdf";
                    string emailTo = User.Identity.Name;
                    SendMail(emailTo, fileToSend);
                }
            }
            catch (Exception)
            {

                throw new OperationCanceledException("Operation failed");
            }
        }
    }
}
