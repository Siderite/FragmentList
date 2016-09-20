using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class FragmentList<T> : IList<T>, IList, IReadOnlyList<T>
{
    private List<List<T>> _bucket;
    private int _count;
    private object _syncRoot;

    public FragmentList()
    {
        _bucket = new List<List<T>>();
        AutoDefragmentThreshold = 0.8;
    }

    #region internal work

    private ArrayInfo getInfo(int index)
    {
        var ls = 0;
        for (var i = 0; i < _bucket.Count; i++)
        {
            var arr = _bucket[i];
            if (ls + arr.Count <= index)
            {
                ls += arr.Count;
            }
            else
            {
                return new ArrayInfo
                {
                    StartIndex = index - ls,
                    StartArrayIndex = i,
                    PrevSum = ls
                };
            }
        }
        return null;
    }

    private ArrayInfo getInfo(int index, int count)
    {
        var ls = 0;
        var le = 0;
        ArrayInfo result = null;
        for (var i = 0; i < _bucket.Count; i++)
        {
            var arr = _bucket[i];
            if (ls + arr.Count < index)
            {
                ls += arr.Count;
            }
            else
            {
                if (result == null)
                {
                    result = new ArrayInfo {
                        StartIndex = index - ls,
                        StartArrayIndex = i,
                        PrevSum=ls
                    };
                }
            }
            if (le + arr.Count < index + count)
            {
                le += arr.Count;
            }
            else
            {
                result.EndIndex = index + count - le - 1;
                result.EndArrayIndex = i;
                return result;
            }
        }
        return null;
    }

    private void removeEmptyArrays()
    {
        _bucket.RemoveAll(arr => arr.Count == 0);
        autoDefrag();
    }

    private void ensureAtLeastOneArray()
    {
        if (_bucket.Count == 0)
        {
            var lst = new List<T>();
            _bucket.Add(lst);
        }
    }

    private void autoDefrag()
    {
        var tenth = _count / 10;
        var frg = _bucket.Count(l => l.Count < tenth) / (double)_bucket.Count;
        if (frg > AutoDefragmentThreshold)
        {
            TrimExcess();
        }
    }

    private class ArrayInfo
    {
        public int StartIndex;
        public int StartArrayIndex;
        public int EndIndex;
        public int EndArrayIndex;
        public int PrevSum;
    }

    #endregion

    #region IList<T> implementation
    public T this[int index]
    {
        get
        {
            var info = getInfo(index);
            if (info == null) throw new IndexOutOfRangeException();
            return _bucket[info.StartArrayIndex][info.StartIndex];
        }

        set
        {
            var info = getInfo(index);
            if (info == null) throw new IndexOutOfRangeException();
            _bucket[info.StartArrayIndex][info.StartIndex] = value;
        }
    }

    public int Count
    {
        get
        {
            return _count;
        }
    }

    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    public void Add(T item)
    {
        ensureAtLeastOneArray();
        var arr = _bucket[_bucket.Count - 1];
        arr.Add(item);
        _count++;
    }

    public void Clear()
    {
        _bucket.Clear();
        _count = 0;
    }

    public bool Contains(T item)
    {
        return _bucket.Any(arr => arr.Contains(item));
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        CopyTo(0, array, arrayIndex, _count);
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var arr in _bucket)
        {
            foreach (var item in arr)
            {
                yield return item;
            }
        }
    }

    public int IndexOf(T item)
    {
        return IndexOf(item, 0, _count);
    }

    public void Insert(int index, T item)
    {
        if (index==Count)
        {
            Add(item);
            return;
        }
        ensureAtLeastOneArray();
        var info = getInfo(index);
        if (info == null) throw new IndexOutOfRangeException();
        var arr = _bucket[info.StartArrayIndex];
        if (arr.Count<arr.Capacity)
        {
            arr.Insert(info.StartIndex, item);
            _count++;
            return;
        }
        lock (((ICollection)_bucket).SyncRoot)
        {
            if (info.StartIndex == 0)
            {
                if (info.StartArrayIndex>0)
                {
                    _bucket[info.StartArrayIndex - 1].Add(item);
                } else
                {
                    arr.Insert(0, item);
                }
            }
            else
            {
                if (info.StartIndex < arr.Count - info.StartIndex)
                {
                    var newList = new List<T>(info.StartIndex + 1);
                    newList.AddRange(arr.Take(info.StartIndex));
                    newList.Add(item);
                    _bucket.Insert(info.StartArrayIndex, newList);
                    arr.RemoveRange(0, info.StartIndex);
                }
                else
                {
                    var newList = new List<T>(arr.Count - info.StartIndex + 1);
                    newList.Add(item);
                    newList.AddRange(arr.Skip(info.StartIndex));
                    _bucket.Insert(info.StartArrayIndex + 1, newList);
                    arr.RemoveRange(info.StartIndex, arr.Count - info.StartIndex);
                }
            }
            _count++;
        }
    }

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0) return false;
        RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        var info = getInfo(index);
        if (info == null) throw new IndexOutOfRangeException();
        var arr = _bucket[info.StartArrayIndex];
        arr.RemoveAt(info.StartIndex);
        _count--;
        removeEmptyArrays();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
    #endregion

    #region IList implementation
    public bool IsFixedSize
    {
        get
        {
            return false;
        }
    }

    public bool IsSynchronized
    {
        get
        {
            return false;
        }
    }

    public object SyncRoot
    {
        get
        {
            if (_syncRoot == null)
            {
                System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
            }
            return _syncRoot;
        }
    }

    object IList.this[int index]
    {
        get
        {
            return this[index];
        }

        set
        {
            this[index] = (T)value;
        }
    }

    public int Add(object value)
    {
        Add((T)value);
        return _count - 1;
    }

    public bool Contains(object value)
    {
        return Contains((T)value);
    }

    public int IndexOf(object value)
    {
        return IndexOf((T)value);
    }

    public void Insert(int index, object value)
    {
        Insert(index, (T)value);
    }

    public void Remove(object value)
    {
        Remove((T)value);
    }

    public void CopyTo(Array array, int index)
    {
        var l = 0;
        foreach (var arr in _bucket)
        {
            ((IList)arr).CopyTo(array, index + l);
            l += arr.Count;
        }
    }
    #endregion

    #region extra stuff List<T> has

    public FragmentList(int capacity) : this()
    {
        _bucket.Add(new List<T>(capacity));
    }

    public FragmentList(IEnumerable<T> enumerable) : this()
    {
        _bucket.Add(new List<T>(enumerable));
    }

    public int Capacity
    {
        get { return _bucket.Sum(l=>l.Capacity); }
        set
        {
            if (value < _count)
            {
                throw new ArgumentOutOfRangeException();
            }
            var lst = new List<T>(value);
            lst.AddRange(this);
            _bucket.Clear();
            _bucket.Add(lst);
        }
    }

    public void AddRange(IEnumerable<T> collection)
    {
        var lst = new List<T>(collection);
        var arr = _bucket.LastOrDefault();
        if (arr!=null && arr.Count<=arr.Capacity-lst.Count)
        {
            arr.AddRange(lst);
            _count += lst.Count;
            return;
        }
        _bucket.Add(lst);
        _count += lst.Count;
    }

    public ReadOnlyCollection<T> AsReadOnly()
    {
        return new ReadOnlyCollection<T>(this);
    }

    public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
    {
        var info = getInfo(index,count);
        if (info == null) throw new IndexOutOfRangeException();
        var start = info.StartArrayIndex;
        var end = info.EndArrayIndex;
        while (true)
        {
            var mid = (end + start) / 2;
            var arr = _bucket[mid];
            var c = comparer.Compare(item, arr.First());
            switch (c)
            {
                case -1:
                    end = mid - 1;
                    if (end < start) return ~0;
                    break;
                case 0:
                    return info.PrevSum;
                case 1:
                    {
                        c = comparer.Compare(item, arr.Last());
                        switch (c)
                        {
                            case -1:
                                var idx = arr.BinarySearch(item);
                                if (idx<0)
                                {
                                    return ~(info.PrevSum + ~idx);
                                } else
                                {
                                    return info.PrevSum + idx;
                                }
                            case 0:
                                return info.PrevSum + arr.Count - 1;
                            case 1:
                                start = mid + 1;
                                if (start > end) return ~_count;
                                break;
                        }
                    }
                    break;
            }
        }
    }

    public int BinarySearch(T item)
    {
        return BinarySearch(0, _count, item, Comparer<T>.Default);
    }

    public int BinarySearch(T item, IComparer<T> comparer)
    {
        return BinarySearch(0, _count, item, comparer);
    }

    //Converter delegate not defined in .NET Core
    //public FragmentList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) { }

    public void CopyTo(T[] array)
    {
        CopyTo(array, 0);
    }

    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        var info = getInfo(index, count);
        if (info == null) throw new ArgumentOutOfRangeException();
        index -= info.PrevSum;
        for (var i=info.StartArrayIndex; i<=info.EndArrayIndex; i++)
        {
            var arr = _bucket[i];
            var cnt = Math.Min(count, arr.Count - index);
            if (cnt == 0) return;
            arr.CopyTo(index, array, arrayIndex, cnt);
            count -= cnt;
            arrayIndex += cnt;
            index = 0;
        }
    }

    public T Find(Predicate<T> match)
    {
        var index = FindIndex(match);
        return index == -1
            ? default(T)
            : this[index];
    }

    public List<T> FindAll(Predicate<T> match)
    {
        return this.Where(v => match(v)).ToList();
    }

    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
        var info = getInfo(startIndex, count);
        if (info == null) throw new ArgumentOutOfRangeException();
        startIndex -= info.PrevSum;
        var l = info.PrevSum;
        for (var i=info.StartArrayIndex; i<=info.EndArrayIndex; i++)
        {
            var arr = _bucket[i];
            var cnt = Math.Min(count, arr.Count - startIndex);
            if (cnt == 0) return -1;
            var idx = arr.FindIndex(startIndex, cnt, match);
            if (idx > -1)
            {
                return l + idx;
            }
            count -= cnt;
            l += arr.Count;
            startIndex = 0;
        }
        return -1;
    }

    public int FindIndex(Predicate<T> match)
    {
        return FindIndex(0, _count, match);
    }

    public int FindIndex(int startIndex, Predicate<T> match)
    {
        return FindIndex(startIndex, _count - startIndex, match);
    }

    public bool Exists(Predicate<T> match)
    {
        return FindIndex(match) != -1;
    }

    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
        startIndex -= count-1;
        var info = getInfo(startIndex, count);
        if (info == null) throw new ArgumentOutOfRangeException();
        startIndex -= info.PrevSum;
        for (var i=info.EndArrayIndex; i>=info.StartArrayIndex; i--)
        {
            var arr = _bucket[i];
            var cnt = Math.Min(count, arr.Count - startIndex);
            var idx = arr.FindLastIndex(startIndex, cnt, match);
            if (idx > 0) return idx;
            count -= cnt;
            if (count == 0) return -1;
            startIndex = 0;
        }
        return -1;
    }

    public int FindLastIndex(Predicate<T> match)
    {
        return FindLastIndex(_count - 1, _count, match);
    }

    public int FindLastIndex(int startIndex, Predicate<T> match)
    {
        return FindLastIndex(startIndex, startIndex + 1, match);
    }

    public T FindLast(Predicate<T> match)
    {
        var idx = FindLastIndex(match);
        if (idx < 0) return default(T);
        return this[idx];
    }

    public void ForEach(Action<T> action)
    {
        foreach (var item in this) action(item);
    }

    public List<T> GetRange(int index, int count)
    {
        return new List<T>(GetFragmentRange(index, count));
    }

    public int IndexOf(T item, int index, int count)
    {
        var info = getInfo(index, count);
        if (info == null) throw new ArgumentOutOfRangeException();
        index -= info.PrevSum;
        var l = info.PrevSum;
        for (var i = info.StartArrayIndex; i <= info.EndArrayIndex; i++)
        {
            var arr = _bucket[i];
            var cnt = Math.Min(count, arr.Count - index);
            var idx = arr.IndexOf(item, index, cnt);
            if (idx>=0)
            {
                return idx + l;
            }
            count -= cnt;
            if (count == 0) break;
            index = 0;
            l += arr.Count;
        }
        return -1;
    }

    public int IndexOf(T item, int index)
    {
        return IndexOf(item, index, _count - index);
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        if (index==Count)
        {
            AddRange(collection);
            return;
        }
        ensureAtLeastOneArray();
        var info = getInfo(index);
        if (info == null) throw new IndexOutOfRangeException();
        var arr = _bucket[info.StartArrayIndex];
        var insList = new List<T>(collection);
        if (arr.Count <= arr.Capacity -insList.Count)
        {
            arr.InsertRange(info.StartIndex, insList);
            _count += insList.Count;
            return;
        }
        lock (((ICollection)_bucket).SyncRoot)
        {
            if (info.StartIndex == 0)
            {
                _bucket.Insert(info.StartArrayIndex, insList);
            }
            else
            {
                if (info.StartIndex < arr.Count - info.StartIndex)
                {
                    var newList = new List<T>(info.StartIndex + 1);
                    newList.AddRange(arr.Take(info.StartIndex));
                    _bucket.Insert(info.StartArrayIndex, newList);
                    arr.RemoveRange(0, info.StartIndex);
                }
                else
                {
                    var newList = new List<T>(arr.Count - info.StartIndex + 1);
                    newList.AddRange(arr.Skip(info.StartIndex));
                    _bucket.Insert(info.StartArrayIndex + 1, newList);
                    arr.RemoveRange(info.StartIndex, arr.Count - info.StartIndex);
                }
                _bucket.Insert(info.StartArrayIndex + 1, insList);
            }
            _count+=insList.Count;
        }
    }

    public int LastIndexOf(T item, int index, int count)
    {
        return FindLastIndex(index, count, itm => object.Equals(itm, item));
    }

    public int LastIndexOf(T item, int index)
    {
        return LastIndexOf(item, index, index+1);
    }

    public int LastIndexOf(T item)
    {
        return LastIndexOf(item, _count - 1, _count);
    }

    public int RemoveAll(Predicate<T> match)
    {
        var result = 0;
        foreach (var arr in _bucket)
        {
            result+=arr.RemoveAll(match);
        }
        removeEmptyArrays();
        return result;
    }

    public void RemoveRange(int index, int count)
    {
        var info = getInfo(index, count);
        if (info == null) throw new ArgumentOutOfRangeException();
        var arr = _bucket[info.StartArrayIndex];
        if (info.StartArrayIndex == info.EndArrayIndex)
        {
            arr.RemoveRange(info.StartIndex, count);
        }
        else
        {
            arr.RemoveRange(info.StartIndex, arr.Count - info.StartIndex);
            arr = _bucket[info.EndArrayIndex];
            arr.RemoveRange(0, info.EndIndex+1);
            for (var i = info.EndArrayIndex - 1; i >= info.StartArrayIndex + 1; i--)
            {
                _bucket.RemoveAt(i);
            }
        }
        _count -= count;
        removeEmptyArrays();
    }

    public void Reverse(int index, int count)
    {
        var info = getInfo(index, count);
        if (info == null) throw new ArgumentOutOfRangeException();
        for (var i=0; i<count/2; i++)
        {
            var t = _bucket[info.StartArrayIndex][info.StartIndex];
            _bucket[info.StartArrayIndex][info.StartIndex] = _bucket[info.EndArrayIndex][info.EndIndex];
            _bucket[info.EndArrayIndex][info.EndIndex] = t;
            info.StartIndex++;
            if (info.StartIndex==_bucket[info.StartArrayIndex].Count)
            {
                info.StartArrayIndex++;
                info.StartIndex = 0;
            }
            info.EndIndex--;
            if (info.EndIndex < 0)
            {
                info.EndArrayIndex--;
                info.StartIndex = _bucket[info.EndArrayIndex].Count-1;
            }
        }
    }

    public void Reverse()
    {
        Reverse(0, Count);
    }

    public void Sort(int index, int count, IComparer<T> comparer)
    {
        var range = GetRange(index, count);
        range.Sort(comparer);
        RemoveRange(index, count);
        InsertRange(index, range);
    }

    public void Sort(IComparer<T> comparer)
    {
        var lst = new List<T>(this);
        lst.Sort(comparer);
        lock (((ICollection)_bucket).SyncRoot)
        {
            _bucket.Clear();
            _bucket.Add(lst);
        }
    }

    public void Sort(Comparison<T> comparison)
    {
        var lst = new List<T>(this);
        lst.Sort(comparison);
        lock (((ICollection)_bucket).SyncRoot)
        {
            _bucket.Clear();
            _bucket.Add(lst);
        }
    }

    public void Sort()
    {
        Sort(Comparer<T>.Default);
    }

    //Already implemented as an extension method
    //public T[] ToArray() { }

    public void TrimExcess()
    {
        int threshold = (int)(Capacity * 0.9);
        if (_count < threshold)
        {
            Capacity = _count;
        }
    }

    public bool TrueForAll(Predicate<T> match)
    {
        return _bucket.All(arr => arr.TrueForAll(match));
    }

    #endregion

    #region own stuff

    public double AutoDefragmentThreshold
    {
        get; set;
    }

    public FragmentList<T> GetFragmentRange(int index, int count)
    {
        var info = getInfo(index, count);
        if (info == null) throw new ArgumentOutOfRangeException();
        index -= info.PrevSum;
        var result = new FragmentList<T>();
        for (var i = info.StartArrayIndex; i <= info.EndArrayIndex; i++)
        {
            var arr = _bucket[i];
            var cnt = Math.Min(count, arr.Count - index);
            result._bucket.Add(arr.GetRange(index, cnt));
            count -= cnt;
            if (count == 0) break;
            index = 0;
        }
        return result;
    }

    public override string ToString()
    {
        return $"Count = {Count} \r\n" + string.Join(" ",_bucket.Select(l => l.Count)); ;
    }

    #endregion
}