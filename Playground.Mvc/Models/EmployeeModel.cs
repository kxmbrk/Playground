﻿using System;

namespace Playground.Mvc.Models
{
    public class EmployeeModel
    {
        public int EMP_ID { get; set; }
        public string EMP_NAME { get; set; }
        public string EMP_EMAIL { get; set; }
        public string EMP_PHONE { get; set; }
        public int? EMP_SALARY { get; set; }
        public DateTime EMP_HIRE_DATE { get; set; }
        public string EMP_GENDER { get; set; }
        public int? EMP_PHOTO_Id { get; set; }
        public bool? EMP_IS_ACTIVE { get; set; }

        public string ActionLinks { get; set; }

        public void PrepForView()
        {
            ActionLinks = $@"<a href='#' data-empId-edit='{EMP_ID}'>Edit</a>&nbsp;&nbsp;<a href='#' data-empId-detail='{EMP_ID}'>Details</a>";
        }
    }
}