/* =========================================================
   RaceDay Database
   Section C - SQL Database Script
   SQL Server / SSMS
   ========================================================= */


/* =========================================================
   1. ORGANISER
   ========================================================= */

CREATE TABLE Organiser
(
    OrganiserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    OrganisationName VARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);


/* =========================================================
   2. CATEGORY
   ========================================================= */

CREATE TABLE Category
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    DistanceKm DECIMAL(5,2) NOT NULL,
    Description VARCHAR(300) NULL,

    CONSTRAINT CK_Category_Distance
        CHECK (DistanceKm > 0)
);


/* =========================================================
   3. EVENT
   ========================================================= */

CREATE TABLE Event
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    CategoryID INT NOT NULL,
    EventName VARCHAR(150) NOT NULL,
    EventDate DATE NOT NULL,
    Location VARCHAR(150) NOT NULL,
    Description VARCHAR(500) NULL,
    Status VARCHAR(30) NOT NULL DEFAULT 'Upcoming',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Event_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES Organiser(OrganiserID),

    CONSTRAINT FK_Event_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Category(CategoryID),

    CONSTRAINT CK_Event_Status
        CHECK (Status IN
        ('Upcoming', 'Open', 'Closed', 'Completed', 'Cancelled'))
);


/* =========================================================
   4. PARTICIPANT
   ========================================================= */

CREATE TABLE Participant
(
    ParticipantID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    DateOfBirth DATE NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);


/* =========================================================
   5. ENROLMENT
   ========================================================= */

CREATE TABLE Enrolment
(
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    ParticipantID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status VARCHAR(30) NOT NULL DEFAULT 'Active',
    PaymentStatus VARCHAR(30) NOT NULL DEFAULT 'Pending',

    CONSTRAINT FK_Enrolment_Event
        FOREIGN KEY (EventID)
        REFERENCES Event(EventID),

    CONSTRAINT FK_Enrolment_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES Participant(ParticipantID),

    CONSTRAINT CK_Enrolment_Status
        CHECK (Status IN
        ('Active', 'Cancelled', 'Completed')),

    CONSTRAINT CK_Payment_Status
        CHECK (PaymentStatus IN
        ('Pending', 'Paid', 'Refunded')),

    CONSTRAINT UQ_Enrolment
        UNIQUE (EventID, ParticipantID)
);


/* =========================================================
   6. RESULT
   ========================================================= */

CREATE TABLE Result
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME NULL,
    Position INT NULL,
    ResultStatus VARCHAR(30) NOT NULL DEFAULT 'Pending',
    RecordedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Result_Enrolment
        FOREIGN KEY (EnrolmentID)
        REFERENCES Enrolment(EnrolmentID),

    CONSTRAINT CK_Result_Position
        CHECK (Position IS NULL OR Position > 0),

    CONSTRAINT CK_Result_Status
        CHECK (ResultStatus IN
        ('Pending', 'Finished', 'DNF', 'DNS'))
);


/* =========================================================
   SAMPLE DATA
   ========================================================= */


/* =========================================================
   7. TWO ORGANISERS
   ========================================================= */

INSERT INTO Organiser
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Phone,
    OrganisationName
)
VALUES
(
    'Thabo',
    'Mokoena',
    'thabo@raceday.co.za',
    'hashed_password_123',
    '0821112233',
    'RaceDay Events'
),
(
    'Lerato',
    'Molefe',
    'lerato@raceday.co.za',
    'hashed_password_456',
    '0834445566',
    'Active Sports SA'
);


/* =========================================================
   8. CATEGORIES
   ========================================================= */

INSERT INTO Category
(
    CategoryName,
    DistanceKm,
    Description
)
VALUES
(
    '5KM Fun Run',
    5.00,
    'A short distance suitable for beginners and casual runners.'
),
(
    '10KM Road Race',
    10.00,
    'A medium distance road running race.'
),
(
    '21.1KM Half Marathon',
    21.10,
    'A half marathon road running event.'
);


/* =========================================================
   9. TWO PARTICIPANTS
   ========================================================= */

INSERT INTO Participant
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Phone,
    DateOfBirth
)
VALUES
(
    'Kagiso',
    'Molefe',
    'kagiso@example.com',
    'participant_password_123',
    '0721234567',
    '1998-05-14'
),
(
    'Naledi',
    'Mokoena',
    'naledi@example.com',
    'participant_password_456',
    '0739876543',
    '2000-08-21'
);


/* =========================================================
   10. THREE EVENTS
   ========================================================= */

INSERT INTO Event
(
    OrganiserID,
    CategoryID,
    EventName,
    EventDate,
    Location,
    Description,
    Status
)
VALUES
(
    1,
    2,
    'Pretoria City Run',
    '2026-10-10',
    'Pretoria',
    'A community road running event in Pretoria.',
    'Open'
),
(
    2,
    3,
    'Soweto Community Marathon',
    '2026-11-07',
    'Soweto',
    'A road running event celebrating the Soweto community.',
    'Upcoming'
),
(
    1,
    1,
    'Cape Town Coastal Run',
    '2026-12-05',
    'Cape Town',
    'A scenic running event along the Cape Town coast.',
    'Upcoming'
);


/* =========================================================
   11. SAMPLE ENROLMENTS
   ========================================================= */

INSERT INTO Enrolment
(
    EventID,
    ParticipantID,
    Status,
    PaymentStatus
)
VALUES
(
    1,
    1,
    'Active',
    'Paid'
),
(
    1,
    2,
    'Active',
    'Paid'
),
(
    2,
    1,
    'Active',
    'Pending'
),
(
    3,
    2,
    'Active',
    'Paid'
);


/* =========================================================
   12. SAMPLE RESULTS
   ========================================================= */

INSERT INTO Result
(
    EnrolmentID,
    FinishTime,
    Position,
    ResultStatus
)
VALUES
(
    1,
    '01:05:32',
    12,
    'Finished'
),
(
    2,
    '00:32:15',
    5,
    'Finished'
);


/* =========================================================
   13. TEST THE DATABASE
   ========================================================= */

SELECT * FROM Organiser;

SELECT * FROM Category;

SELECT * FROM Event;

SELECT * FROM Participant;

SELECT * FROM Enrolment;

SELECT * FROM Result;


/* =========================================================
   END OF RACE DAY DATABASE
   ========================================================= */
