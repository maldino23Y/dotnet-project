-- Run this in SSMS (connect as a user with sufficient rights)
-- It will:
-- 1) show the current mapping for the server principal
-- 2) if a DB user already maps to the login it will use that user
-- 3) otherwise it will create the DB user for the login
-- 4) add the user to db_owner (for development; change role for production)

DECLARE @LoginName NVARCHAR(200) = N'DESKTOP-8DRIVKA\malda';
DECLARE @DbName sysname = N'SuiviEntrainementDB';
DECLARE @login_sid VARBINARY(85);

-- 1) get login SID from server principals
SELECT @login_sid = sid
FROM sys.server_principals
WHERE name = @LoginName;

PRINT 'Login SID:';
SELECT @login_sid AS login_sid;

IF @login_sid IS NULL
BEGIN
    RAISERROR('Server login %s not found. Ensure the Windows login exists on the server.', 16, 1, @LoginName);
    RETURN;
END

-- 2) Switch to target database and detect existing DB user mapped by SID
DECLARE @sql NVARCHAR(MAX) = N'
USE ' + QUOTENAME(@DbName) + N';

SELECT dp.name AS db_user_name, dp.type_desc, dp.sid, sp.name AS mapped_login
FROM sys.database_principals dp
LEFT JOIN sys.server_principals sp ON dp.sid = sp.sid
WHERE dp.sid = @login_sid
   OR sp.name = @loginname
   OR dp.name = @loginname;
';

PRINT 'Checking database-level mapping...';
EXEC sp_executesql @sql,
   N'@login_sid varbinary(85), @loginname nvarchar(200)',
   @login_sid = @login_sid,
   @loginname = @LoginName;

-- 3) If the login has no DB user mapped, create it. If another db user already maps, reuse it.
DECLARE @existingDbUser NVARCHAR(200);

SELECT @existingDbUser = dp.name
FROM sys.databases d
CROSS APPLY
(
    SELECT dp.name
    FROM sys.database_principals dp
    LEFT JOIN sys.server_principals sp ON dp.sid = sp.sid
    WHERE dp.sid = @login_sid OR sp.name = @LoginName
) dp
WHERE d.name = @DbName
OPTION (MAXDOP 1);

-- The CROSS APPLY above may not return; we will explicitly query the database
IF (@existingDbUser IS NULL)
BEGIN
    -- Query inside the database to find any user mapped to the login SID
    DECLARE @dbQuery NVARCHAR(MAX) = N'
    USE ' + QUOTENAME(@DbName) + N';
    SELECT TOP(1) @found = dp.name
    FROM sys.database_principals dp
    LEFT JOIN sys.server_principals sp ON dp.sid = sp.sid
    WHERE dp.sid = @login_sid OR sp.name = @loginname;
    ';
    DECLARE @found NVARCHAR(200);

    EXEC sp_executesql @dbQuery,
        N'@login_sid varbinary(85), @loginname nvarchar(200), @found nvarchar(200) OUTPUT',
        @login_sid = @login_sid,
        @loginname = @LoginName,
        @found = @found OUTPUT;

    SET @existingDbUser = @found;
END

-- 4) If still null: create the DB user for the login
IF (@existingDbUser IS NULL)
BEGIN
    PRINT 'No DB user mapped to the login. Creating DB user...';
    DECLARE @createSql NVARCHAR(MAX) = N'
    USE ' + QUOTENAME(@DbName) + N';
    CREATE USER ' + QUOTENAME(@LoginName) + N' FOR LOGIN ' + QUOTENAME(@LoginName) + N';
    ';
    BEGIN TRY
        EXEC(@createSql);
        PRINT 'DB user created: ' + @LoginName;
        SET @existingDbUser = @LoginName;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        RAISERROR('Failed to create DB user. Inspect the database for an existing user mapping.', 16, 1);
        RETURN;
    END CATCH
END
ELSE
BEGIN
    PRINT 'Found existing DB user mapped to login: ' + @existingDbUser;
END

-- 5) Add DB user to db_owner for development (change role in production)
DECLARE @addRoleSql NVARCHAR(MAX) = N'
USE ' + QUOTENAME(@DbName) + N';
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @user)
BEGIN
    THROW 51000, ''Database user not found when attempting to add role.'', 1;
END
EXEC sp_addrolemember N''db_owner'', @user;
';

BEGIN TRY
    EXEC sp_executesql @addRoleSql, N'@user nvarchar(200)', @user = @existingDbUser;
    PRINT 'Added ' + @existingDbUser + ' to db_owner.';
END TRY
BEGIN CATCH
    PRINT ERROR_MESSAGE();
    RAISERROR('Failed to add role. Ensure the user exists and you have sufficient permissions.', 16, 1);
    RETURN;
END CATCH

PRINT 'Done.';
