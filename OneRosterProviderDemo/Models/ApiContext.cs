/*
 * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.
* See LICENSE in the project root for license information.
*/

using Microsoft.EntityFrameworkCore;

namespace OneRosterProviderDemo.Models
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options) { }
        public DbSet<AcademicSession> AcademicSessions { get; set; }
        public DbSet<Klass> Klasses { get; set; }
        public DbSet<KlassAcademicSession> KlassAcademicSessions { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<LineItem> LineItems { get; set; }
        public DbSet<LineItemCategory> LineItemCategories { get; set; }
        public DbSet<Org> Orgs { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserAgent> UserAgents { get; set; }
        public DbSet<UserOrg> UserOrgs { get; set; }
        public DbSet<OauthNonce> OauthNonces { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Klass>()
                .Property("_grades");
            modelBuilder.Entity<Klass>()
                .Property("_subjectCodes");
            modelBuilder.Entity<Klass>()
                .Property("_periods");

            modelBuilder.Entity<Course>()
                .Property("_grades");
            modelBuilder.Entity<Course>()
                .Property("_subjectCodes");

            modelBuilder.Entity<User>()
                .Property("_grades");

            modelBuilder.Entity<UserAgent>()
                .HasOne(ua => ua.Subject)
                .WithMany(u => u.UserAgents)
                .HasForeignKey(ua => ua.SubjectUserId);

            modelBuilder.Entity<UserOrg>()
                .HasOne(uo => uo.User)
                .WithMany(u => u.UserOrgs)
                .HasForeignKey(uo => uo.UserId);
        }
    }
}
