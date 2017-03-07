using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SeasonManagement.Class;
using static SeasonManagement.Enum;

namespace SeasonManagement
{
    public partial class Form1 : Form
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
        #endregion

        #region 変数
        /// <summary>
        /// 情報タブのオブジェクト
        /// </summary>
        private List<CNewKomaSet> NKS = new List<CNewKomaSet>();

        /// <summary>
        /// 
        /// </summary>
        private Data DB;

        /// <summary>
        /// 初期化時の制御フラグ
        /// </summary>
        bool FastFlag = true;

        /// <summary>
        /// TMSからデータを参照した際のフラグ
        /// </summary>
        int TMS2Flag = new int();

        /// <summary>
        /// 講師・生徒
        /// </summary>
        STMode STMode = new STMode();

        /// <summary>
        /// TMS2で取得した名前情報を一時的に格納する
        /// </summary>
        List<string[]> TMS2 = new List<string[]>();

        /// <summary>
        /// 生徒・講師タブのオブジェクト
        /// </summary>
        List<CSTC> STC = new List<CSTC>();

        /// <summary>
        /// 季節講習のクリックの座標
        /// </summary>
        Point Click1 = new Point();

        /// <summary>
        /// 通常授業のクリックの座標
        /// </summary>
        Point Click2 = new Point();

        /// <summary>
        /// 初回時の通常授業用の制御フラグ
        /// </summary>
        bool NomalCheck = true;

        /// <summary>
        /// 季節講習用のコンボボックス
        /// </summary>
        DataGridViewComboBoxEditingControl dataGridViewComboBox = null;

        /// <summary>
        /// 通常授業用のコンボボックス
        /// </summary>
        DataGridViewComboBoxEditingControl dataGridViewComboBox1 = null;

        /// <summary>
        /// 生徒と講師のリストボックスを保持する
        /// </summary>
        ListBox[] ST = new ListBox[2];

        /// <summary>
        /// 操作中の日付
        /// </summary>
        string NowDay;

        /// <summary>
        /// dataGridView3_CellValueChangedの制御用
        /// </summary>
        bool CellValueChangedFlag = false;

        /// <summary>
        /// 
        /// </summary>
        int SelectFlag = 0;

        /// <summary>
        /// 生徒もしくは講師を選択した人
        /// </summary>
        string SelectName;

        /// <summary>
        /// 作業中のセルの値を保持
        /// </summary>
        string ClickCellStudent = "", ClickCellSubject = "";

        /// <summary>
        /// 印刷フラグ
        /// </summary>
        int STFlagPrint = -1;

        #endregion

        public Form1()
        {
            InitializeComponent();
            ITTOPROJECT = $@"{ Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\ITTOProject";
            MYDOCUMENT = $@"{ITTOPROJECT}\SeasonManagement";
            STUDENT = $@"{MYDOCUMENT}\Student.ini";
            TEACHER = $@"{MYDOCUMENT}Teacher.ini\";
            NOMAL = $@"{MYDOCUMENT}\NomalTuition.ini";
            //dataGridViewのちらつき抑制用
            // DataGirdViewのTypeを取得
            Type dgvtype = typeof(DataGridView);
            // プロパティ設定の取得
            System.Reflection.PropertyInfo dgvPropertyInfo =
            dgvtype.GetProperty(
                "DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
                );
            // 対象のDataGridViewにtrueをセットする
            dgvPropertyInfo.SetValue(dataGridView1, true, null);
            dgvPropertyInfo.SetValue(dataGridView2, true, null);
            dgvPropertyInfo.SetValue(dataGridView3, true, null);
            dgvPropertyInfo.SetValue(dataGridView4, true, null);
            dgvPropertyInfo.SetValue(dataGridView5, true, null);
            dgvPropertyInfo.SetValue(dataGridView6, true, null);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            NKS.Add(new CNewKomaSet(textBox13, comboBox5, label22));
            NKS.Add(new CNewKomaSet(textBox14, comboBox6, label23));
            NKS.Add(new CNewKomaSet(textBox15, comboBox7, label24));
            NKS.Add(new CNewKomaSet(textBox16, comboBox8, label25));
            NKS.Add(new CNewKomaSet(textBox17, comboBox9, label26));
            NKS.Add(new CNewKomaSet(textBox18, comboBox10, label27));
            NKS.Add(new CNewKomaSet(textBox19, comboBox11, label28));
            NKS.Add(new CNewKomaSet(textBox20, comboBox22, label29));
            NKS.Add(new CNewKomaSet(textBox21, comboBox23, label30));
            STC.Add(new CSTC(textBox2, comboBox12));
            STC.Add(new CSTC(textBox3, comboBox13));
            STC.Add(new CSTC(textBox4, comboBox14));
            STC.Add(new CSTC(textBox5, comboBox15));
            STC.Add(new CSTC(textBox6, comboBox16));
            STC.Add(new CSTC(textBox7, comboBox17));
            STC.Add(new CSTC(textBox8, comboBox18));
            STC.Add(new CSTC(textBox11, comboBox19));
            STC.Add(new CSTC(textBox9, comboBox20));
            STC.Add(new CSTC(textBox10, comboBox21));
        }


