using System;
using System.Collections.Generic;

namespace albumica.Data
{
    public class ItemTag
    {
        public int ItemId { get; set; }
        public int TagId { get; set; }

        public Item? Item { get; set; }
        public Tag? Tag { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ItemTag tag &&
                   ItemId == tag.ItemId &&
                   TagId == tag.TagId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ItemId, TagId);
        }
    }
}