/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using OneRosterProviderDemo.Serializers;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/classes")]
    public class KlassesController : BaseController
    {
        public KlassesController(ApiContext _db) : base(_db)
        {
        }

        // GET ims/oneroster/v1p1/classes
        [HttpGet]
        public IActionResult GetAllClasses()
        {
            IQueryable<Klass> klassQuery = db.Klasses
                .Include(k => k.KlassAcademicSessions)
                    .ThenInclude(kas => kas.AcademicSession)
                .Include(k => k.Course)
                .Include(k => k.School);
            klassQuery = ApplyBinding(klassQuery);
            var klasses = klassQuery.ToList();

            serializer = new OneRosterSerializer("classes");
            serializer.writer.WriteStartArray();
            foreach (var klass in klasses)
            {
                klass.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/classes/5
        [HttpGet("{id}")]
        public IActionResult GetClass([FromRoute] string id)
        {
            var klass = db.Klasses
                .Include(k => k.KlassAcademicSessions)
                    .ThenInclude(kas => kas.AcademicSession)
                .Include(k => k.Course)
                .Include(k => k.School)
                .SingleOrDefault(k => k.Id == id);

            if (klass == null)
            {
                return NotFound();
            }
            serializer = new OneRosterSerializer("class");
            klass.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/classes/5/students
        [HttpGet("{id}/students")]
        public IActionResult GetStudentsForClass([FromRoute] string id)
        {
            if (db.Klasses.SingleOrDefault(k => k.Id == id) == null)
            {
                return NotFound();
            }

            IQueryable<User> studentsQuery = db.Users
                .Include(u => u.UserOrgs)
                    .ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents)
                    .ThenInclude(ua => ua.Agent)
                .Include(u => u.Enrollments)
                .Where(u => u.Enrollments.Where(e => e.KlassId == id && e.Role == Vocabulary.RoleType.student).Count() > 0);
            studentsQuery = ApplyBinding(studentsQuery);
            var students = studentsQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("users");
            serializer.writer.WriteStartArray();
            foreach (var student in students)
            {
                student.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/classes/5/teachers
        [HttpGet("{id}/teachers")]
        public IActionResult GetTeachersForClass([FromRoute] string id)
        {
            if (db.Klasses.SingleOrDefault(k => k.Id == id) == null)
            {
                return NotFound();
            }

            IQueryable<User> teachersQuery = db.Users
                .Include(u => u.UserOrgs)
                    .ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents)
                    .ThenInclude(ua => ua.Agent)
                .Include(u => u.Enrollments)
                .Where(u => u.Enrollments.Where(e => e.KlassId == id && e.Role == Vocabulary.RoleType.teacher).Count() > 0);
            teachersQuery = ApplyBinding(teachersQuery);
            var teachers = teachersQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("users");
            serializer.writer.WriteStartArray();
            foreach (var teacher in teachers)
            {
                teacher.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/classes/5/lineItems
        [HttpGet("{id}/lineItems")]
        public IActionResult GetLineItemsForClass([FromRoute] string id)
        {
            if (db.Klasses.SingleOrDefault(k => k.Id == id) == null)
            {
                return NotFound();
            }

            IQueryable<LineItem> lineItemQuery = db.LineItems
                .Where(li => li.KlassId == id)
                .Include(li => li.LineItemCategory)
                .Include(li => li.AcademicSession);
            lineItemQuery = ApplyBinding(lineItemQuery);
            var lineItems = lineItemQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("lineItems");
            serializer.writer.WriteStartArray();
            foreach (var lineItem in lineItems)
            {
                lineItem.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/classes/5/results
        [HttpGet("{id}/results")]
        public IActionResult GetResultsForClass([FromRoute] string id)
        {
            if (db.Klasses.SingleOrDefault(k => k.Id == id) == null)
            {
                return NotFound();
            }

            IQueryable<Result> resultQuery = db.Results
                .Include(r => r.Student)
                .Include(r => r.LineItem)
                .Where(r => r.LineItem.KlassId == id);
            resultQuery = ApplyBinding(resultQuery);
            var results = resultQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("results");
            serializer.writer.WriteStartArray();
            foreach (var result in results)
            {
                result.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/classes/5/lineItems/5/results
        [HttpGet("{id}/lineItems/{lineItemId}/results")]
        public IActionResult GetResultsForLineItemForClass([FromRoute] string id, [FromRoute] string lineItemId)
        {
            if (db.Klasses.SingleOrDefault(k => k.Id == id) == null ||
                db.LineItems.SingleOrDefault(li => li.Id == lineItemId) == null)
            {
                return NotFound();
            }

            IQueryable<Result> resultQuery = db.Results
                .Where(r => r.LineItemId == lineItemId)
                .Include(r => r.Student)
                .Include(r => r.LineItem);
            resultQuery = ApplyBinding(resultQuery);
            var results = resultQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("results");
            serializer.writer.WriteStartArray();
            foreach (var result in results)
            {
                result.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/classes/5/students/5/results
        [HttpGet("{id}/students/{studentId}/results")]
        public IActionResult GetResultsForStudentForClass([FromRoute] string id, [FromRoute] string studentId)
        {
            if (db.Klasses.SingleOrDefault(k => k.Id == id) == null ||
                db.Users.SingleOrDefault(u => u.Id == studentId) == null ||
                db.Enrollments.SingleOrDefault(e => e.KlassId == id && e.UserId == studentId && e.Role == Vocabulary.RoleType.student) == null)
            {
                return NotFound();
            }
            IQueryable<Result> resultQuery = db.Results
                .Where(r => r.StudentUserId == studentId && r.LineItem.KlassId == id)
                .Include(r => r.Student)
                .Include(r => r.LineItem);
            resultQuery = ApplyBinding(resultQuery);
            var results = resultQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("results");
            serializer.writer.WriteStartArray();
            foreach (var result in results)
            {
                result.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/classes/5/resources
        [HttpGet("{id}/resources")]
        public IActionResult GetResourcesForClass([FromRoute] string id)
        {
            var klass = db.Klasses
                .Include(k => k.Course)
                .SingleOrDefault(k => k.Id == id);

            if (klass == null)
            {
                return NotFound();
            }

            serializer = new Serializers.OneRosterSerializer("resources");
            serializer.writer.WriteStartArray();

            if (klass.Course != null && klass.Course.Resources != null)
            {
                foreach (var resourceId in klass.Course.Resources)
                {
                    var resource = db.Resources
                        .SingleOrDefault(r => r.Id == resourceId);
                    resource.AsJson(serializer.writer, BaseUrl());
                }
            }

            if (klass.Resources != null)
            {
                foreach (var resourceId in klass.Resources)
                {
                    var resource = db.Resources
                        .SingleOrDefault(r => r.Id == resourceId);
                    resource.AsJson(serializer.writer, BaseUrl());
                }
            }

            serializer.writer.WriteEndArray();
            return JsonOk(FinishSerialization(), ResponseCount);
        }
    }
}
