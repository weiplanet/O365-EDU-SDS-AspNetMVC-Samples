/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using System.Linq;
using OneRosterProviderDemo.Serializers;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/gradingPeriods")]
    public class GradingPeriodsController : BaseController
    {
        public GradingPeriodsController(ApiContext _db) : base(_db)
        {

        }

        // GET ims/oneroster/v1p1/gradingPeriods
        [HttpGet]
        public IActionResult GetAllGradingPeriods()
        {
            IQueryable<AcademicSession> sessionsQuery = db.AcademicSessions.Where(a => a.Type == Vocabulary.SessionType.gradingPeriod);
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

        // GET ims/oneroster/v1p1/gradingPeriods/5
        [HttpGet("{id}")]
        public IActionResult GetGradingPeriod(string id)
        {
            var session = db.AcademicSessions.FirstOrDefault(a => a.Id == id && a.Type == Vocabulary.SessionType.gradingPeriod);

            if (session == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("academicSession");
            session.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }
    }
}
