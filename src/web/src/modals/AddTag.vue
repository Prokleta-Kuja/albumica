<script setup lang="ts">
import { reactive, ref } from 'vue'
import { type TagCM, TagService, type TagVM } from '@/api'
import type IModelState from '@/components/form/modelState'
import Modal from '@/components/Modal.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import Text from '@/components/form/TextBox.vue'
import IntegerBox from '@/components/form/IntegerBox.vue'

export interface IAddTag {
  onAdded?: (addedTag: TagVM) => void
}

const props = defineProps<IAddTag>()
const blank = (): TagCM => ({ name: '', order: 100 })
const shown = ref(false)
const tag = reactive<IModelState<TagCM>>({ model: blank() })

const toggle = () => (shown.value = !shown.value)
const submit = () => {
  tag.submitting = true
  tag.error = undefined
  TagService.createTag({ requestBody: tag.model })
    .then((r) => {
      shown.value = false
      if (props.onAdded) props.onAdded(r)
    })
    .catch((r) => (tag.error = r.body))
    .finally(() => (tag.submitting = false))
}
</script>
<template>
  <button class="btn btn-success" @click="toggle">
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width="16"
      height="16"
      fill="currentColor"
      class="bi bi-plus-lg"
      viewBox="0 0 16 16"
    >
      <path
        fill-rule="evenodd"
        d="M8 2a.5.5 0 0 1 .5.5v5h5a.5.5 0 0 1 0 1h-5v5a.5.5 0 0 1-1 0v-5h-5a.5.5 0 0 1 0-1h5v-5A.5.5 0 0 1 8 2Z"
      />
    </svg>
    Dodaj
  </button>
  <Modal title="Dodaj oznaku" :shown="shown" :onClose="toggle">
    <template #body>
      <form @submit.prevent="submit">
        <Text
          class="mb-3"
          label="Naziv"
          autoFocus
          v-model="tag.model.name"
          required
          :error="tag.error?.errors?.name"
        />
        <IntegerBox
          class="mb-3"
          label="Slijed"
          v-model="tag.model.order"
          required
          :error="tag.error?.errors?.order"
        />
      </form>
    </template>
    <template #footer>
      <p v-if="tag.error" class="text-danger">{{ tag.error.message }}</p>
      <button class="btn btn-outline-danger" @click="toggle">Odustani</button>
      <SpinButton
        class="btn-primary"
        :loading="tag.submitting"
        text="Dodaj"
        loadingText="Dodaje se"
        @click="submit"
      />
    </template>
  </Modal>
</template>
