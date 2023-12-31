﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tranning.DataDBContext;
using Tranning.Models;

namespace Tranning.Controllers
{
    public class TrainerTopicController : Controller
    {
        private readonly TranningDBContext _dbContext;
        public TrainerTopicController(TranningDBContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        public IActionResult Index(string SearchString)
        {
            TrainerTopicModel trainertopicModel = new TrainerTopicModel();
            trainertopicModel.TrainerTopicDetailLists = new List<TrainerTopicDetail>();

            var data = from trainner_topic in _dbContext.TrainerTopics
                       join users in _dbContext.Users on trainner_topic.trainer_id equals users.id
                       join topics in _dbContext.Topics on trainner_topic.topic_id equals topics.id
                       select new { TrainnerTopic = trainner_topic, User = users, Topic = topics };


            data = data.Where(m => m.TrainnerTopic.deleted_at == null);
            if (!string.IsNullOrEmpty(SearchString))
            {
                data = data.Where(m => m.User.full_name.Contains(SearchString) || m.Topic.name.Contains(SearchString));
            }
            data.ToList();

            foreach (var item in data)
            {
                // Use object initialization for better readability
                trainertopicModel.TrainerTopicDetailLists.Add(new TrainerTopicDetail
                {
                    trainer_id = item.TrainnerTopic.trainer_id,
                    topic_id = item.TrainnerTopic.topic_id,
                    created_at = item.TrainnerTopic.created_at,
                    updated_at = item.TrainnerTopic.updated_at,
                    full_name = item.User.full_name,
                    name = item.Topic.name,
                });

            }

            ViewData["CurrentFilter"] = SearchString;
            return View(trainertopicModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            TrainerTopicDetail trainertopic = new TrainerTopicDetail();
            var topicList = _dbContext.Topics
              .Where(m => m.deleted_at == null)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = topicList;

            var trainerList = _dbContext.Users
              .Where(m => m.deleted_at == null && m.role_id == 3)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.full_name }).ToList();
            ViewBag.Stores1 = trainerList;

            return View(trainertopic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(TrainerTopicDetail trainertopic)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var trainertopicData = new TrainerTopic()
                    {
                        topic_id = trainertopic.topic_id,
                        trainer_id = trainertopic.trainer_id,
                        created_at = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    };

                    _dbContext.TrainerTopics.Add(trainertopicData);
                    _dbContext.SaveChanges(true);
                    TempData["saveStatus"] = true;
                }

                catch (Exception ex)
                {

                    TempData["saveStatus"] = false;
                }
                return RedirectToAction(nameof(TrainerTopicController.Index), "TrainerTopic");
            }


            var courseList = _dbContext.Courses
              .Where(m => m.deleted_at == null)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = courseList;

            var traineeList = _dbContext.Users
              .Where(m => m.deleted_at == null && m.role_id == 3)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.full_name }).ToList();
            ViewBag.Stores1 = traineeList;


            Console.WriteLine(ModelState.IsValid);
            foreach (var key in ModelState.Keys)
            {
                var error = ModelState[key].Errors.FirstOrDefault();
                if (error != null)
                {
                    Console.WriteLine($"Error in {key}: {error.ErrorMessage}");
                }
            }
            return View(trainertopic);
        }

        [HttpGet]
        public IActionResult Delete(int trainer_id = 0, int topic_id = 0)
        {
            try
            {
                var data = _dbContext.TrainerTopics
                    .Where(tc => tc.trainer_id == trainer_id && tc.topic_id == topic_id)
                    .FirstOrDefault();

                if (data != null)
                {
                    data.deleted_at = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    _dbContext.SaveChanges(true);
                    TempData["DeleteStatus"] = true;
                }
                else
                {
                    TempData["DeleteStatus"] = false;
                }
            }
            catch
            {
                TempData["DeleteStatus"] = false;
            }

            return RedirectToAction(nameof(TrainerTopicController.Index), "TrainerTopicController");
        }
    }
}
