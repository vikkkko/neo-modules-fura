using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;
using Neo.Plugins.Attribute;
using Neo.VM;

namespace Neo.Plugins.Models
{
    [Collection("Notification")]
    public class NotificationModel : Entity
    {
        [UInt256AsString]
        [BsonElement("txid")]
        public UInt256 Txid { get; set; }

        [BsonElement("index")]
        public int Index { get; set; }

        [UInt256AsString]
        [BsonElement("blockhash")]
        public UInt256 BlockHash { get; set; }

        [UInt160AsString]
        [BsonElement("contract")]
        public UInt160 ContractHash { get; set; }

        [BsonElement("eventname")]
        public string EventName { get; set; }

        [BsonElement("state")]
        public NotificationStateModel State { get; set; }

        [BsonElement("timestamp")]
        public ulong Timestamp { get; set; }

        public string Vmstate { get; set; }

        public NotificationModel(UInt256 txid, int index, UInt256 blockhash, ulong timestamp, UInt160 contractHash, string eventName, string vmstate,Neo.VM.Types.Array array)
        {
            Txid = txid;
            Index = index;
            BlockHash = blockhash;
            ContractHash = contractHash;
            EventName = eventName;
            Vmstate = vmstate;
            State = new NotificationStateModel(array);
            Timestamp = timestamp;
        }
    }

    public class NotificationStateModel
    {
        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("value")]
        public NotificationStateValueModel[] Values { get; set; }

        public NotificationStateModel(Neo.VM.Types.Array array)
        {
            Type = array.Type.ToString();
            Values = NotificationStateValueModel.ToModels(array);
        }
    }

    public class NotificationStateValueModel
    {
        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("value")]
        public string Value { get; set; }

        public NotificationStateValueModel(Neo.VM.Types.StackItem item)
        {
            var json = item.ToJson();
            Type = json["type"]?.GetString();
            if (Type == "Array")
            {
                Value = json["value"]?.ToString();
            }
            else
            {
                Value = json["value"]?.GetString();
            }
        }

        public static NotificationStateValueModel[] ToModels(Neo.VM.Types.Array array)
        {
            NotificationStateValueModel[] models = array.Select(item => new NotificationStateValueModel(item)).ToArray();
            return models;
        }
    }
}
