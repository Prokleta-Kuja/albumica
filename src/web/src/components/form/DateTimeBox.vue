<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
export interface IDateTimeBox {
  label?: string
  autoFocus?: boolean
  required?: boolean
  placeholder?: string
  modelValue?: string | null
  help?: string
  error?: string
}

const el = ref<HTMLInputElement | null>(null)
const state = reactive<{ id: string }>({ id: crypto.randomUUID() })
const props = defineProps<IDateTimeBox>()
const emit = defineEmits<{ (e: 'update:modelValue', modelValue?: string): void }>()

const localStr = computed(() => {
  if (!props.modelValue) return undefined

  return getDateTimeLocalStrFromUtcStr(props.modelValue)
})
const getDateTimeLocalStrFromUtcStr = (dtStr: string) => {
  const dt = new Date(dtStr)
  return `${String(dt.getFullYear()).padStart(4, '0')}-${String(dt.getMonth() + 1).padStart(
    2,
    '0'
  )}-${String(dt.getDate()).padStart(2, '0')}T${String(dt.getHours()).padStart(2, '0')}:${String(
    dt.getMinutes()
  ).padStart(2, '0')}:${String(dt.getSeconds()).padStart(2, '0')}`
}
const update = (e: Event) => {
  let zStr: string | undefined
  const input = e.target as HTMLInputElement
  if (input.value) {
    const dt = new Date(input.value)
    zStr = dt.toISOString()
  }
  emit('update:modelValue', zStr)
}
onMounted(() => {
  if (props.autoFocus) el.value?.focus()
})
</script>
<template>
  <div>
    <label v-if="label" :for="state.id" class="form-label"
      >{{ label }} <span v-if="required">*</span></label
    >
    <input
      ref="el"
      class="form-control"
      :id="state.id"
      :class="{ 'is-invalid': error }"
      :placeholder="placeholder"
      :value="localStr"
      @input="update"
      :required="required"
      type="datetime-local"
    />
    <div v-if="error" class="invalid-feedback">{{ error }}</div>
    <div v-else-if="help" class="form-text">{{ help }}</div>
  </div>
</template>
