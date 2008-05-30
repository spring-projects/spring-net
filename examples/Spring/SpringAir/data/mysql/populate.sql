INSERT INTO cabin_class(description) VALUES('Coach');
INSERT INTO cabin_class(description) VALUES('Business');
INSERT INTO cabin_class(description) VALUES('First');

INSERT INTO airport(code, city, description) VALUES('MCO', 'Orlando', 'Orlando International Airport');
INSERT INTO airport(code, city, description) VALUES('SFO', 'San Francisco', 'San Francisco International Airport');
INSERT INTO airport(code, city, description) VALUES('LHR', 'Heathrow', 'London Heathrow International Airport');
INSERT INTO airport(code, city, description) VALUES('ANC', 'Anchorage', 'Anchorage International Airport');

INSERT INTO passenger(first_name, surname) VALUES('Ivan', 'Goncharov');
INSERT INTO passenger(first_name, surname) VALUES('Harriet', 'Wheeler');
INSERT INTO passenger(first_name, surname) VALUES('Rose', 'Kane');
INSERT INTO passenger(first_name, surname) VALUES('Leonardo', 'Medici');
INSERT INTO passenger(first_name, surname) VALUES('Enrico', 'Dandolo');

INSERT INTO aircraft(model, row_count, seats_per_row) VALUES('Boeing 747', 240, 8);
INSERT INTO aircraft(model, row_count, seats_per_row) VALUES('Little Nellie', 1, 1);
INSERT INTO aircraft(model, row_count, seats_per_row) VALUES('The Fast Lady', 2, 2);
INSERT INTO aircraft(model, row_count, seats_per_row) VALUES('Sopwith Camel', 10, 4);
INSERT INTO aircraft(model, row_count, seats_per_row) VALUES('Boeing 747', 240, 8);

INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(1, 0, 180);
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(1, 1, 40);
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(1, 2, 20);
-- Little Nellie defended her honour... admirably.
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(2, 2, 1);
-- I've taken your advice... I've bought the Fast Lady!
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(3, 2, 4);
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(4, 0, 30);
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(4, 2, 10);
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(5, 0, 180);
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(5, 1, 40);
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(5, 2, 20);

-- uses Universal date format for the insert...
INSERT INTO flight(flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES('S87r17688972', 1, 1, 2, DATE('2005-11-09 12:50'));
INSERT INTO flight(flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES('Mjhgsdjhds', 5, 1, 2, DATE('2005-11-10 12:50'));
INSERT INTO flight(flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES('kbfbxsKLJ34GW', 1, 1, 2, DATE('2005-11-11 12:50'));
INSERT INTO flight(flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES('KJ2BF3JH', 5, 1, 2, DATE('2005-11-12 12:50'));

INSERT INTO flight(flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES('jh34jhdsf', 5, 2, 1, DATE('2005-11-09 13:50'));
INSERT INTO flight(flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES('kbwdjnkdvb', 1, 2, 1, DATE('2005-11-10 13:50'));
INSERT INTO flight(flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES('hb3hjwfhjdvs', 5, 2, 1, DATE('2005-11-11 13:50'));
INSERT INTO flight(flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES('782876bjk', 1, 2, 1, DATE('2005-11-12 13:50'));

-- reserve a coupla seats on the first flight out of Orlando; I'm off to GatorJUG and Macy is coming wit' me, yeehar...
INSERT INTO reserved_seat(flight_id, seat_number) VALUES(1, 'A21');
INSERT INTO reserved_seat(flight_id, seat_number) VALUES(1, 'A22');

commit;
