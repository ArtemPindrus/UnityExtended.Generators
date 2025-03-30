using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityExtended.Generators.Hierarchy;

namespace UnityExtended.Generators;

public readonly struct EquatableList<T> : IEquatable<EquatableList<T>>, IList<T>, IReadOnlyList<T> {
    private readonly List<T> baseList;

    public T this[int i] {
        get => baseList[i];
        set => baseList[i] = value;
    }

    public int Count => baseList.Count;
    
    public bool IsReadOnly => false;

    public EquatableList() {
        baseList = [];
    }

    public EquatableList(T[] baseArray) {
        baseList = new(baseArray);
    }
    
    public EquatableList(List<T> baseList) {
        this.baseList = new(baseList);
    }

    public static bool operator ==(EquatableList<T> l, EquatableList<T> r) => l.Equals(r);

    public static bool operator !=(EquatableList<T> l, EquatableList<T> r) => !(l == r);

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)baseList).GetEnumerator();

    public override bool Equals(object? obj) {
        return obj is EquatableList<T> other && Equals(other);
    }

    public bool Equals(EquatableList<T> other) {
        if (Count != other.Count) return false;

        for (int i = 0; i < Count; i++) {
            if (!EqualityComparer<T>.Default.Equals(this[i], other[i])) return false;
        }

        return true;
    }

    public override int GetHashCode() {
        return baseList.GetHashCode();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public int IndexOf(T item) => baseList.IndexOf(item);

    public void Insert(int index, T item) => baseList.Insert(index, item);

    public void RemoveAt(int index) => baseList.RemoveAt(index);

    public void Add(T item) => baseList.Add(item);

    public void Clear() => baseList.Clear();

    public bool Contains(T item) => baseList.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => baseList.CopyTo(array, arrayIndex);

    public bool Remove(T item) => baseList.Remove(item);
}

public static class EquatableListExtensions {
    public static EquatableList<T> ToEquatableList<T>(this IEnumerable<T> enumerable) {
        var equatableList = new EquatableList<T>();
        
        foreach (var i in enumerable) {
            equatableList.Add(i);
        }

        return equatableList;
    }
}