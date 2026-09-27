BEGIN;

-- As taken from Student-class in Shared Class Library in EventManagement Solution
CREATE TABLE IF NOT EXISTS Students (
	Id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
	Name VARCHAR(255) NOT NULL,
	UserClass VARCHAR(255),
	CardId VARCHAR(255) NOT NULL,
	Image VARCHAR(255)
);

SAVEPOINT TableCreated_TestUserNotInputted;

INSERT INTO Students (Id, Name, CardId)
OVERRIDING SYSTEM VALUE
SELECT 0, 'Bobby Tester Droptables', '00000000'
WHERE NOT EXISTS (
      SELECT 0 FROM Students WHERE CardId = '00000000'
);

SAVEPOINT TestUserInputted_EventTableNotYetMade;

-- As taken from Event-class in Shared Class Library in EventManagement Solution
CREATE TABLE IF NOT EXISTS Events (
	id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
	Name VARCHAR(255) NOT NULL,
	CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL
);

SAVEPOINT EventsTableAdded_CompositeTableNotYetCreated;

CREATE TABLE IF NOT EXISTS EventRegistrations (
	StudentId INT REFERENCES Students(Id),
	EventId INT REFERENCES Events(Id),
	CheckedIn BOOLEAN NOT NULL DEFAULT FALSE
);

SAVEPOINT EventRegistrationsAdded_NoTestEventYet;


INSERT INTO Events (id, name, createdat)
OVERRIDING SYSTEM VALUE
SELECT '0', 'Test Event', '2026-01-01 01:01:01+00'
WHERE NOT EXISTS (
	SELECT 1 FROM Events WHERE id = '0'
);

COMMIT;


--ROLLBACK;
