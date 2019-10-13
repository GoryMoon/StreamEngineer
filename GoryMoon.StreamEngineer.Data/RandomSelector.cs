using System;
using System.Collections.Generic;

namespace GoryMoon.StreamEngineer.Data
{
    /// <summary>
    ///     DynamicRandomSelector allows you adding or removing items.
    ///     Call "Build" after you finished modification.
    ///     Switches between linear or binary search after each Build(),
    ///     depending on count of items, making it more performant for general use case.
    /// </summary>
    /// <typeparam name="T">Type of items you wish this selector returns</typeparam>
    public class DynamicRandomSelector<T> : IRandomSelector<T>, IRandomSelectorBuilder<T>
    {
        private readonly List<float> CDL; // Cummulative Distribution List

        // internal buffers
        private readonly List<T> itemsList;
        private Random random;

        // internal function that gets dynamically swapped inside Build
        private Func<List<float>, float, int> selectFunction;
        private readonly List<float> weightsList;

        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="seed">Leave it -1 if you want seed to be randomly picked</param>
        /// <param name="expectedNumberOfItems">
        ///     Set this if you know how much items the collection will hold, to minimize Garbage
        ///     Collection
        /// </param>
        public DynamicRandomSelector(int seed = -1, int expectedNumberOfItems = 32)
        {
            if (seed == -1)
                random = new Random();
            else
                random = new Random(seed);

            itemsList = new List<T>(expectedNumberOfItems);
            weightsList = new List<float>(expectedNumberOfItems);
            CDL = new List<float>(expectedNumberOfItems);
        }

        /// <summary>
        ///     Constructor, where you can preload collection with items/weights array.
        /// </summary>
        /// <param name="items">Items that will get returned on random selections</param>
        /// <param name="weights">Un-normalized weights/chances of items, should be same length as items array</param>
        /// <param name="seed">Leave it -1 if you want seed to be randomly picked</param>
        /// <param name="expectedNumberOfItems">
        ///     Set this if you know how much items the collection will hold, to minimize Garbage
        ///     Collection
        /// </param>
        public DynamicRandomSelector(T[] items, float[] weights, int seed = -1, int expectedNumberOfItems = 32) :
            this()
        {
            for (var i = 0; i < items.Length; i++)
                Add(items[i], weights[i]);

            Build();
        }

        /// <summary>
        ///     Constructor, where you can preload collection with items/weights list.
        /// </summary>
        /// <param name="items">Items that will get returned on random selections</param>
        /// <param name="weights">Un-normalized weights/chances of items, should be same length as items array</param>
        /// <param name="seed">Leave it -1 if you want seed to be randomly picked</param>
        /// <param name="expectedNumberOfItems">
        ///     Set this if you know how much items the collection will hold, to minimize Garbage
        ///     Collection
        /// </param>
        public DynamicRandomSelector(List<T> items, List<float> weights, int seed = -1,
            int expectedNumberOfItems = 32) : this()
        {
            for (var i = 0; i < items.Count; i++)
                Add(items[i], weights[i]);

            Build();
        }

        public int Count => itemsList.Count;

        /// <summary>
        ///     Selects random item based on its probability.
        ///     Uses linear search or binary search, depending on internal list size.
        /// </summary>
        /// <param name="randomValue">Random value from your uniform generator</param>
        /// <returns>Returns item</returns>
        public T SelectRandomItem(float randomValue)
        {
            return itemsList[selectFunction(CDL, randomValue)];
        }

        /// <summary>
        ///     Selects random item based on its probability.
        ///     Uses linear search or binary search, depending on internal list size.
        /// </summary>
        /// <returns>Returns item</returns>
        public T SelectRandomItem()
        {
            var randomValue = (float) random.NextDouble();
            return itemsList[selectFunction(CDL, randomValue)];
        }

        /// <summary>
        ///     Re/Builds internal CDL (Cummulative Distribution List)
        ///     Must be called after modifying (calling Add or Remove), or it will break.
        ///     Switches between linear or binary search, depending on which one will be faster.
        ///     Might generate some garbage (list resize) on first few builds.
        /// </summary>
        /// <param name="seed">You can specify seed for internal random gen or leave it alone</param>
        /// <returns>Returns itself</returns>
        public IRandomSelector<T> Build(int seed = -1)
        {
            if (itemsList.Count == 0)
                throw new Exception("Cannot build with no items.");

            // clear list and then transfer weights
            CDL.Clear();
            for (var i = 0; i < weightsList.Count; i++)
                CDL.Add(weightsList[i]);

            RandomMath.BuildCumulativeDistribution(CDL);

            // default behavior
            // if seed wasn't specified (it is seed==-1), keep same seed - avoids garbage collection from making new random
            if (seed != -1)
            {
                // input -2 if you want to randomize seed
                if (seed == -2)
                {
                    seed = random.Next();
                    random = new Random(seed);
                }
                else
                {
                    random = new Random(seed);
                }
            }

            // RandomMath.ListBreakpoint decides where to use Linear or Binary search, based on internal buffer size
            // if CDL list is smaller than breakpoint, then pick linear search random selector, else pick binary search selector
            if (CDL.Count < RandomMath.ListBreakpoint)
                selectFunction = RandomMath.SelectIndexLinearSearch;
            else
                selectFunction = RandomMath.SelectIndexBinarySearch;

            return this;
        }

