/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using System.Linq;
using OneRosterProviderDemo.Serializers;
using Microsoft.EntityFrameworkCore;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/terms")]
    public class TermsController : BaseController
    {
        public TermsController(ApiContext _db) : base(_db)
        {

        }

        // GET ims/oneroster/v1p1/terms
        [HttpGet]
        public IActionResult GetAllTerms()
        {
            IQueryable<AcademicSession> termsQuery = db.AcademicSessions
                .Where(ac => ac.Type == Vocabulary.SessionType.term);
            termsQuery = ApplyBinding(termsQuery);
            var terms = termsQuery.ToList();

            serializer = new OneRosterSerializer("terms");
            serializer.writer.WriteStartArray();
            foreach (var tern in terms)
            {
                tern.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/terms/5
        [HttpGet("{id}")]
        public IActionResult GetTerm([FromRoute] string id)
        {
            var term = db.AcademicSessions.SingleOrDefault(a => a.Id == id && a.Type == Vocabulary.SessionType.term);

            if (term == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("term");
            term.AsJson(serializer.writer, BaseUrl());
            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/terms/{id}/classes
        [HttpGet("{id}/classes")]
        public IActionResult GetClassesForTerm([FromRoute] string id)
        {
            var klassAcademicSessions = db.KlassAcademicSessions
                .Include(kas => kas.Klass)
                    .ThenInclude(k => k.Course)
                .Include(kas => kas.Klass)
                    .ThenInclude(k => k.School)
                .Include(kas => kas.AcademicSession)
                .Where(kas => kas.AcademicSessionId == id);

            if(!klassAcademicSessions.Any())
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("terms");
            serializer.writer.WriteStartArray();
            foreach (var kas in klassAcademicSessions)
            {
                kas.Klass.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();
            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/terms/{id}/gradingPeriods
        [HttpGet("{id}/gradingPeriods")]
        public IActionResult GetGradingPeriodsForTerm([FromRoute] string id)
        {
            var term = db.AcademicSessions
                .Include(s => s.Children)
                .SingleOrDefault(a => a.Id == id && a.Type == Vocabulary.SessionType.term);

            if(term == null)
            {
                return NotFound();
            }

            serializer = new OneRosterSerializer("gradingPeriods");
            serializer.writer.WriteStartArray();
            foreach (var child in term.Children)
            {
                if(child.Type == Vocabulary.SessionType.gradingPeriod)
                {
                    child.AsJson(serializer.writer, BaseUrl());
                }
            }
            serializer.writer.WriteEndArray();
            return JsonOk(serializer.Finish());
        }
    }
}
