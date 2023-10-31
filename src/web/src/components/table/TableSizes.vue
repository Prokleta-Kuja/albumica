<script setup lang="ts">
import { ref } from 'vue'
import { type ITableParams, defaultPageSizes } from '.'

const props = defineProps<{ params: ITableParams; onChange: (params: ITableParams) => void }>()
const size = ref(props.params.size)
const change = () => {
  props.onChange({ ...props.params, page: 1, size: size.value })
}
</script>
<template>
  <div>
    <label for="items-per-page" class="form-label">Items per page</label>
    <select id="items-per-page" class="form-select" v-model="size" @change="change">
      <option v-for="size in defaultPageSizes" :key="size" :value="size">{{ size }}</option>
    </select>
  </div>
</template>
