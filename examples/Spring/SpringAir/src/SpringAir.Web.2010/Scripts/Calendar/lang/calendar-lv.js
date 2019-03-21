// ** I18N

// Calendar LV language
// Author: Juris Valdovskis, <juris@dc.lv>
// Encoding: cp1257
// Distributed under the same terms as the calendar itself.

// For translators: please use UTF-8 if possible.  We strongly believe that
// Unicode is the answer to a real internationalized world.  Also please
// include your contact information in the header, as can be seen above.

// full day names
Calendar._DN = new Array
("Sv�tdiena",
 "Pirmdiena",
 "Otrdiena",
 "Tre�diena",
 "Ceturdiena",
 "Piektdiena",
 "Sestdiena",
 "Sv�tdiena");

// Please note that the following array of short day names (and the same goes
// for short month names, _SMN) isn't absolutely necessary.  We give it here
// for exemplification on how one can customize the short day names, but if
// they are simply the first N letters of the full name you can simply say:
//
//   Calendar._SDN_len = N; // short day name length
//   Calendar._SMN_len = N; // short month name length
//
// If N = 3 then this is not needed either since we assume a value of 3 if not
// present, to be compatible with translation files that were written before
// this feature.

// short day names
Calendar._SDN = new Array
("Sv",
 "Pr",
 "Ot",
 "Tr",
 "Ce",
 "Pk",
 "Se",
 "Sv");

// full month names
Calendar._MN = new Array
("Janv�ris",
 "Febru�ris",
 "Marts",
 "Apr�lis",
 "Maijs",
 "J�nijs",
 "J�lijs",
 "Augusts",
 "Septembris",
 "Oktobris",
 "Novembris",
 "Decembris");

// short month names
Calendar._SMN = new Array
("Jan",
 "Feb",
 "Mar",
 "Apr",
 "Mai",
 "J�n",
 "J�l",
 "Aug",
 "Sep",
 "Okt",
 "Nov",
 "Dec");

// tooltips
Calendar._TT = {};
Calendar._TT["INFO"] = "Par kalend�ru";

Calendar._TT["ABOUT"] =
"DHTML Date/Time Selector\n" +
"(c) dynarch.com 2002-2005 / Author: Mihai Bazon\n" + // don't translate this this ;-)
"For latest version visit: http://www.dynarch.com/projects/calendar/\n" +
"Distributed under GNU LGPL.  See https://gnu.org/licenses/lgpl.html for details." +
"\n\n" +
"Datuma izv�le:\n" +
"- Izmanto \xab, \xbb pogas, lai izv�l�tos gadu\n" +
"- Izmanto " + String.fromCharCode(0x2039) + ", " + String.fromCharCode(0x203a) + "pogas, lai izv�l�tos m�nesi\n" +
"- Turi nospiestu peles pogu uz jebkuru no augst�k min�taj�m pog�m, lai pa�trin�tu izv�li.";
Calendar._TT["ABOUT_TIME"] = "\n\n" +
"Laika izv�le:\n" +
"- Uzklik��ini uz jebkuru no laika da��m, lai palielin�tu to\n" +
"- vai Shift-klik��is, lai samazin�tu to\n" +
"- vai noklik��ini un velc uz attiec�go virzienu lai main�tu �tr�k.";

Calendar._TT["PREV_YEAR"] = "Iepr. gads (turi izv�lnei)";
Calendar._TT["PREV_MONTH"] = "Iepr. m�nesis (turi izv�lnei)";
Calendar._TT["GO_TODAY"] = "�odien";
Calendar._TT["NEXT_MONTH"] = "N�ko�ais m�nesis (turi izv�lnei)";
Calendar._TT["NEXT_YEAR"] = "N�ko�ais gads (turi izv�lnei)";
Calendar._TT["SEL_DATE"] = "Izv�lies datumu";
Calendar._TT["DRAG_TO_MOVE"] = "Velc, lai p�rvietotu";
Calendar._TT["PART_TODAY"] = " (�odien)";

// the following is to inform that "%s" is to be the first day of week
// %s will be replaced with the day name.
Calendar._TT["DAY_FIRST"] = "Att�lot %s k� pirmo";

// This may be locale-dependent.  It specifies the week-end days, as an array
// of comma-separated numbers.  The numbers are from 0 to 6: 0 means Sunday, 1
// means Monday, etc.
Calendar._TT["WEEKEND"] = "1,7";

Calendar._TT["CLOSE"] = "Aizv�rt";
Calendar._TT["TODAY"] = "�odien";
Calendar._TT["TIME_PART"] = "(Shift-)Klik��is vai p�rvieto, lai main�tu";

// date formats
Calendar._TT["DEF_DATE_FORMAT"] = "%d-%m-%Y";
Calendar._TT["TT_DATE_FORMAT"] = "%a, %e %b";

Calendar._TT["WK"] = "wk";
Calendar._TT["TIME"] = "Laiks:";
