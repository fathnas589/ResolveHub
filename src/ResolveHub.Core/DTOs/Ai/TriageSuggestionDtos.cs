namespace ResolveHub.Core.DTOs.Ai;

using System;

public class TriageSuggestionRequestDto
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>AI-suggested routing for a new ticket. Every id refers to a real, active catalog entry.</summary>
public class TriageSuggestionDto
{
    public Guid DepartmentGId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid CategoryGId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? SubCategoryGId { get; set; }
    public string? SubCategoryName { get; set; }
    public string Priority { get; set; } = "MEDIUM";
    public string TicketType { get; set; } = "SERVICE_REQUEST";
    /// <summary>One short sentence explaining the suggestion, shown to the user.</summary>
    public string Reason { get; set; } = string.Empty;
}
