﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Functional
{
	internal class SelectManyIterator<TSource, TCollection, TResult> : IAsyncEnumerator<TResult>
	{
		private readonly IAsyncEnumerator<TSource> _enumerator;
		private readonly Func<TSource, int, IAsyncEnumerable<TCollection>> _collectionSelector;
		private readonly Func<TSource, TCollection, TResult> _resultSelector;
		private int _count;
		private IAsyncEnumerator<TCollection> _subEnumerator;

		public TResult Current { get; private set; }

		public SelectManyIterator(IAsyncEnumerable<TSource> source, Func<TSource, int, IAsyncEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
		{
			_enumerator = (source ?? throw new ArgumentNullException(nameof(source))).GetEnumerator();
			_collectionSelector = collectionSelector ?? throw new ArgumentNullException(nameof(collectionSelector));
			_resultSelector = resultSelector ?? throw new ArgumentNullException(nameof(resultSelector));
		}

		public async Task<bool> MoveNext()
		{
			while (_subEnumerator == null || !await _subEnumerator.MoveNext())
			{
				if (!await _enumerator.MoveNext())
					return false;

				_subEnumerator = _collectionSelector.Invoke(_enumerator.Current, _count++).GetEnumerator();
			}

			Current = _resultSelector.Invoke(_enumerator.Current, _subEnumerator.Current);

			return true;
		}
	}
}
