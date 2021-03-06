# `Result<TSuccess, TFailure>` Types

`Result<TSuccess, TFailure>` is an immutable type that can either have a `Success` value (of type `TSuccess`), or a `Failure` value (of type `TFailure`). `Result` types should be used in any scenario where code can produce an error. Results are particularly suitable for expected error cases, but can also be used for all error handling. Results force the handling of failures. Instead of throwing exceptions or returning `null`, return a `Failure` result.

## Creating a `Result<TSuccess, TFailure>` Type

### With a success value

```csharp
Result<int, string> success = Result.Success<int, string>(100);
```

### With a failure value

```csharp
Result<int, string> failure = Result.Failure<int, string>("Failure");
```

### Conditionally

```csharp
Result<int, string> success = Result.Create(true, () => 100, () => "Failure");
Result<int, string> failure = Result.Create(false, () => 100, () => "Failure");
```

### With exception handling

```csharp
Result<int, Exception> success = Result.Try(() => 100));
Result<int, Exception> failure = Result.Try<int>(() => throw new Exception("Exception Message")));
Result<int, string> failure = Result.Try<int, string>(() => throw new Exception("Exception Message"), ex => ex.Message));
```

### By combining 2-9 other Results

``` csharp
// produces Result.Success<(int, string), Exception[]> if all Results supplied to Zip have success values
Result<(int, string), Exception[]> success = Result.Zip(
    Result.Success<int, Exception>(1337),
    Result.Success<string, Exception>("the name"));

// produces Result.Failure<(int, string), Exception[]> if at least one Result supplied to Zip has a failure value
Result<(int, string), Exception[]> failure = Result.Zip(
    Result.Success<int, Exception>(1337),
    Result.Failure<string, Exception>(new Exception()));
Result<(int, string), Exception[]> failure = Result.Zip(
    Result.Failure<int, Exception>(new Exception()),
    Result.Success<string, Exception>("the name"));
Result<(int, string), Exception[]> failure = Result.Zip(
    Result.Failure<int, Exception>(new Exception()),
    Result.Failure<string, Exception>(new Exception()));
```

## Working with `Result<TSuccess, TFailure>`

### Match

You cannot access the values of a Result type directly. Instead you work with Results functionally. Results only expose one function with the following signature:

```csharp
public TResult Match(Func<TSuccess, TResult> onSuccess, Func<TFailure, TResult> onFailure)
```

If the Result is a `Success`, then the delegate in the first parameter is invoked and it's result is returned. If the Result is a `Failure`, then the delegate in the second parameter is invoked instead.

```csharp
// Returns "Has success value of 100"
string value = Result.Success<int, string>(100).Match(
    success => $"Has success value of {success}",
    failure => $"Has failure value of {failure}");

// Returns "Has failure value of Failure"
string value = Result.Success<int, string>("Failure").Match(
    success => $"Has success value of {success}",
    failure => $"Has failure value of {failure}");
```

Working with `Match` can be tedious, but there are many extension methods that make using `Result<TSuccess, TFailure`> easy and very powerful.

### Map

If `Success`, this extension will return a `Success` Result with the value produced by the delegate parameter, and if `Failure` it will return a `Failure` Result with the existing failure value.

```csharp
// Returns Result<int, string> with a success value of 1
Result<int, string> result = Result.Success<float, string>(1.5).Map(s => (int)s);

// Returns Result<int, string> with a failure value of "Failure"
Result<int, string> result = Result.Failure<float, string>("Failure").Map(s => (int)s);
```

### TryMap

If `Success`, this extension will execute the first delegate parameter. If an exception is thrown, it will execute the second delegate parameter (if provided), and return a `Failure` Result with the value produced by the second delegate parameter. If no exception is thrown, it will return a `Success` Result with the value produced by the first delegate parameter. If the input Result is a `Failure` it will return a `Failure` Result with the existing failure value.

```csharp
// Returns Result<int, string> with a success value of 1
Result<int, string> result = Result.Success<float, string>(1.5).TryMap(s => (int)s);

// Returns Result<int, Exception> with a failure value of "Exception Message"
Result<int, string> result = Result.Success<float, string>(1.5).TryMap(s => throw new Exception("Exception Message"), ex => ex.Message);

// Returns Result<int, string> with a failure value of "Failure"
Result<int, string> result = Result.Failure<float, string>("Failure").Map(s => throw new Exception("Exception Message"), ex => ex.Message);
```

### Bind

If `Success`, this extension will return the `Result` returned by the delegate parameter, and if `Failure` it will return a `Failure` `Result` with the existing failure value.

