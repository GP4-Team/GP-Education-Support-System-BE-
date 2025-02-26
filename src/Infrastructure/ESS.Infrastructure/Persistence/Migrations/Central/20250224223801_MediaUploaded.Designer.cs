﻿// <auto-generated />
using System;
using ESS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ESS.Infrastructure.Persistence.Migrations.Central
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250224223801_MediaUploaded")]
    partial class MediaUploaded
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ESS.Domain.Entities.Media.Media", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Collection")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<bool>("IsTemporary")
                        .HasColumnType("boolean");

                    b.Property<Guid>("MediaCollectionId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ResourceId")
                        .HasColumnType("uuid");

                    b.Property<string>("ResourceType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("TempGuid")
                        .HasColumnType("uuid");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MediaCollectionId");

                    b.HasIndex("TenantId", "ResourceId", "ResourceType");

                    b.ToTable("Media");
                });

            modelBuilder.Entity("ESS.Domain.Entities.Media.MediaCollection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AllowedTypes")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<long>("MaxFileSize")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TenantId", "Name")
                        .IsUnique();

                    b.ToTable("MediaCollections");
                });

            modelBuilder.Entity("ESS.Domain.Entities.Tenant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ConnectionString")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime?>("DatabaseCreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DatabaseError")
                        .HasColumnType("text");

                    b.Property<int>("DatabaseStatus")
                        .HasColumnType("integer");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<DateTime?>("LastUpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("UseSharedDatabase")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("Identifier")
                        .IsUnique()
                        .HasDatabaseName("IX_Tenants_Identifier");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("ESS.Domain.Entities.TenantAuditLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Details")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Timestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Id");

                    b.HasIndex("TenantId", "Timestamp")
                        .HasDatabaseName("IX_TenantAuditLogs_TenantId_Timestamp");

                    b.ToTable("TenantAuditLogs");
                });

            modelBuilder.Entity("ESS.Domain.Entities.TenantDomain", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Domain")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<bool>("IsPrimary")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("Domain")
                        .IsUnique()
                        .HasDatabaseName("IX_TenantDomains_Domain");

                    b.HasIndex("TenantId", "IsPrimary")
                        .IsUnique()
                        .HasDatabaseName("IX_TenantDomains_TenantId_IsPrimary")
                        .HasFilter("\"IsPrimary\" = true");

                    b.ToTable("TenantDomains");
                });

            modelBuilder.Entity("ESS.Domain.Entities.TenantSettings", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TenantId", "Key")
                        .IsUnique()
                        .HasDatabaseName("IX_TenantSettings_TenantId_Key");

                    b.ToTable("TenantSettings");
                });

            modelBuilder.Entity("ESS.Domain.Entities.Media.Media", b =>
                {
                    b.HasOne("ESS.Domain.Entities.Media.MediaCollection", "MediaCollection")
                        .WithMany("Media")
                        .HasForeignKey("MediaCollectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("ESS.Domain.ValueObjects.Media.MediaFile", "File", b1 =>
                        {
                            b1.Property<Guid>("MediaId")
                                .HasColumnType("uuid");

                            b1.Property<string>("FileName")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("FileName");

                            b1.Property<string>("FileType")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("FileType");

                            b1.Property<string>("MimeType")
                                .IsRequired()
                                .HasColumnType("text")
                                .HasColumnName("MimeType");

                            b1.Property<long>("Size")
                                .HasColumnType("bigint")
                                .HasColumnName("FileSize");

                            b1.HasKey("MediaId");

                            b1.ToTable("Media");

                            b1.WithOwner()
                                .HasForeignKey("MediaId");
                        });

                    b.Navigation("File")
                        .IsRequired();

                    b.Navigation("MediaCollection");
                });

            modelBuilder.Entity("ESS.Domain.Entities.TenantAuditLog", b =>
                {
                    b.HasOne("ESS.Domain.Entities.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("ESS.Domain.Entities.TenantDomain", b =>
                {
                    b.HasOne("ESS.Domain.Entities.Tenant", "Tenant")
                        .WithMany("Domains")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("ESS.Domain.Entities.TenantSettings", b =>
                {
                    b.HasOne("ESS.Domain.Entities.Tenant", "Tenant")
                        .WithMany("Settings")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("ESS.Domain.Entities.Media.MediaCollection", b =>
                {
                    b.Navigation("Media");
                });

            modelBuilder.Entity("ESS.Domain.Entities.Tenant", b =>
                {
                    b.Navigation("Domains");

                    b.Navigation("Settings");
                });
#pragma warning restore 612, 618
        }
    }
}
