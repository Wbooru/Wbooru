using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.Settings;

namespace Wbooru.Persistence
{
    public class LocalDBContext:DbContext
    {
        private static LocalDBContext _instance;

        public static LocalDBContext Instance => _instance ?? (_instance = new LocalDBContext());

        public LocalDBContext() : base(new DbContextOptionsBuilder().UseSqlite(DBConnectionFactory.GetConnection()).Options)
        {
            Database.Migrate();
        }

        public DbSet<Download> Downloads { get; set; }
        public DbSet<TagRecord> Tags { get; set; }
        public DbSet<GalleryItem> GalleryItems { get; set; }
        public DbSet<VisitRecord> VisitRecords { get; set; }
        public DbSet<GalleryItemMark> ItemMarks { get; set; }

        internal static void BackupDatabase(string to)
        {
            var from = SettingManager.LoadSetting<GlobalSetting>().DBFilePath;
            Log.Info($"Copy sqlite db file from {from} to {to}");

            File.Copy(from, to, true);
        }

        internal static void RestoreDatabase(string from, string to)
        {
            Log.Info($"Copy sqlite db file from {from} to {to}");

            File.Copy(from, to, true);
        }

        private class DatabaseLoggerProvider : ILoggerProvider
        {
            private class DatabaseLogger : ILogger
            {
                public IDisposable BeginScope<TState>(TState state) => null;

                public bool IsEnabled(LogLevel logLevel)
                {
                    if (logLevel != LogLevel.Debug || logLevel != LogLevel.Trace || Setting<GlobalSetting>.Current.EnableOutputDebugMessage)
                        return true;

                    return false;
                }

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                {
                    var message = $"[{logLevel}]  {formatter?.Invoke(state, exception)}";

                    switch (logLevel)
                    {
                        case LogLevel.Trace:
                        case LogLevel.Debug:
                            Wbooru.Log.Debug(message,"Database");
                            break;
                        case LogLevel.None:
                        case LogLevel.Information:
                            Wbooru.Log.Info(message, "Database");
                            break;
                        case LogLevel.Warning:
                            Wbooru.Log.Warn(message, "Database");
                            break;
                        case LogLevel.Error:
                        case LogLevel.Critical:
                            Wbooru.Log.Error(message, "Database");
                            break;
                        default:
                            break;
                    }
                }
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new DatabaseLogger();
            }

            public void Dispose()
            {

            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLoggerFactory(new LoggerFactory(new[] { new DatabaseLoggerProvider() }));
            //optionsBuilder.UseLazyLoadingProxies(true);
        }
    }
}
