CREATE TYPE dbo.MeterReadingTVP AS TABLE
(
	RowId INT PRIMARY KEY NOT NULL, -- Line number within uploaded files.

	AccountId INT NOT NULL,
	MeterReadingDateTime DATETIME NOT NULL,
	MeterReadValue INT NOT NULL,

	ReadingStatus INT NOT NULL, -- Status of row (0 = OK, 1 = Warning, 2 = Error).
	StatusReason VARCHAR(80) NULL -- Reason for warning or error.
);
