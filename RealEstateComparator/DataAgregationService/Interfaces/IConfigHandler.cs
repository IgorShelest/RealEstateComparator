namespace DataAggregationService.Services
{
    public interface IConfigHandler
    {
        string GetConnectionString(string connectionStringName);
    }
}