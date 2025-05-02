-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2025-04-29 16:20:18.455

-- tables
-- Table: Group
CREATE TABLE "Group" (
    Id int  NOT NULL IDENTITY,
    Name varchar(10)  NOT NULL,
    CONSTRAINT Group_pk PRIMARY KEY  (Id)
);

-- Table: GroupAssignment
CREATE TABLE GroupAssignment (
    Student_Id int  NOT NULL,
    Group_Id int  NOT NULL,
    CONSTRAINT GroupAssignment_pk PRIMARY KEY  (Student_Id,Group_Id)
);

-- Table: Student
CREATE TABLE Student (
    Id int  NOT NULL IDENTITY,
    FirstName varchar(50)  NOT NULL,
    LastName varchar(50)  NOT NULL,
    Age int  NOT NULL,
    CONSTRAINT Student_pk PRIMARY KEY  (Id)
);

-- foreign keys
-- Reference: Table_3_Group (table: GroupAssignment)
ALTER TABLE GroupAssignment ADD CONSTRAINT Table_3_Group
    FOREIGN KEY (Group_Id)
    REFERENCES "Group" (Id);

-- Reference: Table_3_Student (table: GroupAssignment)
ALTER TABLE GroupAssignment ADD CONSTRAINT Table_3_Student
    FOREIGN KEY (Student_Id)
    REFERENCES Student (Id);

-- End of file.

