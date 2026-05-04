USE Antigravity_ECommerce;
GO

-- Create Force Delete Procedure for Products
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_Products_ForceDelete') DROP PROCEDURE SP_Products_ForceDelete;
GO
CREATE PROCEDURE SP_Products_ForceDelete
    @ProductId INT
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Set ProductId to 0 in OrderItems to maintain referential integrity if FK exists
        -- Note: If there is no FK, this just ensures we don't have dangling IDs
        UPDATE OrderItems SET ProductId = 0 WHERE ProductId = @ProductId;
        
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