        /// <summary>
        ///     Clears internal buffers, should make no garbage (unless internal lists hold objects that aren't referenced anywhere
        ///     else)
        /// </summary>
        public void Clear()
        {
            itemsList.Clear();
            weightsList.Clear();
            CDL.Clear();
        }

        /// <summary>
        ///     Add new item with weight into collection. Items with zero weight will be ignored.
        ///     Do not add duplicate items, because removing them will be buggy (you will need to call remove for duplicates too!).
        ///     Be sure to call Build() after you are done adding items.
        /// </summary>
        /// <param name="item">Item that will be returned on random selection</param>
        /// <param name="weight">Non-zero non-normalized weight</param>
        public void Add(T item, float weight)
        {
            // ignore zero weight items
            if (weight == 0)
                return;

            itemsList.Add(item);
            weightsList.Add(weight);
        }

        /// <summary>
        ///     Remove existing item with weight into collection.
        ///     Be sure to call Build() after you are done removing items.
        /// </summary>
        /// <param name="item">Item that will be removed out of collection, if found</param>
        public void Remove(T item)
        {
            var index = itemsList.IndexOf(item);
            ;

            // nothing was found
            if (index == -1)
                return;

            itemsList.RemoveAt(index);
            weightsList.RemoveAt(index);
            // no need to remove from CDL, should be rebuilt instead
        }
    }


    public static class RandomMath
    {
        /// <summary>
        ///     Breaking point between using Linear vs. Binary search for arrays (StaticSelector).
        ///     Was calculated empirically.
        /// </summary>
        public static readonly int ArrayBreakpoint = 51;

        /// <summary>
        ///     Breaking point between using Linear vs. Binary search for lists (DynamicSelector).
        ///     Was calculated empirically.
        /// </summary>
        public static readonly int ListBreakpoint = 26;

        /// <summary>
        ///     Builds cummulative distribution out of non-normalized weights inplace.
        /// </summary>
        /// <param name="CDL">List of Non-normalized weights</param>
        public static void BuildCumulativeDistribution(List<float> CDL)
        {
            var Length = CDL.Count;

            // Use double for more precise calculation
            double Sum = 0;

            // Sum of weights
            for (var i = 0; i < Length; i++)
                Sum += CDL[i];

            // k is normalization constant
            // calculate inverse of sum and convert to float
            // use multiplying, since it is faster than dividing      
            var k = 1f / Sum;

            Sum = 0;

            // Make Cummulative Distribution Array
            for (var i = 0; i < Length; i++)
            {
                Sum += CDL[i] * k; //k, the normalization constant is applied here
                CDL[i] = (float) Sum;
            }

            CDL[Length - 1] =
                1f; //last item of CDA is always 1, I do this because numerical inaccurarcies add up and last item probably wont be 1
        }

        /// <summary>
        ///     Builds cummulative distribution out of non-normalized weights inplace.
        /// </summary>
        /// <param name="CDA">Array of Non-normalized weights</param>
        public static void BuildCumulativeDistribution(float[] CDA)
        {
            var Length = CDA.Length;

            // Use double for more precise calculation
            double Sum = 0;

            // Sum of weights
            for (var i = 0; i < Length; i++)
                Sum += CDA[i];

            // k is normalization constant
            // calculate inverse of sum and convert to float
            // use multiplying, since it is faster than dividing   
            var k = 1f / Sum;

            Sum = 0;

            // Make Cummulative Distribution Array
            for (var i = 0; i < Length; i++)
            {
                Sum += CDA[i] * k; //k, the normalization constant is applied here
                CDA[i] = (float) Sum;
            }

            CDA[Length - 1] =
                1f; //last item of CDA is always 1, I do this because numerical inaccurarcies add up and last item probably wont be 1
        }


        /// <summary>
        ///     Linear search, good/faster for small arrays
        /// </summary>
        /// <param name="CDL">Cummulative Distribution Array</param>
        /// <param name="randomValue">Uniform random value</param>
        /// <returns>Returns index of an value inside CDA</returns>
        public static int SelectIndexLinearSearch(this float[] CDA, float randomValue)
        {
            var i = 0;

            // last element, CDA[CDA.Length-1] should always be 1
            while (CDA[i] < randomValue)
                i++;

            return i;
        }


