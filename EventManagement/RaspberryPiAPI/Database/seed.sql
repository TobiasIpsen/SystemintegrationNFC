BEGIN;

CREATE TABLE IF NOT EXISTS Users_XX (
	id primary key,
	xx,
	xx,
	xx,
	xx
);

SAVEPOINT TableCreated_TestUserNotInputted;

INSERT INTO Users_XX (xx,xx,xx)
VALUES
(xx,xx,"0000000000000000000000000000000000000000");

COMMIT;
