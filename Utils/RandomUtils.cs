using System;
using System.Collections.Generic;

namespace CloverAPI.Utils;

public static class RandomUtils
{
	public static IList<T> Pick<T>(this IList<T> list, int count)
	{
		if (count == 1)
		{
			return new List<T> { list.Pick() };
		}
		if (list == null)
		{
			throw new ArgumentNullException(nameof(list), "The list cannot be null.");
		}
		if (list.Count == 0)
		{
			throw new ArgumentException("The list cannot be empty.", nameof(list));
		}
		if (count > list.Count)
		{
			count = list.Count;
		}
		var selectedIndices = new HashSet<int>();
		while (selectedIndices.Count < count)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			selectedIndices.Add(index);
		}
		var result = new List<T>();
		foreach (int index2 in selectedIndices)
		{
			result.Add(list[index2]);
		}
		return result;
	}

	public static T Pick<T>(this IList<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException(nameof(list), "The list cannot be null.");
		}
		if (list.Count == 0)
		{
			throw new ArgumentException("The list cannot be empty.", nameof(list));
		}
		var index = UnityEngine.Random.Range(0, list.Count);
		return list[index];
	}
}
