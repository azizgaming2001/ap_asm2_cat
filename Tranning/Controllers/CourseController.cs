﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tranning.DataDBContext;
using Tranning.Models;

namespace Tranning.Controllers
{
    public class CourseController : Controller
    {
        private readonly TranningDBContext _dbContext;

        public CourseController(TranningDBContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        public IActionResult Index(string searchString)
        {
            CourseModel courseModel = new CourseModel();
            courseModel.CourseDetailLists = new List<CourseDetail>();
            var data = from course in _dbContext.Courses
                       join category in _dbContext.Categories on course.category_id equals category.id
                       select new { Course = course, Category = category };
            data = data.Where(m => m.Course.deleted_at == null);

            if (!string.IsNullOrEmpty(searchString))
            {
                data = data.Where(m => m.Course.name.Contains(searchString) || m.Course.description.Contains(searchString));
            }
            var courses = data.ToList();

            foreach (var item in courses)
            {
                courseModel.CourseDetailLists.Add(new CourseDetail
                {
                    id = item.Course.id,
                    name = item.Course.name,
                    category_id = item.Category.id,
                    namecategory = item.Category.name,
                    description = item.Course.description,
                    avatar = item.Course.avatar,
                    status = item.Course.status,
                    start_date = item.Course.start_date,
                    end_date = item.Course.end_date,
                    created_at = item.Course.created_at,
                    updated_at = item.Course.updated_at
                });
            }

            ViewData["CurrentFilter"] = searchString;
            return View(courseModel);
        }


        [HttpGet]
        public IActionResult Add()
        {
            CourseDetail course = new CourseDetail();
            var categoryList = _dbContext.Categories
                .Where(m => m.deleted_at == null)
                .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = categoryList;
       
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CourseDetail course, IFormFile Photo)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadFile(Photo);
                    var courseData = new Course()
                    {
                        name = course.name,
                        description = course.description,
                        category_id = course.category_id,
                        start_date = course.start_date,
                        end_date = course.end_date,
                        status = course.status,
                        avatar = uniqueFileName,
                        created_at = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    };
                
                    _dbContext.Courses.Add(courseData);
                    _dbContext.SaveChanges(true);
                    TempData["saveStatus"] = true;
                }
                catch(Exception ex)
                {
                    TempData["saveStatus"] = false;
                }
                return RedirectToAction(nameof(CourseController.Index), "Course");
            }

            var categoryList = _dbContext.Categories
              .Where(m => m.deleted_at == null)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = categoryList;


            Console.WriteLine(ModelState.IsValid);
            return View(course);
        }

        private string UploadFile(IFormFile file)
        {
            string uniqueFileName;
            try
            {
                string pathUploadServer = "wwwroot\\uploads\\images";

                string fileName = file.FileName;
                fileName = Path.GetFileName(fileName);
                string uniqueStr = Guid.NewGuid().ToString(); // random tao ra cac ky tu khong trung lap
                // tao ra ten fil ko trung nhau
                fileName = uniqueStr + "-" + fileName;
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), pathUploadServer, fileName);
                var stream = new FileStream(uploadPath, FileMode.Create);
                file.CopyToAsync(stream);
                // lay lai ten anh de luu database sau nay
                uniqueFileName = fileName;
            }
            catch (Exception ex)
            {
                uniqueFileName = ex.Message.ToString();
            }
            return uniqueFileName;
        }
        [HttpGet]
        public IActionResult Delete(int id = 0)
        {
            try
            {
                var data = _dbContext.Courses.Where(m => m.id == id).FirstOrDefault();
                if (data != null)
                {
                    data.deleted_at = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    _dbContext.SaveChanges(true);
                    TempData["DeleteStatus"] = true;
                }
                else
                {
                    TempData["DeleteStatus"] = true;
                }
            }
            catch (Exception ex)
            {
                TempData["DeleteStatus"] = false;
                //return Ok(ex.Message);
            }
            return RedirectToAction(nameof(CategoryController.Index), "Course");
        }
        [HttpGet]
        public IActionResult Update(int id = 0)
        {
            CourseDetail course = new CourseDetail();
            var data = _dbContext.Courses.Where(m => m.id == id).FirstOrDefault();
            if (data != null)
            {
                course.id = data.id;
                course.name = data.name;
                course.description = data.description;
                course.status = data.status;
                course.avatar = data.avatar;
            }
            var categoryList = _dbContext.Categories.Where(m => m.deleted_at == null).Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = categoryList;
            var userList = _dbContext.Users
                .Where(m => m.deleted_at == null && m.role_id == 4)
                .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.full_name }).ToList();
            ViewBag.Stores1 = userList;
            return View(course);
        }
        [HttpPost]
        public IActionResult Update(CourseDetail course, IFormFile Photo)
        {
            try
            {
                var data = _dbContext.Courses.Where(m => m.id == course.id).FirstOrDefault();
                if (data != null)
                {
                    data.name = course.name;
                    data.description = course.description;
                    data.status = course.status;
                    data.updated_at = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (Photo != null)
                    {
                        string uniqueIconAvatar = UploadFile(Photo);
                        data.avatar = uniqueIconAvatar;
                    }

                    _dbContext.SaveChanges(true);
                    TempData["UpdateStatus"] = true;
                }
                else
                {
                    TempData["UpdateStatus"] = false;
                }
            }
            catch
            {
                TempData["UpdateStatus"] = false;
            }
            return RedirectToAction(nameof(CategoryController.Index), "Course");
        }

        [HttpGet]
        public IActionResult TrainerIndex(string SearchString)
        {
            CourseModel courseModel = new CourseModel();
            courseModel.CourseDetailLists = new List<CourseDetail>();

            var data = _dbContext.Courses.Where(m => m.deleted_at == null);

            if (!string.IsNullOrEmpty(SearchString))
            {
                data = data.Where(m => m.name.Contains(SearchString) || m.description.Contains(SearchString));
            }

            var courses = data.ToList();

            foreach (var item in courses)
            {
                courseModel.CourseDetailLists.Add(new CourseDetail
                {
                    id = item.id,
                    category_id = item.category_id,
                    name = item.name,
                    description = item.description,
                    avatar = item.avatar,
                    status = item.status,
                    start_date = item.start_date,
                    end_date = item.end_date,
                    created_at = item.created_at,
                    updated_at = item.updated_at
                });
            }

            ViewData["CurrentFilter"] = SearchString;
            return View(courseModel);
        }
    }
}
