using InfluxDB.Client;

namespace ModMetaDataDownloader.Services;

public class InfluxDbService
{
    public InfluxDbService(IConfiguration configuration)
    {
        InfluxConfig = configuration.GetSection("InfluxDb").Get<InfluxConfig>();
    }

    public InfluxConfig InfluxConfig { get; }

    public void Write(Action<WriteApi> action)
    {
        using var client = new InfluxDBClient(InfluxConfig.Host, InfluxConfig.Token);
        using var write = client.GetWriteApi();
        action(write);
    }

    public async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
    {
        using var client = new InfluxDBClient(InfluxConfig.Host, InfluxConfig.Token);
        var query = client.GetQueryApi();
        return await action(query);
    }
}