using Microsoft.EntityFrameworkCore.Migrations;

namespace BulkyBook.DataAccess.Migrations
{
    public partial class AddStoredProcForCoverType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create SQL Stored Procedures
            // GetAll
            migrationBuilder.Sql(@"CREATE PROC usp_GetCoverTypes
                                    AS
                                    BEGIN
                                    SELECT * FROM dbo.CoverTypes
                                    END");

            // GetById
            migrationBuilder.Sql(@"CREATE PROC usp_GetCoverType
                                    @Id int
                                    AS
                                    BEGIN
                                    SELECT * FROM dbo.CoverTypes WHERE (Id = @Id)
                                    END");

            // Update
            migrationBuilder.Sql(@"CREATE PROC usp_UpdateCoverType
                                    @Id int,
                                    @Name varchar(100)
                                    AS
                                    BEGIN
                                    UPDATE dbo.CoverTypes
                                    SET Name = @Name
                                    WHERE Id = @Id
                                    END");

            // Delete
            migrationBuilder.Sql(@"CREATE PROC usp_DeleteCoverType
                                    @Id int
                                    AS
                                    BEGIN
                                    DELETE FROM dbo.CoverTypes
                                    WHERE Id = @Id
                                    END");

            // Create
            migrationBuilder.Sql(@"CREATE PROC usp_CreateCoverType
                                    @Name varchar(100)
                                    AS
                                    BEGIN
                                    INSERT INTO dbo.CoverTypes(Name)
                                    VALUES (@Name)
                                    END");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete Stored Procedures
            migrationBuilder.Sql(@"DROP PROCEDURE usp_GetCoverTypes");
            migrationBuilder.Sql(@"DROP PROCEDURE usp_GetCoverType");
            migrationBuilder.Sql(@"DROP PROCEDURE usp_UpdateCoverType");
            migrationBuilder.Sql(@"DROP PROCEDURE usp_DeleteCoverType");
            migrationBuilder.Sql(@"DROP PROCEDURE usp_CreateCoverType");
        }
    }
}
