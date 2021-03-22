using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Data
{
    public class VideoCdnDbContext : IdentityDbContext<VideoCdnUser, IdentityRole<int>, int>
    {
        public DbSet<CatalogItem> Catalog { get; set; }
        public VideoCdnDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CatalogItem>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.Id).ValueGeneratedOnAdd();
                e.Property(i => i.Title).IsRequired();
                e.Property(i => i.FileId).IsRequired();
                e.Property(i => i.Uploaded)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
