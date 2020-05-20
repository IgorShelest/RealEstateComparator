using Microsoft.EntityFrameworkCore.Migrations;

namespace ApplicationContexts.Migrations
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
                    Source = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    CityName = table.Column<string>(maxLength: 50, nullable: false),
                    Url = table.Column<string>(maxLength: 200, nullable: false)
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
                    NumberOfRooms = table.Column<int>(nullable: false),
                    HasMultipleFloors = table.Column<bool>(nullable: false),
                    DwellingSpaceMin = table.Column<int>(nullable: false),
                    DwellingSpaceMax = table.Column<int>(nullable: false),
                    SquareMeterPriceMin = table.Column<int>(nullable: false),
                    SquareMeterPriceMax = table.Column<int>(nullable: false),
                    ComplexId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Apartments_ApartComplexes_ComplexId",
                        column: x => x.ComplexId,
                        principalTable: "ApartComplexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
