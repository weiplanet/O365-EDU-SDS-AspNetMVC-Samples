/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Serializers;
using OneRosterProviderDemo.Models;

namespace OneRosterProviderDemo.Controllers
{
    public class CsvController : Controller
    {
        private ApiContext db;
        public CsvController(ApiContext _db)
        {
            db = _db;
        }
        [Route("csv/bulk")]
        public void Bulk()
        {
            Response.ContentType = "binary/octet-stream";
            Response.Headers["Content-Disposition"] = "attachment; filename=oneroster.zip";
            var serializer = new CsvSerializer(db);
            serializer.Serialize(Response.Body);
        }
    }
}