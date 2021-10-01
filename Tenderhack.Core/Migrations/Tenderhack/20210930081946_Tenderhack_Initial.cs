using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Tenderhack.Core.Migrations.Tenderhack
{
    public partial class Tenderhack_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(511)", maxLength: 511, nullable: false),
                    Kpgz = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(511)", maxLength: 511, nullable: false),
                    Inn = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Kpp = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(511)", maxLength: 511, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(511)", maxLength: 511, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(type: "character varying(511)", maxLength: 511, nullable: false),
                    PublicAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConclusionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    ProducerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Organizations_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contracts_Organizations_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductProperties",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    AttributeId = table.Column<int>(type: "integer", nullable: false),
                    ValueId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProperties", x => new { x.ProductId, x.AttributeId, x.ValueId });
                    table.ForeignKey(
                        name: "FK_ProductProperties_ProductAttributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "ProductAttributes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductProperties_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductProperties_ProductValues_ValueId",
                        column: x => x.ValueId,
                        principalTable: "ProductValues",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductsAttributes",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    AttributeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsAttributes", x => new { x.ProductId, x.AttributeId });
                    table.ForeignKey(
                        name: "FK_ProductsAttributes_ProductAttributes_AttributeId",
                        column: x => x.AttributeId,
                        principalTable: "ProductAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductsAttributes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductsValues",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ValueId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsValues", x => new { x.ProductId, x.ValueId });
                    table.ForeignKey(
                        name: "FK_ProductsValues_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductsValues_ProductValues_ValueId",
                        column: x => x.ValueId,
                        principalTable: "ProductValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContractId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Kpgz",
                table: "Categories",
                column: "Kpgz",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Title",
                table: "Categories",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ConclusionAt",
                table: "Contracts",
                column: "ConclusionAt");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CustomerId",
                table: "Contracts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_Number",
                table: "Contracts",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_Price",
                table: "Contracts",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ProducerId",
                table: "Contracts",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PublicAt",
                table: "Contracts",
                column: "PublicAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Amount",
                table: "Orders",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ContractId_ProductId",
                table: "Orders",
                columns: new[] { "ContractId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductId",
                table: "Orders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Quantity",
                table: "Orders",
                column: "Quantity");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Inn_Kpp",
                table: "Organizations",
                columns: new[] { "Inn", "Kpp" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Kpp",
                table: "Organizations",
                column: "Kpp");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_Name",
                table: "ProductAttributes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductProperties_AttributeId",
                table: "ProductProperties",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProperties_ValueId",
                table: "ProductProperties",
                column: "ValueId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ExternalId",
                table: "Products",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsAttributes_AttributeId",
                table: "ProductsAttributes",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsValues_ValueId",
                table: "ProductsValues",
                column: "ValueId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductValues_Name",
                table: "ProductValues",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ProductProperties");

            migrationBuilder.DropTable(
                name: "ProductsAttributes");

            migrationBuilder.DropTable(
                name: "ProductsValues");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "ProductAttributes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProductValues");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
