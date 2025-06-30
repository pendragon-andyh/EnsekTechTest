CREATE OR ALTER PROCEDURE [dbo].[MeterReadingBulkLoad] (
	@RequestId UNIQUEIDENTIFIER,
	@Data dbo.MeterReadingTVP READONLY
)
AS BEGIN
	SET NOCOUNT ON;

	DECLARE @BlockSize INT = 1000;

	-- Make a writable copy of the bulk-loaded data.
	SELECT * INTO #temp1 FROM @Data;

	-- Validate the AccountId values in the uploaded data.
	UPDATE t1 SET
			ReadingStatus = 2,
			StatusReason = 'Account does not exist'
		FROM #temp1 AS t1
		LEFT OUTER JOIN dbo.Account AS acc ON acc.AccountId = t1.AccountId
		WHERE t1.ReadingStatus < 2
		AND acc.AccountId IS NULL;

	-- Look for duplicate readings within the uploaded data.
	SELECT AccountId, MeterReadingDateTime, MIN(RowId) AS FirstRowId
		INTO #duplicatesWithinUploadedData
		FROM #temp1 AS t1
		WHERE t1.ReadingStatus < 2
		GROUP BY AccountId, MeterReadingDateTime;
	UPDATE t1 SET
			ReadingStatus = 2,
			StatusReason = 'Duplicate reading (within uploaded data)'
		FROM #duplicatesWithinUploadedData AS dups
		INNER JOIN #temp1 AS t1
			ON t1.AccountId = dups.AccountId
			AND t1.MeterReadingDateTime = dups.MeterReadingDateTime
			AND t1.RowId > dups.FirstRowId
		WHERE t1.ReadingStatus < 2;

	-- Look for duplicate readings within the existing data.
	SELECT t1.AccountId, t1.MeterReadingDateTime, MIN(t1.RowId) AS FirstRowId
		INTO #duplicatesWithinExistingData
		FROM #temp1 AS t1
		INNER JOIN dbo.MeterReadingLine AS existingLine
			ON existingLine.AccountId = t1.AccountId
			AND existingLine.MeterReadingDateTime = t1.MeterReadingDateTime
		WHERE t1.ReadingStatus < 2
		GROUP BY t1.AccountId, t1.MeterReadingDateTime;
	UPDATE t1 SET
			ReadingStatus = 2,
			StatusReason = 'Duplicate reading (within existing data)'
		FROM #duplicatesWithinExistingData AS dups
		INNER JOIN #temp1 AS t1
			ON t1.AccountId = dups.AccountId
			AND t1.MeterReadingDateTime = dups.MeterReadingDateTime
		WHERE t1.ReadingStatus < 2;

	-- Create a new MeterReadingFile record.
	INSERT INTO dbo.MeterReadingFile (RequestId, UploadTime)
		VALUES (@RequestId, GETDATE());
	DECLARE @FileId INT = SCOPE_IDENTITY();

	BEGIN TRY
		-- Store blocks of meter readings so we don't hit the transaction log and locks too hard.
		DECLARE @MaxRowId INT = (SELECT MAX(RowId) FROM #temp1);
		DECLARE @BlockStart INT = 1;
		DECLARE @BlockEnd INT = @BlockSize;
		WHILE (@BlockStart <= @MaxRowId) BEGIN
			INSERT INTO dbo.MeterReadingLine (MeterReadingFileId, RowId, AccountId, MeterReadingDateTime, MeterReadValue)
				SELECT @FileId, RowId, AccountId, MeterReadingDateTime, MeterReadValue
				FROM #temp1
				WHERE RowId BETWEEN @BlockStart AND @BlockEnd
				AND ReadingStatus <= 1;

			SET @BlockStart = @BlockStart + @BlockSize;
			SET @BlockEnd = @BlockEnd + @BlockSize;
		END

		-- Return statistics.
		SELECT SUM(CASE WHEN ReadingStatus = 0 THEN 1 ELSE 0 END) AS SuccessCount,
				SUM(CASE WHEN ReadingStatus = 1 THEN 1 ELSE 0 END) AS WarningCount,
				SUM(CASE WHEN ReadingStatus = 2 THEN 1 ELSE 0 END) AS ErrorCount
			FROM #temp1;
	END TRY
	BEGIN CATCH
		-- If there was any problems then attempt to remove any half-loaded data.
		WHILE (1 = 1) BEGIN
			DELETE TOP(@BlockSize) FROM dbo.MeterReadingLine
				WHERE MeterReadingFileId = @FileId;
			IF (@@ROWCOUNT = 0) BREAK;
		END;

		THROW;
	END CATCH
END
GO
