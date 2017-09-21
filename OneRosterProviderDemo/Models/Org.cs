using Newtonsoft.Json;
using OneRosterProviderDemo.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;

namespace OneRosterProviderDemo.Models
{
    public class Org : BaseModel
    {
        internal override string ModelType()
        {
            return "org";
        }

        internal override string UrlType()
        {
            return "orgs";
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public OrgType Type { get; set; }

        public string Identifier { get; set; }

        public string ParentOrgId { get; set; }
        
        public virtual Org Parent { get; set; }

        [InverseProperty("Parent")]
        public virtual List<Org> Children { get; set; }

        public new void AsJson(JsonWriter writer, string baseUrl)
        {
            writer.WriteStartObject();
            base.AsJson(writer, baseUrl);

            writer.WritePropertyName("name");
            writer.WriteValue(Name);

            writer.WritePropertyName("type");
            writer.WriteValue(Enum.GetName(typeof(Vocabulary.OrgType), Type));

            if (!String.IsNullOrEmpty(Identifier))
            {
                writer.WritePropertyName("identifier");
                writer.WriteValue(Identifier);
            }

            if (Parent != null)
            {
                writer.WritePropertyName("parent");
                Parent.AsJsonReference(writer, baseUrl);
            }

            if (Children != null && Children.Count > 0)
            {
                writer.WritePropertyName("children");
                writer.WriteStartArray();
                Children.ForEach(child => child.AsJsonReference(writer, baseUrl));
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
