﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using LanguageExt;
using static LanguageExt.Prelude;

namespace LanguageExt
{
    public static partial class List
    {
        /// <summary>
        /// Create an empty IEnumerable<T>
        /// </summary>
        public static Lst<T> empty<T>() =>
            new Lst<T>();

        public static Lst<T> create<T>() =>
            new Lst<T>();

        public static Lst<T> create<T>(params T[] items) =>
            new Lst<T>(items);

        public static Lst<T> createRange<T>(IEnumerable<T> items) =>
            new Lst<T>(items);

        public static Lst<T> add<T>(Lst<T> list, T value) =>
            list.Add(value);

        public static Lst<T> addRange<T>(Lst<T> list, IEnumerable<T> value) =>
            list.AddRange(value);

        public static Lst<T> remove<T>(Lst<T> list, T value) =>
            list.Remove(value);

        public static Lst<T> removeAt<T>(Lst<T> list, int index) =>
            list.RemoveAt(index);

        public static T head<T>(IEnumerable<T> list) => list.First();

        public static Option<T> headOrNone<T>(IEnumerable<T> list) =>
            (from x in list
             select Some(x))
            .DefaultIfEmpty(None)
            .FirstOrDefault();

        public static IEnumerable<T> tail<T>(IEnumerable<T> list) =>
            list.Skip(1);

        public static IEnumerable<R> map<T, R>(IEnumerable<T> list, Func<T, R> map) =>
            list.Select(map);

        public static IEnumerable<R> map<T, R>(IEnumerable<T> list, Func<int, T, R> map) =>
            zip(list, Range(0, Int32.MaxValue), (t, i) => map(i, t));

        public static IEnumerable<T> filter<T>(IEnumerable<T> list, Func<T, bool> predicate) =>
            list.Where(predicate);

        public static IEnumerable<T> choose<T>(IEnumerable<T> list, Func<T, Option<T>> selector) =>
            map(filter(map(list, selector), t => t.IsSome), t => t.Value);

        public static IEnumerable<T> choose<T>(IEnumerable<T> list, Func<int, T, Option<T>> selector) =>
            map(filter(map(list, selector), t => t.IsSome), t => t.Value);

        public static IEnumerable<R> collect<T, R>(IEnumerable<T> list, Func<T, IEnumerable<R>> map) =>
            from t in list
            from r in map(t)
            select r;

        public static int sum(IEnumerable<int> list) => 
            fold(list, 0, (s, x) => s + x);

        public static float sum(IEnumerable<float> list) => 
            fold(list, 0.0f, (s, x) => s + x);

        public static double sum(IEnumerable<double> list) => 
            fold(list, 0.0, (s, x) => s + x);

        public static decimal sum(IEnumerable<decimal> list) => 
            fold(list, (decimal)0, (s, x) => s + x);

        public static IEnumerable<T> rev<T>(IEnumerable<T> list) =>
            list.Reverse();

        public static IEnumerable<T> append<T>(IEnumerable<T> lhs, IEnumerable<T> rhs) =>
            lhs.Concat(rhs);

        public static S fold<S, T>(IEnumerable<T> list, S state, Func<S, T, S> folder)
        {
            foreach (var item in list)
            {
                state = folder(state, item);
            }
            return state;
        }

        public static S foldBack<S, T>(IEnumerable<T> list, S state, Func<S, T, S> folder) =>
            fold(rev(list), state, folder);

        public static T reduce<T>(IEnumerable<T> list, Func<T, T, T> reducer) =>
            match(headOrNone(list),
                Some: x => fold(tail(list), x, reducer),
                None: () => failwith<T>("Input list was empty")
            );

        public static T reduceBack<T>(IEnumerable<T> list, Func<T, T, T> reducer) =>
            reduce(rev(list), reducer);

        public static IEnumerable<S> scan<S, T>(IEnumerable<T> list, S state, Func<S, T, S> folder)
        {
            yield return state;
            foreach (var item in list)
            {
                state = folder(state, item);
                yield return state;
            }
        }

        public static IEnumerable<S> scanBack<S, T>(IEnumerable<T> list, S state, Func<S, T, S> folder) =>
            scan(rev(list), state, folder);

        public static Option<T> find<T>(IEnumerable<T> list, Func<T, bool> pred)
        {
            foreach (var item in list)
            {
                if (pred(item)) return Some(item);
            }
            return None;
        }

        public static Lst<T> freeze<T>(IEnumerable<T> list) =>
            Prelude.toList(list);

