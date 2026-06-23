namespace NexXSensi.Models;

public sealed record ActionResult(bool Success, string Output)
{
    public static ActionResult Ok(string output = "") => new(true, output);
    public static ActionResult Fail(string output) => new(false, output);
}
