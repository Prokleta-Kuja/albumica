import { MediaService } from '@/api';
import { defineStore } from 'pinia'
import { reactive } from 'vue'

export const useBasket = defineStore('basket', () => {
    const data = reactive<{ initialized: boolean, itemIds: Set<number> }>({ initialized: false, itemIds: new Set() });

    if (!data.initialized)
        MediaService.getBasketItems().then(r => {
            r.forEach(itemId => data.itemIds.add(itemId));
            data.initialized = false;
        });

    const addItem =(itemId: number) => {
        if (!data.itemIds.has(itemId))
            data.itemIds.add(itemId);
    }
    const removeItem = (itemId: number) => {
        if (data.itemIds.has(itemId))
            data.itemIds.delete(itemId);
    }

    return {
        itemIds: data.itemIds,
        addItem,
        removeItem,
    }
})