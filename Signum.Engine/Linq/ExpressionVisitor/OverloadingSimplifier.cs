﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Signum.Entities;
using Signum.Utilities.ExpressionTrees;
using Signum.Utilities.Reflection;
using Signum.Utilities;
using Signum.Entities.Reflection;

namespace Signum.Engine.Linq
{


    /// <summary>
    ///  returns the set of all aliases produced by a query source
    /// </summary>
    internal class OverloadingSimplifier : SimpleExpressionVisitor
    {
        static MethodInfo miSelectQ = ReflectionTools.GetMethodInfo(() => Queryable.Select((IQueryable<string>)null, s => s)).GetGenericMethodDefinition();
        static MethodInfo miSelectE = ReflectionTools.GetMethodInfo(() => Enumerable.Select((IEnumerable<string>)null, s => s)).GetGenericMethodDefinition();

        static MethodInfo miGroupBySQ = ReflectionTools.GetMethodInfo(() => Queryable.GroupBy((IQueryable<string>)null, s => s)).GetGenericMethodDefinition();
        static MethodInfo miGroupBySE = ReflectionTools.GetMethodInfo(() => Enumerable.GroupBy((IEnumerable<string>)null, s => s)).GetGenericMethodDefinition();

        static MethodInfo miGroupByNQ = ReflectionTools.GetMethodInfo(() => Queryable.GroupBy((IQueryable<string>)null, s => s, s => s)).GetGenericMethodDefinition();
        static MethodInfo miGroupByNE = ReflectionTools.GetMethodInfo(() => Queryable.GroupBy((IQueryable<string>)null, s => s, s => s)).GetGenericMethodDefinition();

        static MethodInfo miGroupBySRQ = ReflectionTools.GetMethodInfo(() => Queryable.GroupBy((IQueryable<string>)null, s => s, (s,g)=>s)).GetGenericMethodDefinition();
        static MethodInfo miGroupBySRE = ReflectionTools.GetMethodInfo(() => Enumerable.GroupBy((IEnumerable<string>)null, s => s, (s, g) => s)).GetGenericMethodDefinition();

        static MethodInfo miGroupByNRQ = ReflectionTools.GetMethodInfo(() => Queryable.GroupBy((IQueryable<string>)null, s => s, s => s, (s, g) => s)).GetGenericMethodDefinition();
        static MethodInfo miGroupByNRE = ReflectionTools.GetMethodInfo(() => Queryable.GroupBy((IQueryable<string>)null, s => s, s => s, (s, g) => s)).GetGenericMethodDefinition();

        static MethodInfo miGroupJoinQ = ReflectionTools.GetMethodInfo(() => Queryable.GroupJoin((IQueryable<string>)null, (IQueryable<string>)null, a => a, a => a, (a, g) => a)).GetGenericMethodDefinition();
        static MethodInfo miGroupJoinE = ReflectionTools.GetMethodInfo(() => Enumerable.GroupJoin((IEnumerable<string>)null, (IEnumerable<string>)null, a => a, a => a, (a, g) => a)).GetGenericMethodDefinition();

        static MethodInfo miJoinQ = ReflectionTools.GetMethodInfo(() => Queryable.Join((IQueryable<string>)null, (IQueryable<string>)null, a => a, a => a, (a, g) => a)).GetGenericMethodDefinition();
        static MethodInfo miJoinE = ReflectionTools.GetMethodInfo(() => Enumerable.Join((IEnumerable<string>)null, (IEnumerable<string>)null, a => a, a => a, (a, g) => a)).GetGenericMethodDefinition();

        static MethodInfo miDefaultIfEmptyQ = ReflectionTools.GetMethodInfo(() => Queryable.DefaultIfEmpty<int>(null)).GetGenericMethodDefinition();
        static MethodInfo miDefaultIfEmptyE = ReflectionTools.GetMethodInfo(() => Enumerable.DefaultIfEmpty<int>(null)).GetGenericMethodDefinition();

        static MethodInfo miCountQ = ReflectionTools.GetMethodInfo(() => Queryable.Count((IQueryable<string>)null)).GetGenericMethodDefinition();
        static MethodInfo miCountE = ReflectionTools.GetMethodInfo(() => Enumerable.Count((IEnumerable<string>)null)).GetGenericMethodDefinition();