        public static IEnumerable<V> zip<T, U, V>(IEnumerable<T> list, IEnumerable<U> other, Func<T, U, V> zipper) =>
            list.Zip(other, zipper);

        public static int length<T>(IEnumerable<T> list) =>
           list.Count();

        public static Unit iter<T>(IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
            return unit;
        }

        public static Unit iter<T>(IEnumerable<T> list, Action<int, T> action)
        {
            int i = 0;
            foreach (var item in list)
            {
                action(i++, item);
            }
            return unit;
        }

        public static bool forall<T>(IEnumerable<T> list, Func<T, bool> pred) =>
            list.All(pred);

        public static IEnumerable<T> distinct<T>(IEnumerable<T> list) =>
            list.Distinct();

        public static IEnumerable<T> distinct<T>(IEnumerable<T> list, Func<T, T, bool> compare) =>
            list.Distinct(new EqCompare<T>(compare));

        public static IEnumerable<T> take<T>(IEnumerable<T> list, int count) =>
            list.Take(count);

        public static IEnumerable<T> takeWhile<T>(IEnumerable<T> list, Func<T,bool> pred) =>
            list.TakeWhile(pred);

        public static IEnumerable<T> takeWhile<T>(IEnumerable<T> list, Func<T, int, bool> pred) =>
            list.TakeWhile(pred);

        public static IEnumerable<S> unfold<S>(S state, Func<S, Option<S>> unfolder)
        {
            while (true)
            {
                yield return state;
                var res = unfolder(state);
                if (res.IsNone)
                {
                    yield break;
                }
                else
                {
                    state = res.Value;
                    yield return res.Value;
                }
            }
        }

        public static IEnumerable<T> unfold<S, T>(S state, Func<S, Option<Tuple<T, S>>> unfolder)
        {
            while (true)
            {
                var res = unfolder(state);
                if (res.IsNone)
                {
                    yield break;
                }
                else
                {
                    state = res.Value.Item2;
                    yield return res.Value.Item1;
                }
            }
        }

        public static IEnumerable<T> unfold<S1, S2, T>(Tuple<S1,S2> state, Func<S1, S2, Option<Tuple<T, S1, S2>>> unfolder)
        {
            while (true)
            {
                var res = unfolder(state.Item1,state.Item2);
                if (res.IsNone)
                {
                    yield break;
                }
                else
                {
                    state = System.Tuple.Create(res.Value.Item2, res.Value.Item3);
                    yield return res.Value.Item1;
                }
            }
        }

        public static IEnumerable<T> unfold<S1, S2, S3, T>(Tuple<S1, S2, S3> state, Func<S1, S2, S3, Option<Tuple<T, S1, S2, S3>>> unfolder)
        {
            while (true)
            {
                var res = unfolder(state.Item1, state.Item2, state.Item3);
                if (res.IsNone)
                {
                    yield break;
                }
                else
                {
                    state = System.Tuple.Create(res.Value.Item2, res.Value.Item3, res.Value.Item4);
                    yield return res.Value.Item1;
                }
            }
        }

        public static IEnumerable<T> unfold<S1, S2, S3, S4, T>(Tuple<S1, S2, S3, S4> state, Func<S1, S2, S3, S4, Option<Tuple<T, S1, S2, S3, S4>>> unfolder)
        {
            while (true)
            {
                var res = unfolder(state.Item1, state.Item2, state.Item3, state.Item4);
                if (res.IsNone)
                {
                    yield break;
                }
                else
                {
                    state = System.Tuple.Create(res.Value.Item2, res.Value.Item3, res.Value.Item4, res.Value.Item5);
                    yield return res.Value.Item1;
                }
            }
        }

        public static bool exists<T>(IEnumerable<T> list, Func<T, bool> pred)
        {
            foreach (var item in list)
            {
                if (pred(item))
                    return true;
            }
            return false;
        }

        [Obsolete("headSafe has been deprecated, please use headOrNone")]
        public static Option<T> headSafe<T>(IEnumerable<T> list) =>
            (from x in list
             select Some(x))
            .DefaultIfEmpty(None)
            .FirstOrDefault();
    }

    class EqCompare<T> : IEqualityComparer<T>
    {
        readonly Func<T, T, bool> compare;

        public EqCompare(Func<T, T, bool> compare)
        {
            this.compare = compare;
        }

        public bool Equals(T x, T y) =>
            x == null && y == null
                ? true
                : x == null || y == null
                    ? false
                    : compare(x, y);

        public int GetHashCode(T obj) =>
            obj == null
                ? 0
                : obj.GetHashCode();
    }
}

