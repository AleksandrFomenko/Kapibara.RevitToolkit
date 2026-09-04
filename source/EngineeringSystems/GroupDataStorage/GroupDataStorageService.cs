using System.Text.Json;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace EngineeringSystems.GroupDataStorage;

public static class GroupDataStorageService
    {
        private const string SchemaName    = "Kapibara_GroupStorage";
        private const string JsonFieldName = "JsonData";
        private const string VendorId      = "Kapibara";

        private static readonly Guid SchemaGuid = new Guid("B8D0D7C3-35E1-44FE-9FA3-1B8C5D9B4E11");

        private static readonly JsonSerializerOptions? JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        private static Schema GetOrCreateSchema()
        {
            var schema = Schema.Lookup(SchemaGuid);
            if (schema != null)
                return schema;

            var builder = new SchemaBuilder(SchemaGuid);

            builder.SetSchemaName(SchemaName);
            builder.SetVendorId(VendorId);
            builder.SetDocumentation("Storage for Group list JSON.");

            builder.SetReadAccessLevel(AccessLevel.Public);
            builder.SetWriteAccessLevel(AccessLevel.Public);

            var fieldBuilder = builder.AddSimpleField(JsonFieldName, typeof(string));
            fieldBuilder.SetDocumentation("JSON payload with groups list.");

            schema = builder.Finish();
            return schema;
        }

        private static DataStorage? FindDataStorage(Document doc)
        {
            var schema = Schema.Lookup(SchemaGuid);
            if (schema == null)
                return null;

            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(DataStorage));

            foreach (var element in collector)
            {
                if (element is not DataStorage ds) 
                    continue;

                var entity = ds.GetEntity(schema);
                if (entity.IsValid())
                    return ds;
            }

            return null;
        }
        
        private static DataStorage GetOrCreateDataStorage(Document doc)
        {
            var existing = FindDataStorage(doc);
            if (existing != null)
                return existing;

            var schema = GetOrCreateSchema();

            var ds = DataStorage.Create(doc);
            var entity = new Entity(schema);
            ds.SetEntity(entity);

            return ds;
        }
        
        public static GroupStorageDto? Load(Document doc)
        {
            if (doc == null) 
                throw new ArgumentNullException(nameof(doc));

            var schema = Schema.Lookup(SchemaGuid);
            if (schema == null)
                return null;

            var ds = FindDataStorage(doc);
            if (ds == null)
                return null;

            var entity = ds.GetEntity(schema);
            if (!entity.IsValid())
                return null;

            var field = schema.GetField(JsonFieldName);
            var json = entity.Get<string>(field);

            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<GroupStorageDto>(json, JsonOptions);
        }
        
        public static void Save(Document doc, GroupStorageDto dto)
        {
            if (doc == null) 
                throw new ArgumentNullException(nameof(doc));

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var schema = GetOrCreateSchema();
            var ds     = GetOrCreateDataStorage(doc);

            var entity = ds.GetEntity(schema);
            if (!entity.IsValid())
                entity = new Entity(schema);

            var json  = JsonSerializer.Serialize(dto, JsonOptions);
            var field = schema.GetField(JsonFieldName);

            entity.Set(field, json ?? string.Empty);
            ds.SetEntity(entity);
        }
    }