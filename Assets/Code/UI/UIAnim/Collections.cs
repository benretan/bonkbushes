
#region Using Statements

// C# libs
using System;
using System.Collections.Generic;

#endregion // Using Statements


namespace Core.Collections
{
    //=================================== Multi-Key Dictionary =====================================

    // Two Key Dictionary using a tuple as key
    public class Dictionary<K1, K2, V> : Dictionary<Tuple<K1, K2>, V>
    {
        public void Add(K1 key1, K2 key2, V value)
        {
            Add(new Tuple<K1, K2>(key1, key2), value);
        }

        public bool TryGetValue(K1 key1, K2 key2, out V value)
        {
            return TryGetValue(new Tuple<K1, K2>(key1, key2), out value);
        }
        public bool ContainsKey(K1 key1, K2 key2)
        {
            return ContainsKey(new Tuple<K1, K2>(key1, key2));
        }

        public bool Remove(K1 key1, K2 key2)
        {
            return Remove(new Tuple<K1, K2>(key1, key2));
        }

        public Dictionary() {}

        public Dictionary(Dictionary<K1, K2, V> other) : base(other) { }

        public V this[K1 key1, K2 key2]
        {
            get 
            {
                return this[new Tuple<K1, K2>(key1, key2)];
            }
            set
            {
                this[new Tuple<K1, K2>(key1, key2)] = value;
            }
        }
    }


    // Three Key Dictionary using a tuple as key
    public class Dictionary<K1, K2, K3, V> : Dictionary<Tuple<K1, K2, K3>, V>
    {
        public void Add(K1 key1, K2 key2, K3 key3, V value)
        {
            Add(new Tuple<K1, K2, K3>(key1, key2, key3), value);
        }

        public bool TryGetValue(K1 key1, K2 key2, K3 key3, out V value)
        {
            return TryGetValue(new Tuple<K1, K2, K3>(key1, key2, key3), out value);
        }
        public bool ContainsKey(K1 key1, K2 key2, K3 key3)
        {
            return ContainsKey(new Tuple<K1, K2, K3>(key1, key2, key3));
        }

        public Dictionary() { }

