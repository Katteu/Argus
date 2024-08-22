using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cardano.Sync.Example.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cardanoindexer");

            migrationBuilder.CreateTable(
                name: "Blocks",
                schema: "cardanoindexer",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    BlockCbor = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => new { x.Id, x.Number, x.Slot });
                });

            migrationBuilder.CreateTable(
                name: "ReducerStates",
                schema: "cardanoindexer",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReducerStates", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "TransactionOutputs",
                schema: "cardanoindexer",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SpentSlot = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false),
                    AmountCbor = table.Column<byte[]>(type: "bytea", nullable: false),
                    Datum_Type = table.Column<int>(type: "integer", nullable: true),
                    Datum_Data = table.Column<byte[]>(type: "bytea", nullable: true),
                    ReferenceScript = table.Column<byte[]>(type: "bytea", nullable: true),
                    UtxoStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionOutputs", x => new { x.Id, x.Index });
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                schema: "cardanoindexer",
                columns: table => new
                {
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    TxCbor = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => new { x.Slot, x.Hash });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_Slot",
                schema: "cardanoindexer",
                table: "Blocks",
                column: "Slot");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOutputs_Address",
                schema: "cardanoindexer",
                table: "TransactionOutputs",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOutputs_Id",
                schema: "cardanoindexer",
                table: "TransactionOutputs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOutputs_Index",
                schema: "cardanoindexer",
                table: "TransactionOutputs",
                column: "Index");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOutputs_Slot",
                schema: "cardanoindexer",
                table: "TransactionOutputs",
                column: "Slot");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOutputs_UtxoStatus",
                schema: "cardanoindexer",
                table: "TransactionOutputs",
                column: "UtxoStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Slot",
                schema: "cardanoindexer",
                table: "Transactions",
                column: "Slot");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blocks",
                schema: "cardanoindexer");

            migrationBuilder.DropTable(
                name: "ReducerStates",
                schema: "cardanoindexer");

            migrationBuilder.DropTable(
                name: "TransactionOutputs",
                schema: "cardanoindexer");

            migrationBuilder.DropTable(
                name: "Transactions",
                schema: "cardanoindexer");
        }
    }
}
