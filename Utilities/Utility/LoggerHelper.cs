﻿using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Xml.Linq;

namespace Utilities.Utility
{
    public static class LoggerHelper
    {
        private static readonly string ProjectRootDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
        private static readonly string LogDirectory = Path.Combine(ProjectRootDirectory, "Utilities", "Logs");
        private static readonly string LogFilePath = Path.Combine(LogDirectory, "AdminActions.json");

        public static void LogAdminAction(ILogger logger,string adminId, string adminName, string action, string entityName)
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
            var logEntry = new LogEntry
            {
                AdminId = adminId,
                AdminName = adminName,
                Action = action,
                EntityName = entityName,
                Timestamp = DateTime.UtcNow
            };

            List<LogEntry> logEntries;


            if (File.Exists(LogFilePath))
            {
                var existingLogs = File.ReadAllText(LogFilePath);
                logEntries = JsonSerializer.Deserialize<List<LogEntry>>(existingLogs) ?? new List<LogEntry>();
            }
            else
            {
                logEntries = new List<LogEntry>();
            }

            logEntries.Add(logEntry);
            var jsonLogs = JsonSerializer.Serialize(logEntries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(LogFilePath, jsonLogs);

            // Regular logging
            string logMessage = $"Admin: {adminName}, Action: {action}, Entity: {entityName}, Timestamp: {DateTime.UtcNow}";
            logger.LogInformation(logMessage);

        }
    }
    public class LogEntry
    {
        public string AdminId { get; set; }
        public string AdminName { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
