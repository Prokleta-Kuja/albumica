<script setup lang="ts">
import { reactive } from 'vue'
import { type TagUM, TagService, type TagVM } from '@/api'
import type IModelState from '@/components/form/modelState'
import Modal from '@/components/Modal.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import Text from '@/components/form/TextBox.vue'
import IntegerBox from '@/components/form/IntegerBox.vue'
export interface IEditTag {
  model: TagVM
  onUpdated?: (updatedTag?: TagVM) => void
}

const mapTagModel = (m: TagVM): TagUM => ({
  name: m.name,
  order: m.order
})
const props = defineProps<IEditTag>()
const tag = reactive<IModelState<TagUM>>({ model: mapTagModel(props.model) })

const toggle = () => {
  if (props.onUpdated) props.onUpdated()
}
const submit = () => {
  tag.submitting = true
  tag.error = undefined
  TagService.updateTag({ tagId: props.model.id, requestBody: tag.model })
    .then((r) => {
      if (props.onUpdated) props.onUpdated(r)
    })
    .catch((r) => (tag.error = r.body))
    .finally(() => (tag.submitting = false))
}
</script>
<template>
  <Modal v-if="tag.model" title="Uredi oznaku" shown :onClose="toggle">
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
        text="Spremi"
        loadingText="Spremanje"
        @click="submit"
      />
    </template>
  </Modal>
</template>
