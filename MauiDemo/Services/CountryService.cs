using MauiDemo.Data;
using SQLite;
using System.Text;
using System.Text.Json;

namespace MauiDemo.Services
{
    public class CountryService
    {
        private readonly SQLiteAsyncConnection _db;
        private readonly HttpClient _httpClient = new();

        public CountryService(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<Country>().Wait();
        }

        public Task<List<Country>> GetAllAsync() => _db.Table<Country>().ToListAsync();

        public Task<Country> GetByNameAsync(string name) => _db.Table<Country>().Where(c => c.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();

        public Task<List<Country>> GetUnsyncedAsync() => _db.Table<Country>().Where(c => !c.IsSynced).ToListAsync();
        public Task AddAsync(Country country) => _db.InsertAsync(country);
        public Task MarkAsSyncedAsync(Country country)
        {
            country.IsSynced = true;
            return _db.UpdateAsync(country);
        }

        public async Task SyncToSqlAsync(Country item)
        {
            var json = JsonSerializer.Serialize(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var functionUrl = "https://mauimiddleware-bfhzhdexg6hmb7c9.westeurope-01.azurewebsites.net/api/countries?code=XXXXXX";

            var response = await _httpClient.PostAsync(functionUrl, content);

            if (response.IsSuccessStatusCode)
            {
                await MarkAsSyncedAsync(item);
            }
            else
            {
                // Maneja el error
            }
        }

        public async Task SyncToServiceBusAsync(Country item)
        {
            var json = JsonSerializer.Serialize(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var functionUrl = "https://mauimiddleware-bfhzhdexg6hmb7c9.westeurope-01.azurewebsites.net/api/send-country?code=XXXXXX";

            var response = await _httpClient.PostAsync(functionUrl, content);

            if (response.IsSuccessStatusCode)
            {
                await MarkAsSyncedAsync(item);
            }
            else
            {
                // Maneja el error
            }
        }

    }
}
