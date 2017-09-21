using Newtonsoft.Json;
using OneRosterProviderDemo.Validators;
using OneRosterProviderDemo.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;

namespace OneRosterProviderDemo.Models
{
    public class Klass : BaseModel
    {
        internal override string ModelType()
        {
            return "class";
        }

        internal override string UrlType()
        {
            return "classes";
        }
        [Required]
        public string Title { get; set; }

        public string ClassCode { get; set; }

        [Required]
        public KlassType ClassType { get; set; }

        public string Location { get; set; }

        [Required]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [Required]
        public string SchoolOrgId { get; set; }
        [ForeignKey("SchoolOrgId")]
        public Org School { get; set; }

        // "terms"
        [NotEmptyCollection]
        public virtual List<KlassAcademicSession> KlassAcademicSessions { get; set; } 

        public virtual List<Enrollment> Enrollments { get; set; }

        [NotMapped]
        [Grades]
        public string[] Grades
        {
            get { return _grades == null ? null : JsonConvert.DeserializeObject<string[]>(_grades); }
            set { _grades = JsonConvert.SerializeObject(value); }
        }
        private string _grades { get; set; }

        [NotMapped]
        public string[] Subjects
        {
            get {
                return SubjectCodes == null ? null : SubjectCodes.Select(code => Vocabulary.SubjectCodes.SubjectMap[code]).ToArray();
            }
        }

        [NotMapped]
        [SubjectCodes]
        public string[] SubjectCodes
        {
            get { return _subjectCodes == null ? null : JsonConvert.DeserializeObject<string[]>(_subjectCodes); }
            set { _subjectCodes = JsonConvert.SerializeObject(value); }
        }
        private string _subjectCodes { get; set; }

        [NotMapped]
        public string[] Periods
        {
            get { return _periods == null ? null : JsonConvert.DeserializeObject<string[]>(_periods); }
            set { _periods = JsonConvert.SerializeObject(value); }
        }
        private string _periods { get; set; }

        public new void AsJson(JsonWriter writer, string baseUrl)
        {
            writer.WriteStartObject();
            base.AsJson(writer, baseUrl);

            writer.WritePropertyName("title");
            writer.WriteValue(Title);

            if(!String.IsNullOrEmpty(ClassCode))
            {
                writer.WritePropertyName("classCode");
                writer.WriteValue(ClassCode);
            }

            writer.WritePropertyName("classType");
            writer.WriteValue(Enum.GetName(typeof(Vocabulary.KlassType), ClassType));

            if (!String.IsNullOrEmpty(Location))
            {
                writer.WritePropertyName("location");
                writer.WriteValue(Location);
            }

            if (Grades != null && Grades.Length > 0)
            {
                writer.WritePropertyName("grades");
                writer.WriteStartArray();
                foreach(var grade in Grades)
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
            }

            writer.WritePropertyName("course");
            Course.AsJsonReference(writer, baseUrl);

            writer.WritePropertyName("school");
            School.AsJsonReference(writer, baseUrl);

            writer.WritePropertyName("terms");
            writer.WriteStartArray();
            KlassAcademicSessions.ForEach(join => join.AcademicSession.AsJsonReference(writer, baseUrl));
            writer.WriteEndArray();

            if (SubjectCodes != null && SubjectCodes.Length > 0)
            {
                writer.WritePropertyName("subjectCodes");
                writer.WriteStartArray();
                foreach (var subjectCode in SubjectCodes)
                {
                    writer.WriteValue(subjectCode);
                }
                writer.WriteEndArray();
            }

            if (Periods != null && Periods.Length > 0)
            {
                writer.WritePropertyName("periods");
                writer.WriteStartArray();
                foreach (var period in Periods)
                {
                    writer.WriteValue(period);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
