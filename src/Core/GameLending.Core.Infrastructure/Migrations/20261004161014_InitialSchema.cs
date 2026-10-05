using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameLending.Core.Infrastructure.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_import_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_hash = table.Column<string>(type: "text", nullable: false),
                    parser_version = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    analyzed = table.Column<int>(type: "integer", nullable: false),
                    imported = table.Column<int>(type: "integer", nullable: false),
                    duplicates = table.Column<int>(type: "integer", nullable: false),
                    rejected = table.Column<int>(type: "integer", nullable: false),
                    warnings = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "friends",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_friends", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    platforms = table.Column<string[]>(type: "text[]", nullable: false),
                    genres = table.Column<string[]>(type: "text[]", nullable: false),
                    developer = table.Column<string>(type: "text", nullable: true),
                    release_date = table.Column<DateOnly>(type: "date", nullable: true),
                    rating = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    votes = table.Column<long>(type: "bigint", nullable: true),
                    source_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    source_url = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "loans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    friend_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loaned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    returned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loans", x => x.id);
                    table.CheckConstraint("ck_return_date", "returned_at IS NULL OR returned_at >= loaned_at");
                    table.ForeignKey(
                        name: "FK_loans_friends_friend_id",
                        column: x => x.friend_id,
                        principalTable: "friends",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_loans_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_runs_file_hash_parser_version",
                table: "catalog_import_runs",
                columns: new[] { "file_hash", "parser_version" },
                filter: "status = 'Completed'");

            migrationBuilder.CreateIndex(
                name: "IX_friends_name",
                table: "friends",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_games_source_url",
                table: "games",
                column: "source_url",
                unique: true,
                filter: "source_url IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_games_title",
                table: "games",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "IX_loans_friend_id_loaned_at",
                table: "loans",
                columns: new[] { "friend_id", "loaned_at" });

            migrationBuilder.CreateIndex(
                name: "IX_loans_loaned_at",
                table: "loans",
                column: "loaned_at");

            migrationBuilder.CreateIndex(
                name: "ux_loans_active_game",
                table: "loans",
                column: "game_id",
                unique: true,
                filter: "returned_at IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalog_import_runs");

            migrationBuilder.DropTable(
                name: "loans");

            migrationBuilder.DropTable(
                name: "friends");

            migrationBuilder.DropTable(
                name: "games");
        }
    }
}
