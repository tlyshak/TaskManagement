using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    assignee_id = table.Column<int>(type: "integer", nullable: false),
                    assigned_by_id = table.Column<int>(type: "integer", nullable: false),
                    planned_start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tasks", x => x.id);
                    table.CheckConstraint("ck_tasks_completed_at", "(status = 'Completed') = (completed_at IS NOT NULL)");
                    table.CheckConstraint("ck_tasks_due_after_start", "due_at IS NULL OR planned_start_at IS NULL OR due_at >= planned_start_at");
                    table.CheckConstraint("ck_tasks_not_self_assigned", "assignee_id <> assigned_by_id");
                    table.CheckConstraint("ck_tasks_status", "status IN ('New', 'InProgress', 'OnHold', 'Completed', 'Cancelled')");
                    table.ForeignKey(
                        name: "fk_tasks_employees_assigned_by_id",
                        column: x => x.assigned_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tasks_employees_assignee_id",
                        column: x => x.assignee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "employees",
                columns: new[] { "id", "created_at", "email", "full_name", "is_active", "position" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "jack.sparrow@example.com", "Jack Sparrow", true, "Project Manager" },
                    { 2, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "john.wick@example.com", "John Wick", true, "Electrical Engineer" },
                    { 3, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mickeymouse@example.com", "Mickey Mouse", false, "Electrical Engineer" }
                });

            migrationBuilder.InsertData(
                table: "tasks",
                columns: new[] { "id", "assigned_by_id", "assignee_id", "completed_at", "created_at", "description", "due_at", "planned_start_at", "status", "title", "updated_at" },
                values: new object[,]
                {
                    { 1, 1, 2, null, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 10, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 20, 0, 0, 0, 0, DateTimeKind.Utc), "New", "Fix section 1 electricity", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 1, 2, null, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc), "InProgress", "Fix section 2 electricity", new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 2, 1, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create last month financial report.", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Completed", "Create a report", new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "ix_employees_email",
                table: "employees",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tasks_assigned_by_id",
                table: "tasks",
                column: "assigned_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_assignee_id_due_at",
                table: "tasks",
                columns: new[] { "assignee_id", "due_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_assignee_id_status",
                table: "tasks",
                columns: new[] { "assignee_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "employees");
        }
    }
}
