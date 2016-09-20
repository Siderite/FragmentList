using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rnd = new Random();
            var flist=new FragmentList<int>();
            var list = new List<int>();
            var fsw = new Stopwatch();
            var sw = new Stopwatch();
            Console.Clear();
            Console.WriteLine("Press Escape to leave");
            var left = Console.CursorLeft;
            var top = Console.CursorTop;
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape) break;
                }
                test(rnd, flist, fsw, list, sw);
                display(flist, fsw, list, sw, left, top);
            }
        }

        private static void display(FragmentList<int> flist, Stopwatch fsw, List<int> list, Stopwatch sw, int left, int top)
        {
            Console.CursorLeft = left;
            Console.CursorTop = top;
            Console.WriteLine($"Normal:   {list.Count} {list.Capacity} {sw.Elapsed}".PadRight(20));
            Console.WriteLine($"Fragment: {flist.Count} {flist.Capacity} {fsw.Elapsed}".PadRight(20));
            Console.WriteLine($"{flist}".PadRight(1000));
            if (!flist.SequenceEqual(list)) throw new Exception("Lists are different");
        }

        private static void test(Random rnd, FragmentList<int> flist, Stopwatch fsw, List<int> list, Stopwatch sw)
        {
            var operation = rnd.Next(0,25);
            var seed = rnd.Next();
            switch (operation)
            {
                case 0: //this
                    testThis(new Random(seed), flist, fsw);
                    testThis(new Random(seed), list, sw);
                    break;
                case 1: //Add
                    testAdd(new Random(seed), flist, fsw);
                    testAdd(new Random(seed), list, sw);
                    break;
                case 2: //AddRange
                    testAddRange(new Random(seed), flist, fsw);
                    testAddRange(new Random(seed), list, sw);
                    break;
                case 3: //BinarySearch
                    /*testBinarySearch(new Random(seed), flist, fsw);
                    testBinarySearch(new Random(seed), list, sw);*/
                    break;
                case 4: //Contains
                    testContains(new Random(seed), flist, fsw);
                    testContains(new Random(seed), list, sw);
                    break;
                case 5: //CopyTo
                    break;
                case 6: //Exists
                    testExists(new Random(seed), flist, fsw);
                    testExists(new Random(seed), list, sw);
                    break;
                case 7: //Find
                    testFind(new Random(seed), flist, fsw);
                    testFind(new Random(seed), list, sw);
                    break;
                case 8: //FindAll
                    testFindAll(new Random(seed), flist, fsw);
                    testFindAll(new Random(seed), list, sw);
                    break;
                case 9: //FindIndex
                    testFindIndex(new Random(seed), flist, fsw);
                    testFindIndex(new Random(seed), list, sw);
                    break;
                case 10: //FindLast
                    break;
                case 11: //FindLastIndex
                    break;
                case 12: //GetRange
                    break;
                case 13: //GetFragmentRange
                    break;
                case 14: //IndexOf
                    break;
                case 15: //Insert
                    testInsert(new Random(seed), flist, fsw);
                    testInsert(new Random(seed), list, sw);
                    break;
                case 16: //InsertRange
                    testInsertRange(new Random(seed), flist, fsw);
                    testInsertRange(new Random(seed), list, sw);
                    break;
                case 17: //LastIndexOf
                    break;
                case 18: //Remove
                    testRemove(new Random(seed), flist, fsw);
                    testRemove(new Random(seed), list, sw);
                    break;
                case 19: //RemoveAll
                    break;
                case 20: //RemoveAt
                    testRemoveAt(new Random(seed), flist, fsw);
                    testRemoveAt(new Random(seed), list, sw);
                    break;
                case 21: //RemoveRange
                    testRemoveRange(new Random(seed), flist, fsw);
                    testRemoveRange(new Random(seed), list, sw);
                    break;
                case 22: //Reverse
                    break;
                case 23: //Sort
                    break;
                case 24: //TrueForAll
                    break;
            }
        }

        private static void testFindIndex(Random rnd, List<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = rnd.Next();
            sw.Start();
            var result = lst.FindIndex(v => v < val);
            sw.Stop();
        }
        private static void testFindIndex(Random rnd, FragmentList<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = rnd.Next();
            sw.Start();
            var result = lst.FindIndex(v => v < val);
            sw.Stop();
        }
        private static void testFindAll(Random rnd, List<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = rnd.Next();
            sw.Start();
            var result = lst.FindAll(v => v < val);
            sw.Stop();
        }
        private static void testFindAll(Random rnd, FragmentList<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = rnd.Next();
            sw.Start();
            var result = lst.FindAll(v => v < val);
            sw.Stop();
        }
        private static void testFind(Random rnd, List<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = rnd.Next();
            sw.Start();
            var result = lst.Find(v => v < val);
            sw.Stop();
        }
        private static void testFind(Random rnd, FragmentList<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = rnd.Next();
            sw.Start();
            var result = lst.Find(v => v < val);
            sw.Stop();
        }
        private static void testExists(Random rnd, List<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = rnd.Next();
            sw.Start();
            var result = lst.Exists(v => v < val);
            sw.Stop();
        }
        private static void testExists(Random rnd, FragmentList<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = rnd.Next();
            sw.Start();
            var result = lst.Exists(v => v < val);
            sw.Stop();
        }

        private static void testBinarySearch(Random rnd, FragmentList<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = lst[index];
            var val2 = rnd.Next();
            sw.Start();
            var idx = lst.BinarySearch(val);
            if (idx >= 0 && lst[idx] != val) throw new Exception("Binary search");
            if (idx < 0)
            {
                idx = ~idx;
                if (idx > 0 && lst[idx - 1] >= val) throw new Exception("Binary search");
                if (idx < lst.Count && lst[idx] <= val) throw new Exception("Binary search");
            }
            idx = lst.BinarySearch(val2);
            if (idx >= 0 && lst[idx] != val2) throw new Exception("Binary search");
            if (idx < 0)
            {
                idx = ~idx;
                if (idx > 0 && lst[idx - 1] >= val2) throw new Exception("Binary search");
                if (idx < lst.Count && lst[idx] <= val2) throw new Exception("Binary search");
            }
            sw.Stop();
        }
        private static void testBinarySearch(Random rnd, List<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = lst[index];
            var val2 = rnd.Next();
            sw.Start();
            var idx = lst.BinarySearch(val);
            if (idx >= 0 && lst[idx] != val) throw new Exception("Binary search");
            if (idx < 0)
            {
                idx = ~idx;
                if (idx > 0 && lst[idx - 1] >= val) throw new Exception("Binary search");
                if (idx < lst.Count && lst[idx] <= val) throw new Exception("Binary search");
            }
            idx = lst.BinarySearch(val2);
            if (idx >= 0 && lst[idx] != val2) throw new Exception("Binary search");
            if (idx < 0)
            {
                idx = ~idx;
                if (idx > 0 && lst[idx - 1] >= val2) throw new Exception("Binary search");
                if (idx < lst.Count && lst[idx] <= val2) throw new Exception("Binary search");
            }
            sw.Stop();
        }


        private static void testContains(Random rnd, IList<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = lst[index];
            var val2 = rnd.Next();
            sw.Start();
            var result = lst.Contains(val);
            if (!result) throw new Exception("Contains");
            result = lst.Contains(val2);
            sw.Stop();
        }
        private static void testAdd(Random rnd, IList<int> lst, Stopwatch sw)
        {
            var val = rnd.Next();
            sw.Start();
            lst.Add(val);
            sw.Stop();
        }
        private static void testInsert(Random rnd, IList<int> lst, Stopwatch sw)
        {
            var k = rnd.Next(1, 100);
            while (k-- > 0)
            {
                var val = rnd.Next();
                var n = rnd.Next(0, lst.Count);
                sw.Start();
                lst.Insert(n, val);
                sw.Stop();
            }
        }
        private static void testRemove(Random rnd, IList<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var n = rnd.Next(0, lst.Count);
            var val = lst[n];
            sw.Start();
            lst.Remove(val);
            sw.Stop();
        }
        private static void testRemoveAt(Random rnd, IList<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var n = rnd.Next(0, lst.Count);
            sw.Start();
            lst.RemoveAt(n);
            sw.Stop();
        }
        private static void testThis(Random rnd, IList<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var index = rnd.Next(0, lst.Count);
            var val = rnd.Next();
            sw.Start();
            lst[index] = val;
            sw.Stop();
            if (lst[index] != val) throw new Exception("This");
        }

        private static void testAddRange(Random rnd, List<int> lst, Stopwatch sw)
        {
            var ien = randomRange(rnd);
            sw.Start();
            lst.AddRange(ien);
            sw.Stop();
        }
        private static void testAddRange(Random rnd, FragmentList<int> lst, Stopwatch sw)
        {
            var ien = randomRange(rnd);
            sw.Start();
            lst.AddRange(ien);
            sw.Stop();
        }
        private static void testInsertRange(Random rnd, List<int> lst, Stopwatch sw)
        {
            var ien = randomRange(rnd);
            var n = rnd.Next(0, lst.Count);
            sw.Start();
            lst.InsertRange(n, ien);
            sw.Stop();
        }
        private static void testInsertRange(Random rnd, FragmentList<int> lst, Stopwatch sw)
        {
            var ien = randomRange(rnd);
            var n = rnd.Next(0, lst.Count);
            sw.Start();
            lst.InsertRange(n, ien);
            sw.Stop();
        }
        private static void testRemoveRange(Random rnd, List<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var start = rnd.Next(0, lst.Count);
            var count = rnd.Next(0, lst.Count-start);
            sw.Start();
            lst.RemoveRange(start, count);
            sw.Stop();
        }
        private static void testRemoveRange(Random rnd, FragmentList<int> lst, Stopwatch sw)
        {
            if (lst.Count == 0) return;
            var start = rnd.Next(0, lst.Count);
            var count = rnd.Next(0, lst.Count - start);
            sw.Start();
            lst.RemoveRange(start, count);
            sw.Stop();
        }



        private static IEnumerable<int> randomRange(Random rnd)
        {
            var n = rnd.Next(0, 10000);
            while (n-- > 0)
            {
                yield return rnd.Next();
            }
        }

    }
}
