﻿using System;
using System.Linq;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.Plugins.Cache;
using Neo.Plugins.Models;
using Neo.SmartContract.Native;
using System.Numerics;
using Neo.Json;
using Neo.Extensions;

namespace Neo.Plugins.Notification
{
    public partial class NotificationMgr
    {
        private bool ExecuteBidNotification(NotificationModel notificationModel, NeoSystem system, Block block, DataCache snapshot)
        {
            ContractModel contractModel = DBCache.Ins.cacheContract.Get(notificationModel.ContractHash);
            if (Settings.Default.MarketContractIds.Contains(contractModel.ContractId))
            {
                BigInteger nonce = 0;
                UInt160 user = null;
                UInt160 asset = null;
                string tokenId = "";
                UInt160 auctionAsset = null;
                BigInteger bidAmount = 0;
                bool succ = true;
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[0].Value, out nonce);
                //user
                if (notificationModel.State.Values[1].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[1].Value).Reverse().ToArray().ToHexString(), out user);
                }
                //asset
                if (notificationModel.State.Values[2].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[2].Value).Reverse().ToArray().ToHexString(), out asset);
                }
                //tokenid
                if (notificationModel.State.Values[3].Type == "Integer")  //需要转换一下
                {
                    tokenId = Convert.ToBase64String(BigInteger.Parse(notificationModel.State.Values[3].Value).ToByteArray());
                }
                else
                {
                    tokenId = notificationModel.State.Values[3].Value;
                }
                //auctionAsset
                if (notificationModel.State.Values[4].Value is not null)
                {
                    succ = succ && UInt160.TryParse(Convert.FromBase64String(notificationModel.State.Values[4].Value).Reverse().ToArray().ToHexString(), out auctionAsset);
                }
                //auctionAmount
                succ = succ && BigInteger.TryParse(notificationModel.State.Values[5].Value, out bidAmount);

                MarketModel marketModel = DBCache.Ins.cacheMarket.Get(notificationModel.ContractHash, asset, tokenId);
                DBCache.Ins.cacheMarket.AddNeedUpdate(notificationModel.Index, false, asset, notificationModel.ContractHash, tokenId, notificationModel.ContractHash, marketModel.AuctionType, marketModel.Auctor, marketModel.AuctionAsset, BigInteger.Parse(marketModel.AuctionAmount.ToString()), marketModel.Deadline, user, bidAmount, block.Timestamp);

                JObject json = new JObject();
                json["auctionAsset"] = auctionAsset?.ToString();
                json["bidAmount"] = bidAmount.ToString();
                DBCache.Ins.cacheMatketNotification.Add(notificationModel.Txid, notificationModel.BlockHash, notificationModel.ContractHash, nonce, user, asset, tokenId, "Bid", json.ToString(), notificationModel.Timestamp);
            }
            return true;
        }
    }
}

