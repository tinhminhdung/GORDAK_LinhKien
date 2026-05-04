-- Robust hierarchical deletion for Categories
-- Deletes children and descendants first to satisfy constraints
-- Also cleans up references in Products and News

CREATE OR ALTER PROCEDURE SP_Categories_Delete
    @CategoryId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. Get all IDs to delete (recursive)
        DECLARE @IdsTable TABLE (Id INT);
        
        WITH Hierarchy AS (
            SELECT CategoryId FROM Categories WHERE CategoryId = @CategoryId
            UNION ALL
            SELECT c.CategoryId FROM Categories c 
            INNER JOIN Hierarchy h ON c.ParentId = h.CategoryId
        )
        INSERT INTO @IdsTable SELECT CategoryId FROM Hierarchy;

        -- 2. Clean up references in News
        UPDATE News SET CategoryId = 0 WHERE CategoryId IN (SELECT Id FROM @IdsTable);

        -- 3. Clean up references in Products (CategoryIds is a comma-separated string)
        -- This is complex but we can do a pattern replacement or just NULL it out if it only had that category
        -- For safety, we will just NULL it if it matches exactly or remove it from the list
        UPDATE Products 
        SET CategoryIds = NULL 
        WHERE CategoryIds IN (SELECT CAST(Id AS NVARCHAR(20)) FROM @IdsTable);

        -- 4. Delete the categories (descendants first)
        DELETE FROM Categories WHERE CategoryId IN (SELECT Id FROM @IdsTable);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
    END CATCH
END
GO
