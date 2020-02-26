using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SocketHook.Extensions.Options;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberCanBePrivate.Local

namespace SocketHook.HostedWpfSample.Services
{
    public interface IInjectOptionsService
    {
        (InjectOptions Options, string Path) GetCurrentOptions();

        bool TryLoadFromJson(string atPath, out InjectOptions options);
        bool TrySaveAsJson(string atPath, InjectOptions options);
    }

    internal sealed class InjectOptionsService : IInjectOptionsService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true};
        private const string _jsonExtension = ".json";
        private InjectOptions _currentOptions;
        private string _lastOptionsPath;

        public InjectOptionsService(IConfiguration configuration) => _lastOptionsPath = configuration["InjectOptionsFilePath"];

        public (InjectOptions, string) GetCurrentOptions()
        {
            if (_currentOptions != default) return (_currentOptions, _lastOptionsPath);
            return TryLoadFromJson(_lastOptionsPath, out _currentOptions) 
                ? (_currentOptions, _lastOptionsPath)
                : default;
        }

        public bool TryLoadFromJson(string atPath, out InjectOptions options)
        {
            options = default;
            if (string.IsNullOrWhiteSpace(atPath)) return false;
            if (!atPath.EndsWith(_jsonExtension)) return false;
            if (!File.Exists(atPath)) return false;
            var json = File.ReadAllText(atPath);

            try
            {
                options = JsonSerializer.Deserialize<JsonOptionsModel>(json, _jsonOptions)
                    .AsInjectOptions();

                _currentOptions = options;
                _lastOptionsPath = atPath;
                return true;
            }
            catch { options = default; return false; }
        }

        public bool TrySaveAsJson(string atPath, InjectOptions options)
        {
            if (string.IsNullOrWhiteSpace(atPath)) return false;
            if (!atPath.EndsWith(_jsonExtension)) return false;
            if (!Directory.Exists(Path.GetDirectoryName(atPath))) return false;

            if (options == default) return false;
            if (options.RedirectionPort == default) return false;
            if (options.RedirectedIps == null || !options.RedirectedIps.Any()) return false;

            try
            {
                var json = JsonSerializer.Serialize(new JsonOptionsModel
                    {RedirectionPort = options.RedirectionPort, RedirectedIps = options.RedirectedIps.ToArray()});

                File.WriteAllText(atPath, json);
                _currentOptions = options;
                _lastOptionsPath = atPath;
                return true;
            }
            catch { return false; }
        }

        private class JsonOptionsModel
        {
            public int RedirectionPort { get; set; }
            public string[] RedirectedIps { get; set; }

            public InjectOptions AsInjectOptions() => new InjectOptions(RedirectionPort, RedirectedIps);
        }
    }
}
