<script setup lang="ts">
import { MediaService, type ValidationError } from '@/api'
import Modal from '@/components/Modal.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import Trash3Icon from '@/components/icons/Trash3Icon.vue'
import { useBasket } from '@/stores/basket'
import { reactive } from 'vue'

export interface IRemoveMedia {
  onRemoved?: () => void
}

const props = defineProps<IRemoveMedia>()
const basket = useBasket()
const state = reactive<{ shown?: boolean; working?: boolean; error?: ValidationError }>({})

const toggle = () => {
  state.shown = !state.shown
  state.working = false
}
const removeMedia = async () => {
  state.working = true
  state.error = undefined

  try {
    for await (const itemId of basket.itemIds) {
      if (state.shown && state.working && !state.error && basket.itemIds.size > 0) {
        await MediaService.deleteMedia({ mediaId: itemId })
          .then(() => basket.removeItem(itemId))
          .catch((r) => (state.error = r))
      }
    }
  } catch (error) {
    console.log('dafuq', error)
  }

  if (props.onRemoved) props.onRemoved()
}
</script>
<template>
  <button v-if="basket.itemIds.size > 0" class="btn btn-danger w-100 mb-3" @click="toggle">
    <Trash3Icon />
    Obriši - {{ basket.itemIds.size.toLocaleString() }}
  </button>
  <Modal title="Brisanje medija" :onClose="toggle" :shown="state.shown">
    <template #body>
      <p v-if="state.working">Preostalo {{ basket.itemIds.size.toLocaleString() }} medija.</p>
      <p v-else>Sigurno želiš obrisati {{ basket.itemIds.size.toLocaleString() }} medija?</p>
    </template>
    <template #footer>
      <p v-if="state.error" class="text-danger">{{ state.error.message }}</p>
      <button class="btn btn-outline-danger" @click="toggle">Odustani</button>
      <SpinButton
        class="btn-primary"
        :loading="state.working"
        text="Obriši"
        loadingText="Brišem"
        @click="removeMedia"
      />
    </template>
  </Modal>
</template>
