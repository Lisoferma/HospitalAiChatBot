using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace HospitalAiChatBot.Models;

public class MongoRepository<T> : IRepository<T> where T : class
{
    private readonly IMongoCollection<BsonDocument> _collection;


    public MongoRepository(string connectionString, string databaseName, string collectionName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<BsonDocument>(collectionName);
    }


    public async Task<string> AddAsync(T entity)
    {
        var json = JsonConvert.SerializeObject(entity);
        var document = BsonSerializer.Deserialize<BsonDocument>(json);
        await _collection.InsertOneAsync(document);
        return document["_id"].ToString();
    }


    public async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            var objectId = new ObjectId(id);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
            var document = await _collection.Find(filter).FirstOrDefaultAsync();

            if (document == null) return null;

            var json = document.ToJson();
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (FormatException)
        {
            return null;
        }
    }


    public async Task<IEnumerable<T?>> GetAllAsync()
    {
        var documents = await _collection.Find(new BsonDocument()).ToListAsync();
        var entities = new List<T?>();

        foreach (var doc in documents)
        {
            var json = doc.ToJson();
            entities.Add(JsonConvert.DeserializeObject<T>(json));
        }

        return entities;
    }


    public async Task UpdateAsync(string id, T entity)
    {
        try
        {
            var objectId = new ObjectId(id);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
            var json = JsonConvert.SerializeObject(entity);
            var update = BsonSerializer.Deserialize<BsonDocument>(json);

            await _collection.ReplaceOneAsync(filter, update);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid ID format", nameof(id), ex);
        }
    }


    public async Task DeleteAsync(string id)
    {
        try
        {
            var objectId = new ObjectId(id);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
            await _collection.DeleteOneAsync(filter);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid ID format", nameof(id), ex);
        }
    }
}
