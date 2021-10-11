﻿using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;
using Neo.Plugins.Attribute;

namespace Neo.Plugins.Models
{
    [Collection("ScCall")]
    public class ScCallModel:Entity
    {
        [UInt256AsString]
        [BsonElement("txid")]
        public UInt256 Txid { get; set; }

        [UInt160AsString]
        [BsonElement("originSender")]
        public UInt160 OriginSender { get; set; }

        [UInt160AsString]
        [BsonElement("contractHash")]
        public UInt160 ContractHash { get; set; }

        [BsonElement("method")]
        public string Method { get; set; }

        [BsonElement("callFlags")]
        public string CallFlags { get; set; }

        [BsonElement("hexStringParams")]
        public string[] HexStringParams { get; set; }

        public ScCallModel() { }

        public ScCallModel(UInt256 txid,UInt160 originSender, UInt160 contractHash, string method, string callFlags, params string[] hexStringParams)
        {
            Txid = txid;
            OriginSender = originSender;
            ContractHash = contractHash;
            Method = method;
            CallFlags = callFlags;
            HexStringParams = hexStringParams;
        }

        public ScCallModel(ScCallModel scCallModel)
        {
            ID = scCallModel.ID;
            OriginSender = scCallModel.OriginSender;
            Txid = scCallModel.Txid;
            ContractHash = scCallModel.ContractHash;
            Method = scCallModel.Method;
            CallFlags = scCallModel.CallFlags;
            HexStringParams = scCallModel.HexStringParams;
        }

        public async static Task InitCollectionAndIndex()
        {
            await DB.CreateCollection<ScCallModel>(new CreateCollectionOptions<ScCallModel>());
            await DB.Index<ScCallModel>().Key(a => a.OriginSender, KeyType.Ascending).Option(o => { o.Name = "_originSender_"; }).CreateAsync();
            await DB.Index<ScCallModel>().Key(a => a.OriginSender, KeyType.Ascending).Key(a => a.ContractHash, KeyType.Ascending).Option(o => { o.Name = "_originSender_contractHash_"; }).CreateAsync();
            await DB.Index<ScCallModel>().Key(a => a.ContractHash, KeyType.Ascending).Option(o => { o.Name = "_contractHash_"; }).CreateAsync();
            await DB.Index<ScCallModel>().Key(a => a.Method, KeyType.Ascending).Option(o => { o.Name = "_method_"; }).CreateAsync();
            await DB.Index<ScCallModel>().Key(a => a.ContractHash, KeyType.Ascending).Key(a => a.Method, KeyType.Ascending).Option(o => { o.Name = "_contractHash_method_"; }).CreateAsync();
            await DB.Index<ScCallModel>().Key(a => a.Txid, KeyType.Ascending).Option(o => { o.Name = "_txid_"; }).CreateAsync();
        }
    }
}
