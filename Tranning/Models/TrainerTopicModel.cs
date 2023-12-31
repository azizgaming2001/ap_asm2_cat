﻿using System.ComponentModel.DataAnnotations;

namespace Tranning.Models
{
    public class TrainerTopicModel
    {
        public List<TrainerTopicDetail> TrainerTopicDetailLists { get; set; }
    }

    public class TrainerTopicDetail
    {
        [Required(ErrorMessage = "Choose Topic, please")]
        public int topic_id { get; set; }
        [Required(ErrorMessage = "Choose Trainer, please")]

        public int trainer_id { get; set; }

        public string? full_name { get; set; }

        public string? name { get; set; }
        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
