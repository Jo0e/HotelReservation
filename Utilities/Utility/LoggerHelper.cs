using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Xml.Linq;

namespace Utilities.Utility
{
    public static class LoggerHelper
    {
        private static readonly string LogDirectory = "wwwroot/Logs";
        private static readonly string LogFilePath = Path.Combine(LogDirectory, "AdminActions.json");

        public static async Task LogAdminAction(ILogger logger, string adminName, string action, string entityName)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
                var logEntry = new LogEntry
                {
                    AdminName = adminName,
                    Action = action,
                    EntityName = entityName,
                    Timestamp = DateTime.UtcNow
                };

                List<LogEntry> logEntries;


                if (File.Exists(LogFilePath))
                {
                    var existingLogs = await File.ReadAllTextAsync(LogFilePath);
                    logEntries = JsonSerializer.Deserialize<List<LogEntry>>(existingLogs) ?? new List<LogEntry>();
                }
                else
                {
                    logEntries = new List<LogEntry>();
                }

                logEntries.Add(logEntry);
                var jsonLogs = JsonSerializer.Serialize(logEntries, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(LogFilePath, jsonLogs);

                // Regular logging
                string logMessage = $"Admin: {adminName}, Action: {action}, Entity: {entityName}, Timestamp: {DateTime.UtcNow}";
                logger.LogInformation(logMessage);

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log to another file, display an error message, etc.)
                logger.LogError(ex, $"Failed to log admin action: {action} on {entityName}");
            }
        }
        public class LogEntry
        {
            public string AdminName { get; set; }
            public string Action { get; set; }
            public string EntityName { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
