using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.Settings;
using Wbooru.Utils;

namespace Wbooru.Persistence
{
    public class LocalDBContext : DbContext
    {
        public LocalDBContext() : this(null, true)
        {

        }

        public LocalDBContext(DbContextOptions option, bool migrate) : base(option ?? DBConnectionFactory.GetDbContextOptions())
        {
            if (migrate)
                Database.Migrate();
        }

        public DbSet<Download> Downloads { get; set; }
        public DbSet<TagRecord> Tags { get; set; }
        public DbSet<GalleryItem> GalleryItems { get; set; }
        public DbSet<VisitRecord> VisitRecords { get; set; }
        public DbSet<GalleryItemMark> ItemMarks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (Setting<GlobalSetting>.Current.EnableDatabaseLog)
                optionsBuilder.UseLoggerFactory(new LoggerFactory(new[] { new DatabaseLoggerProvider() }));
        }

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

        internal static async Task CombineDatabase(string fromDBFilePath, Action<string> reportCallback = default, DatabaseCombinePart combinePart = DatabaseCombinePart.All)
        {
            reportCallback = reportCallback ?? ((x) => { });
            var dbOption = new DbContextOptionsBuilder().UseSqlite(new SQLiteConnectionStringBuilder()
            {
                DataSource = fromDBFilePath,
                ForeignKeys = false
            }.ConnectionString).Options;
            var fromDBContext = new LocalDBContext(dbOption, false);
            await fromDBContext.Database.OpenConnectionAsync();
            await fromDBContext.Database.MigrateAsync();

            reportCallback("开始数据库...");
            var changed = 0;

            if (combinePart.HasFlag(DatabaseCombinePart.Downloads))
            {
                reportCallback("开始合并下载列表数据...");
                await PostDbAction(async x =>
                {
                    var diff = (fromDBContext.Downloads as IEnumerable<Download>).Where(t => !x.Downloads.Contains(t)).ToList();
                    await x.Downloads.AddRangeAsync(diff);
                    changed = diff.Count;
                    return x.SaveChangesAsync();
                });
                await Task.Delay(2000);
                reportCallback($"下载列表数据合并完成(+{changed})");
            }

            if (combinePart.HasFlag(DatabaseCombinePart.Tags))
            {
                reportCallback("开始合并标签数据...");
                await PostDbAction(async x =>
                {
                    var diff = (fromDBContext.Tags as IEnumerable<TagRecord>).Where(t => !x.Tags.Contains(t)).ToList();
                    await x.Tags.AddRangeAsync(diff);
                    changed = diff.Count;
                    return x.SaveChangesAsync();
                });
                await Task.Delay(2000);
                reportCallback($"标签数据合并完成(+{changed})");
            }

            if (combinePart.HasFlag(DatabaseCombinePart.GalleryItems))
            {
                reportCallback("开始合并图片数据...");
                await PostDbAction(async x =>
                {
                    var diff = (fromDBContext.GalleryItems as IEnumerable<GalleryItem>).Where(t => !x.GalleryItems.Contains(t)).ToList();
                    await x.GalleryItems.AddRangeAsync(diff);
                    changed = diff.Count;
                    return x.SaveChangesAsync();
                });
                await Task.Delay(2000);
                reportCallback($"图片数据合并完成(+{changed})");
            }

            if (combinePart.HasFlag(DatabaseCombinePart.ItemMarks))
            {
                reportCallback("开始合并本地收藏数据...");
                await PostDbAction(async x =>
                {
                    var diff = (fromDBContext.ItemMarks as IEnumerable<GalleryItemMark>).Where(t => !x.ItemMarks.Contains(t)).ToList();
                    await x.ItemMarks.AddRangeAsync(diff);
                    changed = diff.Count;
                    return x.SaveChangesAsync();
                });
                await Task.Delay(2000);
                reportCallback($"本地收藏数据合并完成(+{changed})");
            }

            if (combinePart.HasFlag(DatabaseCombinePart.VisitRecords))
            {
                reportCallback("开始合并浏览历史数据...");
                await PostDbAction(async x =>
                {
                    var diff = (fromDBContext.VisitRecords as IEnumerable<VisitRecord>).Where(t => !x.VisitRecords.Contains(t)).ToList();
                    await x.VisitRecords.AddRangeAsync(diff);
                    changed = diff.Count;
                    return x.SaveChangesAsync();
                });
                await Task.Delay(2000);
                reportCallback($"浏览历史数据合并完成(+{changed})");
            }

            reportCallback("合并完成!");
        }

