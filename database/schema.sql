-- Osigu Medical Orders - SQLite DDL
-- Creates the database schema from scratch.

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Orders (
    Id TEXT NOT NULL PRIMARY KEY,
    PatientId TEXT NOT NULL,
    PatientName TEXT NOT NULL,
    ServiceCode TEXT NOT NULL,
    ServiceDescription TEXT NOT NULL,
    Priority INTEGER NOT NULL CHECK (Priority IN (1, 2)),
    Status INTEGER NOT NULL CHECK (Status IN (1, 2)),
    CreatedAt TEXT NOT NULL,
    ProcessedAt TEXT NULL
);

CREATE INDEX IF NOT EXISTS IX_Orders_PatientId
    ON Orders (PatientId);

CREATE INDEX IF NOT EXISTS IX_Orders_Status
    ON Orders (Status);

CREATE INDEX IF NOT EXISTS IX_Orders_CreatedAt
    ON Orders (CreatedAt);
