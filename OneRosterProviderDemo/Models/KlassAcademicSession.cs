using System;

namespace OneRosterProviderDemo.Models
{
    public class KlassAcademicSession : BaseModel
    {
        internal override string ModelType()
        {
            throw new NotImplementedException();
        }

        internal override string UrlType()
        {
            throw new NotImplementedException();
        }

        public string KlassId { get; set; }
        public Klass Klass { get; set; }

        public string AcademicSessionId { get; set; }
        public AcademicSession AcademicSession { get; set; }
    }
}