public static class __EnumnerableExt
{
    /// <summary>
    /// List matching
    /// </summary>
    public static R Match<T, R>(this IEnumerable<T> list,
        Func<R> Empty,
        Func<T, IEnumerable<T>, R> More
        )
    {
        if (list == null)
        {
            return Empty();
        }
        else
        {
            var head = list.Take(1).ToList();
            var tail = list.Skip(1);

            return head.Count == 0
                ? Empty()
                : More(head.First(), tail);
        }
    }


    /// <summary>
    /// List matching
    /// </summary>
    public static R Match<T, R>(this IEnumerable<T> list,
        Func<R> Empty,
        Func<T, R> One,
        Func<T, IEnumerable<T>, R> More
        )
    {
        if (list == null)
        {
            return Empty();
        }
        else
        {
            var head = list.Take(1).ToList();
            var tail = list.Skip(1);

            return head.Count == 0
                ? Empty()
                : tail.Take(1).Count() == 0
                    ? One(head.First())
                    : More(head.First(), tail);
        }
    }

    /// <summary>
    /// List matching
    /// TODO: Optimise these, too many repeated pulls from the IEnumerable
    /// </summary>
    public static R Match<T, R>(this IEnumerable<T> list,
        Func<R> Empty,
        Func<T, R> One,
        Func<T, T, R> Two,
        Func<T, IEnumerable<T>, R> More
        ) =>
        Match(list, Empty, One, (x, xs) =>
            xs.Take(2).Count() == 1
                ? Two(x, xs.First())
                : More(x, xs)
        );

    /// <summary>
    /// List matching
    /// TODO: Optimise these, too many repeated pulls from the IEnumerable
    /// </summary>
    public static R Match<T, R>(this IEnumerable<T> list,
        Func<R> Empty,
        Func<T, R> One,
        Func<T, T, R> Two,
        Func<T, T, T, R> Three,
        Func<T, IEnumerable<T>, R> More
        ) =>
        Match(list, Empty, One, Two, (x, xs) =>
            xs.Take(3).Count() < 3
                ? Three(x, xs.First(), xs.Skip(1).First())
                : More(x, xs)
        );

    /// <summary>
    /// List matching
    /// TODO: Optimise these, too many repeated pulls from the IEnumerable
    /// </summary>
    public static R Match<T, R>(this IEnumerable<T> list,
        Func<R> Empty,
        Func<T, R> One,
        Func<T, T, R> Two,
        Func<T, T, T, R> Three,
        Func<T, T, T, T, R> Four,
        Func<T, IEnumerable<T>, R> More
        ) =>
        Match(list, Empty, One, Two, Three, (x, xs) =>
            xs.Take(4).Count() < 4
                ? Four(x, xs.First(), xs.Skip(1).First(), xs.Skip(2).First())
                : More(x, xs)
        );

    /// <summary>
    /// List matching
    /// TODO: Optimise these, too many repeated pulls from the IEnumerable
    /// </summary>
    public static R Match<T, R>(this IEnumerable<T> list,
        Func<R> Empty,
        Func<T, R> One,
        Func<T, T, R> Two,
        Func<T, T, T, R> Three,
        Func<T, T, T, T, R> Four,
        Func<T, T, T, T, T, R> Five,
        Func<T, IEnumerable<T>, R> More
        ) =>
        Match(list, Empty, One, Two, Three, Four, (x, xs) =>
            xs.Take(5).Count() < 5
                ? Five(x, xs.First(), xs.Skip(1).First(), xs.Skip(2).First(), xs.Skip(3).First())
                : More(x, xs)
        );

    /// <summary>
    /// List matching
    /// TODO: Optimise these, too many repeated pulls from the IEnumerable
    /// </summary>
    public static R Match<T, R>(this IEnumerable<T> list,
        Func<R> Empty,
        Func<T, R> One,
        Func<T, T, R> Two,
        Func<T, T, T, R> Three,
        Func<T, T, T, T, R> Four,
        Func<T, T, T, T, T, R> Five,
        Func<T, T, T, T, T, T, R> Six,
        Func<T, IEnumerable<T>, R> More
        ) =>
        Match(list, Empty, One, Two, Three, Four, Five, (x, xs) =>
            xs.Take(6).Count() < 6
                ? Six(x, xs.First(), xs.Skip(1).First(), xs.Skip(2).First(), xs.Skip(3).First(), xs.Skip(4).First())
                : More(x, xs)
        );

    public static T Head<T>(this IEnumerable<T> list) =>
        LanguageExt.List.head(list);

