using System;
using System.Collections.Generic;

namespace Extensions
{
    public static class ListExtensions
    {
        public static T Peek<T>(this IList<T> list)
        {
            return list[list.Count - 1];
        }

        public static T Pop<T>(this List<T> list)
        {
            int index = list.Count - 1;
            var item = list[index];
            list.RemoveAt(index);
            return item;
        }

        public static void Push<T>(this List<T> list, T item)
        {
            list.Add(item);
        }

        public static T CreateAndAdd<T>(this List<T> list) where T : new()
        {
            var n = new T();
            list.Add(n);
            return n;
        }

        public static T GetOrCreate<T>(this List<T> list, int index) where T : class, new()
        {
            if (index >= 0)
            {
                if (index < list.Count)
                {
                    T value = list[index];
                    if (value == null)
                    {
                        value = new T();
                        list[index] = value;
                    }
                    return value;
                }
                else
                {
                    T value = new T();
                    list.SetAt(index, value);
                    return value;
                }
            }
            // Hooow could this haaapen.
            throw new IndexOutOfRangeException();
        }

        public static void SetAt<T>(this List<T> list, int index, T item)
        {
            int requiredSize = index + 1;
            if (list.Capacity < requiredSize)
                list.Capacity = requiredSize;

            for (int i = list.Count; i < requiredSize; i++)
                list.Add(default(T));

            list[index] = item;
        }

        public static void SetAtValidIndex<T>(this IList<T> list, int index, T item)
        {
            if (list.IsValidIndex(index))
                list[index] = item;
        }

        // Give a sequence of sort functions to iteratively be used in a List.Sort, if a comparison function returns 0,
        // it continues to the next function until it completes or finds a comparison that returns nonzero
        //
        // Usage:
        // units.SortSequence((left, right) => left.squadSlot.CompareTo(right.squadSlot),
        //     (left, right) => -1 * left.Rank.CompareTo(right.Rank));
        public static void SortSequence<T>(this List<T> list, params Func<T, T, int>[] funcs)
        {
            list.Sort((left, right) =>
            {
                int result = 0;

                for (int i = 0; i < funcs.Length; ++i)
                {
                    Func<T, T, int> currFunc = funcs[i];

                    if (currFunc != null)
                    {
                        result = currFunc(left, right);

                        if (result != 0)
                            break;
                    }
                }

                return result;
            });
        }

        public static T FirstOrDefaultNoLinq<T>(this IList<T> list, T defaultValue = default(T))
        {
            return list != null && list.Count > 0 ? list[0] : defaultValue;
        }

        public static T SecondOrDefaultNoLinq<T>(this IList<T> list, T defaultValue = default(T))
        {
            return list != null && list.Count > 1 ? list[1] : defaultValue;
        }

        // Equivalent of List.Find without using Linq
        public static T FindNoLinq<T>(this IList<T> list, Func<T, bool> predicate)
        {
            if (predicate != null && list != null)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    T element = list[i];
                    if (element != null)
                    {
                        if (predicate(element))
                            return element;
                    }
                }
            }

            return default(T);
        }

        public static int FindLastIndexNoLinq<T>(this IList<T> list, Func<T, bool> predicate)
        {
            int index = -1;
            for (int i = 0; i < list.Count; ++i)
            {
                if (predicate(list[i]))
                    index = i;
            }

            return index;
        }

        public static int FindIndexNoLinq<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (predicate(list[i]))
                    return i;
            }

            return -1;
        }

        public static void InsertSorted<T>(this List<T> list, T entry) where T : IComparable<T>
        {
            int start = 0;
            int end = list.Count;

            while (start < end)
            {
                int mid = (start + end) / 2;
                if (entry.CompareTo(list[mid]) < 0)
                {
                    end = mid;
                }
                else // if they are the same, insert after
                {
                    start = mid + 1;
                }
            }

            list.Insert(start, entry);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random random = new Random();

            for (int i = 0; i < list.Count - 1; i++)
            {
                int index = random.Next(i, list.Count);

                T tmp = list[i];
                list[i] = list[index];
                list[index] = tmp;
            }
        }

        // In case this isn't defined anywhere else
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }

        public static string JoinString<T>(this IList<T> list, string separator = " ")
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            bool valueType = typeof(T).IsValueType;

            for (int i = 0; i < list.Count; i++)
            {
                if (i != 0)
                    sb.Append(separator);
                // for value types, we want to call ToString to avoid boxing.
                // for reference types, passing as object is better because it handles null.
                if (valueType)
                    sb.Append(list[i].ToString());
                else
                    sb.Append(list[i]);

            }
            return sb.ToString();
        }

        public static void Reverse<T>(this IList<T> list)
        {
            for(int i = 0, last = list.Count - 1, halfLength = list.Count / 2; i < halfLength; i++)
            {
                var tmp = list[i];
                list[i] = list[last - i];
                list[last - i] = tmp;
            }
        }
    }
}