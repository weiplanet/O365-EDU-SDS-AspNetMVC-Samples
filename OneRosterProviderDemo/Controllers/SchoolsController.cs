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

        // GET ims/oneroster/v1p1/schools/5
        [HttpGet("{id}")]
        public IActionResult GetSchool([FromRoute] string id)
        {
            var org = db.Orgs
                .Where(o => o.Type == Vocabulary.OrgType.school)
                .Include(o => o.Parent)
                .Include(o => o.Children)
                .SingleOrDefault(a => a.Id == id);

            if (org == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("org");
            org.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }
    }
}
