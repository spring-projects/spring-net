// ** I18N

// Calendar EN language
// Author: Mihai Bazon, <mihai_bazon@yahoo.com>
// Translation: Yourim Yi <yyi@yourim.net>
// Encoding: EUC-KR
// lang : ko
// Distributed under the same terms as the calendar itself.

// For translators: please use UTF-8 if possible.  We strongly believe that
// Unicode is the answer to a real internationalized world.  Also please
// include your contact information in the header, as can be seen above.

// full day names

Calendar._DN = new Array
("�Ͽ���",
 "������",
 "ȭ����",
 "������",
 "�����",
 "�ݿ���",
 "�����",
 "�Ͽ���");

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
("��",
 "��",
 "ȭ",
 "��",
 "��",
 "��",
 "��",
 "��");

// full month names
Calendar._MN = new Array
("1��",
 "2��",
 "3��",
 "4��",
 "5��",
 "6��",
 "7��",
 "8��",
 "9��",
 "10��",
 "11��",
 "12��");

// short month names
Calendar._SMN = new Array
("1",
 "2",
 "3",
 "4",
 "5",
 "6",
 "7",
 "8",
 "9",
 "10",
 "11",
 "12");

// tooltips
Calendar._TT = {};
Calendar._TT["INFO"] = "calendar �� ���ؼ�";

Calendar._TT["ABOUT"] =
"DHTML Date/Time Selector\n" +
"(c) dynarch.com 2002-2005 / Author: Mihai Bazon\n" + // don't translate this this ;-)
"\n"+
"�ֽ� ������ �����÷��� http://www.dynarch.com/projects/calendar/ �� �湮�ϼ���\n" +
"\n"+
"GNU LGPL ���̼����� �����˴ϴ�. \n"+
"���̼����� ���� �ڼ��� ������ https://gnu.org/licenses/lgpl.html �� ��������." +
"\n\n" +
"��¥ ����:\n" +
"- ������ �����Ϸ��� \xab, \xbb ��ư�� ����մϴ�\n" +
"- ���� �����Ϸ��� " + String.fromCharCode(0x2039) + ", " + String.fromCharCode(0x203a) + " ��ư�� ��������\n" +
"- ��� ������ ������ �� ������ ������ �����Ͻ� �� �ֽ��ϴ�.";
Calendar._TT["ABOUT_TIME"] = "\n\n" +
"�ð� ����:\n" +
"- ���콺�� ������ �ð��� �����մϴ�\n" +
"- Shift Ű�� �Բ� ������ �����մϴ�\n" +
"- ���� ���¿��� ���콺�� �����̸� �� �� ������ ���� ���մϴ�.\n";

Calendar._TT["PREV_YEAR"] = "���� �� (��� ������ ���)";
Calendar._TT["PREV_MONTH"] = "���� �� (��� ������ ���)";
Calendar._TT["GO_TODAY"] = "���� ��¥��";
Calendar._TT["NEXT_MONTH"] = "���� �� (��� ������ ���)";
Calendar._TT["NEXT_YEAR"] = "���� �� (��� ������ ���)";
Calendar._TT["SEL_DATE"] = "��¥�� �����ϼ���";
Calendar._TT["DRAG_TO_MOVE"] = "���콺 �巡�׷� �̵� �ϼ���";
Calendar._TT["PART_TODAY"] = " (����)";
Calendar._TT["MON_FIRST"] = "�������� �� ���� ���� ���Ϸ�";
Calendar._TT["SUN_FIRST"] = "�Ͽ����� �� ���� ���� ���Ϸ�";
Calendar._TT["CLOSE"] = "�ݱ�";
Calendar._TT["TODAY"] = "����";
Calendar._TT["TIME_PART"] = "(Shift-)Ŭ�� �Ǵ� �巡�� �ϼ���";

// date formats
Calendar._TT["DEF_DATE_FORMAT"] = "%Y-%m-%d";
Calendar._TT["TT_DATE_FORMAT"] = "%b/%e [%a]";

Calendar._TT["WK"] = "��";
