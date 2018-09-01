﻿

//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------


namespace Sms.IRepository
{
    using System;
    using System.Threading.Tasks;
    
    
        public partial interface IRepositoryFactory
        {
    
    	ICardHistory ICardHistory{get;}
    	IMemberCard IMemberCard{get;}
    	IPromotion IPromotion{get;}
    	ISysLog ISysLog{get;}
    	ISystemModule ISystemModule{get;}
    	ISystemModuleRight ISystemModuleRight{get;}
    	ISystemRole ISystemRole{get;}
    	ISystemRoleRight ISystemRoleRight{get;}
    	ISystemUser ISystemUser{get;}
        Task<int> SaveChanges();
    }
    
    public partial interface ICardHistory:IBaseRepository<Sms.Entity.CardHistory>
    {
    }
    
    public partial interface IMemberCard:IBaseRepository<Sms.Entity.MemberCard>
    {
    }
    
    public partial interface IPromotion:IBaseRepository<Sms.Entity.Promotion>
    {
    }
    
    public partial interface ISysLog:IBaseRepository<Sms.Entity.SysLog>
    {
    }
    
    public partial interface ISystemModule:IBaseRepository<Sms.Entity.SystemModule>
    {
    }
    
    public partial interface ISystemModuleRight:IBaseRepository<Sms.Entity.SystemModuleRight>
    {
    }
    
    public partial interface ISystemRole:IBaseRepository<Sms.Entity.SystemRole>
    {
    }
    
    public partial interface ISystemRoleRight:IBaseRepository<Sms.Entity.SystemRoleRight>
    {
    }
    
    public partial interface ISystemUser:IBaseRepository<Sms.Entity.SystemUser>
    {
    }
    
    
}