using Retro.Core.Data;

namespace Retro.Infrastructure.Database;

public interface IMediaRepository
{
    public Task CreateMediaAsync(Media media);
    public Task<Media> ReadMediaAsync(string id);
    public Task UpdateMediaAsync(Media media);
    public Task DeleteMediaAsync(string id);
}