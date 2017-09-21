using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OneRosterProviderDemo.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;

namespace OneRosterProviderDemo.Models
{
    public class AcademicSession : BaseModel
    {
        internal override string ModelType()
        {
            return "academicSession";
        }

        internal override string UrlType()
        {
            return "academicSessions";
        }

        [Required]
        public string Title { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public SessionType Type { get; set; }

        [Required]
        public string SchoolYear { get; set; }

        // Associations
        public string ParentAcademicSessionId { get; set; }
        public virtual AcademicSession ParentAcademicSession { get; set; }
        [InverseProperty("ParentAcademicSession")]
        public virtual List<AcademicSession> Children { get; set; }

        public new void AsJson(JsonWriter writer, string baseUrl)
        {
            writer.WriteStartObject();
            base.AsJson(writer, baseUrl);

            writer.WritePropertyName("title");
            writer.WriteValue(Title);
            writer.WritePropertyName("startDate");
            writer.WriteValue(StartDate.ToString("yyyy-MM-dd"));
            writer.WritePropertyName("endDate");
            writer.WriteValue(EndDate.ToString("yyyy-MM-dd"));
            writer.WritePropertyName("type");
            writer.WriteValue(Enum.GetName(typeof(Vocabulary.SessionType), Type));

            if (ParentAcademicSession != null)
            {
                writer.WritePropertyName("parent");
                ParentAcademicSession.AsJsonReference(writer, baseUrl);
            }

            if (Children != null && Children.Count > 0)
            {
                writer.WritePropertyName("children");
                writer.WriteStartArray();
                Children.ForEach(child => child.AsJsonReference(writer, baseUrl));
                writer.WriteEndArray();
            }

            writer.WritePropertyName("schoolYear");
            writer.WriteValue(SchoolYear);

            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
