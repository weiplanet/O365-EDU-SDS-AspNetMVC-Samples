/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OneRosterProviderDemo.ActionResults;
using OneRosterProviderDemo.Exceptions;
using OneRosterProviderDemo.Models;
using OneRosterProviderDemo.Serializers;
using System.Collections.Generic;
using System.Linq;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1")]
    public class BaseController : Controller
    {
        internal OneRosterSerializer serializer;
        internal readonly ApiContext db;
        internal string BaseUrl() => $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/ims/oneroster/v1p1";
        internal string SortField() => Request.Query["sort"];
        internal bool SortDesc() => Request.Query["orderBy"] == "desc";
        internal int Offset() => QueryPositiveInt("offset", 0);
        internal int Limit() => QueryPositiveInt("limit", 100);
        internal IQueryable<T> ApplyPaging<T>(IQueryable<T> modelQuery) => modelQuery.Skip(Offset()).Take(Limit());
        internal List<OneRosterException> exceptions = new List<OneRosterException>();
        internal int? ResponseCount;

        internal int QueryPositiveInt(string name, int defaultValue)
        {
            int val;
            if (int.TryParse(Request.Query[name], out val) && val >= 0)
            {
                return val;
            }
            return defaultValue;
        }

        internal IQueryable<T> ApplyBinding<T>(IQueryable<T> modelQuery)
        {
            var query = modelQuery;
            try
            {
                query = BaseModel.ApplyFilter(query, Request.Query["filter"]);
            }
            catch(InvalidFilterFieldException e)
            {
                exceptions.Add(e);
            }

            ResponseCount = query.Count();

            try
            {
                query = BaseModel.ApplySort(query, Request.Query["sort"], Request.Query["orderBy"]);
            }
            catch(InvalidSortFieldException e2)
            {
                exceptions.Add(e2);
            }

            query = ApplyPaging(query);

            return query;
        }

        internal void SerializeExceptions()
        {
            var writer = serializer.writer;

            if (writer.WriteState == WriteState.Object)
            {
                writer.WritePropertyName("statusInfoSet");
            }
            
            writer.WriteStartArray();

            foreach (var exception in exceptions)
            {
                exception.AsJson(writer, ControllerContext.ActionDescriptor.ActionName);
            }
            writer.WriteEndArray();
        }

        internal string FinishSerialization()
        {
            if (exceptions.Count > 0)
            {
                SerializeExceptions();
            }
            return serializer.Finish();
        }

        public BaseController(ApiContext _db)
        {
            db = _db;
        }

        public IActionResult JsonOk(string json)
        {
            return JsonOk(json, null);
        }

        public IActionResult JsonOk(string json, int? count)
        {
            return JsonWithStatus(json, count, 200);
        }

        public IActionResult JsonWithStatus(string json, int? count, int status)
        {
            if (exceptions.Any(e => e.GetType() == typeof(InvalidFilterFieldException)))
            {
                return ErrorResult();
            }
            
            return new OneRosterResult
            {
                Content = json,
                ContentType = "application/json",
                StatusCode = status,
                count = count
            };
        }

        public IActionResult ErrorResult()
        {
            serializer = new OneRosterSerializer("statusInfoSet");
            SerializeExceptions();

            return new OneRosterResult
            {
                Content = serializer.Finish(),
                ContentType = "application/json",
                StatusCode = 400
            };
        }
    }
}
