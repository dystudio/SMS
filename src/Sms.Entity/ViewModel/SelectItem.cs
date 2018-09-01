using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sms.Entity.ViewModel
{
    /// <summary>
    /// 页面下拉列表模型
    /// </summary>
    public class SelectItem
    {
        /// <summary>
        /// 显示的文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 选项值
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Selected { get; set; }
    }
}
