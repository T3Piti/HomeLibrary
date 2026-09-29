CREATE OR ALTER PROCEDURE dbo.DeleteBook
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

        -- 1. Удаляем связи с авторами
        DELETE FROM dbo.BookAuthors WHERE BookId = @Id;

        -- 2. Удаляем саму книгу
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

CREATE OR ALTER PROCEDURE dbo.GetAllBooks
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
    ORDER BY b.Id, a.LastName, a.FirstName;
END;

CREATE PROCEDURE dbo.GetBookById @Id INT AS
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

CREATE PROCEDURE dbo.CreateSystemTypes
AS 

IF EXISTS (SELECT * FROM sys.types WHERE name = 'AuthorList' AND is_table_type = 1)
    DROP TYPE dbo.AuthorList;
CREATE TYPE dbo.AuthorList AS TABLE
(
    FirstName  NVARCHAR(100) NOT NULL,
    LastName   NVARCHAR(100) NOT NULL,
    MiddleName NVARCHAR(100) NOT NULL DEFAULT ''
);

IF EXISTS (SELECT * FROM sys.types WHERE name = 'AuthorIds' AND is_table_type = 1)
    DROP TYPE dbo.AuthorIds;
CREATE TYPE dbo.AuthorIds AS TABLE
(
    Id INT NOT NULL
);

CREATE   PROCEDURE dbo.AddBook
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

        -- 1. Создаём книгу
        INSERT INTO Books (Name, YearPublished, TableOfContentsXml)
        VALUES (@Name, @YearPublished, @TableOfContentsXml);

        SET @BookId = SCOPE_IDENTITY();

        -- 2. Вставляем только новых авторов
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

        -- 3. Связываем книгу со всеми переданными авторами
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

CREATE OR ALTER PROCEDURE dbo.UpdateBook
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

        -- 1. Обновляем книгу
        UPDATE dbo.Books
        SET Name               = @Name,
            YearPublished      = @YearPublished,
            TableOfContentsXml = @TableOfContentsXml
        WHERE Id = @Id;

        -- 2. Удаляем все старые связи
        DELETE FROM dbo.BookAuthors WHERE BookId = @Id;

        -- 3. Вставляем новых авторов (которых ещё нет)
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

        -- 4. Связываем книгу со всеми переданными авторами
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