        internal static bool CheckIfUsingOldDatabase()
        {
            using var context = new LocalDBContext(null, false);
            var db = context.Database;
            using var command = db.GetDbConnection().CreateCommand();

            command.CommandText = "select * from __MigrationHistory";
            db.OpenConnection();
            var count = -1;

            try
            {
                using var reader = command.ExecuteReader();
                count = reader.FieldCount;
            }
            catch
            {
                count = -1;
            }

            return count >= 0;
        }

        internal static async Task<bool> UpdateOldDatabase()
        {
            var _cache_params = new Dictionary<(int, int), string>();

            var oldDBPath = Setting<GlobalSetting>.Current.DBFilePath;
            var tempDBPath = Path.GetTempFileName();
            var backupDBPath = oldDBPath + ".backup";

            if (!File.Exists(backupDBPath))
            {
                Log.Info($"Backup old db file:{backupDBPath}");
                File.Copy(oldDBPath, backupDBPath);
            }

            Log.Info($"Copy old db file:{tempDBPath}");
            File.Copy(oldDBPath, tempDBPath, true);

            var tempDBOption = new DbContextOptionsBuilder().UseSqlite(new SQLiteConnectionStringBuilder()
            {
                DataSource = tempDBPath,
                ForeignKeys = false
            }.ConnectionString).Options;
            using var targetDBContext = new DbContext(tempDBOption);
            await targetDBContext.Database.OpenConnectionAsync();
            using var targetConnection = targetDBContext.Database.GetDbConnection();

            var tables = new[]
            {
                "__MigrationHistory","Downloads","GalleryItemMarks","ShadowGalleryItems","TagRecords","VisitRecords","sqlite_sequence"
            };

            //drop tables
            foreach (var table in tables)
            {
                try
                {
                    using var command = targetConnection.CreateCommand();
                    command.CommandText = $"drop TABLE {table}";
                    var result = command.ExecuteReader();

                    Log.Info($"delete table {table},affected count :{result.RecordsAffected}");
                }
                catch (Exception e)
                {
                    Log.Warn($"delete table {table} failed :{e.Message}");
                }
            }

            Log.Info($"begin migrate and update...");
            var wrapContext = new LocalDBContext(tempDBOption, false);
            await wrapContext.Database.OpenConnectionAsync();
            await wrapContext.Database.MigrateAsync();
            Log.Info($"migration finished...");

            var oldDBOption = new DbContextOptionsBuilder().UseSqlite(new SQLiteConnectionStringBuilder()
            {
                DataSource = oldDBPath,
                ForeignKeys = false,
            }.ConnectionString).Options;
            using var oldDBContext = new DbContext(oldDBOption);
            await oldDBContext.Database.OpenConnectionAsync();
            var oldDBConnection = oldDBContext.Database.GetDbConnection();

            const int BATCH_SIZE = 5;

            //Downloads
            try
            {
                using var command = oldDBConnection.CreateCommand();
                command.CommandText = $"select * from Downloads";
                var reader = command.ExecuteReader();

                var i1 = reader.GetOrdinal(nameof(Download.DownloadId));
                var i2 = reader.GetOrdinal(nameof(Download.TotalBytes));
                var i3 = reader.GetOrdinal(nameof(Download.DownloadStartTime));
                var i4 = reader.GetOrdinal(nameof(Download.DownloadUrl));
                var i5 = reader.GetOrdinal(nameof(Download.FileName));
                var i6 = reader.GetOrdinal(nameof(Download.DownloadFullPath));
                var i7 = reader.GetOrdinal(nameof(Download.DisplayDownloadedLength));
                var i8 = reader.GetOrdinal("GalleryItem_ID");

                foreach (var set in reader.MakeEnumerable(x => new object[] {
                    reader.GetInt32(i1),
                        reader.GetInt32(i2),
                        reader.GetString(i3),
                        reader.GetString(i4),
                        reader.GetInt32(i8),
                        reader.GetString(i5),
                        reader.GetString(i6),
                        reader.GetInt32(i7)
                }).SequenceWrap(BATCH_SIZE))
                {
                    var cmd = "INSERT INTO \"main\".\"Downloads\"(\"DownloadId\",\"TotalBytes\",\"DownloadStartTime\",\"DownloadUrl\",\"GalleryItemID\",\"FileName\",\"DownloadFullPath\",\"DisplayDownloadedLength\") VALUES "
                        + GetParamString(set.Count(), set.First().Length);

                    var insertResult = await wrapContext.Database.ExecuteSqlRawAsync(cmd, set.SelectMany(x => x));
                }

                Log.Info("migrating table Downloads is finished.");
            }
            catch (Exception e)
            {
                Log.Error("Migrate table Downloads failed:" + e.Message);
                return false;
            }

            //ItemMarks
            try
            {
                using var command = oldDBConnection.CreateCommand();
                command.CommandText = $"select * from GalleryItemMarks";
                var reader = command.ExecuteReader();

                var i1 = reader.GetOrdinal(nameof(GalleryItemMark.GalleryItemMarkID));
                var i2 = reader.GetOrdinal(nameof(GalleryItemMark.Time));
                var i3 = reader.GetOrdinal("Item_ID");

                foreach (var set in reader.MakeEnumerable(x => new object[] {
                    reader.GetInt32(i1),
                    reader.GetString(i2),
                    reader.GetInt32(i3)
                }).SequenceWrap(BATCH_SIZE))
                {
                    var cmd = "INSERT INTO \"main\".\"ItemMarks\"(\"GalleryItemMarkID\",\"Time\",\"GalleryItemID\") VALUES "
                        + GetParamString(set.Count(), set.First().Length);

                    var insertResult = await wrapContext.Database.ExecuteSqlRawAsync(cmd, set.SelectMany(x => x));
                }

                Log.Info("migrating table ItemMarks is finished.");
            }
            catch (Exception e)
            {
                Log.Error("Migrate table ItemMarks failed:" + e.Message);
                return false;
            }

            //GalleryItems
            try
            {
                using var command = oldDBConnection.CreateCommand();
                command.CommandText = $"select * from ShadowGalleryItems";
                var reader = command.ExecuteReader();

                var i1 = reader.GetOrdinal("ID");
                var i2 = reader.GetOrdinal(nameof(GalleryItem.PreviewImageDownloadLink));
                var i3 = reader.GetOrdinal(nameof(GalleryItem.DownloadFileName));
                var i4 = reader.GetOrdinal(nameof(GalleryItem.GalleryItemID));
                var i5 = reader.GetOrdinal(nameof(GalleryItem.GalleryName));
                var i6 = reader.GetOrdinal("PreviewImageWidth");
                var i7 = reader.GetOrdinal("PreviewImageHeight");
                var i8 = reader.GetOrdinal(nameof(GalleryItem.DetailLink));

                foreach (var set in reader.MakeEnumerable(x => new object[] {
                    reader.GetInt32(i1),
                    reader.GetInt32(i6),
                    reader.GetInt32(i7),
                    reader.GetString(i8),
                    reader.GetString(i2),
                    reader.GetString(i3),
                    reader.GetString(i5),
                    reader.GetString(i4)
                }).SequenceWrap(BATCH_SIZE))
                {
                    var cmd = "INSERT INTO \"main\".\"GalleryItems\"(\"ID\",\"PreviewImageSize_Width\",\"PreviewImageSize_Height\",\"DetailLink\",\"PreviewImageDownloadLink\",\"DownloadFileName\",\"GalleryName\",\"GalleryItemID\") VALUES "
                        + GetParamString(set.Count(), set.First().Length);

                    var insertResult = await wrapContext.Database.ExecuteSqlRawAsync(cmd, set.SelectMany(x => x));
                }

                Log.Info("migrating table GalleryItems is finished.");
            }
            catch (Exception e)
            {
                Log.Error("Migrate table GalleryItems failed:" + e.Message);
                return false;
            }

            // Tags
            try
            {
                using var command = oldDBConnection.CreateCommand();
                command.CommandText = $"select * from TagRecords";
                var reader = command.ExecuteReader();

                var i1 = reader.GetOrdinal(nameof(TagRecord.TagID));
                var i2 = reader.GetOrdinal("Tag_Name");
                var i3 = reader.GetOrdinal("Tag_Type");
                var i4 = reader.GetOrdinal(nameof(TagRecord.AddTime));
                var i5 = reader.GetOrdinal(nameof(TagRecord.FromGallery));
                var i6 = reader.GetOrdinal(nameof(TagRecord.RecordType));

                foreach (var set in reader.MakeEnumerable(x => new object[] {
                    reader.GetInt32(i1),
                        reader.GetString(i2),
                        reader.GetInt32(i3),
                        reader.GetString(i4),
                        reader.GetString(i5),
                        reader.GetInt32(i6)
                }).SequenceWrap(BATCH_SIZE))
                {
                    var cmd = "INSERT INTO \"main\".\"Tags\"(\"TagID\",\"Tag_Name\",\"Tag_Type\",\"AddTime\",\"FromGallery\",\"RecordType\") VALUES "
                        + GetParamString(set.Count(), set.First().Length);

                    var insertResult = await wrapContext.Database.ExecuteSqlRawAsync(cmd, set.SelectMany(x => x));
                }

                Log.Info("migrating table Tags is finished.");
            }
            catch (Exception e)
            {
                Log.Error("Migrate table Tags failed:" + e.Message);
                return false;
            }


            // VisitRecords
            try
            {
                using var command = oldDBConnection.CreateCommand();
                command.CommandText = $"select * from VisitRecords";
                var reader = command.ExecuteReader();

                var i1 = reader.GetOrdinal(nameof(VisitRecord.VisitRecordID));
                var i2 = reader.GetOrdinal("GalleryItem_ID");
                var i3 = reader.GetOrdinal(nameof(VisitRecord.LastVisitTime));

                foreach (var set in reader.MakeEnumerable(x => new object[] {
                        reader.GetInt32(i1),
                        reader.GetInt32(i2),
                        reader.GetString(i3)
                }).SequenceWrap(BATCH_SIZE))
                {
                    var cmd = "INSERT INTO \"main\".\"VisitRecords\"(\"VisitRecordID\",\"GalleryItemID\",\"LastVisitTime\") VALUES "
                        + GetParamString(set.Count(), set.First().Length);

                    var insertResult = await wrapContext.Database.ExecuteSqlRawAsync(cmd, set.SelectMany(x => x));
                }

                Log.Info("migrating table VisitRecords is finished.");
            }
            catch (Exception e)
            {
                Log.Error("Migrate table VisitRecords failed:" + e.Message);
                return false;
            }

            await targetDBContext.SaveChangesAsync();

            Log.Info("All tables migration process were done.");

            await wrapContext.Database.CloseConnectionAsync();
            await oldDBContext.DisposeAsync();
            await targetDBContext.DisposeAsync();
            await wrapContext.DisposeAsync();

            Log.Info("All db context were disposed.");

            try
            {
                File.Copy(tempDBPath, oldDBPath, true);
                Log.Info("New db file override copy successfully!");
            }
            catch (Exception e)
            {
                Log.Info("New db file override copy failed: " + e.Message);
                return false;
            }

            return true;

            string GetParamString(int count, int paramCount)
            {
                var key = (count, paramCount);

                if (_cache_params.TryGetValue(key, out var s))
                    return s;

                var i = 0;

                return _cache_params[key] = string.Join(",", Enumerable.Range(0, count).Select(x => "(" + string.Join(",", Enumerable.Range(0, paramCount).Select(x => $"{{{i++}}}")) + ")"));
            }
        }