```csharp
// Returns Result<int, string> with a success value of 1
Result<int, string> result = Result.Success<float, string>(1.5).Bind(s => Result.Success<int, string>((int)s));

// Returns Result<int, string> with a failure value of "Failure"
Result<int, string> result = Result.Success<float, string>(1.5).Bind(s => Result.Failure<int, string>("Failure"));

// Returns Result<int, string> with a failure value of "Failure"
Result<int, string> result = Result.Failure<float, string>("Failure").Bind(s => Result.Success<int, string>((int)s));
```

### BindOnFailure

If `Success`, this extension will return the original Result.  If `Failure`, it will return the Result returned by the delegate parameter.

```csharp
// Returns Result<float, string> with a success value of 1.5
Result<float, string> result = Result.Success<float, string>(1.5).BindOnFailure(f => 
Result.Success<float, string>(s * 2));

// Returns Result<float, string> with a success value of 1337
Result<float, string> result = Result.Failure<float, string>("Failure").BindOnFailure(f => Result.Success<float, string>(1337));

// Returns Result<float, string> with a failure value of "More failure"
Result<float, string> result = Result.Failure<float, string>("Failure").BindOnFailure(f => Result.Failure<float, string>("More failure"));
```

### IsSuccess

If `Success`, this extension will return `true`, and if `Failure` it will return `false`.

```csharp
bool value = Result.Success<int, string>(100).IsSuccess(); // Returns true
bool value = Result.Failure<int, string>("Failure").IsSuccess(); // Returns false
```

### Success

If `Success`, this extension will return an `Option` containing `Some` success value, and if `Failure` it will return `None`.

```csharp
// Returns Option<int> with a value of 100
Option<int> option = Result.Success<int, string>(100).Success();

// Returns Option<int> with no value
Option<int> option = Result.Failure<int, string>("Failure").Success();
```

### Failure

If `Success`, this extension will return `None`, and if `Failure` it will return an `Option` containing `Some` failure value.

```csharp
// Returns Option<string> with no value
Option<string> option = Result.Success<int, string>(100).Failure();

// Returns Option<string> with a value of "Failure"
Option<string> option = Result.Failure<int, string>("Failure").Failure();
```

### ThrowOnFailure

If `Success`, this extension will return the success value, and if `Failure` an exception will be thrown.

```csharp
int value = Result.Success<int, string>(1337).ThrowOnFailure(errorMessage => new InvalidOperationException(errorMessage)); // Returns 1337
Result.Failure<int, string>("Failure").ThrowOnFailure(errorMessage => new InvalidOperationException(errorMessage)); // Throws an exception
```

### Where

If `Success`, this extension will invoke the first delegate parameter and if `true` is returned it will return `Success` of the input success value. If `false` is returns from the first delegate parameter it will return a `Failure` Result with the failure value produced by the second delegate parameter. If the input Result is a `Failure` it will return a `Failure` Result with the existing failure value.

```csharp
// Returns Result<int, string> with a success value of 100
Result<int, string> result = Result.Success<int, string>(100).Where(s => true, s => $"Failed on value {v}");

// Returns Result<int, string> with a failure value of "Failed on value 100"
Result<int, string> result = Result.Success<int, string>(100).Where(s => false, s => $"Failed on value {v}");

// Returns Result<int, string> with a failure value of "Failure"
Result<int, string> result = Result.Failure<int, string>("Failure").Where(s => true, s => $"Failed on value {v}");
```

### MapFailure

If `Success`, this extension will return a `Success` Result with the existing success value, and if `Failure` it will return a `Failure` Result with the value produced by the delegate parameter.

```csharp
// Returns Result<int, int> with a success value of 100
Result<int, int> result = Result.Success<int, string>(100).MapFailure(f => f.Length);

// Returns Result<int, int> with a failure value of 7
Result<int, int> result = Result.Failure<int, string>("Failure").MapFailure(f => f.Length);
```

### Do

This extension returns the input Result and is meant only to create side effects. If `Success`, this extension will invoke the first delegate parameter, and if `Failure` it will invoke the second delegate parameter (if provided).

```csharp
 // Outputs "100" to the console and returns Result<int, string> with a success value of 100
Result<int, string> result = Result.Success<int, string>(100).Do(s => Console.WriteLine(s));

// Returns Result<int, string> with a failure value of "Failure"
Result<int, string> result = Result.Failure<int, string>("Failure").Do(s => Console.WriteLine(s));

// Outputs "Failure" to the console and returns Result<int, string> with a failure value of "Failure"
Result<int, string> result = Result.Failure<int, string>("Failure").Do(s => Console.WriteLine(s), f => Console.WriteLine(f));
```

### Apply

This extension returns void and is meant only to create side effects. If `Success`, this extension will invoke the first delegate parameter, and if `Failure` it will invoke the second delegate parameter (if provided).

