/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using CsvHelper;
using Newtonsoft.Json;
using OneRosterProviderDemo.Vocabulary;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;

namespace OneRosterProviderDemo.Models
{
    public class Enrollment : BaseModel
    {
        internal override string ModelType()
        {
            return "enrollment";
        }

        internal override string UrlType()
        {
            return "enrollments";
        }

        [Required]
        public RoleType Role { get; set; }

        public Boolean? Primary { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string KlassId { get; set; }
        public Klass Klass { get; set; }

        [Required]
        public string SchoolOrgId { get; set; }
        [ForeignKey("SchoolOrgId")]
        public Org School { get; set; }

        public new void AsJson(JsonWriter writer, string baseUrl)
        {
            writer.WriteStartObject();
            base.AsJson(writer, baseUrl);

            writer.WritePropertyName("user");
            User.AsJsonReference(writer, baseUrl);

            writer.WritePropertyName("class");
            Klass.AsJsonReference(writer, baseUrl);

            writer.WritePropertyName("school");
            School.AsJsonReference(writer, baseUrl);

            writer.WritePropertyName("role");
            writer.WriteValue(Enum.GetName(typeof(Vocabulary.RoleType), Role));

            if (Primary != null)
            {
                writer.WritePropertyName("primary");
                writer.WriteValue(Primary.ToString());
            }

            if (BeginDate != null)
            {
                writer.WritePropertyName("beginDate");
                writer.WriteValue(BeginDate.ToString("yyyy-MM-dd"));
            }

            if (EndDate != null)
            {
                writer.WritePropertyName("endDate");
                writer.WriteValue(EndDate.ToString("yyyy-MM-dd"));
            }

            writer.WriteEndObject();
            writer.Flush();
        }

        public static new void CsvHeader(CsvWriter writer)
        {
            BaseModel.CsvHeader(writer);
            writer.WriteField("classSourcedId");
            writer.WriteField("schoolSourcedId");
            writer.WriteField("userSourcedId");
            writer.WriteField("role");
            writer.WriteField("primary");
            writer.WriteField("beginDate");
            writer.WriteField("endDate");

            writer.NextRecord();
        }

        public new void AsCsvRow(CsvWriter writer, bool bulk = true)
        {
            base.AsCsvRow(writer, bulk);
            writer.WriteField(KlassId);
            writer.WriteField(SchoolOrgId);
            writer.WriteField(UserId);
            writer.WriteField(Role);
            writer.WriteField(Primary == null ? Primary.ToString() : "");
            writer.WriteField(BeginDate.ToString("yyyy-MM-dd"));
            writer.WriteField(EndDate.ToString("yyyy-MM-dd"));

            writer.NextRecord();
        }
    }
}
