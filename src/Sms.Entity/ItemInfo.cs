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
    
    public partial class ItemInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ItemInfo()
        {
            this.ItemSku = new HashSet<ItemSku>();
            this.ItemUnionCategory = new HashSet<ItemUnionCategory>();
        }
    
        public int Id { get; set; }
        public string ItemCode { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Poster { get; set; }
        public string Banner { get; set; }
        public int BrandId { get; set; }
        public bool IsOnSale { get; set; }
        public int IsHot { get; set; }
        public int IsTop { get; set; }
        public bool IsDelete { get; set; }
        public int Sort { get; set; }
        public Nullable<decimal> MarketPrice { get; set; }
        public string DetailDesc { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string CreateUser { get; set; }
    
        public virtual ItemBrand ItemBrand { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ItemSku> ItemSku { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ItemUnionCategory> ItemUnionCategory { get; set; }
    }
}
