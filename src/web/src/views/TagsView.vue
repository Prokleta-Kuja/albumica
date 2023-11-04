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
import XLgIcon from '@/components/icons/XLgIcon.vue'
import PencilSquareIcon from '@/components/icons/PencilSquareIcon.vue'

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
                <PencilSquareIcon />
              </button>
              <button class="btn btn-sm btn-danger" title="Delete" @click="showDelete(item)">
                <XLgIcon />
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
