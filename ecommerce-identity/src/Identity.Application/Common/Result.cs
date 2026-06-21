namespace Identity.Application.Common;

/// <summary>
/// BlogPlatform'daki Result deseninin aynısı: başarı/hata bilgisini
/// istisna fırlatmadan, açık biçimde taşıyan zarf.
/// </summary>
public class Result
{
    public bool Succeeded { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static Result Success() => new() { Succeeded = true };

    public static Result Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors.ToList() };

    public static Result Failure(string error) =>
        new() { Succeeded = false, Errors = [error] };
}

public class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Success(T data) =>
        new() { Succeeded = true, Data = data };

    public static new Result<T> Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors.ToList() };

    public static new Result<T> Failure(string error) =>
        new() { Succeeded = false, Errors = [error] };
}
