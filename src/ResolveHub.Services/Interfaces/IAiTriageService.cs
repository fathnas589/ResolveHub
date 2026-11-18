namespace ResolveHub.Services.Interfaces;

using System.Threading;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Ai;

public interface IAiTriageService
{
    /// <summary>True when an Anthropic API key is configured.</summary>
    bool IsEnabled { get; }

    /// <summary>Suggests department, category, subcategory, priority and type for a new ticket.</summary>
    Task<TriageSuggestionDto> SuggestAsync(int companyId, TriageSuggestionRequestDto request, CancellationToken cancellationToken = default);
}
