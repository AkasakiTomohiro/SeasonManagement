using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SeasonManagement.Enum;

namespace SeasonManagement
{
    public class Class
    {
        /// <summary>
        /// 人
        /// </summary>
        public abstract class Human
        {
            #region 変数
            /// <summary>
            /// 出席可能
            /// </summary>
            public Dictionary<string, string[]> DayFlag { get; private set; }
            /// <summary>
            /// 出席確定
            /// </summary>
            public Dictionary<string, string[]> InFlag { get; private set; }
            /// <summary>
            /// 受講回数及び受講予定回数
            /// </summary>
            public Times Times { get; private set; }
            /// <summary>
            /// 管理番号
            /// </summary>
            public int ManagementNumber { get; private set; }
            #endregion

            /// <summary>
            /// 人
            /// </summary>
            /// <param name="name">氏名</param>
            /// <param name="maagemntNumber">管理番号</param>
            public Human(int maagemntNumber)
            {
                ManagementNumber = maagemntNumber;
                DayFlag = new Dictionary<string, string[]>();
                InFlag = new Dictionary<string, string[]>();
                Times = new Times();
            }

            /// <summary>
            /// 二進数文字列
            /// </summary>
            /// <returns></returns>
            public string GetBinaryNumber()
            {
                return GetStringToBinary(ManagementNumber, "");
            }

            /// <summary>
            /// 10進数から二進数文字列に変換用の関数
            /// </summary>
            /// <param name="number">2進数に変換したい数字</param>
            /// <param name="str">初期は""を代入</param>
            /// <returns></returns>
            private string GetStringToBinary(int number, string str)
            {
                if (number == 0) return str;
                str = (number % 2) + str;
                return GetStringToBinary(number / 2, str);
            }

            /// <summary>
            /// DayFlag及びInFlagの追加
            /// </summary>
            public void AddDayInFlag(string day)
            {
                DayFlag.Add(day, new string[] { "True", "True", "True", "True", "True", "True", "True", "True", "True" });
                InFlag.Add(day, new string[9]);
            }
        }

        /// <summary>
        /// 生徒用
        /// </summary>
        public class CStudent : Human
        {
            #region 変数
            /// <summary>
            /// 学年
            /// </summary>
            public string SchoolYear { get; private set; }
            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="maagemntNumber">管理番号</param>
            /// <param name="schoolYear">学年</param>
            public CStudent(int maagemntNumber, string schoolYear) : base(maagemntNumber)
            {
                SchoolYear = schoolYear;
            }

            /// <summary>
            /// 保存
            /// </summary>
            /// <returns></returns>
            public string Save()
            {
                var d = DayFlag.Aggregate("", (n, next) => $"{n},{next.Key},{next.Value.Aggregate("", (nn, nnext) => $"{nn},{nnext}")}");
                return $"{Times.Save()},{SchoolYear},{ManagementNumber},{Environment.NewLine}{d}";
            }
        }

        /// <summary>
        /// 講師用
        /// </summary>
        public class CTeacher : Human
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="maagemntNumber">管理番号</param>
            public CTeacher(int maagemntNumber) : base(maagemntNumber) { }

            /// <summary>
            /// 保存
            /// </summary>
            /// <returns></returns>
            public string Save()
            {
                var d = DayFlag.Aggregate("", (n, next) => $"{n},{next.Key},{next.Value.Aggregate("", (nn, nnext) => $"{nn},{nnext}")}");
                return $"{Times.Save()},{ManagementNumber},{Environment.NewLine}{d}";
            }
        }

        /// <summary>
        /// 団体授業用
        /// </summary>
        public class COrganization : Human
        {
            #region 変数
            /// <summary>
            /// 学年
            /// </summary>
            public string SchoolYear { get; private set; }
            /// <summary>
            /// 団体授業に所属する生徒
            /// </summary>
            public List<string> Student { get; set; }
            #endregion

            /// <summary>
            /// 団体授業
            /// </summary>
            /// <param name="maagemntNumber">管理番号</param>
            /// <param name="schoolYear">学年</param>
            public COrganization(int maagemntNumber, string schoolYear) : base(maagemntNumber)
            {
                Student = new List<string>();
                SchoolYear = schoolYear;
            }

            /// <summary>
            /// 保存
            /// </summary>
            /// <returns></returns>
            public string Save()
            {
                var d = DayFlag.Aggregate("", (n, next) => $"{n},{next.Key},{next.Value.Aggregate("", (nn, nnext) => $"{nn},{nnext}")}");
                return $"{Times.Save()},{SchoolYear},{ManagementNumber},{Student.Aggregate("", (n, next) => $"{n},{next}")},{Environment.NewLine}{d}";
            }

