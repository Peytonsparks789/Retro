using Retro.Core.Data;

namespace Retro.Infrastructure.Database;

public interface IMediaRepository
{
    public Task UpsertMediaAsync(Media media);
    public Task<List<MediaSummary>> GetMediaSummaryAsync(
        string? search = null,
        int pageSize = 250,
        long lastId = 0,
        bool sortAscending = true);

    public Task<Media?> GetMediaAsync(string id);
    public Task<List<Genre>> GetMediaGenresAsync(string id);
    public Task<Review?> GetMediaReviewAsync(string id);
    public Task<List<Consumption>> GetMediaConsumptionHistoryAsync(string id);
    public Task DeleteMediaAsync(string id);
}