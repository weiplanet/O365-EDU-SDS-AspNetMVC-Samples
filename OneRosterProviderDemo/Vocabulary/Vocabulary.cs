/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using CsvHelper;
using System.Collections.Generic;
using System.IO;

// https://www.imsglobal.org/oneroster-v11-final-specification#_Toc480452020
namespace OneRosterProviderDemo.Vocabulary
{
    public enum KlassType { homeroom, scheduled }
    public enum Gender { male, female }
    public enum Importance { primary, secondary }
    public enum OrgType { department, school, district, local, state, national }
    public enum RoleType { administrator, aide, guardian, parent, proctor, relative, student, teacher }
    public enum ScoreStatus { exempt, fully_graded, not_submitted, partially_graded, submitted }
    public enum SessionType { gradingPeriod, semester, schoolYear, term }
    public enum StatusType { active, tobedeleted, inactive }

    public class Grades
    {
        public static ICollection<string> Members = new HashSet<string>(new string[]
        {
            "IT", "PR", "PK", "TK", "KG", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "PS", "UG", "Other"
        });
    }

    public class SubjectCodes
    {
        public static Dictionary<string, string> SubjectMap;
        public static void Initialize()
        {
            SubjectMap = new Dictionary<string, string>();

            using (StreamReader sr = new StreamReader("Vocabulary/sced-v4.csv"))
            {
                var csv = new CsvReader(sr);
                
                while( csv.Read() )
                {
                    var code = csv.GetField<string>(0);
                    var title = csv.GetField<string>(1);
                    SubjectMap.Add(code, title);
                }
            }
        }
    }
}
