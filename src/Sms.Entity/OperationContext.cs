using System.Data.Entity;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace Sms.Entity
{
    public class OperationContext
    {
        /// <summary>
        /// 从当前线程中获取 线程唯一的 操作上下文
        /// </summary>
        public static DbContext Current
        {
            get
            {
                DbContext dbContext = CallContext.LogicalGetData("CurrentContext") as DbContext;
                if (dbContext == null)
                {
                    dbContext = new SmsEntities();
                    CallContext.LogicalSetData("CurrentContext", dbContext);
                }
                return dbContext;
            }
        }

        /// <summary>
        /// 调用 线程唯一 的 EF容器 对象的 SaveChanges方法 来保存数据
        /// </summary>
        public static async Task<int> SaveChanges()
        {
            return await Current.SaveChangesAsync();
        }
    }
}
