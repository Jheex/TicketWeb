using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PIM.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitanteColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Admins",
                newName: "Password");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Chamados",
                columns: table => new
                {
                    ChamadoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitanteId = table.Column<int>(type: "int", nullable: false),
                    Solicitante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Prioridade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataAbertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtribuidoA_AdminId = table.Column<int>(type: "int", nullable: true),
                    NomeArquivoAnexo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaminhoArquivoAnexo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chamados", x => x.ChamadoId);
                    table.ForeignKey(
                        name: "FK_Chamados_Admins_AtribuidoA_AdminId",
                        column: x => x.AtribuidoA_AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_AtribuidoA_AdminId",
                table: "Chamados",
                column: "AtribuidoA_AdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chamados");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Admins");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Admins",
                newName: "PasswordHash");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Admins",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
