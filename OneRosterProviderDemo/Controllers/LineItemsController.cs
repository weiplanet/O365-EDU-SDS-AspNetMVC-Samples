/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneRosterProviderDemo.Models;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using OneRosterProviderDemo.Serializers;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/lineItems")]
    public class LineItemsController : BaseController
    {
        public LineItemsController(ApiContext _db) : base(_db)
        {
        }

        // GET ims/oneroster/v1p1/lineItems
        [HttpGet]
        public IActionResult GetAllLineItems()
        {
            IQueryable<LineItem> lineItemsQuery = db.LineItems
                .Include(li => li.LineItemCategory)
                .Include(li => li.Klass)
                .Include(li => li.AcademicSession);
            lineItemsQuery = ApplyBinding(lineItemsQuery);
            var lineItems = lineItemsQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("lineItems");
            serializer.writer.WriteStartArray();
            foreach (var lineItem in lineItems)
            {
                lineItem.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/lineItems/5
        [HttpGet("{id}")]
        public IActionResult GetLineItem([FromRoute] string id)
        {
            var lineItem = db.LineItems
                .Include(li => li.LineItemCategory)
                .Include(li => li.Klass)
                .Include(li => li.AcademicSession)
                .SingleOrDefault(li => li.Id == id);

            if (lineItem == null)
            {
                return NotFound();
            }

            serializer = new Serializers.OneRosterSerializer("lineItem");
            lineItem.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }

        // DELETE ims/oneroster/v1p1/lineItems/5
        [HttpDelete("{id}")]
        public IActionResult DeleteLineItem([FromRoute] string id)
        {
            var lineItem = db.LineItems.SingleOrDefault(c => c.Id == id);

            if (lineItem == null)
            {
                return NotFound();
            }

            db.LineItems.Remove(lineItem);
            db.SaveChanges();

            return NoContent();
        }

        // PUT ims/oneroster/v1p1/lineItems/5
        [HttpPut("{id}")]
        public IActionResult PutLineItem([FromRoute] string id)
        {
            var insert = false;
            var lineItem = db.LineItems.SingleOrDefault(li => li.Id == id);

            if (lineItem == null)
            {
                lineItem = new LineItem()
                {
                    Id = id
                };
                insert = true;
            }

            using (var reader = new StreamReader(Request.Body))
            {
                var requestJson = (JObject)JObject.Parse(reader.ReadToEnd())["lineItem"];

                if (!lineItem.UpdateWithJson(requestJson))
                {
                    return new StatusCodeResult(422);
                }
            }

            if (TryValidateModel(lineItem))
            {
                if (insert)
                {
                    db.LineItems.Add(lineItem);
                }
                db.SaveChanges();

                db.LineItems
                    .Include(li => li.AcademicSession)
                    .Include(li => li.Klass)
                    .Include(li => li.LineItemCategory)
                    .Where(li => li.Id == id)
                    .First();

                serializer = new OneRosterSerializer("lineItem");
                lineItem.AsJson(serializer.writer, BaseUrl());
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