using System;
using System.ComponentModel.DataAnnotations;

namespace OneRosterProviderDemo.Models
{
    public class OauthNonce
    {
        [Key, Required]
        public string Value { get; set; }

        [Required]
        public DateTime UsedAt { get; set; }

        public bool CanBeUsed()
        {
            long elapsedTicks = DateTime.Now.Ticks - UsedAt.Ticks;

            var elapsedSpan = new TimeSpan(elapsedTicks);

            return elapsedSpan.Minutes > 90;
        }
    }
}
