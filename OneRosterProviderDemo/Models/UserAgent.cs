using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneRosterProviderDemo.Models
{
    public class UserAgent : BaseModel
    {
        internal override string ModelType()
        {
            throw new NotImplementedException();
        }

        internal override string UrlType()
        {
            throw new NotImplementedException();
        }

        public string SubjectUserId { get; set; }
        [ForeignKey("SubjectUserId")]
        public User Subject { get; set; }

        public string AgentUserId { get; set; }
        [ForeignKey("AgentUserId")]
        public User Agent { get; set; }
    }
}