        static MethodInfo miCount2Q = ReflectionTools.GetMethodInfo(() => Queryable.Count((IQueryable<string>)null, null)).GetGenericMethodDefinition();
        static MethodInfo miCount2E = ReflectionTools.GetMethodInfo(() => Enumerable.Count((IEnumerable<string>)null, null)).GetGenericMethodDefinition();

        static MethodInfo miWhereQ = ReflectionTools.GetMethodInfo(() => Queryable.Where((IQueryable<string>)null, a => false)).GetGenericMethodDefinition();
        static MethodInfo miWhereE = ReflectionTools.GetMethodInfo(() => Enumerable.Where((IEnumerable<string>)null, a=>false)).GetGenericMethodDefinition();

        static MethodInfo miWhereIndexQ = ReflectionTools.GetMethodInfo(() => Queryable.Where((IQueryable<string>)null, (a, i) => false)).GetGenericMethodDefinition();
        static MethodInfo miWhereIndexE = ReflectionTools.GetMethodInfo(() => Enumerable.Where((IEnumerable<string>)null, (a, i) => false)).GetGenericMethodDefinition();

        static MethodInfo miContainsQ = ReflectionTools.GetMethodInfo(() => Queryable.Contains((IQueryable<string>)null, null)).GetGenericMethodDefinition();
        static MethodInfo miContainsE = ReflectionTools.GetMethodInfo(() => Enumerable.Contains((IEnumerable<string>)null, null)).GetGenericMethodDefinition();

        static MethodInfo miElementAtQ = ReflectionTools.GetMethodInfo(() => Queryable.ElementAt((IQueryable<string>)null, 0)).GetGenericMethodDefinition();
        static MethodInfo miElementAtE = ReflectionTools.GetMethodInfo(() => Enumerable.ElementAt((IEnumerable<string>)null, 0)).GetGenericMethodDefinition();

        static MethodInfo miElementAtOrDefaultQ = ReflectionTools.GetMethodInfo(() => Queryable.ElementAtOrDefault((IQueryable<string>)null, 0)).GetGenericMethodDefinition();
        static MethodInfo miElementAtOrDefaultE = ReflectionTools.GetMethodInfo(() => Enumerable.ElementAtOrDefault((IEnumerable<string>)null, 0)).GetGenericMethodDefinition();

        static MethodInfo miSkipQ = ReflectionTools.GetMethodInfo(() => Queryable.Skip((IQueryable<string>)null, 0)).GetGenericMethodDefinition();
        static MethodInfo miSkipE = ReflectionTools.GetMethodInfo(() => Enumerable.Skip((IEnumerable<string>)null, 0)).GetGenericMethodDefinition();

        static MethodInfo miTakeQ = ReflectionTools.GetMethodInfo(() => Queryable.Take((IQueryable<string>)null, 0)).GetGenericMethodDefinition();
        static MethodInfo miTakeE = ReflectionTools.GetMethodInfo(() => Enumerable.Take((IEnumerable<string>)null, 0)).GetGenericMethodDefinition();

        static MethodInfo miFirstQ = ReflectionTools.GetMethodInfo(() => Queryable.First((IQueryable<string>)null)).GetGenericMethodDefinition();
        static MethodInfo miFirstE = ReflectionTools.GetMethodInfo(() => Enumerable.First((IEnumerable<string>)null)).GetGenericMethodDefinition();

        static MethodInfo miFirstOrDefaultQ = ReflectionTools.GetMethodInfo(() => Queryable.FirstOrDefault((IQueryable<string>)null)).GetGenericMethodDefinition();
        static MethodInfo miFirstOrDefaultE = ReflectionTools.GetMethodInfo(() => Enumerable.FirstOrDefault((IEnumerable<string>)null)).GetGenericMethodDefinition();

        static int i = 0; 

