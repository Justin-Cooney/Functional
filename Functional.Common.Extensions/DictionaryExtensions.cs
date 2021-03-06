﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;

namespace Functional
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class DictionaryExtensions
    {
		public static Option<TValue> TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key)
			=> Option.Create(source.TryGetValue(key, out var value) && value != null, value);

		public static Option<TValue> TryGetValue<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source, TKey key)
			=> Option.Create(source.TryGetValue(key, out var value) && value != null, value);

		public static Option<TValue> TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
			=> Option.Create(source.TryGetValue(key, out var value) && value != null, value);

		public static Option<TValue> TryGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key)
			=> Option.Create(source.TryGetValue(key, out var value) && value != null, value);

		public static Option<TValue> TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source, TKey key)
			=> Option.Create(source.TryRemove(key, out var value) && value != null, value);
	}
}
