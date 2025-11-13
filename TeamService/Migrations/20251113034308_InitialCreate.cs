using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TeamService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:currency_enum.currency_enum", "usd,eur,mxn")
                .Annotation("Npgsql:Enum:thick_speed_enum.thick_speed_enum", "high,medium,low")
                .Annotation("Npgsql:Enum:transaction_fee_enum.transaction_fee_enum", "high,medium,low,disabled")
                .Annotation("Npgsql:Enum:volatility_enum.volatility_enum", "high,medium,low,disabled");

            migrationBuilder.CreateTable(
                name: "team",
                schema: "public",
                columns: table => new
                {
                    teamid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    professorid = table.Column<Guid>(type: "uuid", nullable: false),
                    teamname = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    teampic = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team", x => x.teamid);
                });

            migrationBuilder.CreateTable(
                name: "marketconfiguration",
                schema: "public",
                columns: table => new
                {
                    configid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    teamid = table.Column<int>(type: "integer", nullable: false),
                    initialcash = table.Column<double>(type: "double precision", nullable: false),
                    currency = table.Column<int>(type: "integer", nullable: false),
                    marketvolatility = table.Column<int>(type: "integer", nullable: false),
                    marketliquidity = table.Column<int>(type: "integer", nullable: false),
                    thickspeed = table.Column<int>(type: "integer", nullable: false),
                    transactionfee = table.Column<int>(type: "integer", nullable: false),
                    eventfrequency = table.Column<int>(type: "integer", nullable: false),
                    dividendimpact = table.Column<int>(type: "integer", nullable: false),
                    crashimpact = table.Column<int>(type: "integer", nullable: false),
                    allowshortselling = table.Column<bool>(type: "boolean", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marketconfiguration", x => x.configid);
                    table.ForeignKey(
                        name: "FK_marketconfiguration_team_teamid",
                        column: x => x.teamid,
                        principalSchema: "public",
                        principalTable: "team",
                        principalColumn: "teamid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teamasset",
                schema: "public",
                columns: table => new
                {
                    teamassetid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    teamid = table.Column<int>(type: "integer", nullable: false),
                    assetid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teamasset", x => x.teamassetid);
                    table.ForeignKey(
                        name: "FK_teamasset_team_teamid",
                        column: x => x.teamid,
                        principalSchema: "public",
                        principalTable: "team",
                        principalColumn: "teamid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teammembership",
                schema: "public",
                columns: table => new
                {
                    membershipid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    teamid = table.Column<int>(type: "integer", nullable: false),
                    userid = table.Column<Guid>(type: "uuid", nullable: false),
                    joinedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teammembership", x => x.membershipid);
                    table.ForeignKey(
                        name: "FK_teammembership_team_teamid",
                        column: x => x.teamid,
                        principalSchema: "public",
                        principalTable: "team",
                        principalColumn: "teamid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_marketconfiguration_teamid",
                schema: "public",
                table: "marketconfiguration",
                column: "teamid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teamasset_assetid",
                schema: "public",
                table: "teamasset",
                column: "assetid");

            migrationBuilder.CreateIndex(
                name: "IX_teamasset_teamid",
                schema: "public",
                table: "teamasset",
                column: "teamid");

            migrationBuilder.CreateIndex(
                name: "IX_teammembership_teamid",
                schema: "public",
                table: "teammembership",
                column: "teamid");

            migrationBuilder.CreateIndex(
                name: "IX_teammembership_userid",
                schema: "public",
                table: "teammembership",
                column: "userid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "marketconfiguration",
                schema: "public");

            migrationBuilder.DropTable(
                name: "teamasset",
                schema: "public");

            migrationBuilder.DropTable(
                name: "teammembership",
                schema: "public");

            migrationBuilder.DropTable(
                name: "team",
                schema: "public");
        }
    }
}
