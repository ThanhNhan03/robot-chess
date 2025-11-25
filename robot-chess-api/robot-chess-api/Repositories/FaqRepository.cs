using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public class FaqRepository : IFaqRepository
{
    private readonly PostgresContext _context;

    public FaqRepository(PostgresContext context)
    {
        _context = context;
    }

    public async Task<List<Faq>> GetAllFaqsAsync(bool includeUnpublished = false)
    {
        var query = _context.Faqs.AsQueryable();

        if (!includeUnpublished)
        {
            query = query.Where(f => f.IsPublished);
        }

        return await query
            .OrderBy(f => f.DisplayOrder)
            .ThenByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<Faq?> GetFaqByIdAsync(Guid id)
    {
        return await _context.Faqs.FindAsync(id);
    }

    public async Task<List<Faq>> GetFaqsByCategoryAsync(string category)
    {
        return await _context.Faqs
            .Where(f => f.IsPublished && f.Category == category)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Faq> CreateFaqAsync(Faq faq)
    {
        faq.CreatedAt = DateTime.UtcNow;
        faq.UpdatedAt = DateTime.UtcNow;
        _context.Faqs.Add(faq);
        await _context.SaveChangesAsync();
        return faq;
    }

    public async Task<Faq> UpdateFaqAsync(Faq faq)
    {
        faq.UpdatedAt = DateTime.UtcNow;
        _context.Faqs.Update(faq);
        await _context.SaveChangesAsync();
        return faq;
    }

    public async Task<bool> DeleteFaqAsync(Guid id)
    {
        var faq = await _context.Faqs.FindAsync(id);
        if (faq == null) return false;

        _context.Faqs.Remove(faq);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _context.Faqs
            .Where(f => f.IsPublished && f.Category != null)
            .Select(f => f.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
}