```csharp
// Outputs "100" to the console
Result.Success<int, string>(100).Do(s => Console.WriteLine(s));

// Does nothing
Result.Failure<int, string>("Failure").Do(s => Console.WriteLine(s));

// Outputs "Failure" to the console
Result.Failure<int, string>("Failure").Do(s => Console.WriteLine(s), f => Console.WriteLine(f));
```

### Transpose

This extension returns a `Result` with `Success` and `Failure` values reversed.

```csharp
// Returns Result<string, int> with a failure value of 100
Result.Success<int, string>(100).Transpose();

// Returns Result<string, int> with a success value of "message"
Result.Failure<int, string>("message").Transpose();
```

### MapOnSome

This extension is only available for `Result<Option<T>, TFailure>` types.

- If `Success` and holds an Option with a value, this extension will invoke the delegate parameter.
- If `Success` and holds an Option with no value, the delegate parameter will *not* be invoked, but this extension method will return a `Success` Result holding an Option with no value for the type that would have been produced by the delegate parameter.
- If `Failure` it will return a `Failure` Result with the existing failure value.

```csharp
// Returns Result<Option<float>, string> with a success value of Option.Some(50f)
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).MapOnSome(i => i / 2f);

// Returns Result<Option<float>, string> with a success value of Option.None<float>
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).MapOnSome(i => i / 2f);

// Returns Result<Option<float>, string> with a failure value of "Failure"
Result<Option<float>, string> result = Result.Failure<Option<int>, string>("Failure").MapOnSome(i => i / 2f);
```

### BindOnSome

This extension is only available for `Result<Option<T>, TFailure>` types.

- If `Success` and holds an Option with a value, this extension will return the Result returned by the delegate parameter.
- If `Success` and holds an Option with no value, this extension will return a Result holding an Option with no value matching the type that would be produced by the delegate parameter.
- If `Failure` it will return a `Failure` Result with the existing failure value.

You can either bind to functions producing `Result<TSuccess, TFailure>` or `Result<Option<T>, TFailure>`.

```csharp
// bind to functions producing Result<TSuccess, TFailure>

// Returns Result<Option<float>, string> with a success value of Option.Some(200f)
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).BindOnSome(i => Result.Success<float, string>(i * 2f));

// Returns Result<Option<float>, string> with a failure value of "Failure"
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).BindOnSome(i => Result.Failure<float, string>("Failure"));

// Returns Result<Option<float>, string> with a success value of Option.None<float>
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).BindOnSome(i => Result.Success<float, string>(i * 2f));

// Returns Result<Option<float>, string> with a success value of Option.None<float>
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).BindOnSome(i => Result.Failure<float, string>("Failure"));

// Returns Result<Option<float>, string> with a failure value of "Failure"
Result<Option<float>, string> result = Result.Failure<Option<int>, string>("Failure").BindOnSome(i => Result.Success<float, string>(i * 2f));
```

```csharp
// bind to functions producing Result<Option<T>>, TFailure>

// Returns Result<Option<float>, string> with a success value of Option.Some(200f)
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).BindOnSome(i => Result.Success<Option<float>, string>(Option.Some(i * 2f)));

// Returns Result<Option<float>, string> with a failure value of "Failure"
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).BindOnSome(i => Result.Failure<Option<float>, string>("Failure"));

// Returns Result<Option<float>, string> with a success value of Option.None<float>
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).BindOnSome(i => Result.Success<Option<float>, string>(Option.Some(i * 2f)));

// Returns Result<Option<float>, string> with a success value of Option.None<float>
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).BindOnSome(i => Result.Failure<Option<float>, string>("Failure"));

// Returns Result<Option<float>, string> with a failure value of "Failure"
Result<Option<float>, string> result = Result.Failure<Option<int>, string>("Failure").BindOnSome(i => Result.Success<Option<float>, string>(Option.Some(i * 2f)));
```

### MapOnNone

This extension is only available for `Result<Option<T>, TFailure>` types.

- If `Success` and holds an Option with a value, this extension will return a `Success` `Result` with the existing success value.
- If `Success` and holds an `Option` with no value, it will invoke the delegate parameter, producing a `Success` `Result` holding an `Option` containing the value produced by the delegate.
- If `Failure` it will return a `Failure` `Result` with the existing failure value.

```csharp
// Returns Result<Option<int>, string> with a success value of Option.None<int>
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).MapOnNone(() => 1337);

// Returns Result<Option<int>, string> with a success value of Option.Some(1337)
Result<Option<float>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).MapOnNone(() => 1337);

// Returns Result<Option<int>, string> with a failure value of "Failure"
Result<Option<float>, string> result = Result.Failure<Option<int>, string>("Failure").MapOnNone(() => 1337);
```

### BindOnNone

This extension is only available for `Result<Option<T>, TFailure>` types.