        public static Expression Simplify(Expression expression)
        {
            return new OverloadingSimplifier().Visit(expression); 
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            Type decType = m.Method.DeclaringType; 
            if (m.Method.IsGenericMethod && (decType == typeof(Queryable) || decType == typeof(Enumerable)))
            {
                bool query = decType == typeof(Queryable);

                Type[] paramTypes = m.Method.GetGenericArguments();
                MethodInfo mi = m.Method.GetGenericMethodDefinition();

                //IE<IGrouping<K, S>> GroupBy<S, K>(this IE<S> source, Func<S, K> keySelector);
                //    GroupBy(col, a=>func(a)) -> GroupBy(col, a=>func(a), a=>a) 

                if (ReflectionTools.MethodEqual(mi, miGroupBySE) || ReflectionTools.MethodEqual(mi, miGroupBySQ))
                {
                    var source = Visit(m.GetArgument("source"));
                    var keySelector = (LambdaExpression)Visit(m.GetArgument("keySelector").StripQuotes());

                    MethodInfo miG = (query ? miGroupByNQ : miGroupByNE)
                        .MakeGenericMethod(paramTypes[0], paramTypes[1], paramTypes[0]);

                    ParameterExpression p = Expression.Parameter(paramTypes[0], "p" + i++);

                    return Expression.Call(miG, source, keySelector, Expression.Lambda(p, p));
                }

                //IE<R> GroupBy<S, K, R>(this IE<S> source, Func<S, K> keySelector, Func<K, IE<S>, R> resultSelector);
                //    GroupBy(col, a=>f1(a), a=>f2(a), (a,B)=>f3(a,B)) -> GroupBy(col, a=>f1(a), a=>f2(a)).Select(g=>=>f3(g.Key,g))  
                      
                if (ReflectionTools.MethodEqual(mi, miGroupBySRE) || ReflectionTools.MethodEqual(mi, miGroupBySRQ))
                {
                    var source = Visit(m.GetArgument("source"));
                    var keySelector = (LambdaExpression)Visit(m.GetArgument("keySelector").StripQuotes());
                    var resultSelector = (LambdaExpression)Visit(m.GetArgument("resultSelector").StripQuotes());

                    Type groupingType = typeof(IGrouping<,>).MakeGenericType(paramTypes[1], paramTypes[0]);

                    MethodInfo miG = (query ? miGroupByNQ : miGroupByNE)
                        .MakeGenericMethod(paramTypes[0], paramTypes[1], paramTypes[0]);

                    MethodInfo miS = (query ? miSelectQ : miSelectE)
                        .MakeGenericMethod(groupingType, paramTypes[2]);

                    ParameterExpression g = Expression.Parameter(groupingType, "g" + i++);

                    LambdaExpression newResult =
                        Expression.Lambda(
                            Replacer.Replace(Replacer.Replace(resultSelector.Body,
                            resultSelector.Parameters[0], Expression.MakeMemberAccess(g, groupingType.GetProperty("Key"))),
                             resultSelector.Parameters[1], g),
                        g);


                    ParameterExpression p = Expression.Parameter(paramTypes[0], "p" + i++);
                    return
                        Expression.Call(miS,
                            Expression.Call(miG, source, keySelector, Expression.Lambda(p, p)),
                            newResult);
                }

                //IE<R> GroupBy<S, K, E, R>(this IE<S> source, Func<S, K> keySelector, Func<S, E> elementSelector, Func<K, IE<E>, R> resultSelector)
                //    GroupBy(col, a=>f1(a), a=>f2(a), (k,B)=>f(k,B)) -> GroupBy(col, a=>f1(a), a=>f2(a)).Select(g=>f3(g.Key,g))


                if (ReflectionTools.MethodEqual(mi, miGroupByNRE) || ReflectionTools.MethodEqual(mi, miGroupByNRQ))
                {
                    var source = Visit(m.GetArgument("source"));
                    var keySelector = (LambdaExpression)Visit(m.GetArgument("keySelector").StripQuotes());
                    var elementSelector = (LambdaExpression)Visit(m.GetArgument("elementSelector").StripQuotes());
                    var resultSelector = (LambdaExpression)Visit(m.GetArgument("resultSelector").StripQuotes());

                    Type groupingType = typeof(IGrouping<,>).MakeGenericType(paramTypes[1], paramTypes[2]);

                    MethodInfo miG = (query ? miGroupByNQ : miGroupByNE)
                        .MakeGenericMethod(paramTypes[0], paramTypes[1], paramTypes[2]);

                    MethodInfo miS = (query ? miSelectQ : miSelectE)
                        .MakeGenericMethod(groupingType, paramTypes[3]);

                    ParameterExpression g = Expression.Parameter(groupingType, "g" + i++);

                    LambdaExpression newResult =
                        Expression.Lambda(
                            Replacer.Replace(Replacer.Replace(resultSelector.Body,
                            resultSelector.Parameters[0], Expression.MakeMemberAccess(g, groupingType.GetProperty("Key"))),
                            resultSelector.Parameters[1], g),
                        g);

                    return
                        Expression.Call(miS,
                            Expression.Call(miG, source, keySelector, elementSelector),
                            newResult);
                }

                //IE<R> GroupJoin<O, I, K, R>(this IE<O> outer, IE<I> inner, Func<O, K> outerKeySelector, Func<I, K> innerKeySelector, Func<O, IE<I>, R> resultSelector)
                //    GroupJoin(outer, inner, o=>f1(o), i=>f2
                //(i), (o, gI)=>f3(o,gI)) --> 

                //      Join(outer, GroupBy(inner, i=>f2(i), i=>i) , o=>f1(o), g=>g.Key, (o,g)=>f2(o, g))							


                if (ReflectionTools.MethodEqual(mi, miGroupJoinE) || ReflectionTools.MethodEqual(mi, miGroupJoinQ))
                {
                    Type tO = paramTypes[0], tI = paramTypes[1], tK = paramTypes[2], tR = paramTypes[3];

                    var outer = Visit(m.GetArgument("outer"));
                    var inner = Visit(m.GetArgument("inner"));

                    bool hasDefaultIfEmpty = ExtractDefaultIfEmpty(ref inner); 

                    var outerKeySelector = (LambdaExpression)Visit(m.GetArgument("outerKeySelector").StripQuotes());
                    var innerKeySelector = (LambdaExpression)Visit(m.GetArgument("innerKeySelector").StripQuotes());
                    var resultSelector = (LambdaExpression)Visit(m.GetArgument("resultSelector").StripQuotes());

                    Type groupingType = typeof(IGrouping<,>).MakeGenericType(tK, tI);

                    MethodInfo miG = (query ? miGroupByNQ : miGroupByNQ)
                        .MakeGenericMethod(tI,tK, tI);

                    ParameterExpression p = Expression.Parameter(tI, "p" + i++);
                    Expression group = Expression.Call(miG, inner, innerKeySelector, Expression.Lambda(p, p));

                    if (hasDefaultIfEmpty)
                    {
                        var method = (query ? miDefaultIfEmptyQ : miDefaultIfEmptyE)
                            .MakeGenericMethod(groupingType); 
                     
                        group = Expression.Call(method, group);
                    }

                    //IQueryable<R> Join<TOuter, TInner, TKey, R>(this IQueryable<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, R>> resultSelector);

                    MethodInfo mij = (query ? miJoinQ : miJoinE)
                        .MakeGenericMethod(tO, groupingType, tK, tR);

                    ParameterExpression g = Expression.Parameter(groupingType, "g" + i++);
                    LambdaExpression newResult =
                        Expression.Lambda(
                            Replacer.Replace(resultSelector.Body, resultSelector.Parameters[1], g),
                        resultSelector.Parameters[0],  g);


                    return
                        Expression.Call(mij, outer, group, outerKeySelector, 
                            Expression.Lambda(Expression.MakeMemberAccess(g, groupingType.GetProperty("Key")), g),
                            newResult);
                }

           
                if (ReflectionTools.MethodEqual(mi, miCount2E) || ReflectionTools.MethodEqual(mi, miCount2Q))
                {
                    var source = Visit(m.GetArgument("source"));
                    var predicate = (LambdaExpression)Visit(m.GetArgument("predicate").StripQuotes());

                    MethodInfo mWhere = (query ? miWhereQ : miWhereE).MakeGenericMethod(paramTypes[0]);
                    MethodInfo mCount = (query ? miCountQ : miCountE).MakeGenericMethod(paramTypes[0]);
                    
                    return Expression.Call(mCount, Expression.Call(mWhere, source, predicate)); 
                }                


                if (ReflectionTools.MethodEqual(mi, miElementAtE) || ReflectionTools.MethodEqual(mi, miElementAtOrDefaultE) ||
                   ReflectionTools.MethodEqual(mi, miElementAtQ) || ReflectionTools.MethodEqual(mi, miElementAtOrDefaultQ))
                {
                    bool def = ReflectionTools.MethodEqual(mi, miElementAtOrDefaultE) || ReflectionTools.MethodEqual(mi, miElementAtOrDefaultQ);

                    var source = Visit(m.GetArgument("source"));
                    var index = Visit(m.GetArgument("index"));

                    MethodInfo first = (def ? (query ? miFirstOrDefaultQ : miFirstOrDefaultE) : 
                                       (query ? miFirstQ : miFirstE)).MakeGenericMethod(paramTypes[0]);

                    MethodInfo skip = (query ? miSkipQ : miSkipE).MakeGenericMethod(paramTypes[0]);
                    return Visit(Expression.Call(first, Expression.Call(skip, source, index)));
                }


                if(ReflectionTools.MethodEqual(mi, miSkipE) ||ReflectionTools.MethodEqual(mi, miSkipQ))
                {
                    var source = Visit(m.GetArgument("source"));
                    var count = Visit(m.GetArgument("count"));

                    ParameterExpression pi = Expression.Parameter(typeof(int), "i"); 
                    ParameterExpression pa = Expression.Parameter(paramTypes[0], "a"); 
                    Expression lambda = Expression.Lambda(Expression.LessThanOrEqual(count, pi), pa, pi);

                    MethodInfo miWhereIndex = (query ? miWhereIndexQ : miWhereIndexE).MakeGenericMethod(paramTypes[0]);

                    return Expression.Call(miWhereIndex, source, lambda); 
                }


                if (ReflectionTools.MethodEqual(mi, miTakeE) || ReflectionTools.MethodEqual(mi, miTakeQ))
                {
                    var m2 = m.GetArgument("source") as MethodCallExpression;

                    if(m2 != null)
                    {
                        var mi2 = (((MethodCallExpression)m2).Method).GetGenericMethodDefinition();

                        if(ReflectionTools.MethodEqual(mi2, miSkipE) ||ReflectionTools.MethodEqual(mi2, miSkipQ))
                        {

                            var source = Visit(m2.GetArgument("source"));
                            var skip = Visit(m2.GetArgument("count"));
                            var take = Visit(m.GetArgument("count"));

                            ParameterExpression pi = Expression.Parameter(typeof(int), "i");
                            ParameterExpression pa = Expression.Parameter(paramTypes[0], "a");
                            Expression lambda = Expression.Lambda(
                                Expression.And(
                                    Expression.LessThanOrEqual(skip, pi),
                                    Expression.LessThan(pi, Expression.Add(skip, take))
                                ), pa, pi);

                            MethodInfo miWhereIndex = (query ? miWhereIndexQ : miWhereIndexE).MakeGenericMethod(paramTypes[0]);

                            return Expression.Call(miWhereIndex, source, lambda); 

                        }                       

                    }
                    
                }

            }

            if (m.Method.DeclaringType == typeof(Tuple) && m.Method.Name == "Create")
            {
                var types = m.Arguments.Select(e => e.Type).ToArray();
                if (types.Length < 8)
                {
                    return Expression.New(m.Method.ReturnType.GetConstructor(types), m.Arguments.ToArray());
                }
                else
                {
                    Type lastType = types[7];
                    types[7] = typeof(Tuple<>).MakeGenericType(lastType);

                    return Expression.New(m.Method.ReturnType.GetConstructor(types), m.Arguments.Take(7).And(
                        Expression.New(types[7].GetConstructor(new[] { lastType }), m.Arguments[7])).ToArray());
                }
            }

            return base.VisitMethodCall(m); 
        }


        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.Type.IsGenericType && m.Expression.Type.GetGenericTypeDefinition() == typeof(MList<>) && m.Member is PropertyInfo && m.Member.Name == "Count")
            {
                Type[] paramTypes = m.Expression.Type.GetGenericArguments();

                MethodInfo mCount = (miCountE).MakeGenericMethod(paramTypes[0]);

                var source = Visit(m.Expression);

                return Expression.Call(mCount, source);
            }

            return base.VisitMemberAccess(m);
        }

        public static bool ExtractDefaultIfEmpty(ref Expression expression)
        {
            MethodCallExpression mce = expression as MethodCallExpression;

            if (mce == null || !mce.Method.IsGenericMethod)
                return false;

            MethodInfo me = mce.Method.GetGenericMethodDefinition();

            if (!ReflectionTools.MethodEqual(me, miDefaultIfEmptyE) && !ReflectionTools.MethodEqual(me, miDefaultIfEmptyQ))
                return false;

            expression = mce.GetArgument("source");
            return true;
        }

        public static bool ExtractDefaultIfEmpty(ref LambdaExpression lambda)
        {
            Expression body = lambda.Body;
            if (ExtractDefaultIfEmpty(ref body))
            {
                lambda = Expression.Lambda(body, lambda.Parameters); 
                return true; 
            }
            return false;
        }
    }
}
