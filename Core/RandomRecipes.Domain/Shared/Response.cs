namespace RandomRecipes.Domain.Shared;

public class Response
{
	public bool IsFailure => !IsSuccess;

	public bool IsSuccess { get; }

	public Error Error { get; }

	protected internal Response(bool isSuccess, Error error)
	{
		if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
		{
			throw new InvalidOperationException();
		}

		IsSuccess = isSuccess;
		Error = error;
	}

	public static Response Success() => new(true, Error.None);

	public static Response<T> Success<T>(T value) => new(value, true, Error.None);

	public static Response Failure(Error error) => new(false, error);

	public static Response<T> Failure<T>(Error error) => new(default, false, error);
}

public class Response<TValue> : Response
{
	private readonly TValue? _value;

	protected internal Response(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
		=> _value = value;

	public TValue Value => IsFailure ?
		throw new InvalidOperationException("The value of failed result can't be accessed.")
		:
		_value!;

	public static implicit operator Response<TValue>(TValue value) => new(value, true, Error.None);
}
