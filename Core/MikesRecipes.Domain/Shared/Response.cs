namespace MikesRecipes.Domain.Shared;

public class Response
{
	public bool IsFailure => !IsSuccess;

	public bool IsSuccess { get; }

	public IReadOnlyCollection<Error> Errors { get; }

	protected internal Response(bool isSuccess, Error error)
	{
		if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
		{
            throw new InvalidOperationException("Unable create response.");
        }

		IsSuccess = isSuccess;
		Errors = isSuccess ? Array.Empty<Error>() : new List<Error>() { error };
	}

    protected internal Response(bool isSuccess, IEnumerable<Error> errors)
    {
		if (isSuccess && errors.Any() 
			|| !isSuccess && errors.Distinct().Count() != errors.Count() 
			|| !isSuccess && !errors.Any()
			|| !isSuccess && errors.Contains(Error.None))
		{
            throw new InvalidOperationException("Unable create response.");
        }

        IsSuccess = isSuccess;
        Errors = isSuccess ? Array.Empty<Error>() : new List<Error>(errors);
    }

    public static Response Success() => new(true, Error.None);

	public static Response<T> Success<T>(T value) => new(value, true, Error.None);

	public static Response Failure(Error error) => new(false, error);

	public static Response<T> Failure<T>(Error error) => new(default, false, error);

    public static Response Failure(IEnumerable<Error> errors) => new(false, errors);

    public static Response<T> Failure<T>(IEnumerable<Error> errors) => new(default, false, errors);
}

public class Response<TValue> : Response
{
	private readonly TValue? _value;

	protected internal Response(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
		=> _value = value;

    protected internal Response(TValue? value, bool isSuccess, IEnumerable<Error> errors) : base(isSuccess, errors)
        => _value = value;

    public TValue Value => IsFailure ?
		throw new InvalidOperationException("The value of failed response can't be accessed.")
		:
		_value!;

	public static implicit operator Response<TValue>(TValue value) => new(value, true, Error.None);
}
