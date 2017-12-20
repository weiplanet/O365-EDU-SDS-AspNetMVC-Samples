/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OneRosterProviderDemo.Serializers;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/courses")]
    public class CoursesController : BaseController
    {
        public CoursesController(ApiContext _db) : base(_db)
        {

        }

        // GET ims/oneroster/v1p1/courses
        [HttpGet]
        public IActionResult GetAllCourses()
        {
            IQueryable<Course> courseQuery = db.Courses
                .Include(c => c.SchoolYearAcademicSession)
                .Include(c => c.Org);
            courseQuery = ApplyBinding(courseQuery);
            var courses = courseQuery.ToList();

            serializer = new OneRosterSerializer("courses");
            serializer.writer.WriteStartArray();
            foreach (var course in courses)
            {
                course.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/courses/5
        [HttpGet("{id}")]
        public IActionResult GetCourse([FromRoute] string id)
        {
            var course = db.Courses
                .Include(c => c.SchoolYearAcademicSession)
                .Include(c => c.Org)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }
            serializer = new OneRosterSerializer("course");
            course.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/courses/5
        [HttpGet("{id}/resources")]
        public IActionResult GetResourcesForCourse([FromRoute] string id)
        {
            var course = db.Courses
                .Include(c => c.SchoolYearAcademicSession)
                .Include(c => c.Org)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }
            serializer = new OneRosterSerializer("course");
            course.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }
    }
}
