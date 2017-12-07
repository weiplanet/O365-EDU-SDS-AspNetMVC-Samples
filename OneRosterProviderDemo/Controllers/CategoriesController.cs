/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/categories")]
    public class CategoriesController : BaseController
    {
        public CategoriesController(ApiContext _db) : base(_db)
        {
        }

        // GET ims/oneroster/v1p1/categories
        [HttpGet]
        public IActionResult GetAllCategories()
        {
            IQueryable<LineItemCategory> categoryQuery = db.LineItemCategories;
            categoryQuery = ApplyBinding(categoryQuery);
            var categories = categoryQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("categories");
            serializer.writer.WriteStartArray();
            foreach (var category in categories)
            {
                category.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/categories/5
        [HttpGet("{id}")]
        public IActionResult GetCategory([FromRoute] string id)
        {
            var category = db.LineItemCategories.SingleOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            serializer = new Serializers.OneRosterSerializer("category");
            category.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCategory([FromRoute] string id)
        {
            var category = db.LineItemCategories.SingleOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            db.LineItemCategories.Remove(category);
            db.SaveChanges();

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult PutCategory([FromRoute] string id)
        {
            var insert = false;
            var category = db.LineItemCategories.SingleOrDefault(r => r.Id == id);

            if (category == null)
            {
                category = new LineItemCategory()
                {
                    Id = id
                };
                insert = true;
            }

            using (var reader = new StreamReader(Request.Body))
            {
                var requestJson = (JObject)JObject.Parse(reader.ReadToEnd())["result"];

                if (!category.UpdateWithJson(requestJson))
                {
                    return new StatusCodeResult(422);
                }
            }

            if (TryValidateModel(category))
            {
                if (insert)
                {
                    db.LineItemCategories.Add(category);
                }
                db.SaveChanges();

                if (insert)
                {
                    return new StatusCodeResult(201);
                }
                return new StatusCodeResult(200);
            }
            else
            {
                return new StatusCodeResult(422);
            }
        }
    }
}
