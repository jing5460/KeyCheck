using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PKeyToolWeb_NetCore.Models.PKeyModel
{
    public partial class WAAccountContext : DbContext
    {
        public WAAccountContext()
        {
        }

        public WAAccountContext(DbContextOptions<WAAccountContext> options)
            : base(options)
        {

        }
        //自行导入数据库实体类的引用
        public virtual DbSet<AllModel.BlockedKeys> BlockedKeys { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AllModel.BlockedKeys>(entity =>
            {
//自行导入数据库实体读取
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
