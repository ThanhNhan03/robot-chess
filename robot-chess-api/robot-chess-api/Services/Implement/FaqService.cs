using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Repositories;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Services.Implement;

public class FaqService : IFaqService
{
    private readonly IFaqRepository _faqRepository;

    public FaqService(IFaqRepository faqRepository)
    {
        _faqRepository = faqRepository;
    }

    public async Task<List<FaqDto>> GetAllFaqsAsync(bool includeUnpublished = false)
    {
        var faqs = await _faqRepository.GetAllFaqsAsync(includeUnpublished);
        return faqs.Select(MapToDto).ToList();
    }

    public async Task<FaqDto?> GetFaqByIdAsync(Guid id)
    {
        var faq = await _faqRepository.GetFaqByIdAsync(id);
        return faq == null ? null : MapToDto(faq);
    }

    public async Task<List<FaqDto>> GetFaqsByCategoryAsync(string category)
    {
        var faqs = await _faqRepository.GetFaqsByCategoryAsync(category);
        return faqs.Select(MapToDto).ToList();
    }

    public async Task<FaqDto> CreateFaqAsync(CreateFaqDto dto)
    {
        var faq = new Faq
        {
            Id = Guid.NewGuid(),
            Question = dto.Question,
            Answer = dto.Answer,
            Category = dto.Category,
            IsPublished = dto.IsPublished,
            DisplayOrder = dto.DisplayOrder ?? 0
        };

        var createdFaq = await _faqRepository.CreateFaqAsync(faq);
        return MapToDto(createdFaq);
    }

    public async Task<FaqDto> UpdateFaqAsync(Guid id, UpdateFaqDto dto)
    {
        var faq = await _faqRepository.GetFaqByIdAsync(id);
        if (faq == null)
        {
            throw new KeyNotFoundException($"FAQ with ID {id} not found");
        }

        if (dto.Question != null) faq.Question = dto.Question;
        if (dto.Answer != null) faq.Answer = dto.Answer;
        if (dto.Category != null) faq.Category = dto.Category;
        if (dto.IsPublished.HasValue) faq.IsPublished = dto.IsPublished.Value;
        if (dto.DisplayOrder.HasValue) faq.DisplayOrder = dto.DisplayOrder.Value;

        var updatedFaq = await _faqRepository.UpdateFaqAsync(faq);
        return MapToDto(updatedFaq);
    }

    public async Task<bool> DeleteFaqAsync(Guid id)
    {
        return await _faqRepository.DeleteFaqAsync(id);
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _faqRepository.GetCategoriesAsync();
    }

    private static FaqDto MapToDto(Faq faq)
    {
        return new FaqDto
        {
            Id = faq.Id,
            Question = faq.Question,
            Answer = faq.Answer,
            Category = faq.Category,
            IsPublished = faq.IsPublished,
            DisplayOrder = faq.DisplayOrder,
            CreatedAt = faq.CreatedAt,
            UpdatedAt = faq.UpdatedAt
        };
    }
}
