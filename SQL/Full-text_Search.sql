-- 1. полнотекстовый каталог (если его еще нет)
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'LibraryCatalog')
    CREATE FULLTEXT CATALOG LibraryCatalog AS DEFAULT;

-- 2. полнотекстовый индекс для таблицы Authors
-- объединяем поля в одну виртуальную колонку для поиска
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Authors'))
    CREATE FULLTEXT INDEX ON Authors (FirstName, LastName, SecondName) 
    KEY INDEX PK_Authors
    ON LibraryCatalog;

-- 3. полнотекстовый индекс для таблицы Books
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Books'))
    CREATE FULLTEXT INDEX ON Books (Name) 
    KEY INDEX PK_Books
    ON LibraryCatalog;


CREATE PROCEDURE FindBooksByNameOrAuthor
    @SearchString NVARCHAR(MAX) = NULL
AS
BEGIN

    -- Нормализуем входную строку
    DECLARE @NormalizedSearch NVARCHAR(MAX);
    SET @NormalizedSearch = LTRIM(RTRIM(@SearchString));

    -- Если строка пустая - возвращаем все книги
    IF @NormalizedSearch = '' OR @NormalizedSearch IS NULL
    BEGIN
        SELECT 
            b.Id AS BookId, b.Name AS BookName, b.YearPublished, b.TableOfContents,
            a.Id AS AuthorId, a.LastName, a.FirstName, a.SecondName
        FROM Books b
        JOIN BookAuthor ba ON b.Id = ba.BookId
        JOIN Authors a ON ba.AuthorId = a.Id;
        RETURN;
    END

    -- FREETEXT - он лучше подходит для пользовательского ввода (фразы, опечатки, словоформы)
    -- CONTAINS требует более точного синтаксиса ("слово" AND "слово")
    SELECT 
        b.Id AS BookId, 
        b.Name AS BookName, 
        b.YearPublished, 
        a.Id AS AuthorId, 
        a.LastName, 
        a.FirstName, 
        a.SecondName, 
        b.TableOfContents
    FROM Books b
    JOIN BookAuthor ba ON b.Id = ba.BookId
    JOIN Authors a ON ba.AuthorId = a.Id
    WHERE 
        -- Ищем совпадение фразы в названии книги ИЛИ в ФИО автора
        FREETEXT(b.Name, @NormalizedSearch) 
        OR FREETEXT((a.FirstName, a.LastName, a.SecondName), @NormalizedSearch);
END;
