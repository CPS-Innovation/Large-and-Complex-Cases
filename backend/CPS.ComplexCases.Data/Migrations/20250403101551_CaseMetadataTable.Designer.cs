﻿// <auto-generated />
using CPS.ComplexCases.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CPS.ComplexCases.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250403101551_CaseMetadataTable")]
    partial class CaseMetadataTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CPS.ComplexCases.Data.Entities.CaseMetadata", b =>
                {
                    b.Property<int>("CaseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("case_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CaseId"));

                    b.Property<string>("EgressWorkspaceId")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("egress_workspace_id");

                    b.Property<string>("NetappFolderPath")
                        .HasMaxLength(260)
                        .HasColumnType("character varying(260)")
                        .HasColumnName("netapp_folder_path");

                    b.HasKey("CaseId");

                    b.ToTable("case_metadata", "lcc");
                });
#pragma warning restore 612, 618
        }
    }
}
