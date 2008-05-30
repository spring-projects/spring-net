CREATE TABLE cabin_class
(
    id integer NOT NULL PRIMARY KEY AUTO_INCREMENT,
    description VARCHAR(16)
);

CREATE TABLE passenger
(
    id integer NOT NULL PRIMARY KEY AUTO_INCREMENT,
    first_name VARCHAR(64),
    surname VARCHAR(64)
    -- I'll create a proper relation later, this stub is sufficient for now...
);

CREATE TABLE airport
(
    id integer NOT NULL PRIMARY KEY AUTO_INCREMENT,
    code CHAR(3) NOT NULL,
    city VARCHAR(64),
    description VARCHAR(64)
);

CREATE TABLE aircraft
(
	id integer NOT NULL PRIMARY KEY AUTO_INCREMENT,
	model VARCHAR(50) NOT NULL,
	row_count integer NOT NULL,
	seats_per_row integer NOT NULL
);

-- an aircraft may have many cabins; this relation models how many seats there are per cabin per aircraft...
CREATE TABLE aircraft_cabin_seat
(
	aircraft_id integer NOT NULL,
	cabin_class_id  integer  NOT NULL,
	seat_count integer DEFAULT 0 NOT NULL
);

CREATE TABLE flight
(
	id integer NOT NULL PRIMARY KEY AUTO_INCREMENT,
	flight_number VARCHAR(256) NOT NULL,
	aircraft_id integer NOT NULL,
	departure_airport_id integer NOT NULL,
	destination_airport_id integer NOT NULL,
	departure_date date NOT NULL
);

-- if a seat is in this relation, then it is reserved for that flight...
CREATE TABLE reserved_seat
(
	flight_id integer NOT NULL,
	seat_number VARCHAR(20) NOT NULL
);

CREATE TABLE reservation
(
	id integer NOT NULL PRIMARY KEY AUTO_INCREMENT,
	passenger_id integer NOT NULL UNIQUE,
	price decimal(8, 2) DEFAULT 0 NOT NULL
);

CREATE TABLE leg
(
	id integer NOT NULL PRIMARY KEY AUTO_INCREMENT,
	reservation_id integer NOT NULL UNIQUE,
	flight_id integer NOT NULL,
	cabin_class_id  integer  NOT NULL
);

CREATE TABLE leg_seat 
(
	leg_id integer NOT NULL UNIQUE,
	reservation_id integer NOT NULL UNIQUE,
	seat_number VARCHAR(24) NOT NULL
);