    [Obsolete("HeadSafe has been deprecated, please use HeadOrNone")]
    public static Option<T> HeadSafe<T>(this IEnumerable<T> list) =>
        LanguageExt.List.headOrNone(list);

    public static Option<T> HeadOrNone<T>(this IEnumerable<T> list) =>
        LanguageExt.List.headOrNone(list);

    public static IEnumerable<T> Tail<T>(this IEnumerable<T> list) =>
        LanguageExt.List.tail(list);

    public static IEnumerable<R> Map<T, R>(this IEnumerable<T> list, Func<T, R> map) =>
        LanguageExt.List.map(list, map);

    public static IEnumerable<R> Map<T, R>(this IEnumerable<T> list, Func<int, T, R> map) =>
        LanguageExt.List.map(list, map);

    public static IEnumerable<T> Filter<T>(this IEnumerable<T> list, Func<T, bool> predicate) =>
        LanguageExt.List.filter(list, predicate);

    public static IEnumerable<T> Choose<T>(this IEnumerable<T> list, Func<T, Option<T>> selector) =>
        LanguageExt.List.choose(list, selector);

    public static IEnumerable<T> Choose<T>(this IEnumerable<T> list, Func<int, T, Option<T>> selector) =>
        LanguageExt.List.choose(list, selector);

    public static IEnumerable<R> Collect<T, R>(this IEnumerable<T> list, Func<T, IEnumerable<R>> map) =>
        LanguageExt.List.collect(list, map);

    public static IEnumerable<T> Rev<T>(this IEnumerable<T> list) =>
        LanguageExt.List.rev(list);

    public static IEnumerable<T> Append<T>(this IEnumerable<T> lhs, IEnumerable<T> rhs) =>
        LanguageExt.List.append(lhs, rhs);

    public static S Fold<S, T>(this IEnumerable<T> list, S state, Func<S, T, S> folder) =>
        LanguageExt.List.fold(list, state, folder);

    public static S FoldBack<S, T>(this IEnumerable<T> list, S state, Func<S, T, S> folder) =>
        LanguageExt.List.foldBack(list, state, folder);

    public static T Reduce<T>(this IEnumerable<T> list, Func<T, T, T> reducer) =>
        LanguageExt.List.reduce(list, reducer);

    public static T ReduceBack<T>(this IEnumerable<T> list, Func<T, T, T> reducer) =>
        LanguageExt.List.reduceBack(list, reducer);

    public static IEnumerable<S> Scan<S, T>(this IEnumerable<T> list, S state, Func<S, T, S> folder) =>
        LanguageExt.List.scan(list, state, folder);

    public static IEnumerable<S> ScanBack<S, T>(this IEnumerable<T> list, S state, Func<S, T, S> folder) =>
        LanguageExt.List.scanBack(list, state, folder);

    public static Option<T> Find<T>(this IEnumerable<T> list, Func<T, bool> pred) =>
        LanguageExt.List.find(list, pred);

    public static Lst<T> Freeze<T>(this IEnumerable<T> list) =>
        LanguageExt.List.freeze(list);

    public static int Length<T>(this IEnumerable<T> list) =>
        LanguageExt.List.length(list);

    public static Unit Iter<T>(this IEnumerable<T> list, Action<T> action) =>
        LanguageExt.List.iter(list, action);

    public static Unit Iter<T>(this IEnumerable<T> list, Action<int, T> action) =>
        LanguageExt.List.iter(list, action);

    public static bool ForAll<T>(this IEnumerable<T> list, Func<T, bool> pred) =>
        LanguageExt.List.forall(list, pred);

    public static IEnumerable<T> Distinct<T>(this IEnumerable<T> list, Func<T, T, bool> compare) =>
        LanguageExt.List.distinct(list, compare);

    public static bool Exists<T>(this IEnumerable<T> list, Func<T, bool> pred) =>
        LanguageExt.List.exists(list, pred);

    public static IEnumerable<R> Bind<T, R>(this IEnumerable<T> self, Func<T, IEnumerable<R>> binder) =>
        self.SelectMany(x => binder(x));

