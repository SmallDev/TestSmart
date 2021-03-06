﻿using System;
using Logic.Model;

namespace Logic.Dal.NHibernate.Models
{
    public class LearningDto
    {
        public virtual Int32 Id { get; set; }
        public virtual TimeSpan From { get; set; }
        public virtual TimeSpan To { get; set; }
        public virtual Double? StartLikelihood { get; set; }
        public virtual Double? EndLikelihood { get; set; }

        public virtual DateTime CreatedOn { get; set; }

        public static implicit operator LearningDto(Learning learning)
        {
            return new LearningDto
            {
                Id = learning.Id,
                From = learning.TimeFrom,
                To = learning.TimeTo,
                StartLikelihood = learning.StartLikelihood,
                EndLikelihood = learning.EndLikelihood,
                CreatedOn = learning.CreatedOn
            };
        }

        public static implicit operator Learning(LearningDto learning)
        {
            return new Learning
            {
                Id = learning.Id,
                TimeFrom = learning.From,
                TimeTo = learning.To,
                StartLikelihood = learning.StartLikelihood,
                EndLikelihood = learning.EndLikelihood,
                CreatedOn = learning.CreatedOn
            };
        }
    }
}
