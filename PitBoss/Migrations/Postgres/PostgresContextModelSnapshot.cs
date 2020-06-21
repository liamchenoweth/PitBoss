﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PitBoss;

namespace PitBoss.Migrations.Postgres
{
    [DbContext(typeof(PostgresContext))]
    partial class PostgresContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("PitBoss.DistributedRequestSeed", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("DistributedOperationRequestId")
                        .HasColumnType("text");

                    b.Property<string>("DistributedRequestId")
                        .HasColumnType("text");

                    b.Property<string>("OperationRequestId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DistributedRequestId");

                    b.HasIndex("OperationRequestId");

                    b.ToTable("DistributedRequestSeeds");
                });

            modelBuilder.Entity("PitBoss.OperationRequest", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<string>("CallbackUri")
                        .HasColumnType("text");

                    b.Property<DateTime>("Completed")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("InstigatingRequestId")
                        .HasColumnType("text");

                    b.Property<bool>("IsParentOperation")
                        .HasColumnType("boolean");

                    b.Property<string>("ParentRequestId")
                        .HasColumnType("text");

                    b.Property<string>("PipelineId")
                        .HasColumnType("text");

                    b.Property<string>("PipelineName")
                        .HasColumnType("text");

                    b.Property<string>("PipelineStepId")
                        .HasColumnType("text");

                    b.Property<DateTime>("Started")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("OperationRequests");

                    b.HasDiscriminator<string>("Discriminator").HasValue("OperationRequest");
                });

            modelBuilder.Entity("PitBoss.OperationResponse", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("PipelineId")
                        .HasColumnType("text");

                    b.Property<string>("PipelineName")
                        .HasColumnType("text");

                    b.Property<string>("PipelineStepId")
                        .HasColumnType("text");

                    b.Property<string>("Result")
                        .HasColumnType("text");

                    b.Property<bool>("Success")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("OperationResponses");
                });

            modelBuilder.Entity("PitBoss.PipelineModel", b =>
                {
                    b.Property<string>("Version")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Version");

                    b.ToTable("Pipelines");
                });

            modelBuilder.Entity("PitBoss.PipelineRequest", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CurrentRequestId")
                        .HasColumnType("text");

                    b.Property<string>("Input")
                        .HasColumnType("text");

                    b.Property<string>("PipelineName")
                        .HasColumnType("text");

                    b.Property<string>("PipelineVersion")
                        .HasColumnType("text");

                    b.Property<string>("ResponseId")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("CurrentRequestId");

                    b.HasIndex("PipelineVersion");

                    b.HasIndex("ResponseId");

                    b.ToTable("PipelineRequests");
                });

            modelBuilder.Entity("PitBoss.PipelineStepModel", b =>
                {
                    b.Property<string>("HashCode")
                        .HasColumnType("text");

                    b.Property<string>("BranchEndId")
                        .HasColumnType("text");

                    b.Property<string>("DisplayName")
                        .HasColumnType("text");

                    b.Property<string>("DistributedEndId")
                        .HasColumnType("text");

                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<bool>("IsBranch")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDistributed")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDistributedStart")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("NextSteps")
                        .HasColumnType("text");

                    b.Property<int>("TargetCount")
                        .HasColumnType("integer");

                    b.HasKey("HashCode");

                    b.ToTable("PipelineSteps");
                });

            modelBuilder.Entity("PitBoss.PipelineToStepMapper", b =>
                {
                    b.Property<string>("StepHash")
                        .HasColumnType("text");

                    b.Property<string>("Version")
                        .HasColumnType("text");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.HasKey("StepHash", "Version");

                    b.HasIndex("Version");

                    b.ToTable("PipelineStepMap");
                });

            modelBuilder.Entity("PitBoss.DistributedOperationRequest", b =>
                {
                    b.HasBaseType("PitBoss.OperationRequest");

                    b.Property<string>("BeginingStepId")
                        .HasColumnType("text");

                    b.Property<string>("EndingStepId")
                        .HasColumnType("text");

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

                    b.HasOne("PitBoss.PipelineModel", "PipelineModel")
                        .WithMany()
                        .HasForeignKey("PipelineVersion");

                    b.HasOne("PitBoss.OperationResponse", "Response")
                        .WithMany()
                        .HasForeignKey("ResponseId");
                });

            modelBuilder.Entity("PitBoss.PipelineToStepMapper", b =>
                {
                    b.HasOne("PitBoss.PipelineStepModel", "Step")
                        .WithMany("Pipelines")
                        .HasForeignKey("StepHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PitBoss.PipelineModel", "Pipeline")
                        .WithMany("Steps")
                        .HasForeignKey("Version")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