        #region 情報
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker3.MaxDate = dateTimePicker2.Value.AddDays(50);
            dateTimePicker3.MinDate = dateTimePicker2.Value.AddDays(1);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            NewKisetu(1);
        }
        #endregion

        #region メニュー
        private void NewFile_Click(object sender, EventArgs e)
        {
            DB = new Data();
            button10.Visible = true;
            button9.Visible = true;
            button7.Visible = true;
            comboBox24.Enabled = true;
            dateTimePicker2.Enabled = true;
            dateTimePicker3.Enabled = true;
            NewKisetu(0);
        }

        /// <summary>
        /// 作業手順
        /// </summary>
        /// <param name="i"></param>
        private void NewKisetu(int i)
        {
            switch (i)
            {
                case 0:
                    NewKisetu_0();
                    break;
                case 1:
                    NewKisetu_1();
                    break;
                case 2:
                    NewKisetu_2();
                    break;
                case 3:
                    NewKisetu_3();
                    break;
                case 4:
                    NewKisetu_4();
                    break;
            }
        }

        /// <summary>
        /// 変数の初期化及びスタート
        /// </summary>
        public void NewKisetu_0()
        {
            dateTimePicker2.Value = DateTime.Now;
            toolStripStatusLabel2.Visible = true;
            tabControl4.Visible = true;
            menuToolStripMenuItem.Enabled = false;
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            dataGridView4.Rows.Clear();
            dataGridView5.Rows.Clear();
            dataGridView6.Rows.Clear();
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            button7.Visible = true;
            FastFlag = false;
            string s = null;
            //設定ファイルを開く
            DB.OpenSetting();
            for (int i = 0; i < 9; i++)
            {
                NKS[i].setText(DB.Koma[i, 0]);
                NKS[i].setCombo(DB.Koma[i, 1]);
            }
            comboBox24.Text = DB.Booth.ToString();
            foreach (var i in NKS)
                i.AllTrue();
            toolStripStatusLabel2.Text = "ステップ数　1/5";
        }

        /// <summary>
        /// 初期入力フォーム
        /// </summary>
        public void NewKisetu_1()
        {
            string s = "";
            s += "作成期間" + Environment.NewLine;
            s += "\t開始日" + dateTimePicker2.Value.Year + "年" + dateTimePicker2.Value.Month + "月" + dateTimePicker2.Value.Day + "日　～　";
            s += "終了日" + dateTimePicker3.Value.Year + "年" + dateTimePicker3.Value.Month + "月" + dateTimePicker3.Value.Day + "日" + Environment.NewLine;
            s += "ブースの数\n\t" + comboBox24.Text + Environment.NewLine + "個";
            s += "授業時間割" + Environment.NewLine;
            for (int i = 0; i < 9; i++)
            {
                if (NKS[i].getLabel().CompareTo("") != 0)
                    s += "\t" + (i + 1) + "コマ目：" + NKS[i].getLabel() + Environment.NewLine;
            }
            s += "上記の内容で登録しますか？";
            DialogResult result = MessageBox.Show(s, "質問", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;

            //開始日及び最終日を保存
            DB.SetFastEnd(dateTimePicker2.Value, dateTimePicker3.Value);

            //スケジュールを生成する
            DB.SetSchedule();

            //時間帯とブースを保存する
            DB.SaveSetting();

            //生徒情報があれば参照する
            DB.OpenStudent();
            foreach (var student in DB.Student)
            {
                ListViewItem itemx = new ListViewItem();
                itemx.Text = student.Key;
                itemx.SubItems.Add(student.Value.SchoolYear);
                listView1.Items.Add(itemx);
            }

            button7.Visible = false;
            FastFlag = true;
            tabControl4.SelectedIndex = 3;
            FastFlag = false;
            comboBox1.SelectedIndex = 0;
            comboBox1.Enabled = false;
            toolStripStatusLabel2.Text = "ステップ数　2/5";
        }

        /// <summary>
        /// 生徒・講師情報入力
        /// </summary>
        public void NewKisetu_2()
        {
            var result = MessageBox.Show("登録しますか？", "質問", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;
            switch (STMode)
            {
                case STMode.Student:
                    // 生徒を配列に格納
                    DB.Student.Clear();
                    foreach (ListViewItem Item in listView1.Items)
                    {
                        DB.Student.Add(Item.Text, new CStudent(DB.Student.Count + 1, Item.SubItems[1].Text));
                    }
                    DB.SaveStudent();

                    //講師ファイルがあれば読み込む
                    DB.OpenTeacher();
                    foreach (var teacher in DB.Teacher)
                    {
                        ListViewItem itemx = new ListViewItem();
                        itemx.Text = teacher.Key;
                        listView1.Items.Add(itemx);
                    }

                    comboBox1.SelectedIndex = 1;
                    toolStripStatusLabel2.Text = "ステップ数　3/5";
                    break;
                case STMode.Teacher:
                    DB.Teacher.Clear();
                    foreach (ListViewItem Item in listView1.Items)
                    {
                        DB.Teacher.Add(Item.Text, new CTeacher(DB.Teacher.Count + 1));
                    }
                    DB.SaveTeacher();
                    FastFlag = true;
                    tabControl5.SelectedIndex = 1;
                    FastFlag = false;
                    //comboBox1.SelectedIndex = 0;
                    comboBox1.Enabled = true;
                    button8.Visible = false;
                    listView2.Clear();
                    toolStripStatusLabel1.Text = "";
                    toolStripStatusLabel2.Text = "ステップ数　4/5";
                    break;
            }
        }

        /// <summary>
        /// 団体授業情報入力
        /// </summary>
        public void NewKisetu_3()
        {
            DialogResult result = MessageBox.Show("登録しますか？", "質問", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;
            toolStripStatusLabel1.Text = "通常授業の新規生成";
            //新規で通常授業を生成する
            DB.SetNomalNew();
            //通常授業を開く
            DB.OpenNomal();
            NomalCheck = true;
            DB.NomalCheck();
            comboBox2.SelectedIndex = 0;
            NomalCheck = false;
            nomalStart();
            FastFlag = true;
            tabControl4.SelectedIndex = 4;
            FastFlag = false;
            button9.Visible = false;
            dateTimePicker1.Visible = false;
            label3.Visible = false;
            toolStripStatusLabel2.Text = "ステップ数　5/5";
        }

        /// <summary>
        /// 通常授業情報入力
        /// </summary>
        public void NewKisetu_4()
        {
            DialogResult result = MessageBox.Show("登録しますか？", "質問", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;
            DB.Form1ToTuitionSend();
            New();
            dateTimePicker1.Visible = true;
            label3.Visible = true;
            toolStripStatusLabel2.Text = "";
            foreach (var s in NKS)
                s.ALLFalse();
            tabControl4.SelectedIndex = 1;
            label32.Visible = false;
            FastFlag = true;
            menuToolStripMenuItem.Enabled = true;
        }
        #endregion

        #region　季節講習
        /// <summary>
        /// 日程表の更新
        /// </summary>
        public void DbUpdate()
        {
            var tmp = DB.FDWC(DB.FastEnd[0].DayOfWeek.ToString());
            var count = (int)Math.Ceiling((double)(DB.Days + tmp) / 14);
            int tmp2 = 0, buf = 0;
            for (var jj = 0; jj < count; jj++)
            {
                for (var ii = 0; ii < 14; ii++)
                {
                    for (var k = 0; k < 9; k++)
                    {
                        if (tmp + ii >= 14 || DB.Days <= buf - tmp2) continue;
                        if (ii - tmp2 + jj * 14 > DB.Days) break;
                        dataGridView5[ii + 1 + tmp, jj * 10 + k + 1].Value = DB.ScheduleFlag[DB.GetDay(ii - tmp2 + jj * 14)][k];
                    }
                    buf++;
                }
                if (jj != 0) continue;
                tmp2 = tmp;
                tmp = 0;
            }
        }

        /// <summary>
        /// 日付一覧の文字色の変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //背景を描画する
            e.DrawBackground();
            if (e.Index == -1) return;
            var txt = ((ListBox)sender).Items[e.Index].ToString();
            if (e.Index <= -1) return;
            //文字を描画する色の選択
            Brush b = null;

            if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
            {
                // fColor,bColorで色を指定 
                if (txt.Contains("(土)"))
                {
                    b = new SolidBrush(Color.FromArgb(135, 206, 250));
                }
                else if (txt.Contains("(日)"))
                {
                    b = new SolidBrush(Color.FromArgb(255, 153, 204));
                }
                else
                {
                    b = new SolidBrush(Color.Black);
                }
            }
            else
            {
                b = new SolidBrush(e.ForeColor);
            }
            //文字列の描画
            e.Graphics.DrawString(txt, e.Font, b, e.Bounds);
            b.Dispose();
        }

        /// <summary>
        /// 日付を選択した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0) return;
            if (listBox1.SelectedItem != null)
            {
                NowDay = DB.GetDay(listBox1.SelectedIndex);
            }
            toolStripStatusLabel3.Visible = true;
            toolStripStatusLabel4.Visible = true;
            //ここで、生徒・教科・講師を登録を行う
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < DB.Booth; j++)
                {
                    SetDGV2Cell(i, j);
                }
            }
            //ここから講師を自動反映を行う。
            int flag1 = 0;
            int sum1 = 0, sum2 = 0;
            if (DB.AllTuition[NowDay].Flag == 0)
            {
                DB.AllTuition[NowDay].Flag = 1;
                for (int i = 0; i < 9; i++)
                {
                    sum1 = DB.AllTuition[NowDay].TimeClass[i].TimeFlag.Teacher.Count - 1;
                    for (int m = 0; m < DB.Booth; m++)
                    {
                        if (sum2 >= sum1) break;
                        for (int j = 0; j < DB.Booth; j++)
                        {
                            if (dataGridView2[i * 2, (j + 1) * 4 - 1].Value.ToString().CompareTo(DB.AllTuition[NowDay].TimeClass[i].TimeFlag.Teacher[sum2]) == 0)
                            {
                                flag1 = 1;
                            }
                        }
                        if (dataGridView2[i * 2, (m + 1) * 4 - 1].Value.ToString().CompareTo("") == 0)
                        {
                            if (flag1 == 0 && DB.TimeOverlap(DB.AllTuition[NowDay].TimeClass[i].TimeFlag.Teacher[sum2], i, 0, listBox1.SelectedIndex))
                            {
                                dataGridView2[i * 2, (m + 1) * 4 - 1].Value = DB.AllTuition[NowDay].TimeClass[i].TimeFlag.Teacher[sum2];
                                DB.AllTuition[NowDay].TimeClass[i].NomalClass[m].Tname = DB.AllTuition[NowDay].TimeClass[i].TimeFlag.Teacher[sum2];
                            }
                            sum2++;
                        }
                        flag1 = 0;
                    }
                    sum2 = 0;
                }
            }
            if (SelectFlag == 0) tabControl1.SelectedTab = tabPage1;
            toolStripStatusLabel2.Text = listBox1.SelectedItem.ToString();
            toolStripStatusLabel3.Text = DB.AllTuition[NowDay].getWeightS();
            toolStripStatusLabel4.Text = DB.GetWeight();
        }

        /// <summary>
        /// 一コマ分の更新
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="buf"></param>
        public void SetDGV2Cell(int i, int j)
        {
            if (!DB.GetKomaCheck(i) || DB.Booth < j) return;
            int StudentNumber;
            DataGridViewComboBoxCell StudentCell;
            DataGridViewComboBoxCell SubjectCell;
            for (int k = 0; k < 3; k++)
            {
                string Name = DB.AllTuition[NowDay].TimeClass[i].NomalClass[j].Subject[k].Name;
                StudentCell = new DataGridViewComboBoxCell();
                SubjectCell = new DataGridViewComboBoxCell();
                StudentCell.Items.AddRange(DB.AllTuition[NowDay].TimeClass[i].TimeFlag.Student.Where(w => !StudentCell.Items.Contains(w)).Select(c => c).ToArray());
                if (StudentCell.Items.Count != 0) StudentCell.ToolTipText = ToolTipTex_TuitionFlag_Student(i);
                if (Name.Replace(" ", "").CompareTo("") != 0)
                {
                    int flag = 0;
                    int week = DB.FDWC(DB.FastEnd[0].AddDays(listBox1.SelectedIndex).DayOfWeek.ToString());
                    foreach (var Subject in DB.NomalAllTuition[week].TimeClass[i].NomalClass.SelectMany(s => s.Subject).Where(c => c.Name.CompareTo(Name) == 0))
                    {
                        SubjectCell.Items.Add(Subject.Subject);
                        SubjectCell.ToolTipText = "通常授業";
                        flag = 1;
                        break;
                    }
                    if (flag == 0)
                    {
                        if (DB.Student[Name].Times.New[Subject.Japanes] > 0)
                            SubjectCell.Items.Add("国語");
                        if (DB.Student[Name].Times.New[Subject.English] > 0)
                            SubjectCell.Items.Add("英語");
                        if (DB.Student[Name].Times.New[Subject.Science] > 0)
                            SubjectCell.Items.Add("理科");
                        if (DB.Student[Name].Times.New[Subject.Society] > 0)
                            SubjectCell.Items.Add("社会");
                        if (DB.Student[Name].Times.New[Subject.Mathematics] > 0)
                        {
                            if (DB.Student[Name].SchoolYear.Contains("小"))
                            {
                                SubjectCell.Items.Add("算数");
                            }
                            else
                            {
                                SubjectCell.Items.Add("数学");
                            }
                        }
                        SubjectCell.ToolTipText = "季節講習" + Environment.NewLine;
                        SubjectCell.ToolTipText += "残りのコマ数" + Environment.NewLine;
                        SubjectCell.ToolTipText += "国語：" + DB.Student[Name].Times.getDifference(Subject.Japanes) + Environment.NewLine;
                        SubjectCell.ToolTipText += "数学：" + DB.Student[Name].Times.getDifference(Subject.Mathematics) + Environment.NewLine;
                        SubjectCell.ToolTipText += "英語：" + DB.Student[Name].Times.getDifference(Subject.English) + Environment.NewLine;
                        SubjectCell.ToolTipText += "理科：" + DB.Student[Name].Times.getDifference(Subject.Science) + Environment.NewLine;
                        SubjectCell.ToolTipText += "社会：" + DB.Student[Name].Times.getDifference(Subject.Society) + Environment.NewLine;
                    }
                }
                StudentCell.Items.Remove("");
                StudentCell.Items.Remove(" ");
                StudentCell.Items.Remove("  ");
                StudentCell.Items.Remove("    ");
                StudentCell.Items.Insert(0, "");
                SubjectCell.Items.Remove("");
                SubjectCell.Items.Insert(0, "");
                dataGridView2[i * 2, j * 4 + k] = StudentCell;
                dataGridView2[i * 2, j * 4 + k].Value = Name;
                dataGridView2[i * 2 + 1, j * 4 + k] = SubjectCell;
                dataGridView2[i * 2 + 1, j * 4 + k].Value = DB.AllTuition[NowDay].TimeClass[i].NomalClass[j].Subject[k].Subject;

            }
            DataGridViewComboBoxCell TeacherCell = new DataGridViewComboBoxCell();
            TeacherCell.Items.AddRange(DB.AllTuition[NowDay].TimeClass[i].TimeFlag.Teacher.Where(c => !TeacherCell.Items.Contains(c)).Select(w => w).ToArray());
            TeacherCell.Items.Remove("");
            TeacherCell.Items.Insert(0, "");
            TeacherCell.ToolTipText = ToolTipTex_TuitionFlag_Teacher(i);
            dataGridView2[i * 2, (j + 1) * 4 - 1] = TeacherCell;
            dataGridView2[i * 2, (j + 1) * 4 - 1].Value = DB.AllTuition[NowDay].TimeClass[i].NomalClass[j].Tname;
        }

        /// <summary>
        /// 生徒を選択した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectName = listBox2.SelectedItem.ToString();
            if (DB.Student.Any(a => a.Key.CompareTo(SelectName) == 0)) return;
            var NewTimes = DB.Student[SelectName].Times.New;
            numericUpDown1.Value = NewTimes[Subject.Japanes];
            numericUpDown2.Value = NewTimes[Subject.Mathematics];
            numericUpDown3.Value = NewTimes[Subject.English];
            numericUpDown4.Value = NewTimes[Subject.Science];
            numericUpDown5.Value = NewTimes[Subject.Society];
            numericUpDown1.Enabled = true;
            numericUpDown2.Enabled = true;
            numericUpDown3.Enabled = true;
            numericUpDown4.Enabled = true;
            numericUpDown5.Enabled = true;
            if (SelectFlag == 0) tabControl1.SelectedTab = tabPage3;
            groupBox9.Visible = true;
            toolStripStatusLabel2.Text = listBox2.SelectedItem.ToString();
            toolStripStatusLabel3.Visible = false;
            toolStripStatusLabel4.Visible = false;
            SC(0);
        }

        /// <summary>
        /// 講師を選択した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex == -1) return;
            SelectName = listBox3.SelectedItem.ToString();
            toolStripStatusLabel2.Text = listBox3.SelectedItem.ToString();
            if (SelectFlag == 0) tabControl1.SelectedTab = tabPage3;
            groupBox9.Visible = false;
            toolStripStatusLabel3.Visible = false;
            toolStripStatusLabel4.Visible = false;
            numericUpDown1.Enabled = false;
            numericUpDown2.Enabled = false;
            numericUpDown3.Enabled = false;
            numericUpDown4.Enabled = false;
            numericUpDown5.Enabled = false;
            SC(1);
        }

        #region 授業日程

        /// <summary>
        /// 閉じるボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            groupBox2.Visible = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private string ToolTipTex_TuitionFlag_Student(int i)
        {
            string text = "";
            List<string> name = new List<string>(DB.AllTuition[NowDay].TimeClass[i].TimeFlag.Student);
            name.Remove("");
            int count = 0;
            for (int ii = 0; ii < name.Count; ii++)
            {
                count = 0;
                foreach (var v in DB.AllTuition[NowDay].TimeClass[i].NomalClass)
                {
                    count += v.Subject.Where(c => c.Name.CompareTo(name[ii]) == 0).Count();
                }
                if (count == 0)
                    text += name[ii] + Environment.NewLine;
            }
            return text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private string ToolTipTex_TuitionFlag_Teacher(int i)
        {
            string text = "";
            List<string> name = new List<string>(DB.AllTuition[NowDay].TimeClass[i].TimeFlag.Teacher);
            name.Remove("");
            int count = 0;
            for (int ii = 0; ii < name.Count; ii++)
            {
                count = 0;
                for (int j = 0; j < DB.AllTuition[NowDay].TimeClass[i].NomalClass.Count; j++)
                {
                    if (DB.AllTuition[NowDay].TimeClass[i].NomalClass[j].Tname.CompareTo(name[ii]) == 0)
                        count++;
                }
                if (count == 0)
                    text += name[ii] + Environment.NewLine;
            }
            return text;
        }

        #region DataGridView2
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            //表示されているコントロールがDataGridViewComboBoxEditingControlか調べる
            if (e.Control is DataGridViewComboBoxEditingControl)
            {
                DataGridView dgv = (DataGridView)sender;
                //編集のために表示されているコントロールを取得
                this.dataGridViewComboBox = (DataGridViewComboBoxEditingControl)e.Control;
                //SelectedIndexChangedイベントハンドラを追加
                this.dataGridViewComboBox.SelectedIndexChanged += new EventHandler(dataGridViewComboBox2_SelectedIndexChanged);
            }
        }

        /// <summary>
        /// DataGridViewに表示されているコンボボックスのSelectedIndexChanged2イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataGridViewComboBoxEditingControl cb = (DataGridViewComboBoxEditingControl)sender;
            if (Click1.Y == -1) return;
            if (Click1.Y % 4 == 3)
            {
                //講師の変更
                dataGridViewComboBox2_SelectedIndexChanged_Teacher(cb);
            }
            else if (Click1.X % 2 == 0)
            {
                //生徒の変更
                dataGridViewComboBox2_SelectedIndexChanged_Sutudent(cb);
            }
            else
            {
                //教科の変更
                DB.StudentKomaCheck(cb.SelectedItem.ToString(), dataGridView2[Click1.X - 1, Click1.Y].Value.ToString(), 1, false, NowDay, Click1.X / 2);
                DB.StudentKomaCheck(ClickCellSubject, ClickCellStudent, -1, false, NowDay, Click1.X / 2);
                DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Subject[Click1.Y % 4].Subject = cb.SelectedItem.ToString();
            }
            SetDGV2Cell(Click1.X / 2, Click1.Y / 4);
            cb.EditingControlDataGridView.NotifyCurrentCellDirty(true);
            cb.EditingControlDataGridView.EndEdit();
        }
        private void dataGridViewComboBox2_SelectedIndexChanged_Teacher(DataGridViewComboBoxEditingControl cb)
        {
            //講師の出席状況などの変更処理
            if (DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Tname.Replace(" ", "").CompareTo("") != 0)
            {
                //登録を消すときの処理
                DB.Teacher[DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Tname].InFlag[NowDay][Click1.X / 2] = "";
                DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Tname = "";
            }
            if (cb.SelectedItem.ToString().CompareTo("") != 0)
                if (DB.TimeOverlap(cb.SelectedItem.ToString(), Click1.X / 2, 0, NowDay))
                {
                    bool checkflag = true;
                    for (int i = 0; i < 3; i++)
                    {
                        if (DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Subject[i].Name.CompareTo("") != 0)
                        {
                            checkflag = false;
                        }
                    }
                    if (checkflag)
                    {
                        DB.Teacher[cb.SelectedItem.ToString()].InFlag[NowDay][Click1.X / 2] = "";
                    }
                    else
                    {
                        DB.Teacher[cb.SelectedItem.ToString()].InFlag[NowDay][Click1.X / 2] = "○";
                    }
                    DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Tname = cb.SelectedItem.ToString();
                }
                else
                {
                    MessageBox.Show("重複しています。", "重複", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cb.SelectedItem = "";
                }
        }
        private void dataGridViewComboBox2_SelectedIndexChanged_Sutudent(DataGridViewComboBoxEditingControl cb)
        {
            if (cb.SelectedItem == null) return;
            //生徒の出席や
            if (DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Subject[Click1.Y % 4].Name.Replace(" ", "").CompareTo("") != 0)
            {
                if (DB.Organization.Any(a => a.Key == DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Subject[Click1.Y % 4].Name))
                {
                    DB.PersonalApplicationOrganizationTeaching(DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Subject[Click1.Y % 4].Name, NowDay, Click1.X / 2, "", "False");
                }
                DB.Student[DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Subject[Click1.Y % 4].Name].InFlag[NowDay][Click1.X / 2] = "";
                DB.StudentKomaCheck(ClickCellSubject, ClickCellStudent, -1, true, NowDay, Click1.X / 2);
            }
            if (DB.TimeOverlap(cb.SelectedItem.ToString(), Click1.X / 2, 1, NowDay))
            {
                if (cb.SelectedItem.ToString().CompareTo("") != 0)
                {
                    if (DB.Organization.Any(a => a.Key == cb.SelectedItem.ToString()))
                    {
                        DB.PersonalApplicationOrganizationTeaching(cb.SelectedItem.ToString(), NowDay, Click1.X / 2, "○", "True");
                    }
                    DB.Student[cb.SelectedItem.ToString()].InFlag[NowDay][Click1.X / 2] = "○";
                }
                //コマの管理
                DB.StudentKomaCheck("", cb.SelectedItem.ToString(), 1, true, NowDay, Click1.X / 2);
                DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Subject[Click1.Y % 4].Name = cb.SelectedItem.ToString();
                DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Subject[Click1.Y % 4].Subject = "";
                if (DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Tname.CompareTo("") != 0)
                {
                    if (DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Subject.All(a => a.Name.CompareTo("") == 0))
                    {
                        DB.Teacher[DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Tname].InFlag[NowDay][Click1.X / 2] = "";
                    }
                    else
                    {
                        DB.Teacher[DB.AllTuition[NowDay].TimeClass[Click1.X / 2].NomalClass[Click1.Y / 4].Tname].InFlag[NowDay][Click1.X / 2] = "○";
                    }
                }
                DB.SetWeight(comboBox2.SelectedItem.ToString(), Click2.X / 2, Click2.Y / 4);
            }
            else
            {
                MessageBox.Show("重複しています。", "重複", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cb.SelectedItem = "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dataGridViewComboBox != null)
            {
                this.dataGridViewComboBox.SelectedIndexChanged -= new EventHandler(dataGridViewComboBox2_SelectedIndexChanged);
                this.dataGridViewComboBox = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != -1 && e.RowIndex != -1)
            {
                if (dataGridView2[e.ColumnIndex, e.RowIndex].GetType().Equals(typeof(DataGridViewComboBoxCell)))
                {
                    // 編集モードにする 
                    dataGridView2.BeginEdit(true);
                    // 編集モードにしたので現在の編集コントロールを取得 
                    DataGridViewComboBoxEditingControl edt = dataGridView2.EditingControl as DataGridViewComboBoxEditingControl;
                    // ドロップダウンさせる 
                    edt.DroppedDown = true;
                }
                Click1.X = e.ColumnIndex;
                Click1.Y = e.RowIndex;
                if (Click1.Y % 4 != 3)
                {
                    if (Click1.X % 2 == 0)
                    {
                        dataGridView2_CellClick_StudentCeack(0);
                        dataGridView2_CellClick_SubjectCeack(1);
                    }
                    else
                    {
                        dataGridView2_CellClick_StudentCeack(-1);
                        dataGridView2_CellClick_SubjectCeack(0);
                    }
                }
                toolStripStatusLabel1.Text = (Click1.X / 2 + 1).ToString() + "コマ目:" + ((Click1.Y - 1) / 4 + 1).ToString() + "番ブース";
            }
        }
        private void dataGridView2_CellClick_StudentCeack(int flag)
        {
            if (dataGridView2[Click1.X + flag, Click1.Y].Value != null)
            {
                ClickCellStudent = dataGridView2[Click1.X + flag, Click1.Y].Value.ToString();
            }
            else
            {
                ClickCellStudent = "";
            }
        }
        private void dataGridView2_CellClick_SubjectCeack(int flag)
        {
            if (dataGridView2[Click1.X + flag, Click1.Y].Value != null)
            {
                ClickCellSubject = dataGridView2[Click1.X + flag, Click1.Y].Value.ToString();
            }
            else
            {
                ClickCellSubject = "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView2_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = false;
        }

        #endregion

        #endregion

        #region 個人ページ

        /// <summary>
        /// 生徒・国語のコマ数変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Enabled && SelectName.CompareTo("") != 0)
            {
                DB.Student[SelectName].Times.New[Subject.Japanes] = Convert.ToInt32(numericUpDown1.Value.ToString());
                NewAll();
            }
        }

        /// <summary>
        /// 生徒・数学のコマ変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Enabled && SelectName.CompareTo("") != 0)
            {
                DB.Student[SelectName].Times.New[Subject.Mathematics] = Convert.ToInt32(numericUpDown2.Value.ToString());
                NewAll();
            }
        }

        /// <summary>
        /// 生徒・英語のコマ変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Enabled && SelectName.CompareTo("") != 0)
            {
                DB.Student[SelectName].Times.New[Subject.English] = Convert.ToInt32(numericUpDown3.Value.ToString());
                NewAll();
            }
        }

        /// <summary>
        /// 生徒・理科のコマ変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Enabled && SelectName.CompareTo("") != 0)
            {
                DB.Student[SelectName].Times.New[Subject.Science] = Convert.ToInt32(numericUpDown4.Value.ToString());
                NewAll();
            }
        }

        /// <summary>
        /// 生徒・社会のコマ変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Enabled && SelectName.CompareTo("") != 0)
            {
                DB.Student[SelectName].Times.New[Subject.Society] = Convert.ToInt32(numericUpDown5.Value.ToString());
                NewAll();
            }
        }

        /// <summary>
        /// New.Allコマの更新
        /// </summary>
        public void NewAll()
        {
            DB.Student[SelectName].Times.New[Subject.All] =
                Convert.ToInt32(numericUpDown1.Value.ToString()) +
                Convert.ToInt32(numericUpDown2.Value.ToString()) +
                Convert.ToInt32(numericUpDown3.Value.ToString()) +
                Convert.ToInt32(numericUpDown4.Value.ToString()) +
                Convert.ToInt32(numericUpDown5.Value.ToString());
            foreach (var student in DB.Student.Select((k, v) => new { k, v }))
            {
                dataGridView4[0, student.v].Value = student.k.Key;
                dataGridView4[1, student.v].Value = student.k.Value.Times.getDifference(Subject.Japanes);
                dataGridView4[2, student.v].Value = student.k.Value.Times.getDifference(Subject.Mathematics);
                dataGridView4[3, student.v].Value = student.k.Value.Times.getDifference(Subject.English);
                dataGridView4[4, student.v].Value = student.k.Value.Times.getDifference(Subject.Science);
                dataGridView4[5, student.v].Value = student.k.Value.Times.getDifference(Subject.Society);
                dataGridView4[6, student.v].Value = student.k.Value.Times.getDifference(Subject.All);
            }
        }

        /// <summary>
        /// dataGridView3_Cellの値が変更された時に変数に格納するためのもの
        /// </summary>
        /// <param name="TorF"></param>
        /// <param name="C"></param>
        /// <param name="R"></param>
        private void STFDChenge(bool TorF, int C, int R)
        {
            int buf1 = R % 10 - 1;
            int buf2 = R / 10 * 10;
            string day = dataGridView3[C, buf2].Value.ToString();
            string name = ST[STMode == STMode.Student ? 0 : 1].SelectedItem.ToString();
            if (DB.ScheduleFlag[day][buf1].CompareTo("True") == 0)
            {
                dataGridView3[C, R].Value = "True";
                return;
            }
            if (STMode == STMode.Student)
            {
                DB.Student[name].DayFlag[day][buf1] = TorF ? "True" : "False";
            }
            else
            {
                DB.Teacher[name].DayFlag[day][buf1] = TorF ? "True" : "False";
            }
            //Flagに反映させる
            switch (STMode)
            {
                case STMode.Student:
                    STFDChengeStudent(TorF, name, day, buf1, C, R);
                    break;
                case STMode.Teacher:
                    STFDChengeTeacher(TorF, name, day, buf1, C, R);
                    break;
            }
            toolStripStatusLabel3.Text = DB.AllTuition[NowDay].getWeightS();
            toolStripStatusLabel4.Text = DB.GetWeight();
        }
        public void STFDChengeStudent(bool TorF, string name, string day, int buf1, int C, int R)
        {
            if (TorF)
            {
                //ここで実際に日程表にデータが格納されてないかを確認する
                bool check = true;
                DialogResult result;
                foreach (var subject in DB.AllTuition[day].TimeClass[buf1].NomalClass.SelectMany(s => s.Subject).Where(w => w.Name.CompareTo(name) == 0))
                {
                    result = MessageBox.Show("日程表に登録されています。それでも予定表を変更しますか？", "質問", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        DB.StudentKomaCheck(subject.Subject, SelectName, -1, true, day, buf1);
                        subject.Name = "";
                        subject.Subject = "";
                        DB.Student[name].InFlag[day][buf1] = "";
                    }
                    else
                    {
                        dataGridView3[C, R].Value = "False";
                        check = false;

                    }
                }
            }
            else
            {
                DB.AllTuition[day].TimeClass[buf1].TimeFlag.AddStudent(SelectName);
            }
        }
        public void STFDChengeTeacher(bool TorF, string name, string day, int buf1, int C, int R)
        {
            if (TorF)
            {
                var check = true;
                DialogResult result;
                foreach (var nomalclass in DB.AllTuition[day].TimeClass[buf1].NomalClass.Where(w => w.Tname.CompareTo(name) == 0))
                {
                    if (nomalclass.Subject.Any(a => a.Name.CompareTo("") != 0))
                    {
                        result = MessageBox.Show("日程表に登録されています。それでも予定表を変更しますか？", "質問", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            DB.Teacher[name].InFlag[day][buf1] = "";
                            nomalclass.Tname = "";
                        }
                        else
                        {
                            dataGridView3[C, R].Value = "False";
                            check = false;
                        }
                    }
                    else
                    {
                        nomalclass.Tname = "";
                    }
                }
                if (check)
                {
                    DB.AllTuition[day].TimeClass[buf1].TimeFlag.Teacher.Remove(SelectName);
                }
            }
            else
            {
                DB.AllTuition[day].TimeClass[buf1].TimeFlag.AddTeacher(SelectName);
            }
        }

        #region DataGridView3

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView3_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            splitContainer3.SplitterDistance = splitContainer3.Size.Width - 100;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView3_Paint(object sender, PaintEventArgs e)
        {
            splitContainer3.SplitterDistance = splitContainer3.Size.Width - 100;
        }

        /// <summary>
        /// dataGridView3_Cellの値が変更された時に起こるイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (CellValueChangedFlag) return;
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (dataGridView3[e.ColumnIndex, e.RowIndex].GetType().Equals(typeof(DataGridViewCheckBoxCell)))
                {
                    if (dataGridView3[e.ColumnIndex, e.RowIndex].Value.ToString() == "True")
                    {
                        STFDChenge(true, e.ColumnIndex, e.RowIndex);
                    }
                    else
                    {
                        STFDChenge(false, e.ColumnIndex, e.RowIndex);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView3_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView3.IsCurrentCellDirty)
            {
                //コミットする
                dataGridView3.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (0 < e.ColumnIndex && e.ColumnIndex < 15 && 0 < e.RowIndex)
            {
                if (e.RowIndex % 10 != 0 && dataGridView3[e.ColumnIndex, e.RowIndex / 10 * 10].Value != null)
                {
                    string week, buf, buf1;
                    week = "(" + dataGridView3.Columns[e.ColumnIndex].HeaderText.Substring(0, 1) + ")";
                    buf1 = dataGridView3[e.ColumnIndex, e.RowIndex / 10 * 10].Value.ToString();
                    buf = dataGridView3[0, e.RowIndex].Value.ToString();
                    toolStripStatusLabel1.Text = buf1 + week + ":" + buf;
                    return;
                }
            }
            toolStripStatusLabel1.Text = "";
        }

        /// <summary>
        /// 予定表の出席のタブで複数選択したセルの情報を一括で反転させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData.Equals(Keys.Enter))
            {
                for (int i = 0; i < dataGridView3.GetCellCount(DataGridViewElementStates.Selected); i++)
                {
                    if (dataGridView3[dataGridView3.SelectedCells[i].ColumnIndex, dataGridView3.SelectedCells[i].RowIndex].Value != null)
                    {
                        switch (dataGridView3[dataGridView3.SelectedCells[i].ColumnIndex, dataGridView3.SelectedCells[i].RowIndex].Value.ToString())
                        {
                            case "True":
                                dataGridView3[dataGridView3.SelectedCells[i].ColumnIndex, dataGridView3.SelectedCells[i].RowIndex].Value = "False";
                                break;
                            case "False":
                                dataGridView3[dataGridView3.SelectedCells[i].ColumnIndex, dataGridView3.SelectedCells[i].RowIndex].Value = "True";
                                break;
                        }
                        dataGridView3_CellValueChanged(new object(), new DataGridViewCellEventArgs(dataGridView3.SelectedCells[i].ColumnIndex, dataGridView3.SelectedCells[i].RowIndex));
                    }
                }
                switch (STMode)
                {
                    case STMode.Student:
                        listBox2_SelectedIndexChanged(sender, e);
                        break;
                    case STMode.Teacher:
                        listBox3_SelectedIndexChanged(sender, e);
                        break;
                }
            }
        }
        #endregion

        #endregion

        #region 残りコマ

        /// <summary>
        /// 生徒氏名検索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_Enter(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        /// <summary>
        /// 残りコマでの生徒氏名検索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView4.Rows.Count; i++)
            {
                if (dataGridView4[0, i].Value.ToString().CompareTo(textBox1.Text) == 0)
                {
                    dataGridView4.ClearSelection();
                    dataGridView4[0, i].Selected = true;
                    break;
                }
            }
        }

        #region DataGridView4

        /// <summary>
        /// dataGridView4のセルの値が変更されたときにおこるイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView4_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

            DataGridView dgv = (DataGridView)sender;
            //セルの列を確認
            if (e.Value is int)
            {
                int val = (int)e.Value;
                //セルの値により、背景色を変更する
                if (val < 0)
                {
                    e.CellStyle.BackColor = Color.OrangeRed;
                }
                else if (val == 0)
                {
                    e.CellStyle.BackColor = Color.LightGreen;
                }
                else if (val > 0)
                {
                    e.CellStyle.BackColor = Color.LightYellow;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView4_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = false;
        }

        #endregion

        #endregion

        #endregion

        #region 日程表

        //日程表の更新
        public void DBUpdate()
        {
            int tmp = DB.FDWC(DB.FastEnd[0].DayOfWeek.ToString());
            int count = (int)Math.Ceiling((double)(DB.Days + tmp) / 14);
            int tmp2 = 0, buf = 0;
            for (int jj = 0; jj < count; jj++)
            {
                for (int ii = 0; ii < 14; ii++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        if (tmp + ii < 14 && DB.Days > buf - tmp2)
                        {
                            if (ii - tmp2 + jj * 14 > DB.Days) break;
                            dataGridView5[ii + 1 + tmp, jj * 10 + k + 1].Value = DB.ScheduleFlag[listBox1.Items[ii - tmp2 + jj * 14].ToString()][k];
                        }
                    }
                    buf++;
                }
                if (jj == 0)
                {
                    tmp2 = tmp;
                    tmp = 0;
                }
            }
        }

        #region DataGridView5
        //
        private void dataGridView5_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (dataGridView5[e.ColumnIndex, e.RowIndex].GetType().Equals(typeof(DataGridViewCheckBoxCell)))
                {
                    DB.ScheduleFlag[dataGridView5[e.ColumnIndex, e.RowIndex / 10 * 10].Value.ToString()][e.RowIndex % 10 - 1] = dataGridView5[e.ColumnIndex, e.RowIndex].Value.ToString();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData.Equals(Keys.Enter))
            {
                for (int i = 0; i < dataGridView5.GetCellCount(DataGridViewElementStates.Selected); i++)
                {
                    if (dataGridView5[dataGridView5.SelectedCells[i].ColumnIndex, dataGridView5.SelectedCells[i].RowIndex].Value != null)
                    {
                        if (dataGridView5[dataGridView5.SelectedCells[i].ColumnIndex, dataGridView5.SelectedCells[i].RowIndex].Value.ToString() == "True")
                        {
                            dataGridView5[dataGridView5.SelectedCells[i].ColumnIndex, dataGridView5.SelectedCells[i].RowIndex].Value = "False";
                        }
                        else if (dataGridView5[dataGridView5.SelectedCells[i].ColumnIndex, dataGridView5.SelectedCells[i].RowIndex].Value.ToString() == "False")
                        {
                            dataGridView5[dataGridView5.SelectedCells[i].ColumnIndex, dataGridView5.SelectedCells[i].RowIndex].Value = "True";
                        }
                        dataGridView3_CellValueChanged(new object(), new DataGridViewCellEventArgs(dataGridView5.SelectedCells[i].ColumnIndex, dataGridView5.SelectedCells[i].RowIndex));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView5_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView5_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView5.IsCurrentCellDirty)
            {
                dataGridView5.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
        #endregion

        #endregion

        #region NEW
        /// <summary>
        /// 
        /// </summary>
        public void New()
        {
            if (!Directory.Exists(ITTOPROJECT)) Directory.CreateDirectory(ITTOPROJECT);
            if (!Directory.Exists(MYDOCUMENT)) Directory.CreateDirectory(MYDOCUMENT);
            splitContainer1.Visible = false;
            DTSAdd();
            //STFDの初期化
            ST[0] = listBox2;
            ST[1] = listBox3;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            this.WindowState = FormWindowState.Maximized;
            splitContainer3.SplitterDistance = splitContainer3.Size.Width - 100;
            dataGridView3.RowHeadersVisible = false;
            STDRAdd();
            SC(0);
            Schedule();
            StudentTeacher();
            nomalStart();
            listBox2.SelectedIndex = 0;
            listBox1.SelectedIndex = 0;
            toolStripComboBox3.SelectedIndex = 0;
            splitContainer1.Visible = true;
            NomalCheck = false;
            comboBox2_SelectedIndexChanged(new object(), new EventArgs());
        }

        /// <summary>
        /// 日付・講師・生徒を季節講習日程管理用のリストに追加
        /// </summary>
        private void DTSAdd()
        {
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            if (dataGridView2.Rows.Count != DB.Booth * 4 + 1)
            {
                dataGridView2.RowHeadersVisible = false;
                dataGridView2.AllowUserToAddRows = false;
                dataGridView2.AllowUserToResizeColumns = false;
                dataGridView2.AllowUserToResizeRows = false;
                dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dataGridView2.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
                dataGridView2.Rows.Add(DB.Booth * 4);
                dataGridView4.RowHeadersVisible = false;
                dataGridView4.AllowUserToAddRows = false;
                dataGridView4.AllowUserToResizeColumns = false;
                dataGridView4.AllowUserToResizeRows = false;
                dataGridView4.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dataGridView4.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
                dataGridView4.Rows.Add(DB.Student.Count() - DB.Organization.Count());
                //ループはコマの数
                for (int i = 0; i < 9; i++)
                {
                    dataGridView2.Columns[i * 2].HeaderText = DB.GetTimePeriod(i);
                    if (dataGridView2.Columns[i * 2].HeaderText == "")
                    {
                        dataGridView2.Columns[i * 2].Visible = false;
                        dataGridView2.Columns[i * 2 + 1].Visible = false;
                    }
                    for (int j = 0; j < DB.Booth; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            //教科登録
                            DataGridViewComboBoxCell comboCell = new DataGridViewComboBoxCell();
                            dataGridView2[i * 2 + 1, j * 4 + k] = comboCell;
                            comboCell.Items.Add("　");
                            DataGridViewComboBoxCell StudentCell = new DataGridViewComboBoxCell();
                            StudentCell.Items.Add("  ");
                            //生徒登録
                            dataGridView2[i * 2, j * 4 + k] = StudentCell;
                        }
                        //講師登録
                        DataGridViewComboBoxCell TeachertCell = new DataGridViewComboBoxCell();
                        TeachertCell.Items.Add("  ");
                        dataGridView2[i * 2, (j + 1) * 4 - 1] = TeachertCell;
                        dataGridView2[i * 2 + 1, (j + 1) * 4 - 1].ReadOnly = true;
                    }
                }
            }
            NowDay = "";
            for (int i = 0; i < DB.Days; i++)
            {
                listBox1.Items.Add(DB.GetDay(i));
            }
            splitContainer1.SplitterDistance = 135;
        }

        /// <summary>
        /// 生徒・講師日程表及び結果表の生成
        /// </summary>
        private void STDRAdd()
        {
            int tmp = DB.FDWC(DB.FastEnd[0].DayOfWeek.ToString());
            int tmp1 = (tmp + DB.Days) % 14;
            int count = (int)Math.Ceiling((double)(DB.Days + tmp) / 14);
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
            dataGridView3.Rows.Add(count * 10);
            dataGridView1.Rows.Add(count * 10);
            for (int i = 0; i < 16; i++)
            {
                dataGridView3.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView3.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            int buf = 0;
            for (int i = 0; i < tmp; i++)
            {
                for (int k = 0; k < 10; k++)
                {
                    dataGridView3[i + 1, 0 + k].ReadOnly = true;
                }
            }
            for (int i = 14; i > tmp1; i--)
            {
                for (int k = 0; k < 10; k++)
                {
                    dataGridView3[i + 1, 0 + k + (count - 1) * 10].ReadOnly = true;
                }
            }
            for (int j = 0; j < count; j++)
            {
                for (int i = 0; i < 9; i++)
                {
                    dataGridView3[0, i + 1 + j * 10].Value = DB.GetTimePeriod(i);
                    dataGridView3[15, i + 1 + j * 10].Value = DB.GetTimePeriod(i);
                    dataGridView1[0, i + 1 + j * 10].Value = DB.GetTimePeriod(i);
                    dataGridView1[15, i + 1 + j * 10].Value = DB.GetTimePeriod(i);
                    if (dataGridView3[0, i + 1 + j * 10].Value.ToString().CompareTo("") == 0)
                    {
                        dataGridView3.Rows[i + 1 + j * 10].Visible = false;
                    }
                    if (dataGridView1[0, i + 1 + j * 10].Value.ToString().CompareTo("") == 0)
                    {
                        dataGridView1.Rows[i + 1 + j * 10].Visible = false;
                    }
                }
                for (int i = 0; i < 14; i++)
                {
                    if (listBox1.Items.Count > buf && tmp + i < 14)
                    {
                        dataGridView3[i + 1 + tmp, j * 10].Value = listBox1.Items[buf].ToString().Substring(0, listBox1.Items[buf].ToString().Length - 3);
                        dataGridView3[i + 1 + tmp, j * 10].ReadOnly = true;
                        dataGridView1[i + 1 + tmp, j * 10].Value = listBox1.Items[buf].ToString().Substring(0, listBox1.Items[buf].ToString().Length - 3);
                        dataGridView1[i + 1 + tmp, j * 10].ReadOnly = true;
                    }
                    for (int k = 0; k < 9; k++)
                    {
                        if (listBox1.Items.Count > buf && tmp + i < 14)
                        {
                            DataGridViewCheckBoxCell checkCell = new DataGridViewCheckBoxCell();
                            checkCell.Value = true;
                            dataGridView3[i + 1 + tmp, j * 10 + k + 1] = checkCell;
                        }
                    }
                    buf++;
                }
                if (j == 0) buf -= tmp;
                tmp = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Schedule()
        {
            int tmp = DB.FDWC(DB.FastEnd[0].DayOfWeek.ToString());
            int tmp1 = (tmp + DB.Days) % 14;
            int count = (int)Math.Ceiling((double)(DB.Days + tmp) / 14);
            if (dataGridView5.Rows.Count != count * 10) dataGridView5.Rows.Add(count * 10);
            for (int i = 0; i < 16; i++)
            {
                dataGridView5.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView5.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            int buf = 0;
            for (int i = 0; i < tmp; i++)
            {
                for (int k = 0; k < 10; k++)
                {
                    dataGridView5[i + 1, 0 + k].ReadOnly = true;
                }
            }
            for (int i = 14; i > tmp1; i--)
            {
                for (int k = 0; k < 10; k++)
                {
                    dataGridView5[i, 0 + k + (count - 1) * 10].ReadOnly = true;
                }
            }
            DateTime buff = DB.FastEnd[0].AddDays(-1);
            for (int j = 0; j < count; j++)
            {
                for (int i = 0; i < 9; i++)
                {
                    dataGridView5[0, i + 1 + j * 10].Value = DB.GetTimePeriod(i);
                    dataGridView5[15, i + 1 + j * 10].Value = DB.GetTimePeriod(i);
                    if (dataGridView5[0, i + 1 + j * 10].Value.ToString().CompareTo("") == 0)
                    {
                        dataGridView5.Rows[i + 1 + j * 10].Visible = false;
                    }
                }
                for (int i = 0; i < 14; i++)
                {
                    if (DB.Days > buf && tmp + i < 14)
                    {
                        dataGridView5[i + 1 + tmp, j * 10].Value = DB.GetDate2(buf);
                        dataGridView5[i + 1 + tmp, j * 10].ReadOnly = true;
                    }
                    for (int k = 0; k < 9; k++)
                    {
                        if (DB.Days > buf && tmp + i < 14)
                        {
                            DataGridViewCheckBoxCell checkCell2 = new DataGridViewCheckBoxCell();
                            {
                                checkCell2.TrueValue = "True";
                                checkCell2.FalseValue = "False";
                                checkCell2.ValueType = typeof(string);
                            }
                            DataGridViewCheckBoxCell checkCell1 = new DataGridViewCheckBoxCell();
                            {
                                checkCell1.TrueValue = "True";
                                checkCell1.FalseValue = "False";
                                checkCell1.IndeterminateValue = "";
                                checkCell1.ValueType = typeof(string);
                            }
                            checkCell1.ThreeState = true;
                            dataGridView5[i + 1 + tmp, j * 10 + k + 1] = checkCell1;
                        }
                    }
                    buf++;
                }
                if (j == 0) buf -= tmp;
                tmp = 0;
            }
            DbUpdate();
        }
        #endregion

        #region 通常授業
        /// <summary>
        /// 
        /// </summary>
        public void nomalStart()
        {
            contextset();
            dataGridView6.Rows.Clear();
            dataGridView6.AllowUserToAddRows = false;
            dataGridView6.ColumnHeadersVisible = false;
            dataGridView6.AllowUserToResizeColumns = false;
            dataGridView6.AllowUserToResizeRows = false;
            dataGridView6.RowHeadersVisible = false;
            dataGridView6.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView6.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
            toolStripProgressBar1.Value = 5;
            dataGridView6.RowHeadersWidth = 53;
            dataGridView6.Rows.Add(DB.Booth * 4 + 1);
            string[] weeks = { "月", "火", "水", "木", "金", "土", "日" };
            string[] buff = new string[9];
            for (int i = 0; i < 9; i++)
            {
                buff[i] = DB.GetTimePeriod(i);
            }
            for (int i = 0; i < 9; i++)
            {
                dataGridView6[i * 2, 0].Value = DB.GetTimePeriod(i);
                if (dataGridView6[i * 2, 0].Value.ToString() == "")
                {
                    dataGridView6.Columns[i * 2].Visible = false;
                    dataGridView6.Columns[i * 2 + 1].Visible = false;
                }
                toolStripProgressBar1.Value = toolStripProgressBar1.Value + 10;
                for (int j = 0; j < DB.Booth; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        //教科登録
                        DataGridViewComboBoxCell comboCell = new DataGridViewComboBoxCell();
                        comboCell.Items.AddRange("", "国語", "数学", "英語", "理科", "社会", "算数", " SL ", "通常");
                        dataGridView6[i * 2 + 1, j * 4 + 1 + k] = comboCell;
                        DataGridViewComboBoxCell StudentCell = new DataGridViewComboBoxCell();
                        StudentCell.Items.Add("");
                        foreach (var student in DB.Student.Select(s => s.Key))
                        {
                            StudentCell.Items.Add(student);
                        }
                        //生徒登録
                        dataGridView6[i * 2, j * 4 + 1 + k] = StudentCell;
                    }
                    //講師登録
                    DataGridViewComboBoxCell TeachertCell = new DataGridViewComboBoxCell();
                    TeachertCell.Items.Add("");
                    foreach (var teacher in DB.Teacher.Select(s => s.Key))
                        TeachertCell.Items.Add(teacher);
                    //TeachertCell.Items.AddRange(Human.Teacher.ToArray());
                    dataGridView6[i * 2, (j + 1) * 4] = TeachertCell;
                    dataGridView6[i * 2 + 1, (j + 1) * 4].ReadOnly = true;
                }
            }
            toolStripStatusLabel2.Text = "現在編集中：月曜日";

            comboBox2_SelectedIndexChanged(new object(), new EventArgs());
            toolStripProgressBar1.Value = 100;
        }

        /// <summary>
        /// contextに生徒を登録する
        /// </summary>
        private void contextset()
        {
            foreach (var Student in DB.Student.AsParallel())
            {
                switch (Student.Value.SchoolYear)
                {
                    case "小1":
                        toolStripMenuItem4.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "小2":
                        toolStripMenuItem5.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "小3":
                        toolStripMenuItem6.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "小4":
                        toolStripMenuItem7.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "小5":
                        toolStripMenuItem8.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "小6":
                        toolStripMenuItem9.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "中1":
                        toolStripMenuItem10.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "中2":
                        toolStripMenuItem11.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "中3":
                        toolStripMenuItem12.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "高1":
                        toolStripMenuItem13.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "高2":
                        toolStripMenuItem14.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                    case "高3":
                        toolStripMenuItem15.DropDownItems.Add(Student.Key).Click += toolStripMenuItem_Click;
                        break;
                }
            }
        }

        /// <summary>
        /// 通常授業シートにおいて右クリックでの生徒検索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Click2.Y % 4 != 3)
            {
                if (DB.TimeOverlap(sender.ToString(), Click2.X / 2, 1, comboBox2.SelectedIndex))
                {
                    DB.NomalAllTuition[comboBox2.SelectedIndex].TimeClass[Click2.X / 2].NomalClass[Click2.Y / 4].Subject[Click2.Y % 4].Name = sender.ToString();
                    dataGridView6[Click2.X, Click2.Y + 1].Value = sender.ToString();
                    if (sender.ToString().CompareTo("") == 0) dataGridView6[Click2.X + 1, Click2.Y].Value = "";
                    DB.setWeightNomal(comboBox2.SelectedIndex, Click2.X / 2, Click2.Y / 4);
                }
                else
                {
                    MessageBox.Show("重複しています。", "重複", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 通常授業にて曜日の選択をする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (NomalCheck) return;
            if (comboBox2.SelectedItem != null)
                toolStripStatusLabel1.Text = "現在編集中：" + comboBox2.SelectedItem.ToString();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < DB.Booth; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        dataGridView6[i * 2, j * 4 + 1 + k].Value = DB.NomalAllTuition[comboBox2.SelectedIndex].TimeClass[i].NomalClass[j].Subject[k].Name;
                        dataGridView6[i * 2 + 1, j * 4 + 1 + k].Value = DB.NomalAllTuition[comboBox2.SelectedIndex].TimeClass[i].NomalClass[j].Subject[k].Subject;
                    }
                    dataGridView6[i * 2, (j + 1) * 4].Value = DB.NomalAllTuition[comboBox2.SelectedIndex].TimeClass[i].NomalClass[j].Tname;
                }
            }
        }

        /// <summary>
        /// 編集終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            string ss = "";
            foreach (var s in DB.NomalAllTuition.Select((val, index) => new { val, index }))
            {
                foreach (var g in s.val.TimeClass.SelectMany(sm => sm.NomalClass))
                {
                    if (g.Subject.Count(c => c.Name.Replace(" ", "").CompareTo("") != 0) != 0 && g.Tname.CompareTo("") == 0)
                    {
                        switch (s.index)
                        {
                            case 0:
                                ss += "月曜日";
                                break;
                            case 1:
                                ss += "火曜日";
                                break;
                            case 2:
                                ss += "水曜日";
                                break;
                            case 3:
                                ss += "木曜日";
                                break;
                            case 4:
                                ss += "金曜日";
                                break;
                            case 5:
                                ss += "土曜日";
                                break;
                            case 6:
                                ss += "日曜日";
                                break;
                        }
                        ss += "\n\t" + DB.GetTimePeriod(s.index) + "\n";
                    }
                }
            }
            ss += "生徒が登録されていますが、講師が登録されていません。";
            if (ss.CompareTo("生徒が登録されていますが、講師が登録されていません。") != 0)
            {
                MessageBox.Show(ss);
                return;
            }
            NewKisetu(4);
        }

        #region DG6
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView6_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            //表示されているコントロールがDataGridViewComboBoxEditingControlか調べる
            if (e.Control is DataGridViewComboBoxEditingControl)
            {
                DataGridView dgv = (DataGridView)sender;
                //編集のために表示されているコントロールを取得
                this.dataGridViewComboBox1 = (DataGridViewComboBoxEditingControl)e.Control;
                //SelectedIndexChangedイベントハンドラを追加
                this.dataGridViewComboBox1.SelectedIndexChanged += new EventHandler(dataGridViewComboBox6_SelectedIndexChanged);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView6_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dataGridViewComboBox1 != null)
            {
                this.dataGridViewComboBox1.SelectedIndexChanged -= new EventHandler(dataGridViewComboBox6_SelectedIndexChanged);
                this.dataGridViewComboBox1 = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView6_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView6[e.ColumnIndex, e.RowIndex].GetType().Equals(typeof(DataGridViewComboBoxCell)))
            {
                // 編集モードにする 
                dataGridView6.BeginEdit(true);
                // 編集モードにしたので現在の編集コントロールを取得 
                DataGridViewComboBoxEditingControl edt = dataGridView6.EditingControl as DataGridViewComboBoxEditingControl;
                // ドロップダウンさせる 
                edt.DroppedDown = true;
            }
            Click2.X = e.ColumnIndex;
            Click2.Y = e.RowIndex - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView6_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex % 4 != 0 && e.ColumnIndex % 2 == 0)
                {
                    //コンテキストメニューを表示する
                    this.contextMenuStrip3.Show();
                    //マウスカーソルの位置を画面座標で取得
                    Point p = Control.MousePosition;
                    this.contextMenuStrip3.Top = p.Y;
                    this.contextMenuStrip3.Left = p.X;
                    Click2.X = e.ColumnIndex;
                    Click2.Y = e.RowIndex - 1;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView6_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView6_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                if (e.RowIndex == 0 & e.ColumnIndex == i * 2)
                {
                    e.AdvancedBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
                }
            }
            for (int i = 0; i < DB.Booth; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (e.RowIndex == i * 4 + 4 & e.ColumnIndex == j * 2)
                    {
                        e.AdvancedBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
                    }
                }
            }
        }

        /// <summary>
        /// DataGridViewに表示されているコンボボックスのSelectedIndexChangedイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridViewComboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            //選択されたアイテムを表示
            DataGridViewComboBoxEditingControl cb = (DataGridViewComboBoxEditingControl)sender;
            if (comboBox2.SelectedIndex != -1 && Click2.Y != -1 && cb.SelectedItem.ToString() != null)
            {
                if (Click2.Y % 4 == 3)
                {
                    if (DB.TimeOverlap(cb.SelectedItem.ToString(), Click2.X / 2, 0, comboBox2.SelectedIndex))
                    {
                        DB.NomalAllTuition[comboBox2.SelectedIndex].TimeClass[Click2.X / 2].NomalClass[Click2.Y / 4].Tname = cb.SelectedItem.ToString();
                    }
                    else
                    {
                        MessageBox.Show("重複しています。", "重複", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        cb.SelectedItem = "";
                    }
                }
                else if (Click2.X % 2 == 0)
                {
                    if (DB.TimeOverlap(cb.SelectedItem.ToString(), Click2.X / 2, 1, comboBox2.SelectedIndex))
                    {
                        DB.NomalAllTuition[comboBox2.SelectedIndex].TimeClass[Click2.X / 2].NomalClass[Click2.Y / 4].Subject[Click2.Y % 4].Name = cb.SelectedItem.ToString();
                        if (cb.SelectedItem.ToString().CompareTo("") == 0)
                            DB.NomalAllTuition[comboBox2.SelectedIndex].TimeClass[Click2.X / 2].NomalClass[Click2.Y / 4].Subject[Click2.Y % 4].Subject = "";
                    }
                    else
                    {
                        MessageBox.Show("重複しています。", "重複", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        cb.SelectedItem = "";
                    }
                }
                else
                {
                    DB.NomalAllTuition[comboBox2.SelectedIndex].TimeClass[Click2.X / 2].NomalClass[Click2.Y / 4].Subject[Click2.Y % 4].Subject = cb.SelectedItem.ToString();
                }
            }
            cb.EditingControlDataGridView.NotifyCurrentCellDirty(true);
            cb.EditingControlDataGridView.EndEdit();
            //WeekInfo();
            comboBox2_SelectedIndexChanged(new object(), new EventArgs());
        }
        #endregion

        #endregion

        #region 生徒・講師・団体

        #region 生徒・講師
        /// <summary>
        /// 予定更新
        /// </summary>
        /// <param name="STFlag"></param>
        private void SC(int STFlag)
        {
            CellValueChangedFlag = true;
            int tmp = DB.FDWC(DB.FastEnd[0].DayOfWeek.ToString());
            int count = (int)Math.Ceiling((double)(DB.Days + tmp) / 14);
            int tmp2 = 0, buf = 0;
            if (ST[STFlag].SelectedItem == null) return;
            for (int j = 0; j < count; j++)
            {
                for (int ii = 0; ii < 14; ii++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        if (tmp + ii < 14 && listBox1.Items.Count > buf - tmp2)
                        {
                            if (ii - tmp2 + j * 14 > DB.Student[ST[STFlag].SelectedItem.ToString()].DayFlag.Count()) break;
                            dataGridView3[ii + 1 + tmp, j * 10 + k + 1].Value = DB.Student[ST[STFlag].SelectedItem.ToString()].DayFlag[DB.GetDay(ii - tmp2 + j * 14)][k];
                            dataGridView1[ii + 1 + tmp, j * 10 + k + 1].Value = DB.Student[ST[STFlag].SelectedItem.ToString()].InFlag[DB.GetDay(ii - tmp2 + j * 14)][k];
                        }
                    }
                    buf++;
                }

                if (j == 0)
                {
                    tmp2 = tmp;
                    tmp = 0;
                }
            }
            CellValueChangedFlag = false;
        }

        /// <summary>
        /// TMSの情報を取得する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            string s = null;
            using (StreamReader sr = new StreamReader(openFileDialog1.FileName, System.Text.Encoding.GetEncoding("shift_jis")))
            {
                //カンマで文字列sを分割し、分割された文字列を配列ssに格納する。
                //生徒情報
                TMS2Flag = 1;
                string[] ss;
                openFileDialog1.Filter = "CSV Files(*.csv)|*.csv|ALL Files(*.*)|*.*";
                toolStripStatusLabel1.Text = "CSVファイルを読み込みました。";
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Minimum = 0;
                toolStripProgressBar1.Maximum = 100;
                string buffere1 = "";
                if ((s = sr.ReadLine()) != null)
                {
                    ss = s.Split(new char[] { ',' });
                    buffere1 = ss[0] + ss[1] + ss[2] + ss[3] + ss[4];
                }
                if ((s = sr.ReadLine()) != null) { }
                switch (STMode)
                {
                    case STMode.Student:
                        if (buffere1.Contains("■生徒一覧"))
                        {
                            for (int i = 0; (s = sr.ReadLine()) != null; i++)
                            {
                                ss = s.Split(new char[] { ',' });
                                TMS2.Add(new string[] { ss[2] + ss[3], SchoolYear(ss[6], ss[7]) });
                            }
                        }
                        break;
                    case STMode.Teacher:
                        if (buffere1.Contains("■講師一覧"))
                        {
                            for (int i = 0; (s = sr.ReadLine()) != null; i++)
                            {
                                ss = s.Split(new char[] { ',' });
                                TMS2.Add(new string[] { ss[1] + ss[2], "" });
                            }
                        }
                        break;
                }
                toolStripProgressBar1.Value = 90;
            }
            //フォームに順番に自動で記載する
            TMS2send();
            toolStripProgressBar1.Value = 100;
        }

        /// <summary>
        /// 自動で入力フォームに表示する
        /// </summary>
        public void TMS2send()
        {
            for (int i = 0; i < 10; i++)
                STC[i].setCombo(-1);
            if (TMS2.Count == 0) return;
            for (int i = 0; i < 10; i++)
            {
                if (STC[i].getText().CompareTo("") == 0)
                {
                    STC[i].setText(TMS2[0][0]);
                    if (TMS2[0][1].CompareTo("") != 0)
                    {
                        STC[i].setCombo(TMS2[0][1]);
                    }
                    TMS2.RemoveAt(0);
                    if (TMS2.Count == 0) break;
                }
            }
            if (STC.Count == 0) TMS2Flag = 0;
        }

        /// <summary>
        /// 削除モード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                button6.Enabled = true;
                button4.Enabled = true;
            }
            else
            {
                button6.Enabled = false;
                button4.Enabled = false;
            }
        }

        /// <summary>
        /// Removeボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            Listview(listView1, listView2);
        }

        /// <summary>
        /// Addボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            Listview(listView2, listView1);
        }

        /// <summary>
        /// listviewの状態遷移
        /// </summary>
        /// <param name="List1">移行元</param>
        /// <param name="List2">移行先</param>
        private void Listview(ListView List1, ListView List2)
        {
            foreach (ListViewItem item in List1.SelectedItems)
            {
                string[] ss;
                if (comboBox1.SelectedIndex == 0)
                    ss = new string[] { item.Text, item.SubItems[1].Text };
                else
                    ss = new string[] { item.Text };
                List2.Items.Add(new ListViewItem(ss));
                List1.Items.Remove(item);
            }
        }

        /// <summary>
        /// 氏名登録ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            foreach (var s in STC)
            {
                if (s.getCheck())
                {
                    STcheack(s.getText(), s.getCombo());
                }
                else if (comboBox1.SelectedIndex == 1 && s.getText() != null)
                {
                    STcheack(s.getText(), "");
                }
                s.setCombo("");
                s.setText("");
            }
            if (TMS2Flag == 1) TMS2send();
        }

        /// <summary>
        /// 生徒・講師編集　重複チェック
        /// </summary>
        /// <param name="name"></param>
        /// <param name="gaku"></param>
        public void STcheack(string name, string gaku)
        {
            name = name.Replace(" ", "").Replace("　", "");
            if (STcheackbuf(name) && name.CompareTo("") != 0)
            {
                if (comboBox1.SelectedIndex == 0 && gaku != "")
                {
                    string[] list = { name, gaku };
                    listView1.Items.Add(new ListViewItem(list));
                }
                else if (comboBox1.SelectedIndex == 1)
                {
                    listView1.Items.Add(name);
                }
            }
        }

        /// <summary>
        /// 生徒・講師編集　重複チェック用
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public bool STcheackbuf(string buf)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Text == buf)
                {
                    return false;
                }
            }
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                if (listView2.Items[i].Text == buf)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 編集終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            NewKisetu(2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                return;
            STMode = comboBox1.SelectedIndex == 0 ? STMode.Student : STMode.Teacher;
            StudentTeacher();
        }

        /// <summary>
        /// 生徒及び講師を選択した時
        /// </summary>
        private void StudentTeacher()
        {
            switch (STMode)
            {
                case STMode.Student:
                    foreach (var item in STC)
                        item.setCombo(true);
                    linkLabel8.Visible = true;
                    label45.Visible = true;
                    listView1.Items.Clear();
                    string[] s2 = new string[2];
                    foreach (var s in DB.Student)
                    {
                        s2[0] = s.Key;
                        s2[1] = s.Value.SchoolYear;
                        listView1.Items.Add(new ListViewItem(s2));
                    }
                    checkListView(0);
                    toolStripStatusLabel1.Text = "現在編集中：生徒情報";
                    linkLabel8.Text = "TMS2より生成できるCSV(生徒情報)を参照する場合はこのリンクを選択してください";
                    break;
                case STMode.Teacher:
                    foreach (var item in STC)
                        item.setCombo(false);
                    label45.Visible = false;
                    listView1.Items.Clear();
                    foreach (var name in DB.Teacher.Select(s => s.Key).ToList())
                    {
                        listView1.Items.Add(name);
                    }
                    checkListView(1);
                    toolStripStatusLabel1.Text = "現在編集中：講師情報";
                    linkLabel8.Text = "TMS2より生成できるCSV(講師情報)を参照する場合はこのリンクを選択してください";
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        private void checkListView(int i)
        {
            switch (listView1.Columns.Count)
            {
                case -1:
                case 0:
                    listView1.Columns.Add("氏名");
                    listView1.Columns.Add("学年");
                    break;
                case 1:
                    switch (listView1.Columns[0].Text)
                    {
                        case "氏名":
                            listView1.Columns.Add("学年");
                            break;
                        case "学年":
                            listView1.Columns.Add("氏名");
                            break;
                    }
                    break;
            }
            switch (listView2.Columns.Count)
            {
                case -1:
                case 0:
                    listView2.Columns.Add("氏名");
                    listView2.Columns.Add("学年");
                    break;
                case 1:
                    switch (listView2.Columns[0].Text)
                    {
                        case "氏名":
                            listView2.Columns.Add("学年");
                            break;
                        case "学年":
                            listView2.Columns.Add("氏名");
                            break;
                    }
                    break;
            }
            switch (i)
            {
                case 0:
                    listView1.Columns[1].Width = 40;
                    listView2.Columns[1].Width = 40;
                    break;
                case 1:
                    listView1.Columns[1].Width = 0;
                    listView2.Columns[1].Width = 0;
                    break;
            }
        }
        #endregion

        #region 団体
        /// <summary>
        /// 登録できるかの確認関数
        /// </summary>
        private void Check()
        {
            if (textBox12.BackColor == Color.LightGreen)
            {
                button5.Enabled = true;
            }
        }

        /// <summary>
        /// 団体授業の名前が入力されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            if (textBox12.Text.CompareTo("") == 0)
            {
                textBox12.BackColor = Color.White;
                return;
            }
            textBox12.BackColor = DB.Student.Any(a => a.Key.CompareTo(textBox12.Text) == 0) ? Color.LightPink : Color.LightGreen;
            Check();
        }

        /// <summary>
        /// 既存となる団体が選択されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var item in DB.Organization.First(f => f.Key.CompareTo(comboBox4.SelectedItem.ToString()) == 0).Value.Student)
            {
                listBox4.Items.Remove(item);
                listBox5.Items.Add(item);
            }
            comboBox3.Text = DB.Organization.First(f => f.Key.CompareTo(comboBox4.SelectedItem.ToString()) == 0).Value.SchoolYear;
        }

        /// <summary>
        /// 学年が選択されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox4.Items.Clear();
            foreach (var item in DB.Student.Where(w => w.Value.SchoolYear.CompareTo(comboBox3.Text) == 0).Select(s => s.Key))
            {
                listBox4.Items.Add(item);
            }
            AddButton.Enabled = true;
            RemoveButton.Enabled = true;
        }

        /// <summary>
        /// 登録ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox12.BackColor == Color.LightGreen)
            {
                if (comboBox26.SelectedItem != null)
                {
                    if (comboBox26.SelectedIndex != -1)
                    {
                        DB.Organization.Add(textBox12.Text, new COrganization(DB.Organization.Count + 1, comboBox26.SelectedItem.ToString()));
                        for (int i = 0; i < listBox5.Items.Count; i++)
                        {
                            DB.Organization[textBox12.Text].Student.Add(listBox5.Items[i].ToString());
                        }
                        comboBox4.Items.Add(textBox12.Text);
                        textBox12.Text = "";
                        listBox5.Items.Clear();
                        comboBox3_SelectedIndexChanged(new object(), new EventArgs());
                        return;
                    }
                }
            }
            MessageBox.Show("登録できませんでした。", "エラー", MessageBoxButtons.OK);
        }

        /// <summary>
        /// 追加ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, EventArgs e)
        {
            if (listBox4.SelectedItem == null)
                return;
            comboBox3.Enabled = false;
            DB.Organization[comboBox4.Text].Student.Clear();
            for (int i = 0; i < listBox5.Items.Count; i++)
            {
                DB.Organization[comboBox4.Text].Student.Add(listBox5.Items[i].ToString());
            }
            listBox5.Items.Add(listBox4.SelectedItem.ToString());
            listBox4.Items.RemoveAt(listBox4.SelectedIndex);
            Check();
        }

        /// <summary>
        /// 削除ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (listBox5.SelectedItem == null) return;
            DB.Organization[comboBox4.Text].Student.Clear();
            for (int i = 0; i < listBox5.Items.Count; i++)
            {
                DB.Organization[comboBox4.Text].Student.Add(listBox5.Items[i].ToString());
            }
            listBox4.Items.Add(listBox5.SelectedItem.ToString());
            listBox5.Items.RemoveAt(listBox5.SelectedIndex);
            if (listBox5.Items.Count == 0) comboBox3.Enabled = true;
            Check();
        }

        /// <summary>
        /// 編集終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            NewKisetu(3);
        }
        #endregion

        #endregion

        #region 初期設定
        /// <summary>
        /// 入力された情報を元データを保存する
        /// </summary>
        /// <param name="i">何限目か</param>
        private void KomaIn(int i)
        {
            if (NKS[i].check())
            {
                DB.SetKoma(NKS[i].getText(), NKS[i].getCombo(), i);
                NKS[i].setLabel(DB.GetTimePeriod(i));
            }
        }

        #region テキストボックスの入力チェック
        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            KomaIn(0);
        }
        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            KomaIn(1);
        }
        private void textBox15_TextChanged(object sender, EventArgs e)
        {
            KomaIn(2);
        }
        private void textBox16_TextChanged(object sender, EventArgs e)
        {
            KomaIn(3);
        }
        private void textBox17_TextChanged(object sender, EventArgs e)
        {
            KomaIn(4);
        }
        private void textBox18_TextChanged(object sender, EventArgs e)
        {
            KomaIn(5);
        }
        private void textBox19_TextChanged(object sender, EventArgs e)
        {
            KomaIn(6);
        }
        private void textBox20_TextChanged(object sender, EventArgs e)
        {
            KomaIn(7);
        }
        private void textBox21_TextChanged(object sender, EventArgs e)
        {
            KomaIn(8);
        }
        #endregion

        #region コンボボックスの入力チェック
        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox5.Enabled)
                KomaIn(0);
        }
        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox6.Enabled)
                KomaIn(1);
        }
        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox7.Enabled)
                KomaIn(2);
        }
        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox8.Enabled)
                KomaIn(3);
        }
        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox9.Enabled)
                KomaIn(4);
        }
        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox10.Enabled)
                KomaIn(5);
        }
        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox11.Enabled)
                KomaIn(6);
        }
        private void comboBox22_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox22.Enabled)
                KomaIn(7);
        }
        private void comboBox23_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox23.Enabled)
                KomaIn(8);
        }
        #endregion

        /// <summary>
        /// ブースを値を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox24_SelectedIndexChanged(object sender, EventArgs e)
        {
            DB.Booth = Convert.ToInt32(comboBox24.Text);
            button7.Enabled = true;
        }
        #endregion

        #region 関数

        /// <summary>
        /// 学年を取得する
        /// </summary>
        /// <param name="buf1">小中高</param>
        /// <param name="buf2">学年</param>
        /// <returns></returns>
        public string SchoolYear(string buf1, string buf2)
        {
            string i = "", j = "";
            if (0 <= buf1.IndexOf("小"))
            {
                i = "1";
            }
            else if (0 <= buf1.IndexOf("中"))
            {
                i = "2";
            }
            else if (0 <= buf1.IndexOf("高"))
            {
                i = "3";
            }
            if (0 <= buf2.IndexOf("1") || 0 <= buf2.IndexOf("１"))
            {
                j = "1";
            }
            else if (0 <= buf2.IndexOf("2") || 0 <= buf2.IndexOf("２"))
            {
                j = "2";
            }
            else if (0 <= buf2.IndexOf("3") || 0 <= buf2.IndexOf("３"))
            {
                j = "3";
            }
            else if (0 <= buf2.IndexOf("4") || 0 <= buf2.IndexOf("４"))
            {
                j = "4";
            }
            else if (0 <= buf2.IndexOf("5") || 0 <= buf2.IndexOf("５"))
            {
                j = "5";
            }
            else if (0 <= buf2.IndexOf("6") || 0 <= buf2.IndexOf("６"))
            {
                j = "6";
            }
            switch (i + j)
            {
                case "11":
                    return "小1";
                case "12":
                    return "小2";
                case "13":
                    return "小3";
                case "14":
                    return "小4";
                case "15":
                    return "小5";
                case "16":
                    return "小6";
                case "21":
                    return "中1";
                case "22":
                    return "中2";
                case "23":
                    return "中3";
                case "31":
                    return "高1";
                case "32":
                    return "高2";
                case "33":
                    return "高3";
                default:
                    return "";
            }
        }

        #endregion

        #region 印刷

        //
        private void ToolStripMenuItem17_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < DB.Student.Count + DB.Teacher.Count; i++)
            {
                STFlagPrint = i;
                PanelPeint();
                toolStripMenuItem16_Click(new object(), new EventArgs());
            }
            STFlagPrint = -1;
            PanelPeint();
        }

        //印刷
        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            switch (toolStripComboBox3.SelectedIndex)
            {
                case 0:
                    printDocument1.DefaultPageSettings.Landscape = true;
                    PrintSaportCheck(PaperKind.B4);
                    break;
                case 1:
                    printDocument1.DefaultPageSettings.Landscape = false;
                    PrintSaportCheck(PaperKind.A4);
                    break;
                case 2:
                    printDocument1.DefaultPageSettings.Landscape = true;
                    PrintSaportCheck(PaperKind.A4);
                    break;
            }
            if (DB.Booth != 10)
            {
                printDocument1.DefaultPageSettings.Margins.Bottom = DB.Booth * 13000;
            }
            printDocument1.Print();
        }

        //プリンタがサポートしている用紙サイズを調べる
        private void PrintSaportCheck(PaperKind kind)
        {
            foreach (PaperSize ps in printDocument1.PrinterSettings.PaperSizes)
            {
                if (ps.Kind == kind)
                {
                    printDocument1.DefaultPageSettings.PaperSize = ps;
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (toolStripComboBox3.SelectedIndex)
            {
                case 0:
                    PanelPeint();
                    break;
                case 1:
                case 2:
                    toolStripComboBox2Add();
                    break;
            }
        }

        /// <summary>
        /// 生徒若しくは講師を登録する
        /// </summary>
        private void toolStripComboBox2Add()
        {
            toolStripComboBox2.Items.Clear();
            toolStripComboBox2.Text = "";
            if (toolStripComboBox1.SelectedIndex == 0)
            {
                toolStripComboBox2.Items.AddRange(DB.Student.Select(s => s.Key).ToArray());
            }
            else
            {
                toolStripComboBox2.Items.AddRange(DB.Teacher.Select(s => s.Key).ToArray());
            }
        }

        /// <summary>
        /// panel描画
        /// </summary>
        public void PanelPeint()
        {
            int width = 25;
            WindowState = FormWindowState.Maximized;
            int canvasWidth = 0, canvasHeight = 0;
            int tmp = 0, count = 0;
            switch (toolStripComboBox3.SelectedIndex)
            {
                case 0:
                    for (int i = 0; i < 9; i++)
                    {
                        if (DB.GetKomaCheck(i))
                        {
                            canvasWidth = (i + 1) * 140;
                        }
                    }
                    canvasHeight = DB.Booth * 90 + 30;
                    break;
                case 1:
                    tmp = DB.FDWC(DB.FastEnd[0].DayOfWeek.ToString());
                    count = (int)Math.Ceiling((double)(DB.Days + tmp) / 14);
                    canvasWidth = 720;
                    canvasHeight = count * width * 10 + 70;
                    break;
                case 2:
                    tmp = DB.FDWC(DB.FastEnd[0].DayOfWeek.ToString());
                    count = (int)Math.Ceiling((double)(DB.Days + tmp) / 14);
                    canvasWidth = count * width * 10 + 70;
                    canvasHeight = 720;
                    break;
            }
            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(canvasWidth, canvasHeight);
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            //Brushオブジェクトの作成
            switch (toolStripComboBox3.SelectedIndex)
            {
                case 0:
                    g = PanelPeintJyugyou(g, canvasHeight, canvasWidth);
                    break;
                case 1:
                    g = PanelPeintStudentTeacher(g, canvasHeight, canvasWidth, width);
                    break;
            }
            //リソースを解放する
            g.Dispose();
            //PictureBox1に表示する
            pictureBox1.Image = canvas;
        }

        /// <summary>
        /// 授業表
        /// </summary>
        /// <param name="g">Graphicsオブジェクト</param>
        /// <param name="canvasHeight">画像の高さ</param>
        /// <param name="canvasWidth">画像の幅</param>
        /// <returns></returns>
        private Graphics PanelPeintJyugyou(Graphics g, int canvasHeight, int canvasWidth)
        {
            int flag = 0;
            Pen p = new Pen(Color.Black, 1);
            Font fnt = new Font("ＭＳ ゴシック", 15);
            Font fnt1 = new Font("HGP明朝L", 10);
            SolidBrush b = new SolidBrush(Color.LightYellow);
            for (int i = 0; i < 9; i++)
            {
                if (DB.GetKomaCheck(i))
                {
                    g.FillRectangle(new SolidBrush(Color.White), 140 * i, 0, 140, 30);
                    g.DrawRectangle(p, 140 * i, 0, 140, 30);
                    g.DrawString(DB.GetTimePeriod(i), fnt, Brushes.Black, 5 + 140 * i, 7);
                    for (int j = 0; j < DB.Booth; j++)
                    {
                        b = (j % 2 == 0) ? new SolidBrush(Color.LightYellow) : new SolidBrush(Color.White);
                        g.FillRectangle(b, 140 * i, 90 * j + 30, 140, 90);
                        g.DrawRectangle(p, 140 * i, 90 * j + 30, 140, 90);
                        g.DrawLine(p, 140 * i, 90 * j + 30 + 65, 140 * (i + 1), 90 * j + 30 + 65);
                        foreach (var subject in DB.AllTuition[toolStripComboBox1.SelectedItem.ToString()].TimeClass[i].NomalClass[j].Subject.Select((k, v) => new { k, v }))
                        {
                            g.DrawString(subject.k.Name, fnt1, Brushes.Black, 140 * i + 5, 90 * j + 30 + 7 + subject.v * 20);
                            if (DB.Student.Any(a => a.Key.CompareTo(subject.k.Name) == 0))
                            {
                                flag = 1;
                                string buf = DB.Student[subject.k.Name].SchoolYear;
                                buf += " " + subject.k.Subject.Substring(0, 1);
                                g.DrawString(buf, fnt1, Brushes.Black, 140 * i + 90, 90 * j + 30 + 7 + subject.v * 20);
                            }
                            else
                            {
                                g.DrawString("", fnt1, Brushes.Black, 140 * i + 90, 90 * j + 30 + 7 + subject.v * 20);
                            }
                        }
                        if (flag == 1) g.DrawString(DB.AllTuition[toolStripComboBox1.SelectedItem.ToString()].TimeClass[i].NomalClass[j].Tname, fnt1, Brushes.Black, 140 * i + 5, 90 * j + 30 + 7 + 65);
                        flag = 0;
                    }
                }
            }
            g.DrawLine(p, canvasWidth - 1, 0, canvasWidth - 1, canvasHeight - 1);
            g.DrawLine(p, 0, canvasHeight - 1, canvasWidth - 1, canvasHeight - 1);
            return g;
        }

        /// <summary>
        /// 生徒及び講師、日程表
        /// </summary>
        /// <param name="g">Graphicsオブジェクト</param>
        /// <param name="canvasHeight">画像の高さ</param>
        /// <param name="canvasWidth">画像の幅</param>
        /// <param name="width">表の幅</param>
        /// <returns></returns>
        private Graphics PanelPeintStudentTeacher(Graphics g, int canvasHeight, int canvasWidth, int width)
        {

            DateTime daybuf;
            Pen p = new Pen(Color.Black, 1);
            Font fnt = new Font("ＭＳ ゴシック", 15);
            Font fnt2 = new Font("ＭＳ ゴシック", 9);

            //生徒・講師配布情報
            int tmp = DB.FDWC(DB.FastEnd[0].DayOfWeek.ToString());
            int tmp4 = 0;
            int count = (int)Math.Ceiling((double)(DB.Days + tmp) / 14);
            int buff = 0;
            string StudentNumber = "";
            if (toolStripComboBox3.SelectedIndex == 1) StudentNumber = toolStripComboBox2.SelectedItem.ToString();
            SolidBrush Do = new SolidBrush(Color.FromArgb(120, Color.LightBlue));
            SolidBrush Ni = new SolidBrush(Color.FromArgb(120, Color.LightPink));
            SolidBrush nomal = new SolidBrush(Color.White);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, 719, 30);
            g.DrawRectangle(p, 0, 0, 719, 30);
            if (toolStripComboBox3.SelectedIndex == 1) g.DrawString("氏名：" + toolStripComboBox2.Text, fnt, Brushes.Black, 520, 7);
            g.FillRectangle(new SolidBrush(Color.White), 0, 30, 80, 30);
            g.DrawRectangle(p, 0, 30, 80, 30);
            g.DrawString("時間帯", fnt, Brushes.Black, 5, 37);
            string yasumi = "休" + Environment.NewLine + "校" + Environment.NewLine + "日";
            for (int i = 0; i < 14; i++)
            {
                if ((i + 1) % 7 == 6)
                {
                    g.FillRectangle(new SolidBrush(Color.White), 40 * i + 80, 30, 60, 30);
                    g.FillRectangle(Do, 40 * i + 80, 30, 60, 30);
                }
                else if ((i + 1) % 7 == 0)
                {
                    g.FillRectangle(new SolidBrush(Color.White), 40 * i + 80, 30, 60, 30);
                    g.FillRectangle(Ni, 40 * i + 80, 30, 60, 30);
                }
                else
                {
                    g.FillRectangle(new SolidBrush(Color.White), 40 * i + 80, 30, 60, 30);
                }
                g.DrawRectangle(p, 40 * i + 80, 30, 60, 30);
            }
            string[] week = { "月", "火", "水", "木", "金", "土", "日" };
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    g.DrawString(week[j], fnt, Brushes.Black, 40 * j + 85 + i * 280, 37);
                }
            }
            g.FillRectangle(new SolidBrush(Color.White), 640, 30, 80, 30);
            g.DrawRectangle(p, 640, 30, 79, 30);
            g.DrawString("時間帯", fnt, Brushes.Black, 645, 37);
            for (int j = 0; j < count; j++)
            {
                g.FillRectangle(new SolidBrush(Color.White), 0, 60 + width * 10 * j + j * 3, 80, width);
                g.DrawRectangle(p, 0, 60 + width * 10 * j + j * 3, 80, width);
                g.FillRectangle(new SolidBrush(Color.White), 640, 60 + width * 10 * j + j * 3, 80, width);
                g.DrawRectangle(p, 640, 60 + width * 10 * j + j * 3, 79, width);
                for (int i = 0; ; i++)
                {
                    if (i >= 9) break;
                    if (DB.GetKomaCheck(i))
                    {
                        g.FillRectangle(new SolidBrush(Color.White), 0, 60 + width * (1 + i + 10 * j) + j * 3, 80, width);
                        g.DrawRectangle(p, 0, 60 + width + width * i + width * 10 * j + j * 3, 80, width);
                        g.DrawString(DB.GetTimePeriod(i), fnt2, Brushes.Black, 2, 95 + width * (i + 10 * j) + j * 3);
                        g.FillRectangle(new SolidBrush(Color.White), 640, 60 + width * (1 + i + 10 * j) + j * 3, 80, width);
                        g.DrawRectangle(p, 640, 60 + width + width * i + width * 10 * j + j * 3, 79, width);
                        g.DrawString(DB.GetTimePeriod(i), fnt2, Brushes.Black, 642, 95 + width * (i + 10 * j) + j * 3);
                    }
                    else
                    {
                        break;
                    }
                }
                //2週間分のループ
                for (int i = 0; i < 14; i++)
                {
                    if ((i + 1) % 7 == 6)
                    {
                        nomal = Do;
                    }
                    else if ((i + 1) % 7 == 0)
                    {
                        nomal = Ni;
                    }
                    else
                    {
                        nomal = new SolidBrush(Color.White);
                    }
                    g.FillRectangle(nomal, 80 + 40 * i, 60 + width * 10 * j + j * 3, 40, width);
                    g.DrawRectangle(p, 80 + 40 * i, 60 + width * 10 * j + j * 3, 40, width);
                    for (int k = 0; k < 9; k++)
                    {
                        g.FillRectangle(nomal, 80 + 40 * i, 60 + width * (1 + k + 10 * j) + j * 3, 40, width);
                        g.DrawRectangle(p, 80 + 40 * i, 60 + width * (1 + k + 10 * j) + j * 3, 40, width);
                    }
                }
                for (int ii = 0; ii + tmp < 14; ii++)
                {
                    if (buff == DB.Days) break;
                    daybuf = DB.FastEnd[0].AddDays(buff);
                    g.DrawString(string.Format("{0,2}/{1,2}", daybuf.Month, daybuf.Day), fnt2, Brushes.Black, 83 + 40 * (ii + tmp), 67 + width * 10 * j + j * 3);
                    for (int k = 0; k < 9; k++)
                    {
                        if (tmp + ii < 14 && listBox1.Items.Count > buff - tmp4)
                        {
                            if (ii - tmp4 + j * 14 >= DB.Days) break;
                            if (toolStripComboBox3.SelectedIndex == 1)
                                g.DrawString(DB.Student[StudentNumber].InFlag[listBox1.Items[ii - tmp4 + j * 14].ToString()][k], fnt2, Brushes.Black, 92 + 40 * (ii + tmp), 67 + width * (1 + k + 10 * j) + j * 3);
                        }
                    }

                    //休校日
                    for (int k = 0; k < 9; k++)
                    {
                        if (DB.ScheduleFlag[listBox1.Items[ii - tmp4 + j * 14].ToString()][k].CompareTo("") == 0)
                        {

                            switch (DB.FastEnd[0].AddDays(ii - tmp4 + j * 14).DayOfWeek.ToString())
                            {
                                case "Saturday":
                                    nomal = Do;
                                    break;
                                case "Sunday":
                                    nomal = Ni;
                                    break;
                                default:
                                    nomal = new SolidBrush(Color.White);
                                    break;
                            }
                            g.FillRectangle(new SolidBrush(Color.White), 80 + 40 * (ii + tmp), 60 + width + width * 10 * j + j * 3, 40, width * 9);
                            g.FillRectangle(nomal, 80 + 40 * (ii + tmp), 60 + width + width * 10 * j + j * 3, 40, width * 9);
                            g.DrawRectangle(p, 80 + 40 * (ii + tmp), 60 + width + width * 10 * j + j * 3, 40, width * 9);
                            g.DrawString(yasumi, fnt, Brushes.Black, 85 + 40 * (ii + tmp), 60 + width * 4 + width * 10 * j + j * 3);
                            break;
                        }
                    }
                    buff++;
                }
                if (j == 0)
                {
                    tmp4 = tmp;
                    tmp = 0;
                }
            }
            return g;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox3.SelectedIndex != 0 && toolStripComboBox2.SelectedIndex != -1)
            {
                PanelPeint();
            }
        }

        #endregion

    }
}
