﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApp
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DataContainer : DbContext
    {
        public DataContainer()
            : base("name=DataContainer")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Comments> Comments { get; set; }
        public DbSet<ProjectTypes> ProjectTypes { get; set; }
        public DbSet<Projects> Projects { get; set; }
        public DbSet<UserGroups> UserGroups { get; set; }
        public DbSet<UserProjects> UserProjects { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Announcements> Announcements { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ExceptionLog> ExceptionLogs { get; set; }
        public DbSet<ActivityView> ActivityViews { get; set; }
        public DbSet<TextConstantValues> TextConstantValues { get; set; }
        public DbSet<TextConstants> TextConstants { get; set; }
        public DbSet<FileLookup> FileLookups { get; set; }
        public DbSet<Events> Events { get; set; }
        public DbSet<ProjectCategories> ProjectCategories { get; set; }
    }
}
