using Microsoft.EntityFrameworkCore.Migrations;

namespace PitBoss.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationRequests",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    PipelineName = table.Column<string>(nullable: true),
                    PipelineId = table.Column<string>(nullable: true),
                    PipelineStepId = table.Column<int>(nullable: false),
                    CallbackUri = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PipelineRequests",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    PipelineName = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineRequests", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationRequests");

            migrationBuilder.DropTable(
                name: "PipelineRequests");
        }
    }
}
