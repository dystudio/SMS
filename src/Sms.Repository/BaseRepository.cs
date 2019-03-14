using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Data;
using System.Reflection;
using System.Data.Entity.Infrastructure;

namespace Sms.Repository
{
    public class BaseRepository<TModel> : Sms.IRepository.IBaseRepository<TModel> where TModel : class
    {
        /// <summary>
        /// EF 容器对象
        /// </summary>
        DbContext db = Sms.Entity.OperationContext.Current;

        /// <summary>
        /// 数据集
        /// </summary>
        DbSet<TModel> dbSet;

        public BaseRepository()
        {
            //关闭 EF 容器 根据 edmx 里的 配置进行的 验证
            db.Configuration.ValidateOnSaveEnabled = false;
            //根据传入的 实体类，返回 对应的 EF 数据集对象，可用来操作 实体类 对应的 数据表
            dbSet = db.Set<TModel>();
        }

        #region  新增实体
        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="entity"></param>
        public void Add(TModel entity)
        {
            dbSet.Add(entity);
        }
        #endregion

        #region  删除实体
        /// <summary>
        /// 2.0 删除实体
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(TModel entity)
        {
            //手动附加，调用删除方法改变 其 标识
            //将 对象 加入到 EF容器中
            dbSet.Attach(entity);
            //将 对象 标记为 删除状态
            dbSet.Remove(entity);
        }
        #endregion

        #region 根据条件删除
        /// <summary>
        /// 根据条件删除
        /// </summary>
        /// <param name="where"></param>
        public void DeleteBy(Expression<Func<TModel, bool>> where)
        {
            //根据条件 使用 EF 将要删除的对象 查询出来，并存入 EF容器
            var list = dbSet.Where(where);
            //循环对象，并修改对象的 状态为“删除”
            foreach (var item in list)
            {
                dbSet.Remove(item);
            }
        }
        #endregion

        #region  修改指定对象
        /// <summary>
        /// 3.0 修改指定对象
        /// </summary>
        /// <param name="entity"></param>     
        /// <param name="modifiedPropertyNames">修改了的属性的名字，这些属性的值需要更新到数据库中</param>
        public void Modify(TModel entity, params string[] modifiedPropertyNames)
        {
            //1.将对象 加入 到EF容器中，并获取 Entry对象（包装器）
            DbEntityEntry<TModel> entry = db.Entry<TModel>(entity);
            //2.将 entry 的State属性标记为 未修改状态 (注意导入：System.Data.Entity 程序集)
            dbSet.Attach(entity);
            if (modifiedPropertyNames.Length > 0) entry.State = EntityState.Unchanged;
            else entry.State = EntityState.Modified;
            //3.循环要修改的属性 的名字，并设置 其对应的 IfModified 为 true
            //  SaveChange的时候，EF会找出 IsModified =true的属性，并把他们生成到 update　语句中  
            foreach (var proName in modifiedPropertyNames)
            {
                entry.Property(proName).IsModified = true;
            }
        }
        #endregion

        #region  根据条件修改
        /// <summary>
        ///  根据条件修改
        /// </summary>
        /// <param name="where">修改条件（哪些要修改）</param>
        /// <param name="modifyPropertyNames">要修改的属性名</param>
        /// <param name="modifyPropertyValues">新的属性值</param>
        public void ModifyBy(Expression<Func<TModel, bool>> where, string[] modifyPropertyNames, string[] modifyPropertyValues)
        {
            //获取 要修改的 实体类 的 类型对象Type
            Type typeObj = typeof(TModel);
            //获取要修改的 属性类型 对象 PropertyInfo
            List<PropertyInfo> listModifyProperty = new List<PropertyInfo>();
            foreach (var propertyName in modifyPropertyNames)
            {
                //将要修改的 属性类型对象 存入集合
                listModifyProperty.Add(typeObj.GetProperty(propertyName));
            }
            //查询要修改的对象(注意：对象已经存在EF容器中了)
            var listModifing = dbSet.Where(where);
            //循环修改这些对象的属性
            foreach (var item in listModifing)
            {
                //遍历要修改的 属性类型 集合
                for (int index = 0; index < listModifyProperty.Count; index++)
                {
                    PropertyInfo proModify = listModifyProperty[index];
                    //修改 item 对象 的 proModify对应属性的 值 为 数组modifyPropertyValues里相同下标的值！
                    proModify.SetValue(item, modifyPropertyValues[index], null);
                }
            }
        }
        #endregion