        public Dictionary(Dictionary<K1, K2, K3, V> other) : base(other) { }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }
    }

    public class Tuple<T1, T2> : IEquatable<Tuple<T1, T2>>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        private const int kPrimeNo1 = 31;
        private const int kPrimeNo2 = 37;

        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
        public override int GetHashCode()
        {
            // Hashcode is created using the hashcode of all three items
            int hash = kPrimeNo1;
            hash = hash * kPrimeNo2 + (Item1 == null ? 0 : Item1.GetHashCode());
            hash = hash * kPrimeNo2 + (Item2 == null ? 0 : Item2.GetHashCode());
            return hash;
        }

        public override bool Equals(object obj)
        {
            // check if null and obj is of the correct type
            if (obj == null || !(obj is Tuple<T1, T2>))
                return false;

            var other = (Tuple<T1, T2>)obj;

            return this == other;
        }

        public static bool operator ==(Tuple<T1, T2> tuple1, Tuple<T1, T2> tuple2)
        {
            // check if both are null or not
            if (ReferenceEquals(tuple1, null) || ReferenceEquals(tuple2, null))
                return ReferenceEquals(tuple1, null) && ReferenceEquals(tuple2, null);

            return AreItemsEqual(tuple1.Item1, tuple2.Item1)
                && AreItemsEqual(tuple1.Item2, tuple2.Item2);
        }

        private static bool AreItemsEqual<T>(T object1, T object2)
        {
            return EqualityComparer<T>.Default.Equals(object1, object2);
        }

        public bool Equals(Tuple<T1, T2> other)
        {
            if (other == null)
                return false;
            return this == other;
        }

        public static bool operator !=(Tuple<T1, T2> tuple1, Tuple<T1, T2> tuple2)
        {
            return !(tuple1 == tuple2);
        }
    }

    public class Tuple<T1, T2, T3> : IEquatable<Tuple<T1, T2, T3>>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;

        private const int kPrimeNo1 = 31;
        private const int kPrimeNo2 = 37;

        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }
        public override int GetHashCode()
        {
            // Hashcode is created using the hashcode of all three items
            int hash = kPrimeNo1;
            hash = hash * kPrimeNo2 + (Item1 == null ? 0 : Item1.GetHashCode());
            hash = hash * kPrimeNo2 + (Item2 == null ? 0 : Item2.GetHashCode());
            hash = hash * kPrimeNo2 + (Item3 == null ? 0 : Item3.GetHashCode());
            return hash;
        }

        public override bool Equals(object obj)
        {
            // check if null and obj is of the correct type
            if (obj == null || !(obj is Tuple<T1, T2, T3>))
                return false;

            var other = (Tuple<T1, T2, T3>)obj;

            return this == other;
        }

        public static bool operator ==(Tuple<T1, T2, T3> tuple1, Tuple<T1, T2, T3> tuple2)
        {
            // check if both are null or not
            if (ReferenceEquals(tuple1, null) || ReferenceEquals(tuple2, null))
                return ReferenceEquals(tuple1, null) && ReferenceEquals(tuple2, null);

            return AreItemsEqual(tuple1.Item1, tuple2.Item1)
                && AreItemsEqual(tuple1.Item2, tuple2.Item2)
                && AreItemsEqual(tuple1.Item3, tuple2.Item3);
        }

        private static bool AreItemsEqual<T>(T object1, T object2)
        {
            return EqualityComparer<T>.Default.Equals(object1, object2);
        }

        public bool Equals(Tuple<T1, T2, T3> other)
        {
            if (other == null)
                return false;
            return this == other;
        }

        public static bool operator !=(Tuple<T1, T2, T3> tuple1, Tuple<T1, T2, T3> tuple2)
        {
            return !(tuple1 == tuple2);
        }
    }

    public static class TupleExtensions
    {
        public static T1 Item1OrDefault<T1, T2>(this Tuple<T1, T2> tuple)
        {
            if (tuple != null)
            {
                return tuple.Item1;
            }
            return default(T1);
        }

        public static T2 Item2OrDefault<T1, T2>(this Tuple<T1, T2> tuple)
        {
            if (tuple != null)
            {
                return tuple.Item2;
            }
            return default(T2);
        }

        public static T1 Item1OrDefault<T1, T2, T3>(this Tuple<T1, T2, T3> tuple)
        {
            if (tuple != null)
            {
                return tuple.Item1;
            }
            return default(T1);
        }

        public static T2 Item2OrDefault<T1, T2, T3>(this Tuple<T1, T2, T3> tuple)
        {
            if (tuple != null)
            {
                return tuple.Item2;
            }
            return default(T2);
        }

        public static T3 Item3OrDefault<T1, T2, T3>(this Tuple<T1, T2, T3> tuple)
        {
            if (tuple != null)
            {
                return tuple.Item3;
            }
            return default(T3);
        }

        /// <summary>
        /// Return item in tuple at index (starting at 0 for Item1)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tuple"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T GetAtIndex<T>(this Tuple<T, T> tuple, int index)
        {
            switch (index)
            {
                case 0:
                    return tuple.Item1;
                case 1:
                    return tuple.Item2;
                default:
                    throw new ArgumentOutOfRangeException("index");
            }
        }

        /// <summary>
        /// Return item in tuple at index (starting at 0 for Item1)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tuple"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T GetAtIndex<T>(this Tuple<T, T, T> tuple, int index)
        {
            switch (index)
            {
                case 0:
                    return tuple.Item1;
                case 1:
                    return tuple.Item2;
                case 2:
                    return tuple.Item3;
                default:
                    throw new ArgumentOutOfRangeException("index");
            }
        }

        public static T GetAtIndexOrDefault<T>(this Tuple<T, T> tuple, int index)
        {
            if (tuple == null)
                return default(T);
            if (index < 0 || index > 1)
                return default(T);
            return tuple.GetAtIndex(index);
        }

        public static T GetAtIndexOrDefault<T>(this Tuple<T, T, T> tuple, int index)
        {
            if (tuple == null)
                return default(T);
            if (index < 0 || index > 2)
                return default(T);
            return tuple.GetAtIndex(index);
        }
    }
}
