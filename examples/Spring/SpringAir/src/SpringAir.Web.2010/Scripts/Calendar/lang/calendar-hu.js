// ** I18N

// Calendar HU language
// Author: ???
// Modifier: KARASZI Istvan, <jscalendar@spam.raszi.hu>
// Encoding: any
// Distributed under the same terms as the calendar itself.

// For translators: please use UTF-8 if possible.  We strongly believe that
// Unicode is the answer to a real internationalized world.  Also please
// include your contact information in the header, as can be seen above.

// full day names
Calendar._DN = new Array
("Vas�rnap",
 "H�tf�",
 "Kedd",
 "Szerda",
 "Cs�t�rt�k",
 "P�ntek",
 "Szombat",
 "Vas�rnap");

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
("v",
 "h",
 "k",
 "sze",
 "cs",
 "p",
 "szo",
 "v");

// full month names
Calendar._MN = new Array
("janu�r",
 "febru�r",
 "m�rcius",
 "�prilis",
 "m�jus",
 "j�nius",
 "j�lius",
 "augusztus",
 "szeptember",
 "okt�ber",
 "november",
 "december");

// short month names
Calendar._SMN = new Array
("jan",
 "feb",
 "m�r",
 "�pr",
 "m�j",
 "j�n",
 "j�l",
 "aug",
 "sze",
 "okt",
 "nov",
 "dec");

// tooltips
Calendar._TT = {};
Calendar._TT["INFO"] = "A kalend�riumr�l";

Calendar._TT["ABOUT"] =
"DHTML d�tum/id� kiv�laszt�\n" +
"(c) dynarch.com 2002-2005 / Author: Mihai Bazon\n" + // don't translate this this ;-)
"a legfrissebb verzi� megtal�lhat�: http://www.dynarch.com/projects/calendar/\n" +
"GNU LGPL alatt terjesztve.  L�sd a https://gnu.org/licenses/lgpl.html oldalt a r�szletekhez." +
"\n\n" +
"D�tum v�laszt�s:\n" +
"- haszn�lja a \xab, \xbb gombokat az �v kiv�laszt�s�hoz\n" +
"- haszn�lja a " + String.fromCharCode(0x2039) + ", " + String.fromCharCode(0x203a) + " gombokat a h�nap kiv�laszt�s�hoz\n" +
"- tartsa lenyomva az eg�rgombot a gyors v�laszt�shoz.";
Calendar._TT["ABOUT_TIME"] = "\n\n" +
"Id� v�laszt�s:\n" +
"- kattintva n�velheti az id�t\n" +
"- shift-tel kattintva cs�kkentheti\n" +
"- lenyomva tartva �s h�zva gyorsabban kiv�laszthatja.";

Calendar._TT["PREV_YEAR"] = "El�z� �v (tartsa nyomva a men�h�z)";
Calendar._TT["PREV_MONTH"] = "El�z� h�nap (tartsa nyomva a men�h�z)";
Calendar._TT["GO_TODAY"] = "Mai napra ugr�s";
Calendar._TT["NEXT_MONTH"] = "K�v. h�nap (tartsa nyomva a men�h�z)";
Calendar._TT["NEXT_YEAR"] = "K�v. �v (tartsa nyomva a men�h�z)";
Calendar._TT["SEL_DATE"] = "V�lasszon d�tumot";
Calendar._TT["DRAG_TO_MOVE"] = "H�zza a mozgat�shoz";
Calendar._TT["PART_TODAY"] = " (ma)";

// the following is to inform that "%s" is to be the first day of week
// %s will be replaced with the day name.
Calendar._TT["DAY_FIRST"] = "%s legyen a h�t els� napja";

// This may be locale-dependent.  It specifies the week-end days, as an array
// of comma-separated numbers.  The numbers are from 0 to 6: 0 means Sunday, 1
// means Monday, etc.
Calendar._TT["WEEKEND"] = "0,6";

Calendar._TT["CLOSE"] = "Bez�r";
Calendar._TT["TODAY"] = "Ma";
Calendar._TT["TIME_PART"] = "(Shift-)Klikk vagy h�z�s az �rt�k v�ltoztat�s�hoz";

// date formats
Calendar._TT["DEF_DATE_FORMAT"] = "%Y-%m-%d";
Calendar._TT["TT_DATE_FORMAT"] = "%b %e, %a";

Calendar._TT["WK"] = "h�t";
Calendar._TT["TIME"] = "id�:";
