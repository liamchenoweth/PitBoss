using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace PitBoss.Migrations.MySql
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationRequests",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false),
                    PipelineName = table.Column<string>(nullable: true),
                    PipelineId = table.Column<string>(nullable: true),
                    PipelineStepId = table.Column<string>(nullable: true),
                    CallbackUri = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Queued = table.Column<DateTime>(nullable: false),
                    Started = table.Column<DateTime>(nullable: false),
                    Completed = table.Column<DateTime>(nullable: false),
                    ParentRequestId = table.Column<string>(nullable: true),
                    InstigatingRequestId = table.Column<string>(nullable: true),
                    IsParentOperation = table.Column<bool>(nullable: false),
                    RetryCount = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    EndingStepId = table.Column<string>(nullable: true),
                    BeginingStepId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationResponses",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false),
                    PipelineId = table.Column<string>(nullable: true),
                    PipelineName = table.Column<string>(nullable: true),
                    PipelineStepId = table.Column<string>(nullable: true),
                    Success = table.Column<bool>(nullable: false),
                    Error = table.Column<string>(nullable: true),
                    Result = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pipelines",
                columns: table => new
                {
                    Version = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pipelines", x => x.Version);
                });

            migrationBuilder.CreateTable(
                name: "PipelineSteps",
                columns: table => new
                {
                    HashCode = table.Column<string>(nullable: false),
                    NextSteps = table.Column<string>(nullable: true),
                    IsBranch = table.Column<bool>(nullable: false),
                    BranchEndId = table.Column<string>(nullable: true),
                    Id = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    TargetCount = table.Column<int>(nullable: false),
                    IsDistributedStart = table.Column<bool>(nullable: false),
                    IsDistributed = table.Column<bool>(nullable: false),
                    DistributedEndId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineSteps", x => x.HashCode);
                });

            migrationBuilder.CreateTable(
                name: "DistributedRequestSeeds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    DistributedOperationRequestId = table.Column<string>(nullable: true),
                    DistributedRequestId = table.Column<string>(nullable: true),
                    OperationRequestId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedRequestSeeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistributedRequestSeeds_OperationRequests_DistributedRequest~",
                        column: x => x.DistributedRequestId,
                        principalTable: "OperationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DistributedRequestSeeds_OperationRequests_OperationRequestId",
                        column: x => x.OperationRequestId,
                        principalTable: "OperationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PipelineRequests",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false),
                    PipelineName = table.Column<string>(nullable: true),
                    PipelineVersion = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    CurrentRequestId = table.Column<string>(nullable: true),
                    ResponseId = table.Column<string>(nullable: true),
                    Input = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineRequests_OperationRequests_CurrentRequestId",
                        column: x => x.CurrentRequestId,
                        principalTable: "OperationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PipelineRequests_Pipelines_PipelineVersion",
                        column: x => x.PipelineVersion,
                        principalTable: "Pipelines",
                        principalColumn: "Version",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PipelineRequests_OperationResponses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "OperationResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PipelineStepMap",
                columns: table => new
                {
                    Version = table.Column<string>(nullable: false),
                    StepHash = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineStepMap", x => new { x.StepHash, x.Version });
                    table.ForeignKey(
                        name: "FK_PipelineStepMap_PipelineSteps_StepHash",
                        column: x => x.StepHash,
                        principalTable: "PipelineSteps",
                        principalColumn: "HashCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PipelineStepMap_Pipelines_Version",
                        column: x => x.Version,
                        principalTable: "Pipelines",
                        principalColumn: "Version",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistributedRequestSeeds_DistributedRequestId",
                table: "DistributedRequestSeeds",
                column: "DistributedRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributedRequestSeeds_OperationRequestId",
                table: "DistributedRequestSeeds",
                column: "OperationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineRequests_CurrentRequestId",
                table: "PipelineRequests",
                column: "CurrentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineRequests_PipelineVersion",
                table: "PipelineRequests",
                column: "PipelineVersion");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineRequests_ResponseId",
                table: "PipelineRequests",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineStepMap_Version",
                table: "PipelineStepMap",
                column: "Version");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributedRequestSeeds");

            migrationBuilder.DropTable(
                name: "PipelineRequests");

            migrationBuilder.DropTable(
                name: "PipelineStepMap");

            migrationBuilder.DropTable(
                name: "OperationRequests");

            migrationBuilder.DropTable(
                name: "OperationResponses");

            migrationBuilder.DropTable(
                name: "PipelineSteps");

            migrationBuilder.DropTable(
                name: "Pipelines");
        }
    }
}
