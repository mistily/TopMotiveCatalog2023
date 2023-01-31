using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
//using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using TopmotiveCatalog2023.Controllers;
using TopmotiveCatalog2023.Models;

namespace TopmotiveCatalog2023.DAL
{
    internal class PiecesContext : DbContext
    {
        public Action<String>? Log { get; set; }
        public PiecesContext(DbContextOptions<PiecesContext> options, Action<String>? log) : base(options) {
            if (log != null)
            {
                Log = log;
            }
        }

        public PiecesContext()
        {
        }

        public PiecesContext(Action<String>? log)
        {
            Log = log;
        }

        public DbSet<ManufacturerModel> Manufacturers { get; set; }
        public DbSet<VehicleModelsModel> VehicleModels { get; set; }
        public DbSet<VehicleTypesModel> VehicleTypes { get; set; }

        public DbSet<ProductGroupModel> ProductGroups { get; set; }
        public DbSet<ArticleModel> Articles { get; set; }

        public DbSet<VehicleTypesOfArticlesModel> VehicleTypesOfArticles { get; set; }
        public DbSet<ProductGroupToVehicleTypeModel> ProductGroupToVehicleTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ManufacturerModel>().ToTable("manufacturer");
            modelBuilder.Entity<VehicleModelsModel>().ToTable("vehicle_models");
            modelBuilder.Entity<VehicleTypesModel>().ToTable("vehicle_types");
            modelBuilder.Entity<ProductGroupModel>().ToTable("product_group");
            modelBuilder.Entity<ArticleModel>().ToTable("article");
            modelBuilder.Entity<VehicleTypesOfArticlesModel>().ToTable("vehicle_type_of_article");
            modelBuilder.Entity<ProductGroupToVehicleTypeModel>().ToTable("product_group_to_vehicle_type");

            modelBuilder.Entity<ManufacturerModel>()
                .HasMany<VehicleModelsModel>(d => d.VehicleModels)
                .WithOne(s => s.Manufacturer)
                .HasForeignKey(s => s.ManufacturerId)
                .HasConstraintName("fk_vm_to_manufacturer")
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<VehicleModelsModel>()
                .HasMany<VehicleTypesModel>(d => d.VehicleTypes)
                .WithOne(s => s.VehicleModel)
                .HasForeignKey(s => s.VehicleModelId)
                .HasConstraintName("fk_model")
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProductGroupModel>()
               .HasMany<ArticleModel>(d => d.Articles)
               .WithOne(s => s.ProductGroup)
               .HasForeignKey(s => s.ProductGroupId)
               .HasConstraintName("fk_product_group")
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ManufacturerModel>()
               .HasMany<ArticleModel>(d => d.Articles)
               .WithOne(s => s.Brand)
               .HasForeignKey(s => s.BrandId)
               .HasConstraintName("fk_brand")
               .OnDelete(DeleteBehavior.NoAction);

         /*   modelBuilder.Entity<VehicleTypesModel>()
                           .HasMany(s => s.Articles)
                           .WithMany(c => c.VehicleTypes)
                           .UsingEntity<VehicleTypesOfArticlesModel>(sc =>
                           {
                               sc.HasKey(x => new { x.VehicleTypeId, x.ArticleId });
                               sc.ToTable("vehicle_type_of_article");
                           });
            modelBuilder.Entity<VehicleTypesModel>()
                           .HasMany(s => s.ProductGroups)
                           .WithMany(c => c.VehicleTypes)
                           .UsingEntity<ProductGroupToVehicleTypeModel>(sc =>
                           {
                               sc.HasKey(x => new { x.VehicleTypeId, x.ProductGroupId });
                               sc.ToTable("product_group_to_vehicle_type");
                           });*/
            
               modelBuilder.Entity<VehicleTypesOfArticlesModel>()
                   .HasKey(vtoa => new { vtoa.VehicleTypeId, vtoa.ArticleId });

               modelBuilder.Entity<VehicleTypesOfArticlesModel>()
                   .HasOne(vtoa => vtoa.VehicleType)
                   .WithMany(vtoa => vtoa.VehicleTypesOfArticles)
                   .HasForeignKey(vtoa => vtoa.VehicleTypeId)
                   .HasConstraintName("fk_vehicle_type_of_article")
                   .OnDelete(DeleteBehavior.Cascade);

               modelBuilder.Entity<VehicleTypesOfArticlesModel>()
                   .HasOne(vtoa => vtoa.Article)
                   .WithMany(vtoa => vtoa.VehicleTypesOfArticles)
                   .HasForeignKey(vtoa => vtoa.ArticleId)
                   .HasConstraintName("fk_article_of_vehicle_types")
                   .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductGroupToVehicleTypeModel>()
                   .HasKey(pgtvt => new { pgtvt.ProductGroupId, pgtvt.VehicleTypeId });

               modelBuilder.Entity<ProductGroupToVehicleTypeModel>()
                   .HasOne(pgtvt => pgtvt.VehicleType)
                   .WithMany(pgtvt => pgtvt.ProductGroupToVehicleTypes)
                   .HasForeignKey(pgtvt => pgtvt.VehicleTypeId)
                   .HasConstraintName("fk_vehicle_type_pg")
                   .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductGroupToVehicleTypeModel>()
                   .HasOne(pgtvt => pgtvt.ProductGroup)
                   .WithMany(pgtvt => pgtvt.ProductGroupToVehicleTypes)
                   .HasForeignKey(pgtvt => pgtvt.ProductGroupId)
                   .HasConstraintName("fk_product_group_vt")
                   .OnDelete(DeleteBehavior.Cascade);



        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                optionsBuilder.UseMySql(ConfigController.getConnectionString(), new MySqlServerVersion(new Version(8, 0, 31)))
                    .ConfigureWarnings(
                        b => b.Log(
                            (RelationalEventId.ConnectionOpened, LogLevel.Information),
                            (RelationalEventId.ConnectionClosed, LogLevel.Information)))
                    .LogTo(s => Log?.Invoke(s), LogLevel.Information, DbContextLoggerOptions.DefaultWithLocalTime | DbContextLoggerOptions.SingleLine)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
                Log?.Invoke($"{DateTime.Now.ToString()} Connected to the database with credentials {ConfigController.getConnectionString()}!");
            }
            catch(Exception ex)
            {
                Log?.Invoke($"{DateTime.Now.ToString()} Database connection error: {ex.Message}!");
                Console.WriteLine($"Database connection error: {ex.Message}");
            }
           
        }
    }
}
