namespace NexXSensi.Models;

public sealed record OptimizationAction(
    string Icon,
    string Title,
    string Description,
    string Risk,
    Func<Task<ActionResult>> Runner,
    string Accent,
    bool Heavy = false);
