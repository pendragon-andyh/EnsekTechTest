-- Contains detail about each uploaded file.
CREATE TABLE dbo.MeterReadingLine
(
	MeterReadingFileId INT NOT NULL,
	RowId INT NOT NULL,
	AccountId INT NOT NULL,
	MeterReadingDateTime DATETIME NOT NULL,
	MeterReadValue INT NOT NULL,

	CONSTRAINT MeterReadingLine_PK PRIMARY KEY CLUSTERED (MeterReadingFileId, RowId),
	CONSTRAINT MeterReadingLine_FK_AccountId FOREIGN KEY (AccountId) REFERENCES dbo.Account (AccountId)
);

CREATE INDEX MeterReadingLine_By_AccountId ON dbo.MeterReadingLine (AccountId, MeterReadingDateTime) INCLUDE (MeterReadValue);
