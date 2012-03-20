#region using directives

using System;
using System.Linq.Expressions;

#endregion

namespace AsciiImportExport
{
    /// <summary>
    // Copied from http://stackoverflow.com/questions/4474634/memberexpression-to-memberexpression
    /// </summary>
    public static class ReflectionHelper
    {
        public static MemberExpression GetMemberExpression<TValue, T>(Expression<Func<T, TValue>> expression)
        {
            if (expression == null)
            {
                return null;
            }
            
            if (expression.Body is MemberExpression)
            {
                return (MemberExpression) expression.Body;
            }
            
            if (expression.Body is UnaryExpression)
            {
                Expression operand = ((UnaryExpression) expression.Body).Operand;
                if (operand is MemberExpression)
                {
                    return (MemberExpression) operand;
                }
                if (operand is MethodCallExpression)
                {
                    return ((MethodCallExpression) operand).Object as MemberExpression;
                }
            }
            
            return null;
        }
    }
}