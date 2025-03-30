using FileUpload.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FileUpload.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace FileUpload.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private string _connectionString = @"Data Source=.\sqlexpress; Initial Catalog=FileUpload;Integrated Security=True;";

        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult AddImage()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile image, Image imageInfo)
        {
            var fileName = $"{Guid.NewGuid()}-{image.FileName}";
            var fullImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

            using FileStream fs = new FileStream(fullImagePath, FileMode.Create);
            image.CopyTo(fs);

            var imageMngr = new ImageManager(_connectionString);
            var id = imageMngr.AddImage(new Image
            {
                ImageName = imageInfo.ImageName,
                ImagePath = fileName,
                Password = imageInfo.Password
            });
            return View(new UploadViewModel
            {
                Id = id,
                Password = imageInfo.Password
            });
        }

        public IActionResult ViewImage(int id, int? password)
        {
            var imageMngr = new ImageManager(_connectionString);
            var image = imageMngr.GetImageById(id);
            var ivm = new ImageViewModel();
            ivm.Id = id;

            var ids = HttpContext.Session.Get<List<int>>("Ids");
            if (image.Password == password)
            {
                if (ids == null)
                {
                    ids = new List<int>();
                    ids.Add(id);
                }
                else 
                { 
                    ids.Add(id); 
                }
            } 

            HttpContext.Session.Set("Ids", ids);
            if (password == null)
            {
                ivm.FirstVisit = true;
            }
            if (ids != null && ids.Contains(image.Id))
            {
                ivm.Image = image;
                ivm.Validated = true;
                ivm.FirstVisit = false;
                imageMngr.UpdateViews(image);
            }

            return View(ivm);
        }
    }

    //    public IActionResult ViewImage(int id, int? password)
    //    {
    //        var imageMngr = new ImageManager(_connectionString);
    //        var image = imageMngr.GetImageById(id);
    //        var ivm = new ImageViewModel();

    //        string validated = HttpContext.Session.GetString("validated");
    //        if (image.Password == password)
    //        {
    //            HttpContext.Session.Set("validated", "true");
    //            validated = "true";
    //            imageMngr.UpdateViews(image);
    //        } 
    //        else 
    //        if(password != null)
    //        {
    //            HttpContext.Session.Set("validated", "false");
    //            validated = "false";
    //        }

    //        ivm.Image = image;
    //        ivm.Validated = validated;

    //        return View(ivm);
    //    }
    //}

    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);

            return value == null ? default(T) :
                JsonSerializer.Deserialize<T>(value);
        }
    }
}
