using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PitBoss.Migrations.Sqlite
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
                    Started = table.Column<DateTime>(nullable: false),
                    Completed = table.Column<DateTime>(nullable: false),
                    ParentRequestId = table.Column<string>(nullable: true),
                    InstigatingRequestId = table.Column<string>(nullable: true),
                    IsParentOperation = table.Column<bool>(nullable: false),
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
                    Result = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistributedRequestSeeds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DistributedOperationRequestId = table.Column<string>(nullable: true),
                    DistributedRequestId = table.Column<string>(nullable: true),
                    OperationRequestId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedRequestSeeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistributedRequestSeeds_OperationRequests_DistributedRequestId",
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
                        name: "FK_PipelineRequests_OperationResponses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "OperationResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_PipelineRequests_ResponseId",
                table: "PipelineRequests",
                column: "ResponseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributedRequestSeeds");

            migrationBuilder.DropTable(
                name: "PipelineRequests");

            migrationBuilder.DropTable(
                name: "OperationRequests");

            migrationBuilder.DropTable(
                name: "OperationResponses");
        }
    }
}
