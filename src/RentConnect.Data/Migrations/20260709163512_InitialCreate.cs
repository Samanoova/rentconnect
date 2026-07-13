using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentConnect.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        // هذا الـ migration الأول (baseline) لا يُنفّذ أي شيء فعلياً - الجداول أعلاه
        // كانت موجودة مسبقاً بقاعدة البيانات (أُنشئت سابقاً بـ EnsureCreated قبل تفعيل
        // الـ Migrations). هدفه الوحيد تسجيل نقطة بداية بجدول __EFMigrationsHistory
        // بدون فقدان أي بيانات موجودة.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
