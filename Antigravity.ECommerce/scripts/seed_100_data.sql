-- Script to seed 100 items for each CategoryType (0, 1, 2) with Vietnamese text and UTF-8 support
-- CategoryType: 0 (Menu), 1 (Product Category), 2 (News Category)

SET NOCOUNT ON;

-- Clear existing data for these types to avoid duplicates during seeding
DELETE FROM Categories WHERE CategoryType IN (0, 1, 2);

DECLARE @Type INT;
DECLARE @Level1 INT;
DECLARE @Level2 INT;
DECLARE @Level3 INT;
DECLARE @ParentId INT;
DECLARE @GrandParentId INT;
DECLARE @i INT;
DECLARE @j INT;
DECLARE @k INT;

DECLARE @TypeName NVARCHAR(50);

-- Loop through CategoryTypes: 0, 1, 2
SET @Type = 0;
WHILE @Type <= 2
BEGIN
    SET @TypeName = CASE @Type 
                        WHEN 0 THEN N'Menu' 
                        WHEN 1 THEN N'Danh mục Sản phẩm' 
                        WHEN 2 THEN N'Danh mục Tin tức' 
                    END;

    -- 1. Create 20 Root items
    SET @i = 1;
    WHILE @i <= 20
    BEGIN
        INSERT INTO Categories (ParentId, Name, Slug, SortOrder, Status, CategoryType, CreatedAt, MenuPosition)
        VALUES (0, @TypeName + N' Gốc ' + CAST(@i AS NVARCHAR(10)), 
                LOWER(REPLACE(@TypeName, ' ', '-')) + '-goc-' + CAST(@i AS NVARCHAR(10)) + '-' + CAST(@Type AS NVARCHAR(5)), 
                @i, 1, @Type, GETDATE(), CASE WHEN @Type = 0 THEN 'Header' ELSE NULL END);
        
        SET @ParentId = SCOPE_IDENTITY();

        -- 2. Create 2 Children for each Root (2 * 20 = 40 items)
        SET @j = 1;
        WHILE @j <= 2
        BEGIN
            INSERT INTO Categories (ParentId, Name, Slug, SortOrder, Status, CategoryType, CreatedAt, MenuPosition)
            VALUES (@ParentId, @TypeName + N' Con ' + CAST(@i AS NVARCHAR(10)) + '.' + CAST(@j AS NVARCHAR(10)), 
                    LOWER(REPLACE(@TypeName, ' ', '-')) + '-con-' + CAST(@i AS NVARCHAR(10)) + '-' + CAST(@j AS NVARCHAR(10)) + '-' + CAST(@Type AS NVARCHAR(5)), 
                    @j, 1, @Type, GETDATE(), CASE WHEN @Type = 0 THEN 'Header' ELSE NULL END);
            
            SET @GrandParentId = SCOPE_IDENTITY();

            -- 3. Create 1 Grandchild for each Child (1 * 40 = 40 items)
            INSERT INTO Categories (ParentId, Name, Slug, SortOrder, Status, CategoryType, CreatedAt, MenuPosition)
            VALUES (@GrandParentId, @TypeName + N' Cháu ' + CAST(@i AS NVARCHAR(10)) + '.' + CAST(@j AS NVARCHAR(10)) + '.1', 
                    LOWER(REPLACE(@TypeName, ' ', '-')) + '-chau-' + CAST(@i AS NVARCHAR(10)) + '-' + CAST(@j AS NVARCHAR(10)) + '-1-' + CAST(@Type AS NVARCHAR(5)), 
                    1, 1, @Type, GETDATE(), CASE WHEN @Type = 0 THEN 'Header' ELSE NULL END);

            SET @j = @j + 1;
        END

        SET @i = @i + 1;
    END

    SET @Type = @Type + 1;
END

PRINT 'Seeding completed: 300 records added (100 per type).';