        private static Task currentExecuteTask = null;
        private static Thread currentExecuteThread;

        public static Task PostDbAction(Action<LocalDBContext> executeFunc)
        {
            return PostDbAction(x =>
            {
                executeFunc(x);
                return Task.CompletedTask;
            });
        }

        public static async Task<T> PostDbAction<T>(Func<LocalDBContext, T> executeFunc)
        {
            if (currentExecuteTask != null)
            {
                if (currentExecuteThread == Thread.CurrentThread)
                    throw new Exception("DEAD THREAD LOCK.");

                await currentExecuteTask;
                currentExecuteThread = default;
                currentExecuteTask = null;
            }

            Task<T> task = default;

            task = Task.Run(() =>
            {
                using var _ = ObjectPool<LocalDBContext>.GetWithUsingDisposable(out var context, out var _);
                currentExecuteThread = Thread.CurrentThread;
                var r = executeFunc(context);
                if (currentExecuteTask == task)
                    currentExecuteThread = default;
                return r;
            });

            currentExecuteTask = task;
            return await task;
        }
    }

    internal static class DbDataReaderExtension
    {
        public static IEnumerable<T> MakeEnumerable<T>(this DbDataReader reader, Func<DbDataReader, T> mapFunc)
        {
            while (reader.Read())
            {
                if (!reader.HasRows)
                    continue;

                yield return mapFunc(reader);
            }
        }
    }
}
