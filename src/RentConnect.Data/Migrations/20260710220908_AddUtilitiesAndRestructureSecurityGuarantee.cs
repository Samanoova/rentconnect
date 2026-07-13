using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentConnect.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilitiesAndRestructureSecurityGuarantee : Migration
    {
        // ملاحظة: EF ولّد بالخطأ RenameColumn من RequiresPromissoryNote إلى WaterMeterType
        // (لأنهما بنفس النوع التخزيني) - وهذا كان سيحوّل قيم "مطلوب كمبيالة" القديمة
        // لتصبح قيم "ساعة مياه" بالخطأ. تم تصحيحه يدوياً: كل حقل يُضاف بشكل مستقل،
        // مع ترحيل بيانات RequiresPromissoryNote الفعلية لعمود SecurityGuarantee الجديد
        // قبل حذف العمود القديم.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WaterMeterType",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ElectricityMeterType",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsElectricitySubsidized",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RentalContractImageUrl",
                table: "Listings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresEmployedTenant",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SecurityGuarantee",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WaterCubicMeters",
                table: "Listings",
                type: "INTEGER",
                nullable: true);

            // ترحيل: مطلوب توقيع كمبيالة (القيمة القديمة) => SecurityGuarantee = PromissoryNote (1)
            migrationBuilder.Sql("""
                UPDATE Listings SET SecurityGuarantee = 1 WHERE RequiresPromissoryNote = 1;
                """);

            migrationBuilder.DropColumn(
                name: "RequiresPromissoryNote",
                table: "Listings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresPromissoryNote",
                table: "Listings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE Listings SET RequiresPromissoryNote = 1 WHERE SecurityGuarantee = 1;
                """);

            migrationBuilder.DropColumn(
                name: "ElectricityMeterType",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "IsElectricitySubsidized",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RentalContractImageUrl",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RequiresEmployedTenant",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "SecurityGuarantee",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "WaterCubicMeters",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "WaterMeterType",
                table: "Listings");
        }
    }
}
