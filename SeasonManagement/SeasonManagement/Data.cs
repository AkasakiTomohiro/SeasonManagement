using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SeasonManagement.Class;

namespace SeasonManagement
{
    class Data
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
    }
}
