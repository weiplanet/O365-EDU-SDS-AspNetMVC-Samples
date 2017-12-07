/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using OneRosterProviderDemo.Serializers;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/academicSessions")]
    public class AcademicSessionsController : BaseController
    {
        public AcademicSessionsController(ApiContext _db) : base(_db)
        {
        }

        // GET ims/oneroster/v1p1/academicSessions
        [HttpGet]
        public IActionResult GetAllAcademicSessions()
        {
            IQueryable<AcademicSession> sessionsQuery = db.AcademicSessions;
            sessionsQuery = ApplyBinding(sessionsQuery);
            var sessions = sessionsQuery.ToList();

            serializer = new OneRosterSerializer("academicSessions");
            serializer.writer.WriteStartArray();
            foreach (var session in sessions)
            {
                session.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/academicSessions/5
        [HttpGet("{id}")]
        public IActionResult GetAcademicSession([FromRoute] string id)
        {
            var academicSession = db.AcademicSessions.SingleOrDefault(a => a.Id == id);

            if (academicSession == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("academicSession");
            academicSession.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }
    }
}
