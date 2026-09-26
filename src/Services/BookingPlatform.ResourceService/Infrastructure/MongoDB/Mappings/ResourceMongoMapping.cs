using BookingPlatform.ResourceService.Domain.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace BookingPlatform.ResourceService.Infrastructure.MongoDB.Mappings;

public static class ResourceMongoMapping
{
    public static void Register()
    {   
        //сериализация GUID
        BsonSerializer.RegisterSerializer( new GuidSerializer(GuidRepresentation.Standard));
        if (!BsonClassMap.IsClassMapRegistered(typeof(Resource)))
        {
            BsonClassMap.RegisterClassMap<Resource>(map =>
            {
                map.AutoMap();
                map.MapIdMember(x => x.Id);
                //сохраняю private поле в MongoDB(что бы не менять Domain модель)
                map.MapField("_offers").SetElementName("Offers");
                map.MapField("_scheduleOverrides").SetElementName("ScheduleOverrides");
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ResourceType)))
        {
            BsonClassMap.RegisterClassMap<ResourceType>(map =>
            {
                map.AutoMap();
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ResourceOffer)))
        {
            BsonClassMap.RegisterClassMap<ResourceOffer>(map =>
            {
                map.AutoMap();
                //сохраняю private поле в MongoDB(что бы не менять Domain модель)
                map.MapField("_schedule").SetElementName("Schedule");
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ResourceSchedule)))
        {
            BsonClassMap.RegisterClassMap<ResourceSchedule>(map =>
            {
                map.AutoMap();
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ResourceScheduleOverride)))
        {
            BsonClassMap.RegisterClassMap<ResourceScheduleOverride>(map =>
            {
                map.AutoMap();
            });
        }
    }
}