using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static SeasonManagement.Class;
using static SeasonManagement.Enum;

namespace SeasonManagement
{
    public class Data
    {
        #region 定数
        /// <summary>
        /// ITTOで開発しているないようはすべてこのフォルダに格納する
        /// </summary>
        private readonly string ITTOPROJECT;
        /// <summary>
        /// マイドキュメントの中の設定ファイルの格納場所
        /// </summary>
        private readonly string MYDOCUMENT;
        /// <summary>
        /// 生徒情報の格納場所
        /// </summary>
        private readonly string STUDENT;
        /// <summary>
        /// 講師情報の格納場所
        /// </summary>
        private readonly string TEACHER;
        /// <summary>
        /// 通常授業の格納場所
        /// </summary>
        private readonly string NOMAL;
        /// <summary>
        /// 授業時間とブースの情報
        /// </summary>
        private readonly string SETTING;
        #endregion

        #region 変数
        /// <summary>
        /// 講師
        /// </summary>
        public Dictionary<string, CTeacher> Teacher;
        /// <summary>
        /// 生徒
        /// </summary>
        public Dictionary<string, CStudent> Student;
        /// <summary>
        /// 団体
        /// </summary>
        public Dictionary<string, COrganization> Organization;
        /// <summary>
        /// 通常授業
        /// </summary>
        public List<CTuition> NomalAllTuition;
        /// <summary>
        /// 更新前の古い通常授業を格納する
        /// </summary>
        public List<CTuition> OldNomalAllTuition;
        /// <summary>
        /// 追加された授業を探す
        /// </summary>
        public List<CTuition> AddNomalAllTuition;
        /// <summary>
        /// 季節講習のデータ
        /// </summary>
        public Dictionary<string, CTuition> AllTuition;
        /// <summary>
        /// 通常授業の登録できなかった箇所の登録
        /// </summary>
        public List<CAddClass> AddClass;
        /// <summary>
        /// 配布用のスケジュールを格納
        /// </summary>
        public Dictionary<string, string[]> ScheduleFlag;
        /// <summary>
        /// コマ時間帯
        /// </summary>
        public string[,] Koma;
        /// <summary>
        /// 教室のブースの数
        /// </summary>
        public int Booth;
        /// <summary>
        /// 開始日と終了日
        /// </summary>
        public DateTime[] FastEnd;
        /// <summary>
        /// 期間
        /// </summary>
        public int Days;
        #endregion

        public Data()
        {
            ITTOPROJECT = $@"{ Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\ITTOProject";
            MYDOCUMENT = $@"{ITTOPROJECT}\SeasonManagement";
            STUDENT = $@"{MYDOCUMENT}\Student.ini";
            TEACHER = $@"{MYDOCUMENT}\Teacher.ini";
            NOMAL = $@"{MYDOCUMENT}\NomalTuition.ini";
            SETTING = $@"{MYDOCUMENT}\ClassTimeAndBooth.ini";
            Teacher = new Dictionary<string, CTeacher>();
            Student = new Dictionary<string, CStudent>();
            Organization = new Dictionary<string, COrganization>();
            NomalAllTuition = new List<CTuition>();
            AddNomalAllTuition = new List<CTuition>();
            AllTuition = new Dictionary<string, CTuition>();
            AddClass = new List<CAddClass>();
            ScheduleFlag = new Dictionary<string, string[]>();
            Koma = new string[9, 2];
            for (int i = 0; i < 9; i++)
            {
                Koma[i, 0] = "";
                Koma[i, 1] = "";
            }
            Booth = new int();
            FastEnd = new DateTime[2];
        }

        #region 通常授業
        /// <summary>
        /// 新規で通常授業を生成する
        /// </summary>
        public void SetNomalNew()
        {
            if (NomalAllTuition.Count < 1)
            {
                for (int i = 0; i < 7; i++)
                {
                    NomalAllTuition.Add(new CTuition());
                    for (int j = 0; j < 9; j++)
                    {
                        NomalAllTuition[i].TimeClass.Add(new CTimeClass());
                        for (int k = 0; k < Booth; k++)
                        {
                            NomalAllTuition[i].TimeClass[j].NomalClass.Add(new CNomalClass());
                        }
                    }
                }
            }
            SetAllTuition();
        }

