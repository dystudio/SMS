using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sms.Entity.ViewModel
{
    /// <summary>
    /// 用于页面显示的菜单模型
    /// </summary>
    public class MenuNode
    {
        public MenuNode()
        {
            //初始化子节点
            this.ChildNode = new List<MenuNode>();
        }

        /// <summary>
        /// 菜单编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 菜单名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 跳转地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<MenuNode> ChildNode { get; set; }
    }
}
