// ** I18N

// Calendar - Serbian language (Cyrillic)
// Author: Aleksandar Seovic (aleks@seovic.com)
// Encoding: any
// Distributed under the same terms as the calendar itself.

// For translators: please use UTF-8 if possible.  We strongly believe that
// Unicode is the answer to a real internationalized world.  Also please
// include your contact information in the header, as can be seen above.

// full day names
Calendar._DN = new Array
("недеља",
 "понедељак",
 "уторак",
 "среда",
 "четвртак",
 "петак",
 "субота",
 "недеља");

// First day of the week. "0" means display Sunday first, "1" means display
// Monday first, etc.
Calendar._FD = 1;

// full month names
Calendar._MN = new Array
("јануар",
 "фебруар",
 "март",
 "април",
 "мај",
 "јун",
 "јул",
 "август",
 "септембар",
 "октобар",
 "новембар",
 "децембар");

// short month names
Calendar._SMN = new Array
("јан",
 "феб",
 "мар",
 "апр",
 "мaj",
 "јун",
 "јул",
 "авг",
 "сеп",
 "окт",
 "нов",
 "дец");

// tooltips
Calendar._TT = {};
Calendar._TT["INFO"] = "O календару";

Calendar._TT["ABOUT"] =
"DHTML Date/Time Selector\n" +
"(c) dynarch.com 2002-2005 / Author: Mihai Bazon\n" + // don't translate this this ;-)
"For latest version visit: http://www.dynarch.com/projects/calendar/\n" +
"Distributed under GNU LGPL.  See http://gnu.org/licenses/lgpl.html for details." +
"\n\n" +
"Избор датума:\n" +
"- Уз помоћ тастера \xab, \xbb можете изабрати годину\n" +
"- Уз помоћ тастера " + String.fromCharCode(0x2039) + ", " + String.fromCharCode(0x203a) + " можете изабрати месец\n" +
"- Држите лево дугме миша притиснуто за бржу селекцију.";
Calendar._TT["ABOUT_TIME"] = "\n\n" +
"Избор времена:\n" +
"- Кликните на сат или минут да га увећате\n" +
"- или Shift-click да га умањите\n" +
"- или притисните лево дугме миша и повуците за бржу селекцију.";

Calendar._TT["PREV_YEAR"] = "Претходна година";
Calendar._TT["PREV_MONTH"] = "Претходни месец";
Calendar._TT["GO_TODAY"] = "Данас";
Calendar._TT["NEXT_MONTH"] = "Следећи месец";
Calendar._TT["NEXT_YEAR"] = "Следећа година";
Calendar._TT["SEL_DATE"] = "Изабери датум";
Calendar._TT["DRAG_TO_MOVE"] = "Повуци за промену";
Calendar._TT["PART_TODAY"] = " (данас)";

// the following is to inform that "%s" is to be the first day of week
// %s will be replaced with the day name.
Calendar._TT["DAY_FIRST"] = "Прво прикажи %s";

// This may be locale-dependent.  It specifies the week-end days, as an array
// of comma-separated numbers.  The numbers are from 0 to 6: 0 means Sunday, 1
// means Monday, etc.
Calendar._TT["WEEKEND"] = "0,6";

Calendar._TT["CLOSE"] = "Затвори";
Calendar._TT["TODAY"] = "Данас";
Calendar._TT["TIME_PART"] = "(Shift-)Click или повуци да промениш вредност";

// date formats
Calendar._TT["DEF_DATE_FORMAT"] = "%d.%m.%Y";
Calendar._TT["TT_DATE_FORMAT"] = "%a, %b %e";

Calendar._TT["WK"] = "нед";
Calendar._TT["TIME"] = "Време:";
