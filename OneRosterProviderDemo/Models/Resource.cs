/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OneRosterProviderDemo.Vocabulary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text;

//title 0..1
//For example: Organic Chemistry

//roles 0..*
//The set of roles.See subsection 4.13.5 for the enumeration list.

//importance 0..1
//See subsection 4.13.3 for the enumeration list.

//vendorResourceId 1
//Unique identifier for the resource allocated by the vendor.

//vendorId 0..1
//Identifier for the vendor who created the resource. This will be assigned by IMS as part of Conformance Certification.

//applicationId 0..1
//Identifier for the application associated with the resource.

namespace OneRosterProviderDemo.Models
{
    public class Resource : BaseModel
    {
        internal override string ModelType()
        {
            return "resource";
        }

        internal override string UrlType()
        {
            return "resources";
        }
        
        public string Title { get; set; }
        //public ? Roles { get; set; }
        //public ? Importance { get; set; }

        [Required]
        public string VendorResourceId { get; set; }

        public string VendorId { get; set; }
        public string ApplicationId { get; set; }

        public new void AsJson(JsonWriter writer, string baseUrl)
        {
            writer.WriteStartObject();

            base.AsJson(writer, baseUrl);

            if (Title != null)
            {
                writer.WritePropertyName("title");
                writer.WriteValue(Title);
            }
            // Roles
            // Importance

            writer.WritePropertyName("vendorResourceId");
            writer.WriteValue(VendorResourceId);

            if (VendorId != null)
            {
                writer.WritePropertyName("vendorId");
                writer.WriteValue(Title);
            }

            if (ApplicationId != null)
            {
                writer.WritePropertyName("applicationId");
                writer.WriteValue(ApplicationId);
            }

            //if (Children != null && Children.Count > 0)
            //{
            //    writer.WritePropertyName("children");
            //    writer.WriteStartArray();
            //    Children.ForEach(child => child.AsJsonReference(writer, baseUrl));
            //    writer.WriteEndArray();
            //}

            writer.WriteEndObject();
        }

        public static new void CsvHeader(CsvWriter writer)
        {
            BaseModel.CsvHeader(writer);

            writer.WriteField("title");
            writer.WriteField("vendorResourceId");
            writer.WriteField("vendorId");
            writer.WriteField("applicationId");

            writer.NextRecord();
        }

        public new void AsCsvRow(CsvWriter writer, bool bulk = true)
        {
            base.AsCsvRow(writer, bulk);

            writer.WriteField(Title);
            writer.WriteField(VendorResourceId);
            writer.WriteField(VendorId);
            writer.WriteField(ApplicationId);

            writer.NextRecord();
        }
    }
}
