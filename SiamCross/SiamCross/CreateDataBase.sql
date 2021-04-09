﻿PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS SoundSpeedDictionary (
	Id INTEGER NOT NULL PRIMARY KEY,
	Title TEXT NOT NULL,
	Value BLOB DEFAULT NULL);


CREATE TABLE IF NOT EXISTS FieldDictionary (
	Id INTEGER NOT NULL PRIMARY KEY,
	Title TEXT NOT NULL);

CREATE TABLE IF NOT EXISTS Measurement (
	Id INTEGER NOT NULL PRIMARY KEY,

	Field INTEGER NOT NULL DEFAULT 0,
	Well  TEXT NOT NULL DEFAULT 0,
	Bush  TEXT NOT NULL DEFAULT 0,
	Shop  INTEGER NOT NULL DEFAULT 0,

	--Latitude  NUMERIC DEFAULT NULL,
	--Longitude NUMERIC DEFAULT NULL,
	--Altitude  NUMERIC DEFAULT NULL,
	--Accuracy  NUMERIC DEFAULT NULL,

	DeviceKind  INTEGER NOT NULL,
	DeviceNumber  INTEGER NOT NULL,
	DeviceName  TEXT DEFAULT NULL,
	DeviceProtocolId  INTEGER DEFAULT NULL,
	DevicePhyId  INTEGER DEFAULT NULL,

	MeasureKind INTEGER NOT NULL,
	MeasureBeginTimestamp DATETIME DEFAULT NULL,
	MeasureEndTimestamp DATETIME NOT NULL,
	MeasureComment TEXT DEFAULT NULL, 

	MailDistributionDateTime DATETIME DEFAULT NULL,
	MailDistributioDestination TEXT DEFAULT NULL,
	FileDistributionDateTime DATETIME DEFAULT NULL,
	FileDistributionDestination TEXT DEFAULT NULL

	);

CREATE TABLE IF NOT EXISTS DataDictionary(
	Title TEXT NOT NULL,
	Kind INTEGER NOT NULL DEFAULT 1 CHECK (Kind > 0 AND Kind < 6 ),
	Tag INTEGER NOT NULL DEFAULT 0
	, PRIMARY KEY (Title,Kind)
	, UNIQUE(Title)
);

CREATE TABLE IF NOT EXISTS ValInt (
	MeasureId INTEGER NOT NULL,
	Title TEXT NOT NULL,
	Kind INTEGER NOT NULL DEFAULT 1 CHECK (Kind = 1),
	Value INTEGER DEFAULT NULL
	, FOREIGN KEY(Title, Kind) REFERENCES DataDictionary(Title,Kind) ON DELETE RESTRICT ON UPDATE RESTRICT
	, FOREIGN KEY(MeasureId) REFERENCES Measurement(Id) ON DELETE CASCADE ON UPDATE RESTRICT
	, UNIQUE(MeasureId,Title)
);

CREATE TABLE IF NOT EXISTS ValFloat (
	MeasureId INTEGER NOT NULL,
	Title TEXT NOT NULL,
	Kind INTEGER NOT NULL DEFAULT 5 CHECK (Kind = 5),
	Value NUMERIC DEFAULT NULL
	, FOREIGN KEY(Title, Kind) REFERENCES DataDictionary(Title,Kind) ON DELETE RESTRICT ON UPDATE RESTRICT
	, FOREIGN KEY(MeasureId) REFERENCES Measurement(Id) ON DELETE CASCADE ON UPDATE RESTRICT
	, UNIQUE(MeasureId,Title)
);

CREATE TABLE IF NOT EXISTS ValString (
	MeasureId INTEGER NOT NULL,
	Title TEXT NOT NULL,
	Kind INTEGER NOT NULL DEFAULT 2 CHECK (Kind = 2),
	Value TEXT DEFAULT NULL
	, FOREIGN KEY(Title, Kind) REFERENCES DataDictionary(Title,Kind) ON DELETE RESTRICT ON UPDATE RESTRICT
	, FOREIGN KEY(MeasureId) REFERENCES Measurement(Id) ON DELETE CASCADE ON UPDATE RESTRICT
	, UNIQUE(MeasureId,Title)
);

CREATE TABLE IF NOT EXISTS ValBlob (
	MeasureId INTEGER NOT NULL,
	Title TEXT NOT NULL,
	Kind INTEGER NOT NULL DEFAULT 3 CHECK (Kind = 3),
	Value BLOB DEFAULT NULL
	, FOREIGN KEY(Title, Kind) REFERENCES DataDictionary(Title,Kind) ON DELETE RESTRICT ON UPDATE RESTRICT
	, FOREIGN KEY(MeasureId) REFERENCES Measurement(Id) ON DELETE CASCADE ON UPDATE RESTRICT
	, UNIQUE(MeasureId,Title)
);

