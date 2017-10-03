﻿using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class EquipmentRemoveItemProcessor : ClientPacketProcessor<EquipmentRemoveItem>
    {
        public static readonly int UNEQUIP_EVENT_TYPE_ID = 1;
        
        public override void Process(EquipmentRemoveItem packet)
        {
            GameObject owner = GuidHelper.RequireObjectFrom(packet.OwnerGuid);            
            GameObject item = GuidHelper.RequireObjectFrom(packet.ItemGuid);            
            Pickupable pickupable = item.GetComponent<Pickupable>();

            if (pickupable != null)
            {
                Optional<Equipment> opEquipment = EquipmentHelper.GetBasedOnOwnersType(owner);

                if (opEquipment.IsPresent())
                {
                    Equipment equipment = opEquipment.Get();

                    Dictionary<string, InventoryItem> itemsBySlot = (Dictionary<string, InventoryItem>)equipment.ReflectionGet("equipment");
                    InventoryItem inventoryItem = itemsBySlot[packet.Slot];
                    itemsBySlot[packet.Slot] = null;

                    equipment.ReflectionCall("UpdateCount", false, false, new object[] { pickupable.GetTechType(), false });
                    Equipment.SendEquipmentEvent(pickupable, UNEQUIP_EVENT_TYPE_ID, owner, packet.Slot);
                    equipment.ReflectionCall("NotifyUnequip", false, false, new object[] { packet.Slot, inventoryItem });
                }
                else
                {
                    Console.WriteLine("Could not find equipment type for " + owner.name);
                }                        
            }
            else
            {
                Console.WriteLine("item did not have a pickupable script attached!");
            }

            UnityEngine.Object.Destroy(item);
        }
    }
}