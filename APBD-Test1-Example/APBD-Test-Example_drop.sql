-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2025-04-29 16:20:18.455

-- foreign keys
ALTER TABLE GroupAssignment DROP CONSTRAINT Table_3_Group;

ALTER TABLE GroupAssignment DROP CONSTRAINT Table_3_Student;

-- tables
DROP TABLE "Group";

DROP TABLE GroupAssignment;

DROP TABLE Student;

-- End of file.

