-- Diverse seeding script with UTF-8 support
-- Truncate existing to prevent duplicates
DELETE FROM Categories WHERE CategoryType IN (0, 1, 2);
GO

SET NOCOUNT ON;

DECLARE @Type INT = 0;
WHILE @Type <= 2
BEGIN
    DECLARE @RootName NVARCHAR(100);
    DECLARE @i INT = 1;
    WHILE @i <= 50
    BEGIN
        SET @RootName = CASE @Type
            WHEN 0 THEN CASE (@i % 10) 
                WHEN 1 THEN N'Trang chủ' WHEN 2 THEN N'Giới thiệu' WHEN 3 THEN N'Dịch vụ' 
                WHEN 4 THEN N'Sản phẩm' WHEN 5 THEN N'Tin tức' WHEN 6 THEN N'Tuyển dụng' 
                WHEN 7 THEN N'Liên hệ' WHEN 8 THEN N'Chính sách' WHEN 9 THEN N'Giải pháp' ELSE N'Hỗ trợ' END
            WHEN 1 THEN CASE (@i % 10) 
                WHEN 1 THEN N'Điện thoại' WHEN 2 THEN N'Máy tính' WHEN 3 THEN N'Gia dụng' 
                WHEN 4 THEN N'Thời trang' WHEN 5 THEN N'Nội thất' WHEN 6 THEN N'Mỹ phẩm' 
                WHEN 7 THEN N'Thực phẩm' WHEN 8 THEN N'Y tế' WHEN 9 THEN N'Ô tô' ELSE N'Vật liệu' END
            WHEN 2 THEN CASE (@i % 10) 
                WHEN 1 THEN N'Công nghệ' WHEN 2 THEN N'Kinh tế' WHEN 3 THEN N'Văn hóa' 
                WHEN 4 THEN N'Thể thao' WHEN 5 THEN N'Pháp luật' WHEN 6 THEN N'Giáo dục' 
                WHEN 7 THEN N'Sức khỏe' WHEN 8 THEN N'Giải trí' WHEN 9 THEN N'Du lịch' ELSE N'Ẩm thực' END
        END + N' ' + CAST(@i AS NVARCHAR(10));

        INSERT INTO Categories (ParentId, Name, Slug, SortOrder, Status, CategoryType, CreatedAt, MenuPosition)
        VALUES (0, @RootName, LOWER(REPLACE(@RootName, ' ', '-')) + '-' + CAST(NEWID() AS NVARCHAR(36)), @i, 1, @Type, GETDATE(), CASE WHEN @Type = 0 THEN 'Header' ELSE NULL END);
        
        DECLARE @RootId INT = SCOPE_IDENTITY();

        -- Subcategories (Level 2)
        DECLARE @j INT = 1;
        WHILE @j <= 2
        BEGIN
            DECLARE @ChildName NVARCHAR(100) = @RootName + N' Con ' + CAST(@j AS NVARCHAR(10));
            INSERT INTO Categories (ParentId, Name, Slug, SortOrder, Status, CategoryType, CreatedAt, MenuPosition)
            VALUES (@RootId, @ChildName, LOWER(REPLACE(@ChildName, ' ', '-')) + '-' + CAST(NEWID() AS NVARCHAR(36)), @j, 1, @Type, GETDATE(), CASE WHEN @Type = 0 THEN 'Header' ELSE NULL END);
            
            DECLARE @ChildId INT = SCOPE_IDENTITY();

            -- Sub-subcategories (Level 3)
            DECLARE @k INT = 1;
            WHILE @k <= 1
            BEGIN
                DECLARE @GrandChildName NVARCHAR(100) = @ChildName + N' Cháu ' + CAST(@k AS NVARCHAR(10));
                INSERT INTO Categories (ParentId, Name, Slug, SortOrder, Status, CategoryType, CreatedAt, MenuPosition)
                VALUES (@ChildId, @GrandChildName, LOWER(REPLACE(@GrandChildName, ' ', '-')) + '-' + CAST(NEWID() AS NVARCHAR(36)), @k, 1, @Type, GETDATE(), CASE WHEN @Type = 0 THEN 'Header' ELSE NULL END);
                SET @k = @k + 1;
            END
            SET @j = @j + 1;
        END

        SET @i = @i + 1;
    END
    SET @Type = @Type + 1;
END
PRINT 'Diverse seeding completed: 50 roots per type with multi-level structure.';
