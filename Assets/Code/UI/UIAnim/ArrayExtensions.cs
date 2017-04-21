using UnityEngine;
using System.Collections;
using System.Collections.Generic;

    public static class ArrayExtensions 
    {
        public static void Populate<T>(this IList<T> arr, T value ) 
        {
            for (int i = 0; i < arr.Count; ++i) 
            {
                arr[i] = value;
            }
        }

        public static int WrapIndex<T>(this IList<T> arr, int index)
        {
            return WrapIndex(arr.Count, index);
        }

        public static int WrapIndex(int length, int index)
        {
            // mod, add length and mod again for negative values
            return (((index % length) + length) % length);
        }

        public static T WrapGet<T>(this IList<T> arr, int index)
        {
            return arr[WrapIndex(arr.Count, index)];
        }

        public static bool IsValidIndex<T>(this IList<T> arr, int index)
        {
            return index >= 0 && index < arr.Count;
        }

        public static T GetOrDefault<T>(this IList<T> arr, int index, T defaultValue = default(T))
        {
            return arr.IsValidIndex(index) ? arr[index] : defaultValue;
        }

        public static T[] Slice<T>(this IList<T> source, int start, int end)
        {
            if (end < 0)
                end = source.Count + end;
            int length = end - start;
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
                result[i] = source[start+i];
            return result;
        }

        public static T GetRandomItem<T>(this IList<T> arr)
        {
            if (arr.Count == 0)
            {
                return default(T);
            }

            return arr[UnityEngine.Random.Range(0, arr.Count)];
        }

        public static bool Contains<T>(this T[] arr, T element)
        {
            return System.Array.IndexOf(arr, element) != -1;
        }

        public static IEnumerable<T> ToIEnumerable<T>(this T item)
        {
            yield return item;
        }
    }

    // Instead of creating empty arrays all over the place, just use a shared one.
    public static class EmptyArray<T>
    {
        public static readonly T[] Inst = new T[0];
    }