using Retro.Core.Data;

namespace Retro.Infrastructure.Database;

public interface IMediaRepository
{
    public Task UpsertMediaAsync(Media media);
    public Task<List<MediaSummary>> GetMediaSummaryAsync(
        string search,
        int pageSize = 250,
        int lastId = 0);
    public Task DeleteMediaAsync(string id);
}