            /// <summary>
            /// 生徒の追加
            /// </summary>
            /// <param name="name">氏名</param>
            public void Add(string name)
            {
                Student.Add(name);
            }

            /// <summary>
            /// 生徒の削除
            /// </summary>
            /// <param name="name">氏名</param>
            public void Remove(string name)
            {
                Student.Remove(name);
            }

            /// <summary>
            /// 生徒の一斉削除
            /// </summary>
            public void Clear()
            {
                Student.Clear();
            }
        }

        /// <summary>
        /// 受講回数及び受講予定回数
        /// </summary>
        public class Times
        {
            public Dictionary<Subject, int> New { get; set; }
            public Dictionary<Subject, int> Old { get; set; }

            public Times()
            {
                New = new Dictionary<Subject, int>();
                Old = new Dictionary<Subject, int>();
                New.Add(Subject.Japanes, 0);
                New.Add(Subject.Mathematics, 0);
                New.Add(Subject.English, 0);
                New.Add(Subject.Science, 0);
                New.Add(Subject.Society, 0);
                New.Add(Subject.All, 0);

                Old.Add(Subject.Japanes, 0);
                Old.Add(Subject.Mathematics, 0);
                Old.Add(Subject.English, 0);
                Old.Add(Subject.Science, 0);
                Old.Add(Subject.Society, 0);
                Old.Add(Subject.All, 0);
            }
            public Times(string[] ss)
            {
                New = new Dictionary<Subject, int>();
                Old = new Dictionary<Subject, int>();
                New.Add(Subject.Japanes, Convert.ToInt32(ss[1]));
                New.Add(Subject.Mathematics, Convert.ToInt32(ss[2]));
                New.Add(Subject.English, Convert.ToInt32(ss[3]));
                New.Add(Subject.Science, Convert.ToInt32(ss[4]));
                New.Add(Subject.Society, Convert.ToInt32(ss[5]));
                New.Add(Subject.All, Convert.ToInt32(ss[6]));

                Old.Add(Subject.Japanes, Convert.ToInt32(ss[7]));
                Old.Add(Subject.Mathematics, Convert.ToInt32(ss[8]));
                Old.Add(Subject.English, Convert.ToInt32(ss[9]));
                Old.Add(Subject.Science, Convert.ToInt32(ss[10]));
                Old.Add(Subject.Society, Convert.ToInt32(ss[11]));
                Old.Add(Subject.All, Convert.ToInt32(ss[12]));
            }

            /// <summary>
            /// 残りの受講数
            /// </summary>
            /// <param name="sub">教科</param>
            /// <returns>残りのコマ</returns>
            public int getDifference(Subject sub)
            {
                return New[sub] - Old[sub];
            }

            /// <summary>
            /// 保存用
            /// </summary>
            /// <returns></returns>
            public string Save()
            {
                return $"{New.Aggregate("", (n, next) => $"{n},{next}")},{Old.Aggregate("", (n, next) => $"{n},{next}")}";
            }
        }

        /// <summary>
        /// 通常授業の登録できなかった箇所
        /// </summary>
        public class CAddClass
        {
            #region 変数
            /// <summary>
            /// 講師の名前
            /// </summary>
            private string TName;
            /// <summary>
            /// 生徒の名前
            /// </summary>
            private string SName;
            /// <summary>
            /// 教科
            /// </summary>
            private string Subject;
            /// <summary>
            /// 登録できなかった日にちと時間帯を格納
            /// </summary>
            private List<string> List;
            #endregion

            /// <summary>
            /// 通常授業を変更した際に登録できなかったコマを格納するクラス
            /// </summary>
            /// <param name="tnam">講師の名前</param>
            /// <param name="sname">生徒の名前</param>
            /// <param name="subject">教科</param>
            /// <param name="list">登録できなかった日にちと時間帯を格納</param>
            public CAddClass(string tnam, string sname, string subject)
            {
                TName = tnam;
                SName = sname;
                Subject = subject;
                List = new List<string>();
            }

            /// <summary>
            /// 時間帯を追加する
            /// </summary>
            /// <param name="s">反映できなかった時間帯</param>
            public void AddList(string s)
            {
                List.Add(s);
            }