        /// <summary>
        ///     Binary search, good/faster for big array
        ///     Code taken out of C# array.cs Binary Search & modified
        /// </summary>
        /// <param name="CDA">Cummulative Distribution Array</param>
        /// <param name="randomValue">Uniform random value</param>
        /// <returns>Returns index of an value inside CDA</returns>
        public static int SelectIndexBinarySearch(this float[] CDA, float randomValue)
        {
            var lo = 0;
            var hi = CDA.Length - 1;
            int index;

            while (lo <= hi)
            {
                // calculate median
                index = lo + ((hi - lo) >> 1);

                if (CDA[index] == randomValue) return index;

                if (CDA[index] < randomValue)
                    // shrink left
                    lo = index + 1;
                else
                    // shrink right
                    hi = index - 1;
            }

            index = lo;

            return index;
        }

        /// <summary>
        ///     Linear search, good/faster for small lists
        /// </summary>
        /// <param name="CDL">Cummulative Distribution List</param>
        /// <param name="randomValue">Uniform random value</param>
        /// <returns>Returns index of an value inside CDA</returns>
        public static int SelectIndexLinearSearch(this List<float> CDL, float randomValue)
        {
            var i = 0;

            // last element, CDL[CDL.Length-1] should always be 1
            while (CDL[i] < randomValue)
                i++;

            return i;
        }

        /// <summary>
        ///     Binary search, good/faster for big lists
        ///     Code taken out of C# array.cs Binary Search & modified
        /// </summary>
        /// <param name="CDL">Cummulative Distribution List</param>
        /// <param name="randomValue">Uniform random value</param>
        /// <returns>Returns index of an value inside CDL</returns>
        public static int SelectIndexBinarySearch(this List<float> CDL, float randomValue)
        {
            var lo = 0;
            var hi = CDL.Count - 1;
            int index;

            while (lo <= hi)
            {
                // calculate median
                index = lo + ((hi - lo) >> 1);

                if (CDL[index] == randomValue) return index;

                if (CDL[index] < randomValue)
                    // shrink left
                    lo = index + 1;
                else
                    // shrink right
                    hi = index - 1;
            }

            index = lo;

            return index;
        }

        /// <summary>
        ///     Returns identity, array[i] = i
        /// </summary>
        /// <param name="length">Length of an array</param>
        /// <returns>Identity array</returns>
        public static float[] IdentityArray(int length)
        {
            var array = new float[length];

            for (var i = 0; i < array.Length; i++)
                array[i] = i;

            return array;
        }

        /// <summary>
        ///     Gemerates uniform random values for all indexes in array.
        /// </summary>
        /// <param name="list">The array where all values will be randomized.</param>
        /// <param name="r">Random generator</param>
        public static void RandomWeightsArray(ref float[] array, Random r)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = (float) r.NextDouble();

                if (array[i] == 0)
                    i--;
            }
        }

        /// <summary>
        ///     Creates new array with uniform random variables.
        /// </summary>
        /// <param name="r">Random generator</param>
        /// <param name="length">Length of new array</param>
        /// <returns>Array with random uniform random variables</returns>
        public static float[] RandomWeightsArray(Random r, int length)
        {
            var array = new float[length];

            for (var i = 0; i < length; i++)
            {
                array[i] = (float) r.NextDouble();

                if (array[i] == 0)
                    i--;
            }

            return array;
        }


        /// <summary>
        ///     Returns identity, list[i] = i
        /// </summary>
        /// <param name="length">Length of an list</param>
        /// <returns>Identity list</returns>
        public static List<float> IdentityList(int length)
        {
            var list = new List<float>(length);

            for (var i = 0; i < length; i++)
                list.Add(i);

            return list;
        }

        /// <summary>
        ///     Gemerates uniform random values for all indexes in list.
        /// </summary>
        /// <param name="list">The list where all values will be randomized.</param>
        /// <param name="r">Random generator</param>
        public static void RandomWeightsList(ref List<float> list, Random r)
        {
            for (var i = 0; i < list.Count; i++)
            {
                list[i] = (float) r.NextDouble();

                if (list[i] == 0)
                    i--;
            }
        }
    }

    /// <summary>
    ///     Interface for Random selector
    /// </summary>
    /// <typeparam name="T">Type of items that gets randomly returned</typeparam>
    public interface IRandomSelector<T>
    {
        T SelectRandomItem();
        T SelectRandomItem(float randomValue);
    }

    /// <summary>
    ///     Interface for Random Selector Builders.
    /// </summary>
    /// <typeparam name="T">Type of items that gets randomly returned</typeparam>
    public interface IRandomSelectorBuilder<T>
    {
        IRandomSelector<T> Build(int seed = -1);
    }
}