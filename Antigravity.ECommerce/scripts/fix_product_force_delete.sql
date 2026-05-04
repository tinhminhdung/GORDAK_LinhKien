USE Antigravity_ECommerce;
GO

-- 1. Drop the FK constraint first
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'ALTER TABLE OrderItems DROP CONSTRAINT ' + name + ';'
FROM sys.foreign_keys
WHERE parent_object_id = OBJECT_ID('OrderItems')
  AND referenced_object_id = OBJECT_ID('Products');

IF @sql <> '' EXEC sp_executesql @sql;
GO

-- 2. Make ProductId NULLable in OrderItems
ALTER TABLE OrderItems ALTER COLUMN ProductId INT NULL;
GO

-- 3. Re-add FK with ON DELETE SET NULL
ALTER TABLE OrderItems ADD CONSTRAINT FK_OrderItems_Products 
FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE SET NULL;
GO

-- 4. Update the Force Delete Procedure
CREATE OR ALTER PROCEDURE SP_Products_ForceDelete
    @ProductId INT
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Set ProductId to NULL in OrderItems (now allowed)
        UPDATE OrderItems SET ProductId = NULL WHERE ProductId = @ProductId;
        
        -- Delete the product
        DELETE FROM Products WHERE ProductId = @ProductId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