            /// <summary>
            /// 情報の取得   
            /// </summary>
            /// <returns></returns>
            public string GetInfo()
            {
                string s = "講師: " + TName + ", 生徒: " + SName + ", 教科: " + Subject + Environment.NewLine;
                for (int i = 0; i < List.Count; i += 2)
                {
                    s += List[i];
                    if (i != List.Count - 2) s += ",";
                }
                s += Environment.NewLine;
                return s;
            }

            /// <summary>
            /// 同一かのチェック
            /// </summary>
            /// <param name="te">講師</param>
            /// <param name="st">生徒</param>
            /// <param name="sub">教科</param>
            /// <returns></returns>
            public bool GetAddClassCheck(string te, string st, string sub)
            {
                return TName.CompareTo(te) == 0 && SName.CompareTo(st) == 0 && Subject.CompareTo(sub) == 0;
            }
        }

        /// <summary>
        /// コマに入る生徒情報
        /// </summary>
        public class CSubject
        {
            public string Name { get; set; }
            public string Subject { get; set; }
            public int Flag { get; set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CSubject()
            {
                Name = "";
                Subject = "";
                Flag = 0;
            }
        }

        /// <summary>
        /// 1コマ分の情報
        /// </summary>
        public class CNomalClass
        {
            public int Weighting { get; set; }
            public string Tname { get; set; }
            public List<CSubject> Subject { get; }
            public int TFlag { get; set; }

            public CNomalClass()
            {
                Weighting = 0;
                Tname = "";
                Subject = new List<CSubject>();
                Subject.Add(new CSubject());
                Subject.Add(new CSubject());
                Subject.Add(new CSubject());
                TFlag = 0;
            }
            public CNomalClass(int weighting, string tname, List<CSubject> sub)
            {
                Weighting = weighting;
                Tname = tname;
                Subject = sub;
                TFlag = 0;
            }
        }

        /// <summary>
        /// 一つの時間帯の授業
        /// </summary>
        public class CTimeClass
        {
            /// <summary>
            /// 一つの時間帯
            /// </summary>
            public List<CNomalClass> NomalClass { get; }
            /// <summary>
            /// あるコマに入れる生徒及び講師
            /// </summary>
            public CTimeFlag TimeFlag { get; }

            public CTimeClass()
            {
                NomalClass = new List<CNomalClass>();
                TimeFlag = new CTimeFlag();
            }
        }

        /// <summary>
        /// 1日分の授業
        /// </summary>
        public class CTuition
        {
            /// <summary>
            /// 一日のコマ
            /// </summary>
            public List<CTimeClass> TimeClass { get; }
            public int Flag { get; set; }

            public CTuition()
            {
                TimeClass = new List<CTimeClass>();
                Flag = 0;
            }
            public CTuition(List<CTimeClass> timeClass, int flag)
            {
                TimeClass = timeClass;
                Flag = flag;
            }

            /// <summary>
            /// 授業の重みを計算する
            /// </summary>
            /// <returns></returns>
            public string getWeightS()
            {
                int number10 = 0;
                int number20 = 0;
                int number30 = 0;
                int ALLnumber = 0;
                foreach (var time in TimeClass)
                {
                    ALLnumber += time.NomalClass.Where(d => (d.Weighting > 10)).Count();
                    number20 += time.NomalClass.Where(d => (d.Weighting > 20)).Count();
                    number30 += time.NomalClass.Where(d => (d.Weighting > 30)).Count();
                }
                number10 = ALLnumber - number20;
                number20 -= number30;
                if (ALLnumber != 0)
                    return string.Format("1:3成立比：{0:f1}%,1:2成立比：{1:f1}%,1:1成立比：{2:f1}%",
                        (float)number30 * 100 / ALLnumber, (float)number20 * 100 / ALLnumber, (float)number10 * 100 / ALLnumber);
                else
                    return "成立している授業がありません";
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public int[] getWeightI()
            {
                int[] we = new int[] { 0, 0, 0, 0 };
                foreach (var time in TimeClass)
                {
                    we[3] += time.NomalClass.Where(d => (d.Weighting > 10)).Count();
                    we[1] += time.NomalClass.Where(d => (d.Weighting > 20)).Count();
                    we[2] += time.NomalClass.Where(d => (d.Weighting > 30)).Count();
                }
                we[0] = we[3] - we[1];
                we[1] -= we[2];
                return we;
            }
        }

        /// <summary>
        /// あるコマに入れる生徒及び講師
        /// </summary>
        public class CTimeFlag
        {
            public List<string> Student;
            public List<string> Teacher;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public CTimeFlag()
            {
                Student = new List<string>();
                Teacher = new List<string>();
            }

