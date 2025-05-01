using ManagedCommon;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows;
using System;
using Wox.Plugin;

#if DEBUG
using NLog;
using NLog.Config;
using NLog.Targets;
#endif

namespace UnicodeBuilder
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IDisposable
    {
#if DEBUG
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static bool IsNLogInitialized = false;
#endif
        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "78BCC2D3232011F09B2CE848B8C84000";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "UnicodeBuilder";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "build a unicode character from a codepoint";

        private PluginInitContext Context { get; set; }

        private SqliteConnection Connection { get; set; }

        private string IconPath { get; set; }

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            var search = query.Search;

            var results = new List<Result>();

            if (string.IsNullOrWhiteSpace(search))
            {
                return results;
            }

            if (!int.TryParse(search, System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
            {
                return results;
            }

            using var cmd = Connection.CreateCommand();
            cmd.CommandText = "SELECT name FROM a WHERE code = @cp";
            cmd.Parameters.AddWithValue("@cp", codePoint);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return results;
            }

            var name = reader.GetString(0);
            var text = char.ConvertFromUtf32(codePoint);

            results.Add(new Result
            {
                Title = $"{codePoint:X4}: {text}",
                SubTitle = $"U+{codePoint:X4}: {name}",
                IcoPath = "Images\\icon.png",
                Action = _ =>
                {
                    Clipboard.SetText(text);
                    return true;
                }
            });
            return results;
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
#if DEBUG
            if (!IsNLogInitialized)
            {
                IsNLogInitialized = true;
                var config = new LoggingConfiguration();
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(dir, "debug.log");
                var logfile = new FileTarget("logfile")
                {
                    FileName = path,
                    Layout = "${longdate} | ${level:uppercase=true} | ${logger} | ${message} ${exception:format=ToString}",
                    CreateDirs = true,
                };
                config.AddTarget(logfile);
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
                LogManager.Configuration = config;
            }
            logger.Trace("Init");
#endif
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());

            SetupConnection();
        }

        public void SetupConnection()
        {
#if DEBUG
            logger.Trace("SetupConnection");
#endif
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(folder, "unicode.db");
#if DEBUG
            logger.Trace($"folder = {folder}");
            logger.Trace($"path = {path}");
#endif
            Connection = new SqliteConnection($"Data Source={path}");
            Connection.Open();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }

            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/icon.png" : "Images/icon.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
    }
}
