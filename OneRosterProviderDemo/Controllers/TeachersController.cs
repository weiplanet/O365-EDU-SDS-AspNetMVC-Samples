/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OneRosterProviderDemo.Serializers;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/teachers")]
    public class TeachersController : BaseController
    {
        public TeachersController(ApiContext _db) : base(_db)
        {

        }

        // GET ims/oneroster/v1p1/teachers
        [HttpGet]
        public IActionResult GetAllTeachers()
        {
            IQueryable<User> teacherQuery = db.Users
                .Where(u => u.Role == Vocabulary.RoleType.teacher)
                .Include(u => u.UserOrgs).ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents).ThenInclude(ua => ua.Agent);
            teacherQuery = ApplyBinding(teacherQuery);
            var teachers = teacherQuery.ToList();

            serializer = new OneRosterSerializer("users");
            serializer.writer.WriteStartArray();
            foreach (var teacher in teachers)
            {
                teacher.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/teachers/5
        [HttpGet("{id}")]
        public IActionResult GetTeacher([FromRoute] string id)
        {
            var teacher = db.Users
                .Include(u => u.UserOrgs).ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents).ThenInclude(ua => ua.Agent)
                .SingleOrDefault(u => u.Id == id && u.Role == Vocabulary.RoleType.teacher);

            if (teacher == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("user");
            teacher.AsJson(serializer.writer, BaseUrl());

            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/teachers/5/classes
        [HttpGet("{id}/classes")]
        public IActionResult GetClassesForTeacher([FromRoute] string id)
        {
            if (db.Users.SingleOrDefault(u => u.Id == id) == null)
            {
                return NotFound();
            }

            IQueryable<Klass> klassQuery = db.Klasses
                .Include(k => k.Enrollments).ThenInclude(e => e.User)
                .Include(k => k.Enrollments).ThenInclude(e => e.Klass.Course)
                .Include(k => k.Enrollments).ThenInclude(e => e.Klass.School)
                .Include(k => k.Enrollments).ThenInclude(e => e.Klass.KlassAcademicSessions).ThenInclude(kas => kas.AcademicSession)
                .Where(k => k.Enrollments.Where(e => e.UserId == id && e.Role == Vocabulary.RoleType.teacher).Count() > 0);
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
    }
}
