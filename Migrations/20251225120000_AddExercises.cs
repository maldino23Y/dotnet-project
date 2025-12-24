using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiviEntrainementSportif.Migrations
{
    public partial class AddExercises : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            // seed some default exercises
            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "Name", "Type", "Description" },
                values: new object[,]
                {
                    { 1, "Interval Run", "Cardio", "Alternating fast and slow running intervals." },
                    { 2, "Hill Sprints", "Cardio", "Short maximal sprints uphill with recovery." },
                    { 3, "Kettlebell Swings", "Circuit", "Full-body explosive movement with kettlebell." },
                    { 4, "Squats", "Strength", "Barbell or bodyweight squats for lower body strength." },
                    { 5, "Deadlifts", "Strength", "Compound posterior chain lift." },
                    { 6, "Bench Press", "Strength", "Upper-body pushing compound movement." },
                    { 7, "Pull-ups", "Strength", "Upper-body pulling bodyweight exercise." },
                    { 8, "Yoga Flow", "Recovery", "Mobility and flexibility focused session." },
                    { 9, "Core Mobility", "Mobility", "Core stability and mobility exercises." },
                    { 10, "Burpees", "Circuit", "Full-body conditioning movement." }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises");
        }
    }
}
