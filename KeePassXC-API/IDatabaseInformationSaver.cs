using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeePassXC_API
{
    public interface IDatabaseInformationSaver
    {
        Task<DatabaseInformation[]> LoadAsync();
        Task SaveAsync(DatabaseInformation[] info);
    }

    public class DefaultDatabaseInformationSaver : IDatabaseInformationSaver
    {
        private const string Filename = "info.json";

        public async Task<DatabaseInformation[]> LoadAsync()
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, Filename), FileMode.Open, FileAccess.Read))
                {
                    return await JsonSerializer.DeserializeAsync<DatabaseInformation[]>(fs);
                }
            }
            catch (JsonException)
            {
                return new DatabaseInformation[0];
            }
            catch (FileNotFoundException)
            {
                return new DatabaseInformation[0];
            }
        }

        public async Task SaveAsync(DatabaseInformation[] info)
        {
            using (FileStream fs = new FileStream(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, Filename), FileMode.Create, FileAccess.Write))
            {
                await JsonSerializer.SerializeAsync(fs, info);
            }
        }
    }
}
