using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Height = 590;
            Width = 1000;

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

        private void Form1_Resize(object sender, EventArgs e)
        {
            
        }


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
            //NewKisetu(0);
        }
        #endregion


        #region 情報
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker3.MaxDate = dateTimePicker2.Value.AddDays(50);
            dateTimePicker3.MinDate = dateTimePicker2.Value.AddDays(1);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //NewKisetu(1);
        }
        #endregion

        #region 初期設定
        /// <summary>
        /// 入力された情報を元データを保存する
        /// </summary>
        /// <param name="i">何限目か</param>
        private void KomaIn(int i)
        {
            if (NKS[i].Check())
            {
                DB.SetKoma(NKS[i].GetText(), NKS[i].GetCombo(), i);
                NKS[i].SetLabel(DB.GetTimePeriod(i));
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
    }
}
