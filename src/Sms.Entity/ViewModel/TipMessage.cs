using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Entity.ViewModel
{
    public class TipMessage
    {
        /// <summary>
        /// 操作状态
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 操作结果返回的信息
        /// </summary>
        public string MsgText { get; set; }

        /// <summary>
        /// 要跳转的地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 返回的数据
        /// </summary>
        public object Data { get; set; }
    }
}