        /// <summary>
        /// 通常授業におけるコマの重みを計算する
        /// </summary>
        /// <param name="day">日にち</param>
        /// <param name="time">時間帯</param>
        /// <param name="nomal">ブース番号</param>
        public void setWeightNomal(int day, int time, int nomal)
        {
            try
            {
                var Nomal = NomalAllTuition[day].TimeClass[time].NomalClass[nomal];
                List<string> SchoolYear = new List<string>();
                foreach (var s in Nomal.Subject)
                {
                    SchoolYear.Add(Student[s.Name].SchoolYear);
                }
                if (SchoolYear.Where(c => c.Contains("高")).Count() != 0)
                {
                    Nomal.Weighting = 3 + Nomal.Subject.Where(c => c.Name.CompareTo("") != 0).Count() * 10;
                    return;
                }
                if (SchoolYear.Where(c => c.Contains("中")).Count() != 0)
                {
                    Nomal.Weighting = 2 + Nomal.Subject.Where(c => c.Name.CompareTo("") != 0).Count() * 10;
                    return;
                }
                if (SchoolYear.Where(c => c.Contains("小")).Count() != 0)
                    Nomal.Weighting = 1 + Nomal.Subject.Where(c => c.Name.CompareTo("") != 0).Count() * 10;
            }
            catch (Exception) { }
        }
        #endregion

