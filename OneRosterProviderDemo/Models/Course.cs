/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using CsvHelper;
using Newtonsoft.Json;
using OneRosterProviderDemo.Validators;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OneRosterProviderDemo.Models
{
    public class Course : BaseModel
    {
        internal override string ModelType()
        {
            return "course";
        }

        internal override string UrlType()
        {
            return "courses";
        }

        [Required]
        public string Title { get; set; }

        // SchoolYear
        public string SchoolYearAcademicSessionId { get; set; }
        public AcademicSession SchoolYearAcademicSession { get; set; }

        public string CourseCode { get; set; }

        [Required]
        public string OrgId { get; set; }
        public Org Org { get; set; }

        [NotMapped]
        public string[] Resources
        {
            get { return _resources == null ? null : JsonConvert.DeserializeObject<string[]>(_resources); }
            set { _resources = JsonConvert.SerializeObject(value); }
        }
        private string _resources { get; set; }

        [NotMapped]
        [Grades]
        public string[] Grades
        {
            get { return _grades == null ? null : JsonConvert.DeserializeObject<string[]>(_grades); }
            set { _grades = JsonConvert.SerializeObject(value); }
        }
        private string _grades { get; set; }

        [NotMapped]
        public string[] Subjects => SubjectCodes?.Select(code => Vocabulary.SubjectCodes.SubjectMap[code]).ToArray();

        [NotMapped]
        [SubjectCodes]
        public string[] SubjectCodes
        {
            get { return _subjectCodes == null ? null : JsonConvert.DeserializeObject<string[]>(_subjectCodes); }
            set { _subjectCodes = JsonConvert.SerializeObject(value); }
        }
        private string _subjectCodes { get; set; }

        public new void AsJson(JsonWriter writer, string baseUrl)
        {
            writer.WriteStartObject();
            base.AsJson(writer, baseUrl);

            writer.WritePropertyName("title");
            writer.WriteValue(Title);

            if (SchoolYearAcademicSession != null)
            {
                writer.WritePropertyName("schoolYear");
                SchoolYearAcademicSession.AsJsonReference(writer, baseUrl);
            }

            if (!String.IsNullOrEmpty(CourseCode))
            {
                writer.WritePropertyName("courseCode");
                writer.WriteValue(CourseCode);
            }

            if (Grades != null && Grades.Length > 0)
            {
                writer.WritePropertyName("grades");
                writer.WriteStartArray();
                foreach (var grade in Grades)
                {
                    writer.WriteValue(grade);
                }
                writer.WriteEndArray();
            }

            if (Subjects != null && Subjects.Length > 0)
            {
                writer.WritePropertyName("subjects");
                writer.WriteStartArray();
                foreach (var subject in Subjects)
                {
                    writer.WriteValue(subject);
                }
                writer.WriteEndArray();

                writer.WritePropertyName("subjectCodes");
                writer.WriteStartArray();
                foreach (var subjectCode in SubjectCodes)
                {
                    writer.WriteValue(subjectCode);
                }
                writer.WriteEndArray();
            }

            if (Resources != null && Resources.Length > 0)
            {
                writer.WritePropertyName("resources");
                writer.WriteStartArray();
                foreach (var resource in Resources)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("href");
                    writer.WriteValue(baseUrl + "/resources/" + resource);
                    writer.WritePropertyName("sourceId");
                    writer.WriteValue(resource);
                    writer.WritePropertyName("type");
                    writer.WriteValue("resource");
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WritePropertyName("org");
            Org.AsJsonReference(writer, baseUrl);

            writer.WriteEndObject();
            writer.Flush();
        }

        public static new void CsvHeader(CsvWriter writer)
        {
            BaseModel.CsvHeader(writer);
            writer.WriteField("schoolYearSourcedId");
            writer.WriteField("title");
            writer.WriteField("courseCode");
            writer.WriteField("grades");
            writer.WriteField("orgSourcedId");
            writer.WriteField("subjects");
            writer.WriteField("subjectCodes");

            writer.NextRecord();
        }

        public new void AsCsvRow(CsvWriter writer, bool bulk = true)
        {
            base.AsCsvRow(writer, bulk);
            writer.WriteField(SchoolYearAcademicSessionId);
            writer.WriteField(Title);
            writer.WriteField(CourseCode);
            writer.WriteField(String.Join(',', Grades));
            writer.WriteField(OrgId);
            writer.WriteField(String.Join(',', Subjects));
            writer.WriteField(String.Join(',', SubjectCodes));

            writer.NextRecord();
        }
    }
}
