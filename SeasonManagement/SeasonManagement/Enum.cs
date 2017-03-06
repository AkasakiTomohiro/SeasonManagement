using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeasonManagement
{
    class Enum
    {
        /// <summary>
        /// 教科
        /// </summary>
        public enum Subject
        {
            /// <summary>
            /// 国語
            /// </summary>
            Japanes,
            /// <summary>
            /// 数学
            /// </summary>
            Mathematics,
            /// <summary>
            /// 英語
            /// </summary>
            English,
            /// <summary>
            /// 理科
            /// </summary>
            Science,
            /// <summary>
            /// 社会
            /// </summary>
            Society,
            /// <summary>
            /// 全教科
            /// </summary>
            All,
        }

        /// <summary>
        /// 生徒・講師
        /// </summary>
        public enum STMode
        {
            /// <summary>
            /// 学生
            /// </summary>
            Student,
            /// <summary>
            /// 講師
            /// </summary>
            Teacher,
        }

        /// <summary>
        /// 現在の編集状態を示す
        /// </summary>
        public enum EditState
        {
            /// <summary>
            /// 
            /// </summary>
            First,
            /// <summary>
            /// 
            /// </summary>
            Second,
            /// <summary>
            /// 
            /// </summary>
            Third,
            /// <summary>
            /// 
            /// </summary>
            Forth,
            /// <summary>
            /// 
            /// </summary>
            Fifth,
            /// <summary>
            /// 
            /// </summary>
            Edit,
        }
    }
}
