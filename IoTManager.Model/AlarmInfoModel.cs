using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IoTManager.Model
{
    public class AlarmInfoModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }
        
        [BsonElement("AlarmInfo")]
        public String AlarmInfo { get; set; }
        
        [BsonElement("DeviceId")]
        public String DeviceId { get; set; }
        
        [BsonElement("IndexId")]
        public String IndexId { get; set; }
        
        [BsonElement("IndexName")]
        public String IndexName { get; set; }
        
        [BsonElement("IndexValue")]
        public Double IndexValue { get; set; }
        
        [BsonElement("ThresholdValue")]
        public Double ThresholdValue { get; set; }
        
        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }
        [BsonElement("Severity")]
        public String Severity { get; set; }
        [BsonElement("Processed")]
        public String Processed { get; set; }
    }
}