- If `Success` and holds an `Option` with a value, it will return a `Success` `Result` with the existing success value.
- If `Success` and holds an `Option` with no value, it will invoke the delegate parameter.  
- If `Failure` it will return a `Failure` `Result` with the existing failure value.

You can either bind to functions producing `Result<TSuccess, TFailure>` or `Result<Option<T>, TFailure>`.

```csharp
// bind to functions producing Result<TSuccess, TFailure>

// Returns Result<Option<int>, string> with a success value of Option.Some(100)
Result<Option<int>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).BindOnNone(()) => Result.Success<int, string>(i * 2));

// Returns Result<Option<int>, string> with a success value of Option.Some(100)
Result<Option<int>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).BindOnNone(() => Result.Failure<int, string>("Failure"));

// Returns Result<Option<int>, string> with a success value of Option.None<float>
Result<Option<int>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).BindOnNone(() => Result.Success<int, string>(i * 2));

// Returns Result<Option<float>, string> with a success value of Option.None<float>
Result<Option<int>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).BindOnNone(() => Result.Failure<int, string>("Failure"));

// Returns Result<Option<float>, string> with a failure value of "Failure"
Result<Option<int>, string> result = Result.Failure<Option<int>, string>("Failure").BindOnNone(() => Result.Success<int, string>(i * 2));
```

```csharp
// bind to functions producing Result<Option<T>>, TFailure>

// Returns Result<Option<float>, string> with a success value of Option.Some(200f)
Result<Option<int>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).BindOnNone(() => Result.Success<Option<float>, string>(Option.Some(i * 2f)));

// Returns Result<Option<float>, string> with a failure value of "Failure"
Result<Option<int>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).BindOnNone(() => Result.Failure<Option<float>, string>("Failure"));

// Returns Result<Option<float>, string> with a success value of Option.None<float>
Result<Option<int>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).BindOnNone(() => Result.Success<Option<float>, string>(Option.Some(i * 2f)));

// Returns Result<Option<float>, string> with a success value of Option.None<float>
Result<Option<int>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).BindOnNone(() => Result.Failure<Option<float>, string>("Failure"));

// Returns Result<Option<float>, string> with a failure value of "Failure"
Result<Option<int>, string> result = Result.Failure<Option<int>, string>("Failure").BindOnNone(() => Result.Success<Option<float>, string>(Option.Some(i * 2f)));
```

### DoOnSome

This extension is only available for `Result<Option<T>, TFailure>` types.  It returns the input `Result` and is meant only to create side effects.  If `Success` and holds an `Option` with a value, this extension will invoke the delegate parameter.

```csharp
// Outputs "100" to the console and returns Result<Option<int>, string> with a success value of Option.Some(100)
Result<Option<int>, string> result = Result.Success<Option<int>, string>(Option.Some(100)).DoOnSome(i => Console.WriteLine(i));

// Does nothing and returns Result<Option<int>, string> with a success value of Option.None<int>
Result<Option<int>, string> result = Result.Success<Option<int>, string>(Option.None<int>()).DoOnSome(i => Console.WriteLine(i));

// Does nothing and returns Result<Option<int>, string> with a failure value of "Failure"
Result<Option<int>, string> result = Result.Failure<Option<int>, string>("Failure").DoOnSome(i => Console.WriteLine(i));
```

### ApplyOnSome

This extension is only available for `Result<Option<T>, TFailure>` types.  It returns void and is meant only to create side effects.  If `Success` and holds an `Option` with a value, this extension will invoke the delegate parameter; in all other cases, it does nothing.

```csharp
// Outputs "100" to the console
Result.Success<Option<int>, string>(Option.Some(100)).ApplyOnSome(i => Console.WriteLine(i));

// Does nothing
Result.Success<Option<int>, string>(Option.None<int>()).ApplyOnSome(i => Console.WriteLine(i));

// Does nothing
Result.Failure<Option<int>, string>("Failure").ApplyOnSome(i => Console.WriteLine(i));
```

### Evert

This extension is only available for `Option<Result<TSuccess, TFailure>>` types.

- If `Some` and holds a `Success` `Result`, it returns a `Success` `Result` holding an `Option` with a value.
- If `Some` and holds a `Failure` `Result`, it returns a `Failure` `Result` with the original failure.
- If `None`, it returns a `Success` `Result` holding an `Option` with no value.

``` csharp
// Returns Result<Option<int>, string> with a success value of Option.Some(1337)
Option.Some(Result.Success<int, string>(1337)).Evert();

// Returns Result<Option<int>, string> with a success value of Option.None<int>
Option.None<Result<int, string>>().Evert();

// Returns Result<Option<int>, string> with a failure value of "error"
Option.Some(Result.Failure<int, string>("error")).Evert();
```