            /// <summary>
            /// 生徒の追加
            /// </summary>
            /// <param name="name">氏名</param>
            public void AddStudent(string name)
            {
                if (!Student.Contains(name) && name.CompareTo("") != 0)
                    Student.Add(name);
            }

            /// <summary>
            /// 講師の追加
            /// </summary>
            /// <param name="name">氏名</param>
            public void AddTeacher(string name)
            {
                if (!Teacher.Contains(name) && name.CompareTo("") != 0)
                    Teacher.Add(name);
            }
        }

        /// <summary>
        /// 初期設定時における時間帯を一括管理するためのクラス
        /// </summary>
        public class CNewKomaSet
        {
            TextBox Text;
            Label Label;
            ComboBox Combo;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="text">授業開始時間を入力するテキストボックス</param>
            /// <param name="combo">TextとComboの情報が入力された時の結果を表示するラベル</param>
            /// <param name="label">授業時間間隔を決めるコンボボックス</param>
            public CNewKomaSet(TextBox text, ComboBox combo, Label label)
            {
                Text = text;
                Label = label;
                Combo = combo;
            }

            /// <summary>
            /// テキストボックスにテキストを格納する
            /// </summary>
            /// <param name="text"></param>
            public void setText(string text)
            {
                Text.Text = text;
            }

            /// <summary>
            /// コンボボックスにテキストを格納する
            /// </summary>
            /// <param name="text"></param>
            public void setCombo(string text)
            {
                Combo.Text = text;
            }

            /// <summary>
            /// ラベルにテキストを格納する
            /// </summary>
            /// <param name="text"></param>
            public void setLabel(string text)
            {
                Label.Text = text;
            }

            /// <summary>
            /// テキストボックスのテキストを取得する
            /// </summary>
            /// <returns></returns>
            public string getText()
            {
                return Text.Text;
            }

            /// <summary>
            /// コンボボックスのテキストを取得する
            /// </summary>
            /// <returns></returns>
            public string getCombo()
            {
                return Combo.Text;
            }

            /// <summary>
            /// ラベルのテキスト取得する
            /// </summary>
            /// <returns></returns>
            public string getLabel()
            {
                return Label.Text;
            }

            /// <summary>
            /// コンボボックスの状態取得
            /// </summary>
            /// <returns></returns>
            public bool getComboEnabled()
            {
                return Combo.Enabled;
            }

            /// <summary>
            /// コマの入力されたTEXTがDATETIMEにキャストできるかのチェック
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            private bool timecheck()
            {
                DateTime dt = new DateTime(0);
                if (DateTime.TryParse(Text.Text, out dt))
                {
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 入力が正しいかを確認する
            /// </summary>
            /// <returns></returns>
            public bool check()
            {
                if (timecheck())
                {
                    Combo.Enabled = true;
                    Text.ForeColor = Color.Black;
                    if (Combo.Text.CompareTo("50分") == 0 || Combo.Text.CompareTo("80分") == 0 || Combo.Text.CompareTo("100分") == 0)
                    {
                        return true;
                    }
                    else
                    {
                        Combo.Text = "";
                        Label.Text = "";
                    }
                }
                else
                {
                    Text.ForeColor = Color.Red;
                    Combo.Enabled = false;
                    Combo.Text = "";
                    Label.Text = "";
                }
                return false;
            }

            /// <summary>
            /// Visible To True
            /// </summary>
            public void AllTrue()
            {
                Text.Visible = true;
                Combo.Visible = true;
            }

            /// <summary>
            /// Visible To False
            /// </summary>
            public void ALLFalse()
            {
                Text.Visible = false;
                Combo.Visible = false;
            }
        }

        /// <summary>
        /// 生徒・講師タブ用のオブジェクト
        /// </summary>
        public class CSTC
        {
            TextBox Text;
            ComboBox Combo;
            public CSTC(TextBox text, ComboBox combo)
            {
                this.Combo = combo;
                this.Text = text;
            }

            /// <summary>
            /// 
            /// </summary>
            public void resetText()
            {
                Text.ResetText();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="s"></param>
            public void setText(string s)
            {
                Text.Text = s;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="b"></param>
            public void setCombo(bool b)
            {
                Combo.Enabled = b;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="s"></param>
            public void setCombo(string s)
            {
                Combo.Text = s;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="i"></param>
            public void setCombo(int i)
            {
                Combo.SelectedIndex = i;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public string getText()
            {
                return Text.Text;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public string getCombo()
            {
                return Combo.Text;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool getCheck()
            {
                return Text.Text != null && Combo.Text.CompareTo("") != 0;
            }
        }
    }
}
