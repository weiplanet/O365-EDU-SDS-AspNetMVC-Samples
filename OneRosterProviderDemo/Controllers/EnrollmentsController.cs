/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using System.Linq;
using OneRosterProviderDemo.Serializers;
using Microsoft.EntityFrameworkCore;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/enrollments")]
    public class EnrollmentsController : BaseController
    {
        public EnrollmentsController(ApiContext _db) : base(_db)
        {

        }

        // GET ims/oneroster/v1p1/enrollments
        [HttpGet]
        public IActionResult GetAllEnrollments()
        {
            IQueryable<Enrollment> enrollmentsQuery = db.Enrollments
                .Include(e => e.User)
                .Include(e => e.Klass)
                .Include(e => e.School);
            enrollmentsQuery = ApplyBinding(enrollmentsQuery);
            var enrollments = enrollmentsQuery.ToList();

            serializer = new OneRosterSerializer("enrollments");
            serializer.writer.WriteStartArray();
            foreach (var enrollment in enrollments)
            {
                enrollment.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/enrollments/5
        [HttpGet("{id}")]
        public IActionResult GetEnrollment([FromRoute] string id)
        {
            var enrollment = db.Enrollments
                .Include(e => e.User)
                .Include(e => e.Klass)
                .Include(e => e.School)
                .SingleOrDefault(a => a.Id == id);

            if (enrollment == null)
            {
                return NotFound();
            }
            serializer = new OneRosterSerializer("enrollment");
            enrollment.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }
    }
}
