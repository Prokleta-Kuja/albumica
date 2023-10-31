<script setup lang="ts">
import { MediaService, type ValidationError } from '@/api';
import Modal from '@/components/Modal.vue';
import SpinButton from '@/components/form/SpinButton.vue';
import { useBasket } from '@/stores/basket';
import { reactive } from 'vue';


const basket = useBasket();
const state = reactive<{ shown?: boolean, working?: boolean, error?: ValidationError }>({});

const toggle = () => {
    state.shown = !state.shown;
    state.working = false;
}
const removeMedia = async () => {
    state.working = true;
    state.error = undefined;

    try {
        for await (const itemId of basket.itemIds) {
            if ((state.shown && state.working && !state.error && basket.itemIds.size > 0)) {
                await MediaService.deleteMedia({ mediaId: itemId })
                    .then(() => basket.removeItem(itemId))
                    .catch(r => state.error = r)
            }
        }
    } catch (error) {
        console.log("dafuq", error);
    }
};
</script>
<template>
    <button v-if="basket.itemIds.size > 0" class="btn btn-danger w-100 mb-3" @click="toggle">
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-trash3"
            viewBox="0 0 16 16">
            <path
                d="M6.5 1h3a.5.5 0 0 1 .5.5v1H6v-1a.5.5 0 0 1 .5-.5ZM11 2.5v-1A1.5 1.5 0 0 0 9.5 0h-3A1.5 1.5 0 0 0 5 1.5v1H2.506a.58.58 0 0 0-.01 0H1.5a.5.5 0 0 0 0 1h.538l.853 10.66A2 2 0 0 0 4.885 16h6.23a2 2 0 0 0 1.994-1.84l.853-10.66h.538a.5.5 0 0 0 0-1h-.995a.59.59 0 0 0-.01 0H11Zm1.958 1-.846 10.58a1 1 0 0 1-.997.92h-6.23a1 1 0 0 1-.997-.92L3.042 3.5h9.916Zm-7.487 1a.5.5 0 0 1 .528.47l.5 8.5a.5.5 0 0 1-.998.06L5 5.03a.5.5 0 0 1 .47-.53Zm5.058 0a.5.5 0 0 1 .47.53l-.5 8.5a.5.5 0 1 1-.998-.06l.5-8.5a.5.5 0 0 1 .528-.47ZM8 4.5a.5.5 0 0 1 .5.5v8.5a.5.5 0 0 1-1 0V5a.5.5 0 0 1 .5-.5Z" />
        </svg>
        Obriši - {{ basket.itemIds.size }}
    </button>
    <Modal title="Brisanje medija" :onClose="toggle" :shown="state.shown">
        <template #body>
            <p v-if="state.working">Preostalo {{ basket.itemIds.size.toLocaleString() }} medija.</p>
            <p v-else>Sigurno želiš obrisati {{ basket.itemIds.size.toLocaleString() }} medija?</p>
        </template>
        <template #footer>
            <p v-if="state.error" class="text-danger">{{ state.error.message }}</p>
            <button class="btn btn-outline-danger" @click="toggle">Odustani</button>
            <SpinButton class="btn-primary" :loading="state.working" text="Obriši" loadingText="Brišem"
                @click="removeMedia" />
        </template>
    </Modal>
</template>