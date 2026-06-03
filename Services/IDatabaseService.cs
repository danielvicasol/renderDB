namespace RenderDB.Services
{
    public interface IDatabaseService
    {
        Task<(bool success, string message, DateTime? serverTime)> VerifyConnectionAsync();
        Task<(bool success, string message, int? recordId)> InsertRecordAsync(string nombre, string email);
        Task<(bool success, string message, List<DemoRecord>? records)> GetAllRecordsAsync();
        Task InitializeAsync();
    }

    public class DemoRecord
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
