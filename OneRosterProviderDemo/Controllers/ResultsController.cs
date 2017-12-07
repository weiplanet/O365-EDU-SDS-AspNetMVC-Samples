/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using OneRosterProviderDemo.Serializers;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/results")]
    public class ResultsController : BaseController
    {
        public ResultsController(ApiContext _db) : base(_db)
        {

        }

        // GET ims/oneroster/v1p1/results
        [HttpGet]
        public IActionResult GetAllResults()
        {
            IQueryable<Result> resultsQuery = db.Results
                .Include(r => r.LineItem)
                .Include(r => r.Student);
            resultsQuery = ApplyBinding(resultsQuery);
            var results = resultsQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("results");
            serializer.writer.WriteStartArray();
            foreach (var result in results)
            {
                result.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/results/5
        [HttpGet("{id}")]
        public IActionResult GetResult([FromRoute] string id)
        {
            var result = db.Results
                .Include(r => r.LineItem)
                .Include(r => r.Student)
                .SingleOrDefault(li => li.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            serializer = new Serializers.OneRosterSerializer("result");
            result.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }

        // DELETE ims/oneroster/v1p1/results/5
        [HttpDelete("{id}")]
        public IActionResult DeleteResult ([FromRoute] string id)
        {
            var result = db.Results.SingleOrDefault(c => c.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            db.Results.Remove(result);
            db.SaveChanges();

            return NoContent();
        }

        // PUT ims/oneroster/v1p1/results/5
        [HttpPut("{id}")]
        public IActionResult PutResult([FromRoute] string id)
        {
            var insert = false;
            var result = db.Results.SingleOrDefault(r => r.Id == id);

            if (result == null)
            {
                result = new Result()
                {
                    Id = id
                };
                insert = true;
            }

            using (var reader = new StreamReader(Request.Body))
            {
                var requestJson = (JObject)JObject.Parse(reader.ReadToEnd())["result"];

                if (!result.UpdateWithJson(requestJson))
                {
                    return new StatusCodeResult(422);
                }
            }

            if (TryValidateModel(result))
            {
                if (insert)
                {
                    db.Results.Add(result);
                }
                db.SaveChanges();

                db.Results
                    .Include(r => r.LineItem)
                    .Include(r => r.Student)
                    .Where(r => r.Id == id)
                    .First();

                serializer = new OneRosterSerializer("result");
                result.AsJson(serializer.writer, BaseUrl());

                if (insert)
                {
                    return JsonWithStatus(serializer.Finish(), null, 201);
                }
                return JsonOk(serializer.Finish());
            }
            else
            {
                return new StatusCodeResult(422);
            }
        }
    }
}