<script setup lang="ts">
import { computed } from 'vue'
import type { ITableParams } from '.'

const pageOffset = 2
const props = defineProps<{ params: ITableParams; onChange: (params: ITableParams) => void }>()
const fmt = new Intl.NumberFormat()
const counter = computed<{
  startItem: string
  endItem: string
  totalItems: string
  totalPages: number
  pages: number[]
  startPage: number
  endPage: number
}>(() => {
  const last = (props.params.page - 1) * props.params.size
  const startItem = fmt.format(last + 1)
  const endItem = fmt.format(Math.min(last + props.params.size, props.params.total))
  const totalItems = fmt.format(props.params.total)
  const totalPages = Math.max(1, Math.ceil(props.params.total / props.params.size))
  const startPage = Math.max(1, props.params.page - pageOffset)
  const endPage = Math.min(totalPages, props.params.page + pageOffset)
  const pages = []
  for (let i = startPage; i <= endPage; i++) pages.push(i)

  return { startItem, endItem, totalItems, totalPages, pages, startPage, endPage }
})

const change = (page: number) => {
  if (props.params.page === page) return
  props.onChange({ ...props.params, page: page })
}
</script>
<template>
  <div class="d-flex justify-content-between align-items-center flex-wrap-reverse">
    <div v-if="props.params.total === 0">No results</div>
    <div v-else>
      Showing {{ counter.startItem }} to {{ counter.endItem }} of {{ counter.totalItems }} entries.
    </div>
    <nav v-if="counter.totalPages > 1">
      <ul class="pagination mb-0">
        <li
          class="page-item"
          :class="{ disabled: props.params.page === 1, pointer: props.params.page !== 1 }"
          @click="change(1)"
        >
          <span class="page-link" aria-label="Previous">
            <span aria-hidden="true">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="16"
                height="16"
                fill="currentColor"
                class="bi bi-skip-backward-fill"
                viewBox="0 0 16 16"
              >
                <path
                  d="M.5 3.5A.5.5 0 0 0 0 4v8a.5.5 0 0 0 1 0V8.753l6.267 3.636c.54.313 1.233-.066 1.233-.697v-2.94l6.267 3.636c.54.314 1.233-.065 1.233-.696V4.308c0-.63-.693-1.01-1.233-.696L8.5 7.248v-2.94c0-.63-.692-1.01-1.233-.696L1 7.248V4a.5.5 0 0 0-.5-.5z"
                />
              </svg>
            </span>
          </span>
        </li>

        <li class="page-item disabled" v-if="counter.startPage !== 1">
          <span class="page-link">...</span>
        </li>

        <li class="page-item" v-for="page in counter.pages" :key="page" @click="change(page)">
          <span
            class="page-link"
            :class="{ active: props.params.page === page, pointer: props.params.page !== page }"
            >{{ page }}</span
          >
        </li>

        <li class="page-item disabled" v-if="counter.endPage !== counter.totalPages">
          <span class="page-link">...</span>
        </li>

        <li
          class="page-item"
          @click="change(counter.totalPages)"
          :class="{
            disabled: props.params.page === counter.totalPages,
            pointer: props.params.page !== counter.totalPages
          }"
        >
          <span class="page-link" aria-label="Next">
            <span aria-hidden="true">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="16"
                height="16"
                fill="currentColor"
                class="bi bi-skip-forward-fill"
                viewBox="0 0 16 16"
              >
                <path
                  d="M15.5 3.5a.5.5 0 0 1 .5.5v8a.5.5 0 0 1-1 0V8.753l-6.267 3.636c-.54.313-1.233-.066-1.233-.697v-2.94l-6.267 3.636C.693 12.703 0 12.324 0 11.693V4.308c0-.63.693-1.01 1.233-.696L7.5 7.248v-2.94c0-.63.693-1.01 1.233-.696L15 7.248V4a.5.5 0 0 1 .5-.5z"
                />
              </svg>
            </span>
          </span>
        </li>
      </ul>
    </nav>
  </div>
</template>
<style>
.pointer {
  cursor: pointer;
}
</style>
