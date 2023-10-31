<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
export interface KV {
  value: number
  label: string
}
export interface ITypeaheadBox {
  label: string
  autoFocus?: boolean
  required?: boolean
  modelValue?: number | null
  modelText?: string
  help?: string
  error?: string
  onSearch: (term: string) => Promise<KV[]>
  onChange?: () => void
}

const el = ref<HTMLInputElement | null>(null)
const props = defineProps<ITypeaheadBox>()
const emit = defineEmits<{ (e: 'update:modelValue', modelValue?: number): void }>()
const results = ref<KV[]>([])
const state = reactive<{ id: string; shown?: boolean; selectedText?: string }>({
  id: crypto.randomUUID(),
  selectedText: props.modelText
})

const show = () => (state.shown = true)
const hide = () => setTimeout(() => (state.shown = false), 150)

let timeoutRef: number | null = null
const search = (e: Event) => {
  if (timeoutRef !== null) clearTimeout(timeoutRef)

  const input = e.target as HTMLInputElement
  if (input.value)
    timeoutRef = setTimeout(async () => {
      results.value = await props.onSearch(input.value)
    }, 500)
}

const select = (item: KV) => {
  state.selectedText = item.label
  emit('update:modelValue', item.value)
  if (props.onChange) props.onChange()
}
const clearSelection = () => {
  state.selectedText = undefined
  results.value = []
  emit('update:modelValue', undefined)
  if (props.onChange) props.onChange()
}
onMounted(() => {
  if (props.autoFocus) el.value?.focus()
})
</script>
<template>
  <div>
    <label :for="state.id" class="form-label">{{ props.label }}</label>
    <template v-if="!modelValue">
      <input
        ref="el"
        type="text"
        @input="search"
        class="form-control"
        :id="state.id"
        @focus="show"
        @blur="hide"
      />
      <ul v-if="results.length" class="dropdown-menu mt-1" :class="{ show: state.shown }">
        <li v-for="item in results" :key="item.value">
          <button class="dropdown-item" type="button" @click="select(item)">
            {{ item.label }}
          </button>
        </li>
      </ul>
    </template>
    <template v-else>
      <div class="input-group">
        <input class="form-control" :value="state.selectedText" disabled />
        <button class="btn btn-outline-danger" type="button" @click.prevent="clearSelection">
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
      </div>
    </template>
    <div v-if="error" class="invalid-feedback">{{ error }}</div>
    <div v-else-if="help" class="form-text">{{ help }}</div>
  </div>
</template>
