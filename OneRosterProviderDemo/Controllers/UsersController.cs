/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OneRosterProviderDemo.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace OneRosterProviderDemo.Controllers
{
    [Route("ims/oneroster/v1p1/users")]
    public class UsersController : BaseController
    {
        public UsersController(ApiContext _db) : base(_db)
        {
        }

        // GET ims/oneroster/v1p1/users
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            IQueryable<User> userQuery = db.Users
                .Include(u => u.UserOrgs)
                    .ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents)
                    .ThenInclude(ua => ua.Agent);
            userQuery = ApplyBinding(userQuery);
            var users = userQuery.ToList();

            serializer = new Serializers.OneRosterSerializer("users");
            serializer.writer.WriteStartArray();
            foreach (var user in users)
            {
                user.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(FinishSerialization(), ResponseCount);
        }

        // GET ims/oneroster/v1p1/users/5
        [HttpGet("{id}")]
        public IActionResult GetUser([FromRoute] string id)
        {
            var user = db.Users
                .Include(u => u.UserOrgs)
                    .ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents)
                    .ThenInclude(ua => ua.Agent)
                .SingleOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            serializer = new Serializers.OneRosterSerializer("user");
            user.AsJson(serializer.writer, BaseUrl());

            return JsonOk(serializer.Finish());
        }

        // GET ims/oneroster/v1p1/users/5/classes
        [HttpGet("{id}/classes")]
        public IActionResult GetClassesForUser([FromRoute] string id)
        {
            var user = db.Users
                .Include(u => u.UserOrgs)
                    .ThenInclude(uo => uo.Org)
                .Include(u => u.UserAgents)
                    .ThenInclude(ua => ua.Agent)
                .SingleOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // get all Enrollments for the given userId
            var enrollments = db.Enrollments
                .Include(e => e.Klass)
                    .ThenInclude(k => k.KlassAcademicSessions)
                        .ThenInclude(kas => kas.AcademicSession)
                .Include(e => e.Klass)
                    .ThenInclude(k => k.Course)
                .Include(e => e.Klass)
                    .ThenInclude(k => k.School)
                .Where(e => e.UserId == id);
            serializer = new Serializers.OneRosterSerializer("classes");
            serializer.writer.WriteStartArray();
            foreach (var enrollment in enrollments)
            {
                enrollment.Klass.AsJson(serializer.writer, BaseUrl());
            }
            serializer.writer.WriteEndArray();

            return JsonOk(serializer.Finish());
        }
    }
}
