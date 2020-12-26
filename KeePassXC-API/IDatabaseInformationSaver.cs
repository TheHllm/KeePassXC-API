using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeePassXC_API
{
    public interface IDatabaseInformationSaver
    {
        public Task<DatabaseInformation[]> LoadAsync();
        public Task SaveAsync(DatabaseInformation[] info);
    }

    public class DefaultDatabaseInformationSaver : IDatabaseInformationSaver
    {
        private const string Filename = "info.json";

        public async Task<DatabaseInformation[]> LoadAsync()
        {
            try
            {
                using FileStream fs = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                return await JsonSerializer.DeserializeAsync<DatabaseInformation[]>(fs);
            }
            catch (FileNotFoundException)
            {
                return new DatabaseInformation[0];
            }
        }

        public async Task SaveAsync(DatabaseInformation[] info)
        {
            using FileStream fs = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.Write);
            await JsonSerializer.SerializeAsync(fs, info);
        }
    }
}
