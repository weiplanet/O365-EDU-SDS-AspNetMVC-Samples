/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using Microsoft.EntityFrameworkCore;
using OneRosterProviderDemo.Serializers;
using System.Linq;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/schools")]
    public class SchoolsController : BaseController
    {
        public SchoolsController(ApiContext _db) : base(_db)
        {

        }

        // GET ims/oneroster/v1p1/schools
        [HttpGet]
        public IActionResult GetAllSchools()
        {
            IQueryable<Models.Org> orgsQuery = db.Orgs
                .Where(o => o.Type == Vocabulary.OrgType.school)
                .Include(o => o.Parent)
                .Include(o => o.Children);
            orgsQuery = ApplyBinding(orgsQuery);
            var orgs = orgsQuery.ToList();

            serializer = new OneRosterSerializer("orgs");
            serializer.writer.WriteStartArray();
            foreach (var org in orgs)
            {
                org.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        private Org LookupSchool(string id)
        {
            return db.Orgs
                .Where(o => o.Type == Vocabulary.OrgType.school)
                .Include(o => o.Parent)
                .Include(o => o.Children)
                .SingleOrDefault(o => o.Id == id);
        }

        // GET ims/oneroster/v1p1/schools/5
        [HttpGet("{id}")]
        public IActionResult GetSchool([FromRoute] string id)
        {
            var org = LookupSchool(id);

            if (org == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("org");
            org.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }

        // GET schools/{id}/courses
        [HttpGet("{id}/courses")]
        public IActionResult GetCoursesForSchool([FromRoute] string id)
        {
            var courses = db.Courses
                .Include(c => c.SchoolYearAcademicSession)
                .Include(c => c.Org)
                .Where(c => c.OrgId == id);

            if(!courses.Any())
            {
                return NotFound();
            }
            serializer = new OneRosterSerializer("courses");
            serializer.writer.WriteStartArray();
            foreach (var course in courses)
            {
                course.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/schools/{id}/enrollments
        [HttpGet("{id}/enrollments")]
        public IActionResult GetEnrollmentsForSchool([FromRoute] string id)
        {
            var enrollments = db.Enrollments
                .Include(e => e.User)
                .Include(e => e.Klass)
                .Include(e => e.School)
                .Where(e => e.Klass.SchoolOrgId == id);

            if(!enrollments.Any())
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("enrollments");
            serializer.writer.WriteStartArray();
            foreach (var enrollment in enrollments)
            {
                enrollment.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/schools/{id}/classes
        [HttpGet("{id}/classes")]
        public IActionResult GetClassesForSchool([FromRoute] string id)
        {
            var klasses = db.Klasses
                .Include(k => k.KlassAcademicSessions)
                    .ThenInclude(kas => kas.AcademicSession)
                .Include(k => k.Course)
                .Include(k => k.School)
                .Where(k => k.SchoolOrgId == id);

            if(!klasses.Any())
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("classes");
            serializer.writer.WriteStartArray();
            foreach (var klass in klasses)
            {
                klass.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();
            return JsonOk(serializer.Finish());
        }   

        // GET ims/oneroster/v1p1/schools/{id}/students
        [HttpGet("{id}/students")]
        public IActionResult GetStudentsForSchool([FromRoute] string id)
        {
            var students = db.Users
                .Where(u => u.Role == Vocabulary.RoleType.student)
                .Include(u => u.UserOrgs).ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents).ThenInclude(ua => ua.Agent);

            if(!students.Any())
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("students");
            serializer.writer.WriteStartArray();
            foreach(var student in students)
            {
                foreach(var org in student.UserOrgs)
                {
                    if(org.OrgId == id)
                    {
                        student.AsJson(serializer.writer, BaseUrl());
                    }
                }
            }
            serializer.writer.WriteEndArray();
            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/schools/{id}/teachers
        [HttpGet("{id}/teachers")]
        public IActionResult GetTeachersForSchool([FromRoute] string id)
        {
            var teachers = db.Users
                .Where(u => u.Role == Vocabulary.RoleType.teacher)
                .Include(u => u.UserOrgs).ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents).ThenInclude(ua => ua.Agent);
    
            if(!teachers.Any())
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("teachers");
            serializer.writer.WriteStartArray();
            foreach (var teacher in teachers)
            {
                foreach(var org in teacher.UserOrgs)
                {
                    if(org.OrgId == id)
                    {
                        teacher.AsJson(serializer.writer, BaseUrl());
                    }
                }
            }
            serializer.writer.WriteEndArray();
            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/schools/{id}/terms
        [HttpGet("{id}/terms")]
        public IActionResult GetTermsForSchool([FromRoute] string id)
        {
            var courses = db.Courses
                .Include(c => c.SchoolYearAcademicSession)
                .Where(c => c.OrgId == id);

            if (!courses.Any())
            {
                return NotFound();
            }

            var terms = courses.Select(c => c.SchoolYearAcademicSession);

            serializer = new OneRosterSerializer("terms");
            serializer.writer.WriteStartArray();
            foreach (var term in terms)
            {
                term.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();
            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/schools/{school_id}/classes/class_id}/enrollments
        [HttpGet("{schoolId}/classes/{classId}/enrollments")]
        public IActionResult GetEnrollmentsForClassInSchool([FromRoute] string schoolId, string classId)
        {
            var klass = db.Klasses
                .Where(k => k.SchoolOrgId == schoolId)
                .Include(k => k.Enrollments)
                    .ThenInclude(e => e.User)
                .Include(k => k.Enrollments)
                    .ThenInclude(e => e.Klass)
                .Include(k => k.Enrollments)
                    .ThenInclude(e => e.School)
                .SingleOrDefault(k => k.Id == classId);

            if(klass == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("enrollments");
            serializer.writer.WriteStartArray();
            foreach (var enrollment in klass.Enrollments)
            {
                enrollment.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();
            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/schools/{school_id}/classes/{class_id}/students
        [HttpGet("{schoolId}/classes/{classId}/students")]
        public IActionResult GetStudentsForClassInSchool([FromRoute] string schoolId, string classId)
        {
            var klass = db.Klasses
                .Where(k => k.SchoolOrgId == schoolId)
                .Include(k => k.Enrollments)
                    .ThenInclude(e => e.User)
                        .ThenInclude(u => u.UserOrgs)
                            .ThenInclude(uo => uo.Org)
                .Include(k => k.Enrollments)
                    .ThenInclude(e => e.User)
                        .ThenInclude(u => u.UserAgents)
                            .ThenInclude(ua => ua.Agent)
                .SingleOrDefault(k => k.Id == classId);

            if(klass == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("students");
            serializer.writer.WriteStartArray();
            foreach (var enrollment in klass.Enrollments)
            {
                var user = enrollment.User;
                if(user.Role == Vocabulary.RoleType.student)
                {
                    user.AsJson(serializer.writer, BaseUrl());
                }
            }
            serializer.writer.WriteEndArray();
            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/schools/{school_id}/classes/{class_id}/teachers
        [HttpGet("{schoolId}/classes/{classId}/teachers")]
        public IActionResult GetTeachersForClassInSchool([FromRoute] string schoolId, string classId)
        {
            var klass = db.Klasses
                .Where(k => k.SchoolOrgId == schoolId)
                .Include(k => k.Enrollments)
                    .ThenInclude(e => e.User)
                        .ThenInclude(u => u.UserOrgs)
                            .ThenInclude(uo => uo.Org)
                .Include(k => k.Enrollments)
                    .ThenInclude(e => e.User)
                        .ThenInclude(u => u.UserAgents)
                            .ThenInclude(ua => ua.Agent)
                .SingleOrDefault(k => k.Id == classId);

            if (klass == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("teachers");
            serializer.writer.WriteStartArray();
            foreach (var enrollment in klass.Enrollments)
            {
                var user = enrollment.User;
                if (user.Role == Vocabulary.RoleType.teacher)
                {
                    user.AsJson(serializer.writer, BaseUrl());
                }
            }
            serializer.writer.WriteEndArray();
            return JsonOk(serializer.Finish());
        }
    }
}
