﻿CREATE TABLE [Credits]
(
    [
    CreditID] [
    int]
    IDENTITY
    NOT
    NULL, [
    CreditAmount] [
    float]
    NOT
    NULL,
    CONSTRAINT [
    PK_CreditID]
    PRIMARY
    KEY
    CLUSTERED
    (
[
    CreditID]
    ASC
) ON [PRIMARY]
    )
  ON [PRIMARY]
;

CREATE TABLE [Debits]
(
    [
    DebitID] [
    int]
    IDENTITY
    NOT
    NULL, [
    DebitAmount] [
    float]
    NOT
    NULL,
    CONSTRAINT [
    PK_DebitID]
    PRIMARY
    KEY
    CLUSTERED
    (
[
    DebitID]
    ASC
) ON [PRIMARY]
    )
  ON [PRIMARY]
;