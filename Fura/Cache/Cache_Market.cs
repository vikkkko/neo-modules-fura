﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Entities;
using Neo.Persistence;
using Neo.Plugins.Models;
using System.Numerics;
using System.Threading.Tasks;

namespace Neo.Plugins.Cache
{
    public class CacheMarketParams
    {
        public int NotificationIndex;
        public bool SimpleUpdate;
        public UInt160 Owner;
        public UInt160 Asset;
        public string TokenId;
        public BigInteger Amount;
        public UInt160 Market;
        public uint AuctionType;
        public UInt160 Auctor;
        public UInt160 AuctionAsset;
        public BigInteger AuctionAmount;
        public ulong Deadline;
        public UInt160 Bidder;
        public BigInteger BidAmount;
        public BigInteger Timestamp;
    }

    public class CacheMarket : IDBCache
    {
        private ConcurrentDictionary<(UInt160, UInt160, string), CacheMarketParams> D_Market;

        private ConcurrentDictionary<(UInt160, UInt160, string), MarketModel> D_MarketModel;

        public void AddNeedUpdate(int notificationIndex, bool simpleUpdate, UInt160 asset, UInt160 owner, string tokenid, BigInteger timestamp)
        {
            lock (D_Market)
            {
                if (!D_Market.ContainsKey((asset, owner, tokenid)) || notificationIndex > D_Market[(asset, owner, tokenid)].NotificationIndex )
                {
                    D_Market[(asset, owner, tokenid)] = new()
                    {
                        NotificationIndex = notificationIndex,
                        SimpleUpdate = simpleUpdate,
                        Asset = asset,
                        TokenId = tokenid,
                        Owner = owner,
                        Timestamp = timestamp
                    };
                }
            }

        }

        public void AddNeedUpdate(int notificationIndex, bool simpleUpdate, UInt160 asset, UInt160 owner, string tokenid, UInt160 market, BigInteger auctionType, UInt160 auctor, UInt160 auctionAsset, BigInteger auctionAmount, BigInteger deadline, UInt160 bidder, BigInteger bidAmount, BigInteger timestamp)
        {
            lock (D_Market)
            {
                if (!D_Market.ContainsKey((asset, owner, tokenid)) || notificationIndex > D_Market[(asset, owner, tokenid)].NotificationIndex)
                {
                    D_Market[(asset, owner, tokenid)] = new()
                    {
                        SimpleUpdate = simpleUpdate,
                        Asset = asset,
                        TokenId = tokenid,
                        Owner = owner,
                        Market = market,
                        AuctionType = (uint)auctionType,
                        Auctor = auctor,
                        AuctionAsset = auctionAsset,
                        AuctionAmount = auctionAmount,
                        Deadline = (ulong)deadline,
                        Bidder = bidder,
                        BidAmount = bidAmount,
                        Timestamp = timestamp,
                        NotificationIndex = notificationIndex
                    };
                }
            }
        }

        public List<CacheMarketParams> GetNeedUpdate()
        {
            return D_Market.Values.ToList();
        }

        public void Update(NeoSystem system, DataCache snapshot)
        {
            List<CacheMarketParams> list = GetNeedUpdate();
            Parallel.For(0, list.Count, (i) =>
            {
                list[i].Amount = Neo.Plugins.VM.Helper.GetNep11BalanceOf(system, snapshot, list[i].Asset, list[i].TokenId, list[i].Owner);
                AddOrUpdate(list[i]);
            });
        }

        public MarketModel Get(UInt160 owner, UInt160 asset, string tokenid)
        {
            if (D_MarketModel.ContainsKey((owner, asset, tokenid)))
            {
                return D_MarketModel[(owner, asset, tokenid)];
            }
            else
            {
                return MarketModel.Get(owner, asset, tokenid);
            }
        }

        public void AddOrUpdate(CacheMarketParams cacheMarketParams)
        {
            if (cacheMarketParams.Owner == null || cacheMarketParams.Asset == null)
                return;
            MarketModel marketModel = Get(cacheMarketParams.Owner, cacheMarketParams.Asset, cacheMarketParams.TokenId);
            if (marketModel is null)
            {
                marketModel = new MarketModel()
                {
                    Asset = cacheMarketParams.Asset,
                    TokenId = cacheMarketParams.TokenId,
                    Owner = cacheMarketParams.Owner,
                    Amount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.Amount.ToString().WipeNumStrToFitDecimal128()),
                    Market = cacheMarketParams.Market,
                    AuctionType = cacheMarketParams.AuctionType,
                    AuctionAmount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.AuctionAmount.ToString().WipeNumStrToFitDecimal128()),
                    AuctionAsset = cacheMarketParams.AuctionAsset,
                    Auctor = cacheMarketParams.Auctor,
                    BidAmount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.BidAmount.ToString().WipeNumStrToFitDecimal128()),
                    Bidder = cacheMarketParams.Bidder,
                    Deadline = cacheMarketParams.Deadline,
                    Timestamp = (ulong)cacheMarketParams.Timestamp
                };
            }
            else if(cacheMarketParams.SimpleUpdate)
            {
                marketModel.Timestamp = (ulong)cacheMarketParams.Timestamp;
                marketModel.Amount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.Amount.ToString().WipeNumStrToFitDecimal128());
            }
            else
            {
                marketModel.Asset = cacheMarketParams.Asset;
                marketModel.TokenId = cacheMarketParams.TokenId;
                marketModel.Owner = cacheMarketParams.Owner;
                marketModel.Amount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.Amount.ToString().WipeNumStrToFitDecimal128());
                marketModel.Market = cacheMarketParams.Market;
                marketModel.AuctionType = cacheMarketParams.AuctionType;
                marketModel.AuctionAmount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.AuctionAmount.ToString().WipeNumStrToFitDecimal128());
                marketModel.AuctionAsset = cacheMarketParams.AuctionAsset;
                marketModel.Auctor = cacheMarketParams.Auctor;
                marketModel.BidAmount = MongoDB.Bson.BsonDecimal128.Create(cacheMarketParams.BidAmount.ToString().WipeNumStrToFitDecimal128());
                marketModel.Bidder = cacheMarketParams.Bidder;
                marketModel.Deadline = cacheMarketParams.Deadline;
                marketModel.Timestamp = (ulong)cacheMarketParams.Timestamp;
            }
            D_MarketModel[((marketModel.Owner, marketModel.Asset, marketModel.TokenId))] = marketModel;
        }

        public void Clear()
        {
            D_Market = new ConcurrentDictionary<(UInt160, UInt160, string), CacheMarketParams>();
            D_MarketModel = new ConcurrentDictionary<(UInt160, UInt160, string), MarketModel>();
        }

        public void Save(Transaction tran)
        {
            if (D_MarketModel.Values.Count > 0)
                tran.SaveAsync(D_MarketModel.Values).Wait();
        }
    }
}