        #region  根据条件修改
        /// <summary>
        ///  根据条件修改
        /// </summary>
        /// <param name="where">修改条件（哪些要修改）</param>
        /// <param name="modifyPropertyNames">要修改的属性名</param>
        /// <param name="modifyPropertyValues">新的属性值</param>
        public void ModifyBy(Expression<Func<TModel, bool>> where, string[] modifyPropertyNames, object[] modifyPropertyValues)
        {
            //获取 要修改的 实体类 的 类型对象Type
            Type typeObj = typeof(TModel);
            //获取要修改的 属性类型 对象 PropertyInfo
            List<PropertyInfo> listModifyProperty = new List<PropertyInfo>();
            foreach (var propertyName in modifyPropertyNames)
            {
                //将要修改的 属性类型对象 存入集合
                listModifyProperty.Add(typeObj.GetProperty(propertyName));
            }
            //查询要修改的对象(注意：对象已经存在EF容器中了)
            var listModifing = dbSet.Where(where);
            //循环修改这些对象的属性
            foreach (var item in listModifing)
            {
                //遍历要修改的 属性类型 集合
                for (int index = 0; index < listModifyProperty.Count; index++)
                {
                    PropertyInfo proModify = listModifyProperty[index];
                    //修改 item 对象 的 proModify对应属性的 值 为 数组modifyPropertyValues里相同下标的值！
                    proModify.SetValue(item, modifyPropertyValues[index], null);
                }
            }
        }
        #endregion



        #region  根据条件查询单个实体
        /// <summary>
        /// 4.0 根据条件查询单个实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public TModel Single(Expression<Func<TModel, bool>> where)
        {
            return dbSet.FirstOrDefault(where);
        }

        public async Task<TModel> SingleAsync(Expression<Func<TModel, bool>> where)
        {
            return await dbSet.FirstOrDefaultAsync(where);
        }
        #endregion

        #region  根据条件查询
        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <param name="where">条件表达式</param>
        /// <returns></returns>
        public IQueryable<TModel> Where(Expression<Func<TModel, bool>> where)
        {
            return dbSet.Where(where);
        }
        #endregion


        /// <summary>
        /// 根据条件查询数量
        /// </summary>
        /// <param name="where">查询条件表达式</param>
        /// <returns></returns>
        public int Count(Expression<Func<TModel, bool>> where)
        {
            return dbSet.Count(where);
        }
        public async Task<int> CountAsync(Expression<Func<TModel, bool>> where)
        {
            return await dbSet.CountAsync(where);
        }

        public bool Any(Expression<Func<TModel, bool>> where)
        {
            return dbSet.Any(where);
        }
        public async Task<bool> AnyAsync(Expression<Func<TModel, bool>> where)
        {
            return await dbSet.AnyAsync(where);
        }

        #region 根据 条件 查询 和 排序
        /// <summary>
        /// 根据 条件 查询 和 排序
        /// </summary>
        /// <typeparam name="TOrderKey">排序属性类型</typeparam>
        /// <param name="where">查询条件表达式</param>
        /// <param name="orderBy">排序表达式</param>
        /// <returns></returns>
        public IQueryable<TModel> Where<TOrderKey>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TOrderKey>> orderBy, bool isAsc = true)
        {
            if (isAsc)
                return dbSet.Where(where).OrderBy(orderBy);
            else
                return dbSet.Where(where).OrderByDescending(orderBy);
        }
        #endregion

        #region 根据 条件 查询 和 排序，并 使用连接查询
        /// <summary>
        /// 根据 条件 查询 和 排序，并 使用连接查询
        /// </summary>
        /// <typeparam name="TOrderKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <param name="includeNames">要连接查询的 导航属性名称</param>
        /// <returns></returns>
        public IQueryable<TModel> WhereJoin<TOrderKey>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TOrderKey>> orderBy, bool isAsc = true, params string[] includeNames)
        {
            DbQuery<TModel> dbQuery = null;
            //1.先获取 Include 之后的 DBQuery对象
            foreach (string includePropertyName in includeNames)
            {
                //1.1如果 第一次 Include，则调用 dbSet的 Include 方法
                if (dbQuery == null)
                {
                    //将获取的 dbQuery返回，已留作 后面的 链式编程使用
                    dbQuery = dbSet.Include(includePropertyName);
                }
                else
                {
                    //如果之前已经 Include 过了，则直接通过dbQuery完成链式编程
                    dbQuery = dbQuery.Include(includePropertyName);
                }
            }
            //2.加入 查询 和排序条件
            if (isAsc)
                return dbQuery.Where(where).OrderBy(orderBy);
            else
                return dbQuery.Where(where).OrderByDescending(orderBy);
        }
        #endregion


        #region  直接执行 sql语句
        /// <summary>
        /// 直接执行 sql语句
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public int ExcuteNonQuery(string strSql, params System.Data.SqlClient.SqlParameter[] paras)
        {
            return db.Database.ExecuteSqlCommand(strSql, paras);
        }
        #endregion
    }
}
