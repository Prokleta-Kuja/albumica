<script setup lang="ts">
import { onMounted, ref } from 'vue'
import XLgIcon from '@/components/icons/XLgIcon.vue'
import SearchIcon from '@/components/icons/SearchIcon.vue'
export interface ISearch {
  label?: string
  autoFocus?: boolean
  placeholder?: string
  modelValue?: string
  onChange: () => void
}

const el = ref<HTMLInputElement | null>(null)
const props = defineProps<ISearch>()
const emit = defineEmits<{ (e: 'update:modelValue', modelValue?: string): void }>()

const clear = () => {
  emit('update:modelValue', undefined)
  props.onChange()
}
const search = () => {
  emit('update:modelValue', el.value?.value)
  props.onChange()
}
onMounted(() => {
  if (props.autoFocus) el.value?.focus()
})
</script>
<template>
  <div>
    <label for="search" class="form-label">
      <span v-if="label">{{ label }}</span>
      <span v-else>Search</span>
    </label>
    <div class="input-group">
      <input
        ref="el"
        class="form-control"
        id="search"
        :placeholder="placeholder"
        :value="props.modelValue"
        @keyup.enter="search"
        type="search"
      />
      <button
        v-if="props.modelValue"
        class="btn btn-outline-danger"
        type="button"
        @click.prevent="clear"
      >
        <XLgIcon />
      </button>
      <button v-else class="btn btn-success" @click="search">
        <SearchIcon />
      </button>
    </div>
  </div>
</template>
