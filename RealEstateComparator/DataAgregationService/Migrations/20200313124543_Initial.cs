using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAgregationService.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApartComplexes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    CityName = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApartComplexes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Apartments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumberOfRooms = table.Column<string>(nullable: true),
                    DwellingSpaceMin = table.Column<int>(nullable: false),
                    DwellingSpaceMax = table.Column<int>(nullable: false),
                    SquareMeterPriceMin = table.Column<int>(nullable: false),
                    SquareMeterPriceMax = table.Column<int>(nullable: false),
                    ComplexId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Apartments_ApartComplexes_ComplexId",
                        column: x => x.ComplexId,
                        principalTable: "ApartComplexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apartments_ComplexId",
                table: "Apartments",
                column: "ComplexId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Apartments");

            migrationBuilder.DropTable(
                name: "ApartComplexes");
        }
    }
}