        #region　季節講習
        /// <summary>
        /// 新規で季節講習を生成する
        /// </summary>
        private void SetAllTuition()
        {
            if (AllTuition.Count < 1)
            {
                for (int i = 0; i < Days; i++)
                {
                    string Day = GetDay(i);
                    AllTuition.Add(Day, new CTuition());
                    for (int j = 0; j < 9; j++)
                    {
                        AllTuition[Day].TimeClass.Add(new CTimeClass());
                        for (int k = 0; k < Booth; k++)
                        {
                            AllTuition[Day].TimeClass[j].NomalClass.Add(new CNomalClass());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 通常授業を季節講習にコピーする
        /// </summary>
        public void Form1ToTuitionSend()
        {
            //Tuitionsの初期化　新規作成時には配列が初期化されてないため
            for (int i = 0; i < Days; i++)
            {
                var day = GetDay(i);
                foreach (var student in Student) student.Value.AddDayInFlag(day);
                foreach (var teacher in Teacher) teacher.Value.AddDayInFlag(day);
                AllTuitionWeek(FastEnd[0].AddDays(i).DayOfWeek.ToString(), GetDay(i));
            }
            string studentName, teacherName;
            for (int i = 0; i < Days; i++)
            {
                var day = GetDay(i);
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < Booth; k++)
                    {
                        int flag = 0;
                        //生徒
                        for (int l = 0; l < 3; l++)
                        {
                            AllTuition[day].TimeClass[j].TimeFlag.Student.Add(AllTuition[day].TimeClass[j].NomalClass[k].Subject[l].Name);
                            studentName = AllTuition[day].TimeClass[j].NomalClass[k].Subject[l].Name;
                            if (studentName.CompareTo("") != 0)
                            {
                                Student[studentName].DayFlag[day][j] = "False";
                                Student[studentName].InFlag[day][j] = "○";
                                if (Organization.Any(a => a.Key.CompareTo(studentName) == 0))
                                    PersonalApplicationOrganizationTeaching(studentName, day, j, "○", "False");
                                flag = 1;
                            }
                        }
                        //講師
                        teacherName = AllTuition[day].TimeClass[j].NomalClass[k].Tname;
                        if (teacherName.CompareTo("") != 0)
                        {
                            Teacher[teacherName].DayFlag[day][j] = "False";
                            if (flag == 1) Teacher[teacherName].InFlag[day][j] = "○";
                        }
                        AllTuition[day].TimeClass[j].TimeFlag.Teacher.Add(AllTuition[day].TimeClass[j].NomalClass[k].Tname);
                        SetWeight(day, j, k);
                    }
                }
            }
        }

        //初回時通常授業を季節講習に反映させる
        private void AllTuitionWeek(string buf, string day)
        {
            int tmp = FDWC(buf);
            for (int j = 0; j < AllTuition[day].TimeClass.Count; j++)
            {
                for (int k = 0; k < AllTuition[day].TimeClass[j].NomalClass.Count; k++)
                {
                    AllTuition[day].TimeClass[j].NomalClass[k].Tname = NomalAllTuition[tmp].TimeClass[j].NomalClass[k].Tname.ToString();
                    AllTuition[day].TimeClass[j].NomalClass[k].TFlag = NomalAllTuition[tmp].TimeClass[j].NomalClass[k].TFlag;
                    AllTuition[day].TimeClass[j].NomalClass[k].Weighting = NomalAllTuition[tmp].TimeClass[j].NomalClass[k].Weighting;
                    for (int l = 0; l < AllTuition[day].TimeClass[j].NomalClass[k].Subject.Count; l++)
                    {
                        AllTuition[day].TimeClass[j].NomalClass[k].Subject[l].Name = NomalAllTuition[tmp].TimeClass[j].NomalClass[k].Subject[l].Name.ToString();
                        AllTuition[day].TimeClass[j].NomalClass[k].Subject[l].Flag = NomalAllTuition[tmp].TimeClass[j].NomalClass[k].Subject[l].Flag;
                        AllTuition[day].TimeClass[j].NomalClass[k].Subject[l].Subject = NomalAllTuition[tmp].TimeClass[j].NomalClass[k].Subject[l].Subject.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// 全期間の駒の成立比を取得
        /// </summary>
        /// <returns></returns>
        public string GetWeight()
        {
            int number10 = 0;
            int number20 = 0;
            int number30 = 0;
            int ALLnumber = 0;
            foreach (var v in AllTuition.Select(s => s.Value))
            {
                int[] te = v.getWeightI();
                number10 += te[0];
                number20 += te[1];
                number30 += te[2];
                ALLnumber += te[3];
            }
            return string.Format("季節講習期間中授業成立比　1:3成立比：{0:f1}%,1:2成立比：{1:f1}%,1:1成立比：{2:f1}%",
                    (float)number30 * 100 / ALLnumber, (float)number20 * 100 / ALLnumber, (float)number10 * 100 / ALLnumber);
        }

        /// <summary>
        /// コマの重みを計算する
        /// </summary>
        /// <param name="day">日にち</param>
        /// <param name="time">時間帯</param>
        /// <param name="nomal">ブース番号</param>
        public void SetWeight(string day, int time, int nomal)
        {
            try
            {
                var Nomal = AllTuition[day].TimeClass[time].NomalClass[nomal];
                List<string> SchoolYear = new List<string>();
                //そのコマにいる学年を取得する
                foreach (var s in Nomal.Subject)
                {
                    SchoolYear.Add(Student[s.Name].SchoolYear);
                }
                if (SchoolYear.Any(a => a.Contains("高")))
                {
                    Nomal.Weighting = 3 + Nomal.Subject.Count(c => c.Name.CompareTo("") != 0) * 10;
                    return;
                }
                if (SchoolYear.Any(a => a.Contains("中")))
                {
                    Nomal.Weighting = 2 + Nomal.Subject.Count(c => c.Name.CompareTo("") != 0) * 10;
                    return;
                }
                if (SchoolYear.Any(a => a.Contains("小")))
                    Nomal.Weighting = 1 + Nomal.Subject.Count(c => c.Name.CompareTo("") != 0) * 10;
            }
            catch (Exception) { }
        }
        #endregion

        #region 古い授業
        /// <summary>
        /// 古い通常授業の情報を格納する
        /// </summary>
        public void SetOldNomalAllTuition()
        {
            OldNomalAllTuition = new List<CTuition>();
            //季節講習管理から通常授業を変更する際に行う
            for (int i = 0; i < 7; i++)
            {
                OldNomalAllTuition.Add(new CTuition(new List<CTimeClass>(), 0));
                for (int j = 0; j < NomalAllTuition[i].TimeClass.Count; j++)
                {
                    OldNomalAllTuition[i].TimeClass.Add(new CTimeClass());
                    for (int k = 0; k < NomalAllTuition[i].TimeClass[j].NomalClass.Count; k++)
                    {
                        OldNomalAllTuition[i].TimeClass[j].NomalClass.Add(new CNomalClass());
                        for (int l = 0; l < 3; l++)
                        {
                            OldNomalAllTuition[i].TimeClass[j].NomalClass[k].Subject[l].Name =
                                NomalAllTuition[i].TimeClass[j].NomalClass[k].Subject[l].Name.ToString();
                            OldNomalAllTuition[i].TimeClass[j].NomalClass[k].Subject[l].Subject =
                                NomalAllTuition[i].TimeClass[j].NomalClass[k].Subject[l].Subject.ToString();
                        }
                        OldNomalAllTuition[i].TimeClass[j].NomalClass[k].Tname =
                            NomalAllTuition[i].TimeClass[j].NomalClass[k].Tname.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// 通常授業から削除した生徒を探す
        /// </summary>
        private void FindDeletedStudentFromNormalTeaching()
        {
            ///古い通常授業と新しい通常授業を比較する
            ///削除された生徒・講師がいないかを確認する
            ///3:削除された
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < OldNomalAllTuition[i].TimeClass.Count; j++)
                {
                    for (int k = 0; k < OldNomalAllTuition[i].TimeClass[j].NomalClass.Count; k++)
                    {
                        if (OldNomalAllTuition[i].TimeClass[j].NomalClass[k].Tname.Replace(" ", "").CompareTo("") != 0)
                        {
                            //講師の確認
                            if (!NomalAllTuition[i].TimeClass[j].NomalClass.Any(a => a.TFlag.CompareTo(OldNomalAllTuition[i].TimeClass[j].NomalClass[k].Tname) == 0))
                            {
                                OldNomalAllTuition[i].TimeClass[j].NomalClass[k].TFlag = 3;
                            }
                            //生徒の確認
                            for (int l = 0; l < 3; l++)
                            {
                                if (!NomalAllTuition[i].TimeClass[j].NomalClass
                                    .SelectMany(s => s.Subject.SelectMany(ss => ss.Name))
                                    .Any(a => a.CompareTo(OldNomalAllTuition[i].TimeClass[j].NomalClass[k].Subject[l].Name) == 0))
                                {
                                    OldNomalAllTuition[i].TimeClass[j].NomalClass[k].Subject[l].Flag = 3;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 生徒がどの状態にあるかを調べる
        /// </summary>
        /// <param name="Sname">生徒</param>
        /// <param name="Tname">講師</param>
        /// <param name="i">何曜日</param>
        /// <param name="j">時間帯</param>
        /// <returns></returns>
        private int OldNormalTeachingConfirmation(CSubject Sname, string Tname, int i, int j)
        {
            foreach (var sub in OldNomalAllTuition[i].TimeClass[j].NomalClass
                .Where(w => w.Tname.CompareTo(Tname) == 0)
                .SelectMany(s => s.Subject)
                .Where(w => w.Name.CompareTo(Sname.Name) == 0))
            {
                if (sub.Subject.CompareTo(Sname.Subject) == 0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            return 2;
        }

        /// <summary>
        /// 生徒及び講師の削除
        /// </summary>
        public void StudentAndTeacherDeletion(DateTime dateTime)
        {
            //削除が必要であれば削除を行う
            for (int day = 0; day < Days; day++)
            {
                if (dateTime <= FastEnd[0].AddDays(day))
                {
                    int Week = FDWC(FastEnd[0].AddDays(day).DayOfWeek.ToString());
                    foreach (var TimeClass in OldNomalAllTuition[Week].TimeClass)
                    {
                        foreach (var NomalClass in TimeClass.NomalClass.Where(w => w.Tname.Replace(" ", "").CompareTo("") != 0))
                        {
                            string Day = GetDay(day);
                            //講師削除
                            if (NomalClass.TFlag == 3)
                            {
                                foreach (var time in AllTuition[Day].TimeClass.Select((k, v) => new { k, v }))
                                {
                                    foreach (var nomal in time.k.NomalClass.Where(w => w.Tname.CompareTo(NomalClass.Tname) == 0))
                                    {
                                        nomal.Tname = "";
                                        time.k.TimeFlag.Teacher.Remove(NomalClass.Tname);
                                        Teacher[NomalClass.Tname].InFlag[Day][time.v] = "";
                                        Teacher[NomalClass.Tname].DayFlag[Day][time.v] = "True";
                                    }
                                }
                            }
                            //生徒削除
                            foreach (var Subject in NomalClass.Subject.Where(w => w.Name.Replace(" ", "").CompareTo("") != 0 && w.Flag == 3))
                            {
                                foreach (var time in AllTuition[Day].TimeClass.Select((k, v) => new { k, v }))
                                {
                                    foreach (var sub in time.k.NomalClass.SelectMany(s => s.Subject).Where(w => w.Name.CompareTo(Subject.Name) == 0))
                                    {
                                        sub.Name = "";
                                        time.k.TimeFlag.Student.Remove(Subject.Name);
                                        Student[Subject.Name].InFlag[Day][time.v] = "";
                                        Student[Subject.Name].DayFlag[Day][time.v] = "True";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 通常授業を変更した時の差分取得
        /// </summary>
        public void NormalTeachingApplication()
        {
            ///新しい通常授業と古い通常授業を比較する
            ///0:変更なし
            ///1:講師は変わらず教科が変わった生徒
            ///2:講師が変わった・新規
            for (int i = 0; i < 7; i++)
            {
                AddNomalAllTuition.Add(new CTuition());
                for (int j = 0; j < NomalAllTuition[i].TimeClass.Count; j++)
                {
                    AddNomalAllTuition[i].TimeClass.Add(new CTimeClass());
                    int flag = 0;
                    foreach (var nomal in NomalAllTuition[i].TimeClass[j].NomalClass.Where(w => w.Tname.Replace(" ", "").CompareTo("") != 0))
                    {
                        AddNomalAllTuition[i].TimeClass[j].NomalClass.Add(new CNomalClass(0, nomal.Tname, new List<CSubject>()));
                        foreach (var sub in nomal.Subject.Select((k, v) => new { k, v }))
                        {
                            AddNomalAllTuition[i].TimeClass[j].NomalClass[flag].Subject[sub.v].Name = sub.k.Name.ToString();
                            AddNomalAllTuition[i].TimeClass[j].NomalClass[flag].Subject[sub.v].Subject = sub.k.Subject.ToString();
                            AddNomalAllTuition[i].TimeClass[j].NomalClass[flag].Subject[sub.v].Flag = OldNormalTeachingConfirmation(sub.k, nomal.Tname, i, j);
                        }
                    }
                }
            }
            //通常授業から削除した生徒を探す
            FindDeletedStudentFromNormalTeaching();
        }
        #endregion

        #region 時間帯
        /// <summary>
        /// 期間の開始日・最終日の設定
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        public void SetFastEnd(DateTime item1, DateTime item2)
        {
            FastEnd[0] = item1;
            FastEnd[1] = item2;
            Days = (item2 - item1).Days + 1;
        }

        /// <summary>
        /// 月　日　(曜日)
        /// </summary>
        /// <param name="addDay">加算日数</param>
        /// <returns></returns>
        public string GetDay(int addDay)
        {
            DateTime buf = FastEnd[0].AddDays(addDay);
            return buf.Month + "月" + buf.Day + "日 (" + GetWeekday(buf.DayOfWeek.ToString()) + ")";
        }

        /// <summary>
        /// 月　日
        /// </summary>
        /// <param name="addDay">加算日数</param>
        /// <returns></returns>
        public string GetDate2(int addDay)
        {
            DateTime buf = FastEnd[0].AddDays(addDay);
            return buf.Month + "月" + buf.Day + "日 ";
        }

        /// <summary>
        /// 月/日 (曜日)
        /// </summary>
        /// <param name="addDay">加算日数</param>
        /// <returns></returns>
        public string GetDate3(int addDay)
        {
            DateTime buf = FastEnd[0].AddDays(addDay);
            return string.Format("{0,2}/{1,2} ({2})", buf.Month, buf.Day, GetWeekday(buf.DayOfWeek.ToString()));
        }

        /// <summary>
        /// 英語から日本語に変更する
        /// </summary>
        /// <param name="week">英語の曜日を引数</param>
        /// <returns>曜日</returns>
        public string GetWeekday(string week)
        {
            switch (week)
            {
                case "Monday":
                    return "月";
                case "Tuesday":
                    return "火";
                case "Wednesday":
                    return "水";
                case "Thursday":
                    return "木";
                case "Friday":
                    return "金";
                case "Saturday":
                    return "土";
                case "Sunday":
                    return "日";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 時間を元に文字列を変換する
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        private string GetTimePeriod(DateTime buf)
        {
            return buf.Hour + ":" + (buf.Minute != 0 ? (buf.Minute < 10 ? "0" + buf.Minute.ToString() : buf.Minute.ToString()) : "00");
        }
        #endregion

        #region スケジュール
        /// <summary>
        /// 初回にスケジュールを生成する
        /// </summary>
        public void SetSchedule()
        {
            for (var i = 0; i < Days; i++)
            {
                var s = GetDay(i);
                ScheduleFlag.Add(s, new string[] { "False", "False", "False", "False", "False", "False", "False", "False", "False" });
                if (FastEnd[0].AddDays(i).DayOfWeek.ToString().CompareTo("Sunday") != 0) continue;
                for (var k = 0; k < 9; k++) ScheduleFlag[s][k] = "";
            }
        }
        #endregion

        #region コマ
        /// <summary>
        /// コマの設定
        /// </summary>
        /// <param name="koma1">開始時間</param>
        /// <param name="koma2">授業時間</param>
        /// <param name="i">何コマ目</param>
        public void SetKoma(string koma1, string koma2, int i)
        {
            Koma[i, 0] = koma1;
            Koma[i, 1] = koma2;
        }

        /// <summary>
        /// コマが代入されているのかのチェック
        /// </summary>
        /// <param name="i">何コマ目</param>
        /// <returns></returns>
        public bool GetKomaCheck(int i)
        {
            return (Koma[i, 0] != null) && (Koma[i, 1] != null) && (Koma[i, 0].CompareTo("") != 0) && (Koma[i, 1].CompareTo("") != 0);
        }

        /// <summary>
        /// コマの情報を取得
        /// </summary>
        /// <param name="i">コマ目</param>
        /// <param name="j">開始時間：授業時間</param>
        /// <returns></returns>
        public string GetKoma(int i, int j)
        {
            if (Koma[i, j] != null)
                return Koma[i, j];
            return "";
        }

        /// <summary>
        /// 文字列から数字を返す
        /// </summary>
        /// <param name="Time">時間帯を確認する</param>
        /// <returns>時間間隔</returns>
        public int GetTime(int i)
        {
            int tmp = 0;
            switch (GetKoma(i, 1))
            {
                case "50分":
                    tmp = 50;
                    break;
                case "80分":
                    tmp = 80;
                    break;
                case "100分":
                    tmp = 100;
                    break;
            }
            return tmp;
        }

        /// <summary>
        /// 時間帯を取得
        /// </summary>
        /// <param name="i">何コマ目</param>
        /// <returns></returns>
        public string GetTimePeriod(int i)
        {
            if (GetKomaCheck(i))
            {
                return GetTimePeriod(DateTime.Parse(Koma[i, 0])) + "～" + GetTimePeriod(DateTime.Parse(Koma[i, 0]).AddMinutes(GetTime(i)));
            }
            return "";
        }
        #endregion

        #region　生徒
        /// <summary>
        /// 生徒を検索しコマを加算する
        /// </summary>
        /// <param name="sub">教科</param>
        /// <param name="StudentName">生徒氏名</param>
        /// <param name="Add">加算量</param>
        /// <param name="flag1">全体に加算するか</param>
        /// <param name="day">初日から何日目か</param>
        /// <param name="time">時間帯</param>
        public void StudentKomaCheck(string sub, string StudentName, int Add, bool flag1, string day, int time)
        {
            int week = FDWC(day.Substring(day.IndexOf('('), 1));
            for (int m = 0; m < NomalAllTuition[week].TimeClass[time].NomalClass.Count; m++)
            {
                for (int n = 0; n < 3; n++)
                {
                    if (NomalAllTuition[week].TimeClass[time].NomalClass[m].Subject[n].Name.CompareTo(StudentName) == 0)
                    {
                        return;
                    }
                }
            }
            if (flag1) Student[StudentName].Times.Old[Subject.All] += Add;
            try
            {
                Student[StudentName].Times.Old[GetSubject(sub)] += Add;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 教科を取得する
        /// </summary>
        /// <param name="Sub">教科</param>
        /// <returns></returns>
        private Subject GetSubject(string Sub)
        {
            switch (Sub)
            {
                case "国語":
                    return Subject.Japanes;
                case "数学":
                case "算数":
                    return Subject.Mathematics;
                case "英語":
                    return Subject.English;
                case "理科":
                    return Subject.Science;
                case "社会":
                    return Subject.Society;
            }
            return new Subject();
        }
        #endregion

        #region　講師

        #endregion

        #region　団体授業
        /// <summary>
        /// 団体授業が授業に登録された時、団体に登録されている生徒に出席情報を記載する
        /// </summary>
        /// <param name="groupname">団体の管理番号</param>
        /// <param name="day">日にち</param>
        /// <param name="timePeriod">時間帯</param>
        public void PersonalApplicationOrganizationTeaching(string groupname, string day, int timePeriod, string InFlag, string DayFlag)
        {
            foreach (var student in Organization[groupname].Student)
            {
                Student[student].DayFlag[day][timePeriod] = DayFlag;
                Student[student].InFlag[day][timePeriod] = InFlag;
            }
        }
        #endregion

        #region サブルーチン
        /// <summary>
        /// 英語の曜日から引数を返却する
        /// </summary>
        /// <param name="buf">英語の曜日を引数</param>
        /// <returns>曜日の位置</returns>
        public int FDWC(string buf)
        {
            switch (buf)
            {
                case "Monday":
                case "月":
                    return 0;
                case "Tuesday":
                case "火":
                    return 1;
                case "Wednesday":
                case "水":
                    return 2;
                case "Thursday":
                case "木":
                    return 3;
                case "Friday":
                case "金":
                    return 4;
                case "Saturday":
                case "土":
                    return 5;
                case "Sunday":
                case "日":
                    return 6;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// コマの重複確認を行う
        /// </summary>
        /// <param name="name">重複してるかの確認を行うのか名前</param>
        /// <param name="time">どの時間帯を基準にするのか</param>
        /// <param name="flag">講師と生徒のどちらで確認を行うのか</param>
        /// <param name="WeekFlag">曜日や日にちの引数</param>
        /// <returns>True OR False</returns>
        public bool TimeOverlap(string name, int time, int flag, int WeekFlag)
        {
            if (name.CompareTo("") == 0) return true;
            DateTime buf = DateTime.Parse(GetKoma(time, 0));
            DateTime buff = buf.AddMinutes(GetTime(time));
            name = name.Replace(" ", "");
            name = name.Replace("　", "");
            List<CTuition> buffere = NomalAllTuition;
            for (int i = 0; i < 9; i++)
            {
                if (GetKomaCheck(i))
                {
                    DateTime buf1 = DateTime.Parse(GetKoma(i, 0));
                    if ((buf1 <= buf && buf <= buf1.AddMinutes(GetTime(i))) || (buf1 <= buff && buff <= buf1.AddMinutes(GetTime(i))))
                    {
                        foreach (var nomal in buffere[WeekFlag].TimeClass[i].NomalClass)
                        {
                            if (flag == 0)
                            {
                                if (nomal.Tname.CompareTo(name) == 0)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                foreach (var sub in nomal.Subject.Where(w => w.Name.CompareTo("") != 0))
                                {
                                    if (sub.Name.CompareTo(name) == 0)
                                    {
                                        return false;
                                    }
                                    if (Organization.Select(s => s.Key).Any(a => a.CompareTo(sub.Name) == 0))
                                    {
                                        if (Organization[sub.Name].Student.Any(a => a.CompareTo(name) == 0))
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// コマの重複確認を行う
        /// </summary>
        /// <param name="name">重複してるかの確認を行うのか名前</param>
        /// <param name="time">どの時間帯を基準にするのか</param>
        /// <param name="flag">講師と生徒のどちらで確認を行うのか</param>
        /// <param name="WeekFlag">曜日や日にちの引数</param>
        /// <returns>True OR False</returns>
        public bool TimeOverlap(string name, int time, int flag, string WeekFlag)
        {
            if (name.CompareTo("") == 0) return true;
            DateTime buf = DateTime.Parse(GetKoma(time, 0));
            DateTime buff = buf.AddMinutes(GetTime(time));
            name = name.Replace(" ", "");
            name = name.Replace("　", "");
            var buffere = AllTuition;
            for (int i = 0; i < 9; i++)
            {
                if (GetKomaCheck(i))
                {
                    DateTime buf1 = DateTime.Parse(GetKoma(i, 0));
                    if ((buf1 <= buf && buf <= buf1.AddMinutes(GetTime(i))) || (buf1 <= buff && buff <= buf1.AddMinutes(GetTime(i))))
                    {
                        foreach (var nomal in buffere[WeekFlag].TimeClass[i].NomalClass)
                        {
                            if (flag == 0)
                            {
                                if (nomal.Tname.CompareTo(name) == 0)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                foreach (var sub in nomal.Subject.Where(w => w.Name.CompareTo("") != 0))
                                {
                                    if (sub.Name.CompareTo(name) == 0)
                                    {
                                        return false;
                                    }
                                    if (Organization.Select(s => s.Key).Any(a => a.CompareTo(sub.Name) == 0))
                                    {
                                        if (Organization[sub.Name].Student.Any(a => a.CompareTo(name) == 0))
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// もし通常授業に登録されている名前が生徒情報になかった場合
        /// その生徒を通常授業から削除を行う
        /// </summary>
        public void NomalCheck()
        {
            foreach (var m in NomalAllTuition.SelectMany(s => s.TimeClass.SelectMany(n => n.NomalClass)))
            {
                if (!Teacher.Select(s => s.Key).Any(a => a.CompareTo(m.Tname) == 0) && m.Tname.Replace(" ", "").CompareTo("") != 0)
                {
                    foreach (var item in NomalAllTuition.SelectMany(t => t.TimeClass.SelectMany(n => n.NomalClass.Where(w => w.Tname.CompareTo(m.Tname) == 0 && w.Tname.CompareTo("") == 0))))
                        item.Tname = "";
                }
                foreach (var j in m.Subject)
                {
                    if (!Student.Select(s => s.Key).Any(a => a.CompareTo(j.Name) == 0) && j.Name.Replace(" ", "").CompareTo("") != 0)
                    {
                        foreach (var item in NomalAllTuition.SelectMany(t => t.TimeClass.SelectMany(n => n.NomalClass.SelectMany(s => s.Subject.Where(w => w.Name.CompareTo(j.Name) == 0 && w.Name.CompareTo("") != 0)))))
                        {
                            item.Name = "";
                            item.Subject = "";
                        }
                    }
                }
            }
        }
        #endregion

        #region 保存
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="filename">ファイルネーム</param>
        public void Save(string filename)
        {
            if (!Directory.Exists(ITTOPROJECT)) Directory.CreateDirectory(ITTOPROJECT);
            if (!Directory.Exists(MYDOCUMENT)) Directory.CreateDirectory(MYDOCUMENT);
        }

        /// <summary>
        /// 生徒保存
        /// </summary>
        public void SaveStudent()
        {
            if (!Directory.Exists(ITTOPROJECT)) Directory.CreateDirectory(ITTOPROJECT);
            if (!Directory.Exists(MYDOCUMENT)) Directory.CreateDirectory(MYDOCUMENT);
            using (var sw = new StreamWriter(STUDENT))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                var s = Student.Aggregate("", (n, next) => $"{n}{next.Key},{next.Value.SchoolYear}{Environment.NewLine}");
                var ss = s.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
                serializer.Serialize(sw, ss);
            }
        }

        /// <summary>
        /// 講師保存
        /// </summary>
        public void SaveTeacher()
        {
            if (!Directory.Exists(ITTOPROJECT)) Directory.CreateDirectory(ITTOPROJECT);
            if (!Directory.Exists(MYDOCUMENT)) Directory.CreateDirectory(MYDOCUMENT);
            using (var sw = new StreamWriter(TEACHER))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                serializer.Serialize(sw, Teacher.Select(s => s.Key).ToList());
            }
        }

        /// <summary>
        /// 通常授業保存
        /// </summary>
        public void SaveNomal()
        {
            if (!Directory.Exists(ITTOPROJECT)) Directory.CreateDirectory(ITTOPROJECT);
            if (!Directory.Exists(MYDOCUMENT)) Directory.CreateDirectory(MYDOCUMENT);
        }

        /// <summary>
        /// 時間帯及びブースを保存
        /// </summary>
        public void SaveSetting()
        {
            using (var sw = new StreamWriter(SETTING))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                var ss = new List<string>();
                ss.Add(Booth.ToString());
                for (int i = 0; i < 9; i++)
                {
                    ss.Add($"{Koma[i, 0]},{Koma[i, 1]}");
                }
                serializer.Serialize(sw, ss);
            }
        }
        #endregion

        #region 開く
        /// <summary>
        /// 開く
        /// </summary>
        /// <param name="filename">ファイル名</param>
        public void Open(string filename)
        {
            if (!Directory.Exists(ITTOPROJECT)) Directory.CreateDirectory(ITTOPROJECT);
            if (!Directory.Exists(MYDOCUMENT)) Directory.CreateDirectory(MYDOCUMENT);
        }

        /// <summary>
        /// 生徒開く
        /// </summary>
        public void OpenStudent()
        {
            if (!Directory.Exists(ITTOPROJECT)) Directory.CreateDirectory(ITTOPROJECT);
            if (!Directory.Exists(MYDOCUMENT)) Directory.CreateDirectory(MYDOCUMENT);
            if (!File.Exists(STUDENT)) return;
            using (var sr = new StreamReader(STUDENT))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                foreach (var item in ((List<string>)serializer.Deserialize(sr)).Where(w => w.CompareTo("") != 0))
                {
                    var ss = item.Split(',');
                    if (!Student.Any(a => a.Key.CompareTo(ss[0]) == 0))
                        Student.Add(ss[0], new CStudent(Student.Count + 1, ss[1]));
                }
            }
        }

        /// <summary>
        /// 講師開く
        /// </summary>
        public void OpenTeacher()
        {
            if (!Directory.Exists(ITTOPROJECT)) Directory.CreateDirectory(ITTOPROJECT);
            if (!Directory.Exists(MYDOCUMENT)) Directory.CreateDirectory(MYDOCUMENT);
            if (!File.Exists(TEACHER)) return;
            using (var sr = new StreamReader(TEACHER))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                foreach (var item in ((List<string>)serializer.Deserialize(sr)).Where(w => w.CompareTo("") != 0))
                {
                    if (!Teacher.Any(a => a.Key.CompareTo(item) == 0))
                        Teacher.Add(item, new CTeacher(Teacher.Count + 1));
                }
            }
        }

        /// <summary>
        /// 通常授業開く
        /// </summary>
        public void OpenNomal()
        {
            if (!Directory.Exists(ITTOPROJECT)) Directory.CreateDirectory(ITTOPROJECT);
            if (!Directory.Exists(MYDOCUMENT)) Directory.CreateDirectory(MYDOCUMENT);
        }

        /// <summary>
        /// 時間帯及びブースを取得
        /// </summary>
        public void OpenSetting()
        {
            if (!Directory.Exists(ITTOPROJECT)) Directory.CreateDirectory(ITTOPROJECT);
            if (!Directory.Exists(MYDOCUMENT)) Directory.CreateDirectory(MYDOCUMENT);
            if (!File.Exists(SETTING)) return;
            using (var sr = new StreamReader(SETTING))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<string>));
                var item = (List<string>)serializer.Deserialize(sr);
                Booth = Convert.ToInt32(item[0]);
                for (int i = 1; i < item.Count; i++)
                {
                    var ss = item[i].Split(',');
                    Koma[i - 1, 0] = ss[0];
                    Koma[i - 1, 1] = ss[1];
                }
            }
        }
        #endregion
    }
}
