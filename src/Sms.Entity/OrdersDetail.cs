//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Sms.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrdersDetail
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public int DetailType { get; set; }
        public int ItemId { get; set; }
        public int SkuId { get; set; }
        public string ItemTitle { get; set; }
        public string SkuTitle { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Discount { get; set; }
        public decimal ActualPrice { get; set; }
        public int Quantity { get; set; }
        public int PresentPoints { get; set; }
        public System.DateTime CreateTime { get; set; }
    
        public virtual Orders Orders { get; set; }
    }
}
