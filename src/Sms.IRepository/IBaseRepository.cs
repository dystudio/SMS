using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sms.IRepository
{
    public interface IBaseRepository<TModel> where TModel : class
    {
        /// <summary>
        /// 新增 一个新对象
        /// </summary>
        /// <param name="entity"></param>
        void Add(TModel entity);

        /// <summary>
        /// 删除指定对象
        /// </summary>
        /// <param name="entity"></param>
        void Delete(TModel entity);

        /// <summary>
        /// 根据条件删除
        /// </summary>
        /// <param name="where"></param>
        void DeleteBy(Expression<Func<TModel, bool>> where);

        /// <summary>
        /// 修改指定对象
        /// </summary>
        /// <param name="entity"></param>    
        /// <param name="modifiedPropertyNames">修改了的属性的名字，这些属性的值需要更新到数据库中</param>
        void Modify(TModel entity, params string[] modifiedPropertyNames);

        /// <summary>
        /// 根据条件修改
        /// </summary>
        /// <param name="where">修改条件（哪些要修改）</param>
        /// <param name="modifyPropertyNames">要修改的属性名</param>
        /// <param name="modifyPropertyValues">新的属性值</param>
        void ModifyBy(Expression<Func<TModel, bool>> where, string[] modifyPropertyNames, string[] modifyPropertyValues);


        /// <summary>
        /// 根据条件修改
        /// </summary>
        /// <param name="where">修改条件（哪些要修改）</param>
        /// <param name="modifyPropertyNames">要修改的属性名</param>
        /// <param name="modifyPropertyValues">新的属性值</param>
        void ModifyBy(Expression<Func<TModel, bool>> where, string[] modifyPropertyNames, object[] modifyPropertyValues);


        /// <summary>
        /// 根据 条件 查询单个实体
        /// </summary>
        /// <param name="where">查询条件表达式</param>
        /// <returns></returns>
        TModel Single(Expression<Func<TModel, bool>> where);

        Task<TModel> SingleAsync(Expression<Func<TModel, bool>> where);

        /// <summary>
        /// 根据 条件 查询
        /// </summary>
        /// <param name="where">查询条件表达式</param>
        /// <returns></returns>
        IQueryable<TModel> Where(Expression<Func<TModel, bool>> where);

        /// <summary>
        /// 根据条件查询数量
        /// </summary>
        /// <param name="where">查询条件表达式</param>
        /// <returns></returns>
        int Count(Expression<Func<TModel, bool>> where);

        Task<int> CountAsync(Expression<Func<TModel, bool>> where);

        /// <summary>
        /// 根据 条件 查询 和 排序
        /// </summary>
        /// <typeparam name="TOrderKey">排序属性类型</typeparam>
        /// <param name="where">查询条件表达式</param>
        /// <param name="orderBy">排序表达式</param>
        /// <returns></returns>
        IQueryable<TModel> Where<TOrderKey>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TOrderKey>> orderBy, bool isAsc = true);

        /// <summary>
        ///  根据 条件 查询 和 排序，并 使用连接查询
        /// </summary>
        /// <typeparam name="TOrderKey"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAsc"></param>
        /// <param name="includeNames"></param>
        /// <returns></returns>
        IQueryable<TModel> WhereJoin<TOrderKey>(Expression<Func<TModel, bool>> where, Expression<Func<TModel, TOrderKey>> orderBy, bool isAsc = true, params string[] includeNames);


        /// <summary>
        /// 直接执行 sql语句
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        int ExcuteNonQuery(string strSql, params System.Data.SqlClient.SqlParameter[] paras);
    }
}
