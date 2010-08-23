// ** I18N

// Calendar - Serbian language (Latin)
// Author: Aleksandar Seovic (aleks@seovic.com)
// Encoding: any
// Distributed under the same terms as the calendar itself.

// For translators: please use UTF-8 if possible.  We strongly believe that
// Unicode is the answer to a real internationalized world.  Also please
// include your contact information in the header, as can be seen above.

// full day names
Calendar._DN = new Array
("nedelja",
 "ponedeljak",
 "utorak",
 "sreda",
 "četvrtak",
 "petak",
 "subota",
 "nedelja");

// First day of the week. "0" means display Sunday first, "1" means display
// Monday first, etc.
Calendar._FD = 1;

// full month names
Calendar._MN = new Array
("januar",
 "februar",
 "mart",
 "april",
 "maj",
 "jun",
 "jul",
 "avgust",
 "septembar",
 "oktobar",
 "novembar",
 "decembar");

// short month names
Calendar._SMN = new Array
("jan",
 "feb",
 "mar",
 "apr",
 "maj",
 "jun",
 "jul",
 "avg",
 "sep",
 "okt",
 "nov",
 "dec");

// tooltips
Calendar._TT = {};
Calendar._TT["INFO"] = "O kalendaru";

Calendar._TT["ABOUT"] =
"DHTML Date/Time Selector\n" +
"(c) dynarch.com 2002-2005 / Author: Mihai Bazon\n" + // don't translate this this ;-)
"For latest version visit: http://www.dynarch.com/projects/calendar/\n" +
"Distributed under GNU LGPL.  See http://gnu.org/licenses/lgpl.html for details." +
"\n\n" +
"Izbor datuma:\n" +
"- Uz pomoć tastera \xab, \xbb možete izabrati godinu\n" +
"- Uz pomoć tastera " + String.fromCharCode(0x2039) + ", " + String.fromCharCode(0x203a) + " možete izabrati mesec\n" +
"- Držite levo dugme miša pritisnuto za bržu selekciju.";
Calendar._TT["ABOUT_TIME"] = "\n\n" +
"Izbor vremena:\n" +
"- Kliknite na sat ili minut da ga uvećate\n" +
"- ili Shift-click da ga umanjite\n" +
"- ili pritisnite levo dugme miša i povucite za bržu selekciju.";

Calendar._TT["PREV_YEAR"] = "Prethodna godina";
Calendar._TT["PREV_MONTH"] = "Prethodni mesec";
Calendar._TT["GO_TODAY"] = "Danas";
Calendar._TT["NEXT_MONTH"] = "Sledeći mesec";
Calendar._TT["NEXT_YEAR"] = "Sledeća godina";
Calendar._TT["SEL_DATE"] = "Izaberi datum";
Calendar._TT["DRAG_TO_MOVE"] = "Povuci za promenu";
Calendar._TT["PART_TODAY"] = " (danas)";

// the following is to inform that "%s" is to be the first day of week
// %s will be replaced with the day name.
Calendar._TT["DAY_FIRST"] = "Prvo prikaži %s";

// This may be locale-dependent.  It specifies the week-end days, as an array
// of comma-separated numbers.  The numbers are from 0 to 6: 0 means Sunday, 1
// means Monday, etc.
Calendar._TT["WEEKEND"] = "0,6";

Calendar._TT["CLOSE"] = "Zatvori";
Calendar._TT["TODAY"] = "Danas";
Calendar._TT["TIME_PART"] = "(Shift-)Click ili povuci da promeniš vrednost";

// date formats
Calendar._TT["DEF_DATE_FORMAT"] = "%d.%m.%Y";
Calendar._TT["TT_DATE_FORMAT"] = "%a, %b %e";

Calendar._TT["WK"] = "ned";
Calendar._TT["TIME"] = "Vreme:";
