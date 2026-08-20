using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedByUserIdToSavingAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedByUserId",
                table: "SavingAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Safely convert ClientPaymentStatus from string to int
            migrationBuilder.Sql(@"
                UPDATE Loans
                SET ClientPaymentStatus = 
                    CASE 
                        WHEN ClientPaymentStatus = 'OnTime' THEN 0
                        WHEN ClientPaymentStatus = 'Late' THEN 1
                        WHEN ClientPaymentStatus = 'Default' THEN 2
                        WHEN ClientPaymentStatus = 'Completed' THEN 3
                        ELSE 0
                    END
                WHERE ClientPaymentStatus IS NOT NULL
                  AND TRY_CAST(ClientPaymentStatus AS INT) IS NULL
            ");

            migrationBuilder.AlterColumn<int>(
                name: "ClientPaymentStatus",
                table: "Loans",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidDate",
                table: "LoanInstallments",
                type: "datetime2",
                nullable: true);

            // Safely convert ExpirationDate from string to datetime2
            migrationBuilder.Sql(@"
                UPDATE CreditCards
                SET ExpirationDate = 
                    CASE 
                        WHEN ExpirationDate LIKE '[0-9][0-9]/[0-9][0-9]' 
                        THEN CAST('01/' + ExpirationDate AS DATETIME2)
                        WHEN TRY_CAST(ExpirationDate AS DATETIME2) IS NOT NULL
                        THEN TRY_CAST(ExpirationDate AS DATETIME2)
                        ELSE '2030-01-01'
                    END
                WHERE ExpirationDate IS NOT NULL 
                  AND TRY_CAST(ExpirationDate AS DATETIME2) IS NULL
            ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpirationDate",
                table: "CreditCards",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedByUserId",
                table: "SavingAccounts");

            migrationBuilder.DropColumn(
                name: "PaidDate",
                table: "LoanInstallments");

            migrationBuilder.AlterColumn<string>(
                name: "ClientPaymentStatus",
                table: "Loans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ExpirationDate",
                table: "CreditCards",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