    public static IEnumerable<V> SelectMany<T, U, V>(this IEnumerable<T> self, Func<T, Lst<U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            foreach (var u in bind(t))
            {
                yield return project(t, u);
            }
        }
    }

    public static IEnumerable<V> SelectMany<K, T, U, V>(this IEnumerable<T> self, Func<T, Map<K, U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            foreach (var u in bind(t).Values)
            {
                yield return project(t, u);
            }
        }
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this IEnumerable<T> self, Func<T, Option<U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t);
            if( resU.IsSome )
            {
                yield return project(t, resU.Value);
            }
        }
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this IEnumerable<T> self, Func<T, OptionUnsafe<U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t);
            if (resU.IsSome)
            {
                yield return project(t, resU.Value);
            }
        }
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this IEnumerable<T> self, Func<T, Try<U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t).Try();
            if (!resU.IsFaulted)
            {
                yield return project(t, resU.Value);
            }
        }
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this IEnumerable<T> self, Func<T, TryOption<U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t).Try();
            if (!resU.IsFaulted && resU.Value.IsSome)
            {
                yield return project(t, resU.Value.Value);
            }
        }
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this IEnumerable<T> self, Func<T, Reader<T, U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t)(t);
            if (!resU.IsBottom)
            {
                yield return project(t, resU.Value);
            }
        }
    }

    public static Writer<Out, IEnumerable<V>> SelectMany<Out, T, U, V>(this IEnumerable<T> self, Func<T, Writer<Out, U>> bind, Func<T, U, V> project)
    {
        return () =>
        {
            var res = new System.Collections.Generic.List<V>();
            foreach (var t in self)
            {
                var resU = bind(t)();
                if (!resU.IsBottom)
                {
                    res.Add(project(t, resU.Value));
                }
            }
            return res;
        };
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this IEnumerable<T> self, Func<T, State<T, U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t)(t);
            if (!resU.IsBottom)
            {
                yield return project(t, resU.Value);
            }
        }
    }




    public static IEnumerable<V> SelectMany<T, U, V>(this Lst<T> self, Func<T, Lst<U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            foreach (var u in bind(t))
            {
                yield return project(t, u);
            }
        }
    }

    public static IEnumerable<V> SelectMany<K, T, U, V>(this Lst<T> self, Func<T, Map<K, U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            foreach (var u in bind(t).Values)
            {
                yield return project(t, u);
            }
        }
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this Lst<T> self, Func<T, Option<U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t);
            if (resU.IsSome)
            {
                yield return project(t, resU.Value);
            }
        }
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this Lst<T> self, Func<T, OptionUnsafe<U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t);
            if (resU.IsSome)
            {
                yield return project(t, resU.Value);
            }
        }
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this Lst<T> self, Func<T, Try<U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t).Try();
            if (!resU.IsFaulted)
            {
                yield return project(t, resU.Value);
            }
        }
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this Lst<T> self, Func<T, TryOption<U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t).Try();
            if (!resU.IsFaulted && resU.Value.IsSome)
            {
                yield return project(t, resU.Value.Value);
            }
        }
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this Lst<T> self, Func<T, Reader<T, U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t)(t);
            if (!resU.IsBottom)
            {
                yield return project(t, resU.Value);
            }
        }
    }

    public static Reader<Env, IEnumerable<V>> SelectMany<Env, T, U, V>(this Lst<T> self, Func<T, Reader<Env, U>> bind, Func<T, U, V> project)
    {
        return env =>
        {
            var res = new System.Collections.Generic.List<V>();
            foreach (var t in self)
            {
                var resU = bind(t)(env);
                if (!resU.IsBottom)
                {
                    res.Add(project(t, resU.Value));
                }
            }
            return res;
        };
    }

    public static Writer<Out, IEnumerable<V>> SelectMany<Out, T, U, V>(this Lst<T> self, Func<T, Writer<Out, U>> bind, Func<T, U, V> project)
    {
        return () =>
        {
            var res = new System.Collections.Generic.List<V>();
            foreach (var t in self)
            {
                var resU = bind(t)();
                if (!resU.IsBottom)
                {
                    res.Add(project(t, resU.Value));
                }
            }
            return res;
        };
    }

    public static IEnumerable<V> SelectMany<T, U, V>(this Lst<T> self, Func<T, State<T, U>> bind, Func<T, U, V> project)
    {
        foreach (var t in self)
        {
            var resU = bind(t)(t);
            if (!resU.IsBottom)
            {
                yield return project(t, resU.Value);
            }
        }
    }

    public static State<S, IEnumerable<V>> SelectMany<S, T, U, V>(this Lst<T> self, Func<T, State<S, U>> bind, Func<T, U, V> project)
    {
        return state =>
        {
            var res = new System.Collections.Generic.List<V>();
            foreach (var t in self)
            {
                var resU = bind(t)(state);
                if (!resU.IsBottom)
                {
                    res.Add(project(t, resU.Value));
                }
            }
            return res;
        };
    }

}
