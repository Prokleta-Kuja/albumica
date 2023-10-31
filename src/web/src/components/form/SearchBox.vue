<script setup lang="ts">
import { onMounted, ref } from 'vue'
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
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="16"
          height="16"
          fill="currentColor"
          class="bi bi-x-lg"
          viewBox="0 0 16 16"
        >
          <path
            d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z"
          />
        </svg>
      </button>
      <button v-else class="btn btn-success" @click="search">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="16"
          height="16"
          fill="currentColor"
          class="bi bi-search"
          viewBox="0 0 16 16"
        >
          <path
            d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"
          />
        </svg>
      </button>
    </div>
  </div>
</template>
