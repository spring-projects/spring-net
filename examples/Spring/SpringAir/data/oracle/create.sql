CREATE TABLE t_cabin_class
(
    id integer NOT NULL PRIMARY KEY,
    description VARCHAR(16)
);

CREATE TABLE t_passenger
(
    id integer NOT NULL PRIMARY KEY,
    first_name VARCHAR(64),
    surname VARCHAR(64)
    -- I'll create a proper relation later, this stub is sufficient for now...
);

CREATE TABLE t_airport
(
    id integer NOT NULL PRIMARY KEY,
    code CHAR(3) NOT NULL,
    city VARCHAR(64),
    description VARCHAR(64)
);

CREATE TABLE t_aircraft
(
	id integer NOT NULL PRIMARY KEY,
	model VARCHAR(50) NOT NULL,
	row_count integer NOT NULL,
	seats_per_row integer NOT NULL
);

-- an aircraft may have many cabins; this relation models how many seats there are per cabin per aircraft...
CREATE TABLE t_aircraft_cabin_seat
(
	aircraft_id integer NOT NULL,
	cabin_class_id  integer  NOT NULL,
	seat_count integer DEFAULT(0) NOT NULL
);

alter table t_aircraft_cabin_seat add (
	constraint fk_cabin_seat_to_aircraft foreign key(aircraft_id) references t_aircraft(id),
	constraint fk_cabin_seat_to_cabin foreign key(cabin_class_id) references t_cabin_class(id)
);

CREATE TABLE t_flight
(
	id integer NOT NULL PRIMARY KEY,
	flight_number VARCHAR(256) NOT NULL UNIQUE,
	aircraft_id integer NOT NULL,
	departure_airport_id integer NOT NULL,
	destination_airport_id integer NOT NULL,
	departure_date date NOT NULL
);

alter table t_flight add (
	constraint fk_flight_to_aircraft foreign key(aircraft_id) references t_aircraft(id),
	constraint fk_flight_to_dep_airport foreign key(departure_airport_id) references t_airport(id),
	constraint fk_flight_to_dest_airport foreign key(destination_airport_id) references t_airport(id)
);

-- if a seat is in this relation, then it is reserved for that flight...
CREATE TABLE t_reserved_seat
(
	flight_id integer NOT NULL,
	seat_number VARCHAR(20) NOT NULL
);

alter table t_reserved_seat add (
	constraint fk_reserved_seat_to_flight foreign key(flight_id) references t_flight(id)
);

CREATE TABLE t_reservation
(
	id integer NOT NULL PRIMARY KEY,
	passenger_id integer NOT NULL UNIQUE,
	price number(8, 2) DEFAULT(0) NOT NULL
);

alter table t_reservation add (
	constraint fk_reservation_to_flight foreign key(passenger_id) references t_passenger(id)
);

CREATE TABLE t_leg
(
	id integer NOT NULL PRIMARY KEY,
	reservation_id integer NOT NULL UNIQUE,
	flight_id integer NOT NULL,
	cabin_class_id  integer  NOT NULL
);

alter table t_leg add (
	constraint fk_leg_to_reservation foreign key(reservation_id) references t_reservation(id),
	constraint fk_leg_to_flight foreign key(flight_id) references t_flight(id),
	constraint fk_leg_to_cabin_class foreign key(cabin_class_id) references t_cabin_class(id)
);

CREATE TABLE t_leg_seat 
(
	leg_id integer NOT NULL UNIQUE,
	reservation_id integer NOT NULL UNIQUE,
	seat_number VARCHAR(24) NOT NULL
);

alter table t_leg_seat add (
	constraint fk_leg_seat_to_leg foreign key(leg_id) references t_leg(id),
	constraint fk_leg_seat_to_reservation foreign key(reservation_id) references t_reservation(id)
);
