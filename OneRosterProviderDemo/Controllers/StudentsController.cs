using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OneRosterProviderDemo.Serializers;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/students")]
    public class StudentsController : BaseController
    {
        public StudentsController(ApiContext _db) : base(_db)
        {

        }

        // GET ims/oneroster/v1p1/students
        [HttpGet]
        public IActionResult GetAllStudents()
        {
            IQueryable<User> studentQuery = db.Users
                .Where(u => u.Role == Vocabulary.RoleType.student)
                .Include(u => u.UserOrgs).ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents).ThenInclude(ua => ua.Agent);
            studentQuery = ApplyBinding(studentQuery);
            var students = studentQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("users");
            serializer.writer.WriteStartArray();
            foreach (var student in students)
            {
                student.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/students/5
        [HttpGet("{id}")]
        public IActionResult GetStudent([FromRoute] string id)
        {
            var student = db.Users
                .Include(u => u.UserOrgs).ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents).ThenInclude(ua => ua.Agent)
                .SingleOrDefault(u => u.Id == id && u.Role == Vocabulary.RoleType.student);

            if (student == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("user");
            student.AsJson(serializer.writer, BaseUrl());

            return JsonOk(serializer.Finish());
        }
    }
}
