using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeLibrary.Migrations
{
  /// <inheritdoc />
  public partial class AddLibraryProcedures : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      AuthorList(migrationBuilder);
      UpdateBook(migrationBuilder);
      AddBook(migrationBuilder);
      FinBooksByAuthorOrName(migrationBuilder);
      GetBookById(migrationBuilder);
      DeleteBook(migrationBuilder);

    }


    private void UpdateBook(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
          @"EXEC ('
CREATE PROCEDURE dbo.UpdateBook
    @Id                 INT,
    @Name               NVARCHAR(200),
    @YearPublished      INT,
    @TableOfContentsXml VARBINARY(MAX) = NULL,
    @Authors            dbo.AuthorList READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Books
        SET Name               = @Name,
            YearPublished      = @YearPublished,
            TableOfContentsXml = @TableOfContentsXml
        WHERE Id = @Id;

        DELETE FROM dbo.BookAuthors WHERE BookId = @Id;

        INSERT INTO Authors (FirstName, LastName, MiddleName)
        SELECT A.FirstName, A.LastName, A.MiddleName
        FROM @Authors AS A
        WHERE NOT EXISTS (
            SELECT 1
            FROM Authors AS Ex
            WHERE Ex.FirstName  = A.FirstName
              AND Ex.LastName   = A.LastName
              AND Ex.MiddleName = A.MiddleName
        );

        INSERT INTO BookAuthors (BookId, AuthorId)
        SELECT @Id, Au.Id
        FROM Authors AS Au
        INNER JOIN @Authors AS A ON
            Au.FirstName  = A.FirstName
            AND Au.LastName = A.LastName
            AND Au.MiddleName = A.MiddleName;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
');",
          suppressTransaction: false
      );
    }

    private void AddBook(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
              @"EXEC ('
CREATE PROCEDURE dbo.AddBook
    @Name               NVARCHAR(200),
    @YearPublished      INT,
    @TableOfContentsXml VARBINARY(MAX) = NULL,
    @Authors            dbo.AuthorList READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @BookId INT;
    DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Books (Name, YearPublished, TableOfContentsXml)
        VALUES (@Name, @YearPublished, @TableOfContentsXml);

        SET @BookId = SCOPE_IDENTITY();

        INSERT INTO Authors (FirstName, LastName, MiddleName)
        SELECT A.FirstName, A.LastName, A.MiddleName
        FROM @Authors AS A
        WHERE NOT EXISTS (
            SELECT 1
            FROM Authors AS Ex
            WHERE Ex.FirstName  = A.FirstName
              AND Ex.LastName   = A.LastName
              AND Ex.MiddleName = A.MiddleName
        );

        INSERT INTO BookAuthors (BookId, AuthorId)
        SELECT @BookId, Au.Id
        FROM Authors AS Au
        INNER JOIN @Authors AS A ON
            Au.FirstName  = A.FirstName
            AND Au.LastName = A.LastName
            AND Au.MiddleName = A.MiddleName;

        COMMIT TRANSACTION;

        SELECT @BookId AS BookId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
');",
              suppressTransaction: false
          );
    }

    private void FinBooksByAuthorOrName(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
              @"EXEC ('
CREATE PROCEDURE dbo.FindBooksByAuthorOrName
    @SearchString NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NormalizedSearch NVARCHAR(MAX);
    SET @NormalizedSearch = LTRIM(RTRIM(@SearchString));

    DECLARE @Words TABLE (Word NVARCHAR(200) NOT NULL PRIMARY KEY);
    DECLARE @TotalWords INT = 0;

    IF @NormalizedSearch IS NOT NULL AND @NormalizedSearch <> ''''
    BEGIN
        INSERT INTO @Words (Word)
        SELECT DISTINCT UPPER(LTRIM(RTRIM(value)))
        FROM STRING_SPLIT(@NormalizedSearch, '''')
        WHERE LTRIM(RTRIM(value)) <> ''''
          AND LEN(LTRIM(RTRIM(value))) > 1;

        SELECT @TotalWords = COUNT(*) FROM @Words;
    END

    SELECT 
        b.Id,
        b.Name,
        b.YearPublished,
        b.TableOfContentsXml,
        a.Id           AS AuthorId,
        a.FirstName    AS AuthorFirstName,
        a.LastName     AS AuthorLastName,
        a.MiddleName   AS AuthorMiddleName
    FROM Books b
    JOIN BookAuthors ba ON b.Id = ba.BookId
    JOIN Authors a     ON ba.AuthorId = a.Id
    WHERE
        (@TotalWords = 0)
        OR
        b.Id IN (
            SELECT m.BookId
            FROM (
                SELECT DISTINCT b2.Id AS BookId, w.Word
                FROM Books b2
                JOIN BookAuthors ba2 ON b2.Id = ba2.BookId
                JOIN Authors a2      ON ba2.AuthorId = a2.Id
                CROSS JOIN @Words w
                WHERE
                    UPPER(a2.LastName)    LIKE ''%'' + w.Word + ''%''
                    OR UPPER(a2.FirstName)  LIKE ''%'' + w.Word + ''%''
                    OR UPPER(a2.MiddleName) LIKE ''%'' + w.Word + ''%''
                    OR UPPER(b2.Name)       LIKE ''%'' + w.Word + ''%''
            ) m
            GROUP BY m.BookId
            HAVING COUNT(DISTINCT m.Word) = @TotalWords
        )
    ORDER BY b.Name, a.LastName, a.FirstName;
END;
');",
              suppressTransaction: false
          );
    }

    private void GetBookById(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
              @"EXEC ('
CREATE PROCEDURE dbo.GetBookById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.Id,
        b.Name,
        b.YearPublished,
        b.TableOfContentsXml,
        a.Id           AS AuthorId,
        a.FirstName    AS AuthorFirstName,
        a.LastName     AS AuthorLastName,
        a.MiddleName   AS AuthorMiddleName
    FROM dbo.Books AS b
    LEFT JOIN dbo.BookAuthors AS ba ON ba.BookId = b.Id
    LEFT JOIN dbo.Authors     AS a  ON a.Id = ba.AuthorId
    WHERE b.Id = @Id
    ORDER BY b.Id, a.LastName, a.FirstName;
END;
');",
              suppressTransaction: false
          );
    }

    private void DeleteBook(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
              @"EXEC ('
CREATE PROCEDURE dbo.DeleteBook
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.BookAuthors WHERE BookId = @Id;
        DELETE FROM dbo.Books WHERE Id = @Id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
');",
              suppressTransaction: false
          );
    }

    private void AuthorList(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
          @"EXEC ('CREATE TYPE dbo.AuthorList AS TABLE (
                FirstName NVARCHAR(50) NOT NULL,
                LastName NVARCHAR(50) NULL,
                MiddleName NVARCHAR(50) NOT NULL
            )');",
          suppressTransaction: false
      );
    }


    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.UpdateBook;", suppressTransaction: false);
      migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.AddBook;", suppressTransaction: false);
      migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.FindBooksByAuthorOrName;", suppressTransaction: false);
      migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.GetBookById;", suppressTransaction: false);
      migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.DeleteBook;", suppressTransaction: false);

      migrationBuilder.Sql("DROP TYPE IF EXISTS dbo.AuthorList;", suppressTransaction: false);
    }
  }
}
