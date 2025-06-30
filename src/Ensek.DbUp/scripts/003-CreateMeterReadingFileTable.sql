-- Contains detail about each uploaded file.
CREATE TABLE dbo.MeterReadingFile
(
	MeterReadingFileId INT IDENTITY(1,1) NOT NULL,
	RequestId UNIQUEIDENTIFIER NOT NULL, --Placeholder for more interesting information.
	UploadTime DATETIME NOT NULL,

	CONSTRAINT MeterReadingFile_PK PRIMARY KEY CLUSTERED (MeterReadingFileId)
);
