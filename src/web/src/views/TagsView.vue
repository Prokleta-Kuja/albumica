<script setup lang="ts">
import { reactive } from 'vue'
import { type TagLM, type TagVM, TagService } from '@/api'
import Search from '@/components/form/SearchBox.vue'
import {
  Header,
  Pages,
  Sizes,
  type ITableParams,
  initParams,
  updateParams
} from '@/components/table'
import AddTag from '@/modals/AddTag.vue'
import ConfirmationModal from '@/components/ConfirmationModal.vue'
import EditTag from '@/modals/EditTag.vue'

interface ITagParams extends ITableParams {
  searchTerm?: string
}

const data = reactive<{ params: ITagParams; items: TagLM[]; update?: TagLM; delete?: TagLM }>({
  params: initParams(),
  items: []
})
const refresh = (params?: ITableParams) => {
  if (params) data.params = params

  TagService.getTags({ ...data.params }).then((r) => {
    data.items = r.items
    updateParams(data.params, r)
  })
}
const showEdit = (tag: TagLM) => (data.update = tag)
const hideEdit = (tag?: TagVM) => {
  data.update = undefined
  if (tag) refresh()
}
const showDelete = (tag: TagLM) => (data.delete = tag)
const hideDelete = () => (data.delete = undefined)
const deleteTag = () => {
  if (!data.delete) return

  TagService.deleteTag({ tagId: data.delete.id })
    .then(() => {
      refresh()
      hideDelete()
    })
    .catch(() => {
      /* TODO: show error */
    })
}

refresh()
</script>
<template>
  <div class="d-flex align-items-center flex-wrap">
    <h1 class="display-6 me-3">Oznake</h1>
    <AddTag :onAdded="() => refresh()" />
    <EditTag v-if="data.update" :model="data.update" :onUpdated="hideEdit" />
  </div>
  <div class="d-flex flex-wrap">
    <Sizes class="me-3 mb-2" style="max-width: 8rem" :params="data.params" :on-change="refresh" />
    <Search
      label="Pretraži"
      autoFocus
      class="me-3 mb-2"
      style="max-width: 16rem"
      placeholder="Naziv"
      v-model="data.params.searchTerm"
      :on-change="refresh"
    />
  </div>
  <div class="table-responsive">
    <table class="table">
      <thead>
        <tr>
          <Header :params="data.params" :on-sort="refresh" column="name" display="Naziv" />
          <Header :params="data.params" :on-sort="refresh" column="order" display="Slijed" />
          <Header :params="data.params" :on-sort="refresh" column="mediaCount" display="Medija" />
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in data.items" :key="item.id" class="align-middle">
          <td>{{ item.name }}</td>
          <td>{{ item.order }}</td>
          <td>{{ item.mediaCount }}</td>
          <td class="text-end p-1">
            <div class="btn-group" role="group">
              <button class="btn btn-sm btn-secondary" @click="showEdit(item)" title="Edit">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="16"
                  height="16"
                  fill="currentColor"
                  class="bi bi-pencil-square"
                  viewBox="0 0 16 16"
                >
                  <path
                    d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z"
                  />
                  <path
                    fill-rule="evenodd"
                    d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5v11z"
                  />
                </svg>
              </button>
              <button class="btn btn-sm btn-danger" title="Delete" @click="showDelete(item)">
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
          </td>
        </tr>
      </tbody>
    </table>
  </div>
  <Pages :params="data.params" :on-change="refresh" />
  <ConfirmationModal
    v-if="data.delete"
    title="Brisanje oznake"
    :onClose="hideDelete"
    :onConfirm="deleteTag"
    shown
  >
    Jesi siguran da želiš obrisati oznaku <b>{{ data.delete.name }}</b
    >?
  </ConfirmationModal>
</template>
