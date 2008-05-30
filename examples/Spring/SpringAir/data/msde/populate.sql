--
-- Copyright © 2002-2005 the original author or authors.
--
-- Licensed under the Apache License, Version 2.0 (the "License");
-- you may not use this file except in compliance with the License.
-- You may obtain a copy of the License at
--
--      http://www.apache.org/licenses/LICENSE-2.0
--
-- Unless required by applicable law or agreed to in writing, software
-- distributed under the License is distributed on an "AS IS" BASIS,
-- WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
-- See the License for the specific language governing permissions and
-- limitations under the License.
--
--
-- MSDE schema population script for the SpringAir reference application
--
-- $Id: populate.sql,v 1.1 2005/10/01 22:57:09 springboy Exp $
--

USE SpringAir
GO

INSERT INTO cabin_class(id, description) VALUES(0, "Coach")
INSERT INTO cabin_class(id, description) VALUES(1, "Business")
INSERT INTO cabin_class(id, description) VALUES(2, "First")

GO

INSERT INTO airport(id, code, city, description) VALUES(0, "MCO", "Orlando", "Orlando International Airport")
INSERT INTO airport(id, code, city, description) VALUES(1, "SFO", "San Francisco", "San Francisco International Airport")
INSERT INTO airport(id, code, city, description) VALUES(2, "LHR", "Heathrow", "London Heathrow International Airport")
INSERT INTO airport(id, code, city, description) VALUES(3, "ANC", "Anchorage", "Anchorage International Airport")

GO

INSERT INTO passenger(id, first_name, surname) VALUES(0, "Ivan", "Goncharov")
INSERT INTO passenger(id, first_name, surname) VALUES(1, "Harriet", "Wheeler")
INSERT INTO passenger(id, first_name, surname) VALUES(3, "Rose", "Kane")
INSERT INTO passenger(id, first_name, surname) VALUES(5, "Leonardo", "Medici")
INSERT INTO passenger(id, first_name, surname) VALUES(6, "Enrico", "Dandolo")

GO

INSERT INTO aircraft(id, model, row_count, seats_per_row) VALUES(0, "Boeing 747", 240, 8)
INSERT INTO aircraft(id, model, row_count, seats_per_row) VALUES(1, "Little Nellie", 1, 1)
INSERT INTO aircraft(id, model, row_count, seats_per_row) VALUES(2, "The Fast Lady", 2, 2)
INSERT INTO aircraft(id, model, row_count, seats_per_row) VALUES(3, "Sopwith Camel", 10, 4)
INSERT INTO aircraft(id, model, row_count, seats_per_row) VALUES(4, "Boeing 747", 240, 8)

GO

INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(0, 0, 180)
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(0, 1, 40)
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(0, 2, 20)
-- Little Nellie defended her honour... admirably.
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(1, 2, 1)
-- I've taken your advice... I've bought the Fast Lady!
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(2, 2, 4)
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(3, 0, 30)
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(3, 2, 10)
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(4, 0, 180)
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(4, 1, 40)
INSERT INTO aircraft_cabin_seat(aircraft_id, cabin_class_id, seat_count) VALUES(4, 2, 20)

GO

-- uses British date format for the insert...
INSERT INTO flight(id, flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES(0, "S87r17688972", 0, 0, 1, CONVERT(DATETIME, '09-11-2005 12:50:00', 103))
INSERT INTO flight(id, flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES(1, "Mjhgsdjhds", 4, 0, 1, CONVERT(DATETIME, '10-11-2005 12:50:00', 103))
INSERT INTO flight(id, flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES(2, "kbfbxsKLJ34GW", 0, 0, 1, CONVERT(DATETIME, '11-11-2005 12:50:00', 103))
INSERT INTO flight(id, flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES(3, "KJ2BF3JH", 4, 0, 1, CONVERT(DATETIME, '12-11-2005 12:50:00', 103))

INSERT INTO flight(id, flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES(4, "jh34jhdsf", 4, 1, 0, CONVERT(DATETIME, '09-11-2005 13:50:00', 103))
INSERT INTO flight(id, flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES(5, "kbwdjnkdvb", 0, 1, 0, CONVERT(DATETIME, '10-11-2005 13:50:00', 103))
INSERT INTO flight(id, flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES(6, "hb3hjwfhjdvs", 4, 1, 0, CONVERT(DATETIME, '11-11-2005 13:50:00', 103))
INSERT INTO flight(id, flight_number, aircraft_id, departure_airport_id, destination_airport_id, departure_date)
	VALUES(7, "782876bjk", 0, 1, 0, CONVERT(DATETIME, '12-11-2005 13:50:00', 103))

GO

-- reserve a coupla seats on the first flight out of Orlando; I'm off to GatorJUG and Macy is coming wit' me, yeehar...
INSERT INTO reserved_seat(flight_id, seat_number) VALUES(0, "A21")
INSERT INTO reserved_seat(flight_id, seat_number) VALUES(0, "A22")

GO