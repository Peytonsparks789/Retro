using Retro.Core.Data;

namespace Retro.Infrastructure.Database;

public interface IMediaRepository
{
    public Task UpsertMediaAsync(Media media);
    public Task<List<MediaSummary>> GetMediaSummaryAsync(
        string search,
        int pageSize = 250,
        int lastId = 0);

    public Task<Media?> GetMediaAsync(string id);
    public Task<List<Genre>> GetMediaGenresAsync(string id);
    public Task<Review?> GetMediaReviewAsync(string id);
    public Task<List<DateTime>> GetMediaConsumptionHistoryAsync(string id);
    public Task DeleteMediaAsync(string id);
}