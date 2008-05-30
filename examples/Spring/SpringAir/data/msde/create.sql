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
-- MSDE schema creation script for the SpringAir reference application
--
-- $Id: create.sql,v 1.2 2005/10/02 00:04:01 springboy Exp $
--

USE MASTER  
GO

IF EXISTS(SELECT * FROM sysdatabases WHERE name='SpringAir')
    DROP DATABASE SpringAir  
GO


CREATE DATABASE SpringAir
GO

USE SpringAir
GO

CREATE TABLE cabin_class
(
    id INT PRIMARY KEY NOT NULL,
    description VARCHAR(20)
)
GO

CREATE TABLE passenger
(
    id INT PRIMARY KEY NOT NULL,
    first_name VARCHAR(30),
    surname VARCHAR(30)
    -- I'll create a proper relation later, this stub is sufficient for now...
)
GO

CREATE TABLE airport
(
    id INT PRIMARY KEY NOT NULL,
    code CHAR(3) NOT NULL,
    city VARCHAR(50),
    description VARCHAR(100)
)
GO

CREATE TABLE aircraft
(
	id INT PRIMARY KEY NOT NULL,
	model VARCHAR(50) NOT NULL,
	row_count INT NOT NULL,
	seats_per_row INT NOT NULL
)
GO

-- an aircraft may have many cabins; this relation models how many seats there are per cabin per aircraft...
CREATE TABLE aircraft_cabin_seat
(
	aircraft_id INT NOT NULL FOREIGN KEY REFERENCES aircraft(id),
	cabin_class_id  INT  NOT NULL FOREIGN KEY REFERENCES cabin_class(id),
	seat_count INT NOT NULL DEFAULT 0
)
GO

CREATE TABLE flight
(
	id INT PRIMARY KEY NOT NULL,
	flight_number VARCHAR(20) NOT NULL UNIQUE,
	aircraft_id INT NOT NULL FOREIGN KEY REFERENCES aircraft(id),
	departure_airport_id INT NOT NULL FOREIGN KEY REFERENCES airport(id),
	destination_airport_id INT NOT NULL FOREIGN KEY REFERENCES airport(id),
	departure_date DATETIME NOT NULL
)
GO

-- if a seat is in this relation, then it is reserved for that flight...
CREATE TABLE reserved_seat
(
	flight_id int NOT NULL FOREIGN KEY REFERENCES flight(id),
	seat_number VARCHAR(20) NOT NULL
)
GO

CREATE TABLE reservation
(
	id INT PRIMARY KEY NOT NULL,
	passenger_id INT NOT NULL UNIQUE FOREIGN KEY REFERENCES passenger(id),
	price MONEY NOT NULL DEFAULT 0
)
GO

CREATE TABLE leg
(
	id INT PRIMARY KEY NOT NULL,
	reservation_id INT NOT NULL UNIQUE FOREIGN KEY REFERENCES reservation(id),
	flight_id INT NOT NULL UNIQUE FOREIGN KEY REFERENCES flight(id),
	cabin_class_id  INT  NOT NULL FOREIGN KEY REFERENCES cabin_class(id)
)
GO

CREATE TABLE leg_seat 
(
	leg_id INT NOT NULL UNIQUE FOREIGN KEY REFERENCES leg(id),
	reservation_id INT NOT NULL UNIQUE FOREIGN KEY REFERENCES reservation(id),
	seat_number VARCHAR(200) NOT NULL
)
GO