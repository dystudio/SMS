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
    
    public partial class ItemBrand
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ItemBrand()
        {
            this.ItemInfo = new HashSet<ItemInfo>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public int Sort { get; set; }
        public string CategoryList { get; set; }
        public bool IsStop { get; set; }
        public string Remark { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string CreateUser { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ItemInfo> ItemInfo { get; set; }
    }
}
