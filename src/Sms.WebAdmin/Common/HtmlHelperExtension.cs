using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Sms.WebAdmin.Common
{
    public static class HtmlHelperExtension
    {
        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, string id,
            Expression<Func<TModel, TProperty>> province, Expression<Func<TModel, TProperty>> city,
            Expression<Func<TModel, TProperty>> district)
        {
            string pname = GetExpressionFieldName(htmlHelper, province);
            string cname = GetExpressionFieldName(htmlHelper, city);
            string dname = GetExpressionFieldName(htmlHelper, district);
            return MvcHtmlString.Create($"<div id=\"{id}\"><select id=\"{id}_{pname}\"></select><select id=\"{id}_{cname}\"></select><select id=\"{id}_{dname}\"></select></div><script></script>");
        }

        private static string GetExpressionFieldName<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            string fullHtmlFieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            return fullHtmlFieldName.Replace(".", "_");
        }

        private static string GetModelMetadata<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression<TModel, TProperty>(expression, htmlHelper.ViewData);
            object value = metadata.Model;
            if (value != null)
            {
                return value.ToString();
            }
            return string.Empty;
        }
    }
}