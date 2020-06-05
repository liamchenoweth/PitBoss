﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PitBoss;

namespace PitBoss.Migrations.MSSQL
{
    [DbContext(typeof(MSSQLContext))]
    partial class MSSQLContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PitBoss.DistributedRequestSeed", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("DistributedOperationRequestId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DistributedRequestId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("OperationRequestId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("DistributedRequestId");

                    b.HasIndex("OperationRequestId");

                    b.ToTable("DistributedRequestSeeds");
                });

            modelBuilder.Entity("PitBoss.OperationRequest", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("CallbackUri")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Completed")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InstigatingRequestId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsParentOperation")
                        .HasColumnType("bit");

                    b.Property<string>("ParentRequestId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PipelineId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PipelineName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PipelineStepId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Started")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("OperationRequests");

                    b.HasDiscriminator<string>("Discriminator").HasValue("OperationRequest");
                });

            modelBuilder.Entity("PitBoss.OperationResponse", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("PipelineId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PipelineName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PipelineStepId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Result")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Success")
                        .HasColumnType("bit");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("OperationResponses");
                });

            modelBuilder.Entity("PitBoss.PipelineRequest", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CurrentRequestId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Input")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PipelineName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResponseId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("CurrentRequestId");

                    b.HasIndex("ResponseId");

                    b.ToTable("PipelineRequests");
                });

            modelBuilder.Entity("PitBoss.DistributedOperationRequest", b =>
                {
                    b.HasBaseType("PitBoss.OperationRequest");

                    b.Property<string>("BeginingStepId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EndingStepId")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("DistributedOperationRequest");
                });

            modelBuilder.Entity("PitBoss.DistributedRequestSeed", b =>
                {
                    b.HasOne("PitBoss.DistributedOperationRequest", "DistributedOperationRequest")
                        .WithMany("SeedingRequestIds")
                        .HasForeignKey("DistributedRequestId");

                    b.HasOne("PitBoss.OperationRequest", "OperationRequest")
                        .WithMany()
                        .HasForeignKey("OperationRequestId");
                });

            modelBuilder.Entity("PitBoss.PipelineRequest", b =>
                {
                    b.HasOne("PitBoss.OperationRequest", "CurrentRequest")
                        .WithMany()
                        .HasForeignKey("CurrentRequestId");

                    b.HasOne("PitBoss.OperationResponse", "Response")
                        .WithMany()
                        .HasForeignKey("ResponseId");
                });
#pragma warning restore 612, 618
        }
    }
}
