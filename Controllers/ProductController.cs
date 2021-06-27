using BooksNepal.Data;
using BooksNepal.Models;
using BooksNepal.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BooksNepal.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;

        }
        public IActionResult Index()
        {
            IEnumerable<Product> objList = _db.Products;

            foreach (var obj in objList)
            {
                obj.Category = _db.Categories.FirstOrDefault(u => u.Id == obj.CategoryId);
            };
            return View(objList);
        }


        //get for Create and Update called Upsert

        public IActionResult Upsert(int? id)
        {
            //IEnumerable<SelectListItem> CategoryDropDown = _db.Categories.Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});

            ////ViewBag.CategoryDropDown = CategoryDropDown;
            //ViewData["CategoryDropDown"] = CategoryDropDown;

            //Product product = new Product();

            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _db.Categories.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            if (id==null)
            {
                //this is for create
                return View(productVM);
            }
            else
            {
                productVM.Product = _db.Products.Find(id);
                    if(productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        //create Post / edit 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                if(productVM.Product.ID == 0)
                {
                    //create
                    string upload = webRootPath + WebConstraints.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName+extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);

                    }

                    productVM.Product.Image = fileName + extension;
                    _db.Products.Add(productVM.Product);
                    
                }
                else
                {
                    //update
                    var objFromDb = _db.Products.AsNoTracking().FirstOrDefault(u => u.ID == productVM.Product.ID);
                    if(files.Count > 0)
                    {
                        string upload = webRootPath + WebConstraints.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var oldfile = Path.Combine(upload, objFromDb.Image);
                        if(System.IO.File.Exists(oldfile))
                        {
                            System.IO.File.Delete(oldfile);
                        }
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);

                        }

                        productVM.Product.Image = fileName + extension;

                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }
                    _db.Products.Update(productVM.Product);

                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            productVM.CategorySelectList = _db.Categories.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            return View(productVM);

        }

 


        //get for Delete

        public IActionResult Delete(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            Product product = _db.Products.Include(u => u.Category).FirstOrDefault(u => u.ID == Id);
            //product.Category = _db.Categories.Find(product.CategoryId);

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //post for Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirm(int? id)
        {
            var obj = _db.Products.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            string upload = _webHostEnvironment.WebRootPath + WebConstraints.ImagePath;
            var oldfile = Path.Combine(upload, obj.Image);

            if (System.IO.File.Exists(oldfile))
            {
                System.IO.File.Delete(oldfile);
            }

                _db.Products.Remove(obj);
                    _db.SaveChanges();
                return RedirectToAction("Index");
            

        }
    }
}
