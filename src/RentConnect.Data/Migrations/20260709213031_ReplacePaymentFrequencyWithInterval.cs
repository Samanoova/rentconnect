using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentConnect.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePaymentFrequencyWithInterval : Migration
    {
        // العمود القديم كان يخزّن قيمة Enum (0=شهري، 1=كل شهرين، 2=كل 3 شهور، 3=كل 6 شهور،
        // 4=سنوي) - بعد إعادة التسمية لازم نحوّل هالقيم لعدد الأشهر الفعلي قبل ما نعتبرها رقماً حراً.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentFrequency",
                table: "Listings",
                newName: "PaymentIntervalMonths");

            migrationBuilder.Sql("""
                UPDATE Listings SET PaymentIntervalMonths = CASE PaymentIntervalMonths
                    WHEN 0 THEN 1
                    WHEN 1 THEN 2
                    WHEN 2 THEN 3
                    WHEN 3 THEN 6
                    WHEN 4 THEN 12
                    ELSE 1
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Listings SET PaymentIntervalMonths = CASE PaymentIntervalMonths
                    WHEN 1 THEN 0
                    WHEN 2 THEN 1
                    WHEN 3 THEN 2
                    WHEN 6 THEN 3
                    WHEN 12 THEN 4
                    ELSE 0
                END;
                """);

            migrationBuilder.RenameColumn(
                name: "PaymentIntervalMonths",
                table: "Listings",
                newName: "PaymentFrequency");
        }
    }
}
