<script setup lang="ts">
import { computed } from 'vue'
import type { ITableParams } from '.'
import SortUpIcon from '../icons/SortUpIcon.vue'
import SortDownIcon from '../icons/SortDownIcon.vue'

const props = defineProps<{
  params: ITableParams
  onSort: (params: ITableParams) => void
  column: string
  display?: string
  unsortable?: boolean
}>()

const text = computed(() => {
  if (props.display) return props.display
  return props.column[0].toUpperCase() + props.column.substring(1).toLowerCase()
})

const sort = () => {
  if (props.unsortable) return
  let ascending = props.params.ascending
  if (props.params.sortBy === props.column) ascending = !ascending
  props.onSort({ ...props.params, page: 1, sortBy: props.column, ascending })
}
</script>
<template>
  <th @click="sort" :class="{ pointer: !unsortable }">
    <b>{{ text }}</b>
    <span class="ms-2" v-if="!unsortable && props.params.sortBy === props.column">
      <template v-if="props.params.ascending">
        <SortUpIcon />
      </template>
      <template v-else>
        <SortDownIcon />
      </template>
    </span>
  </th>
</template>
<style>
.pointer {
  cursor: pointer;
}
</style>
