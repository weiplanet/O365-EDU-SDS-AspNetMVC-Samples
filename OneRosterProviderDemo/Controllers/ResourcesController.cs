using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using OneRosterProviderDemo.Serializers;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/resources")]
    public class ResourcesController : BaseController
    {
        public ResourcesController(ApiContext _db) : base(_db)
        {
        }

        // GET ims/oneroster/v1p1/resources
        [HttpGet]
        public IActionResult GetAllResources()
        {
            IQueryable<Resource> resourcesQuery = db.Resources;
            resourcesQuery = ApplyBinding(resourcesQuery);
            var resources = resourcesQuery.ToList();

            serializer = new OneRosterSerializer("resources");
            serializer.writer.WriteStartArray();
            foreach (var resource in resources)
            {
                resource.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/resources/5
        [HttpGet("{id}")]
        public IActionResult GetResource([FromRoute] string id)
        {
            var resource = db.Resources.SingleOrDefault(a => a.Id == id);

            if (resource == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("resource");
            resource.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }
    }
}