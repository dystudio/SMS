﻿


//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------



namespace Sms.Repository
{
    using System;
    using System.Threading.Tasks;
    
    
        public partial class RepositoryFactory:Sms.IRepository.IRepositoryFactory
        {
    
            	    public Sms.IRepository.ICardHistory ICardHistory
        {
           get
           {
               return new Sms.Repository.CardHistory();
           }
        }
            	    public Sms.IRepository.IMemberCard IMemberCard
        {
           get
           {
               return new Sms.Repository.MemberCard();
           }
        }
            	    public Sms.IRepository.IPromotion IPromotion
        {
           get
           {
               return new Sms.Repository.Promotion();
           }
        }
            	    public Sms.IRepository.ISysLog ISysLog
        {
           get
           {
               return new Sms.Repository.SysLog();
           }
        }
            	    public Sms.IRepository.ISystemModule ISystemModule
        {
           get
           {
               return new Sms.Repository.SystemModule();
           }
        }
            	    public Sms.IRepository.ISystemModuleRight ISystemModuleRight
        {
           get
           {
               return new Sms.Repository.SystemModuleRight();
           }
        }
            	    public Sms.IRepository.ISystemRole ISystemRole
        {
           get
           {
               return new Sms.Repository.SystemRole();
           }
        }
            	    public Sms.IRepository.ISystemRoleRight ISystemRoleRight
        {
           get
           {
               return new Sms.Repository.SystemRoleRight();
           }
        }
            	    public Sms.IRepository.ISystemUser ISystemUser
        {
           get
           {
               return new Sms.Repository.SystemUser();
           }
        }
         public async Task<int> SaveChanges()
         {
             return await Sms.Entity.OperationContext.SaveChanges();
         }
    
        }
    
         public partial class CardHistory:BaseRepository<Sms.Entity.CardHistory>,Sms.IRepository.ICardHistory
         {
         }
    
         public partial class MemberCard:BaseRepository<Sms.Entity.MemberCard>,Sms.IRepository.IMemberCard
         {
         }
    
         public partial class Promotion:BaseRepository<Sms.Entity.Promotion>,Sms.IRepository.IPromotion
         {
         }
    
         public partial class SysLog:BaseRepository<Sms.Entity.SysLog>,Sms.IRepository.ISysLog
         {
         }
    
         public partial class SystemModule:BaseRepository<Sms.Entity.SystemModule>,Sms.IRepository.ISystemModule
         {
         }
    
         public partial class SystemModuleRight:BaseRepository<Sms.Entity.SystemModuleRight>,Sms.IRepository.ISystemModuleRight
         {
         }
    
         public partial class SystemRole:BaseRepository<Sms.Entity.SystemRole>,Sms.IRepository.ISystemRole
         {
         }
    
         public partial class SystemRoleRight:BaseRepository<Sms.Entity.SystemRoleRight>,Sms.IRepository.ISystemRoleRight
         {
         }
    
         public partial class SystemUser:BaseRepository<Sms.Entity.SystemUser>,Sms.IRepository.ISystemUser
         {
         }
    
    
}