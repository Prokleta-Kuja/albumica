<script setup lang="ts">
import { reactive } from 'vue'
import { type UserLM, UserService } from '@/api'
import Search from '@/components/form/SearchBox.vue'
import {
  Header,
  Pages,
  Sizes,
  type ITableParams,
  initParams,
  updateParams
} from '@/components/table'
import AddUser from '@/modals/AddUser.vue'
import ConfirmationModal from '@/components/ConfirmationModal.vue'
import EditUser from '@/modals/EditUser.vue'
import XLgIcon from '@/components/icons/XLgIcon.vue'
import PencilSquareIcon from '@/components/icons/PencilSquareIcon.vue'
import CheckLgIcon from '@/components/icons/CheckLgIcon.vue'

interface IUserParams extends ITableParams {
  searchTerm?: string
}

const data = reactive<{ params: IUserParams; items: UserLM[]; update?: UserLM; delete?: UserLM }>({
  params: initParams(),
  items: []
})
const refresh = (params?: ITableParams) => {
  if (params) data.params = params

  UserService.getUsers({ ...data.params }).then((r) => {
    data.items = r.items
    updateParams(data.params, r)
  })
}
const showEdit = (user: UserLM) => (data.update = user)
const hideEdit = (user?: UserLM) => {
  data.update = undefined
  if (user) refresh()
}
const showDelete = (user: UserLM) => (data.delete = user)
const hideDelete = () => (data.delete = undefined)
const deleteUser = () => {
  if (!data.delete) return

  UserService.deleteUser({ userId: data.delete.id })
    .then(() => {
      refresh()
      hideDelete()
    })
    .catch(() => {
      /* TODO: show error */
    })
}

const disabledText = (dateTime: string | null | undefined) => {
  if (!dateTime) return '-'
  var dt = new Date(dateTime)
  return dt.toLocaleString()
}

refresh()
</script>
<template>
  <div class="d-flex align-items-center flex-wrap">
    <h1 class="display-6 me-3">Korisnici</h1>
    <AddUser :onAdded="() => refresh()" />
    <EditUser v-if="data.update" :model="data.update" :onUpdated="hideEdit" />
  </div>
  <div class="d-flex flex-wrap">
    <Sizes class="me-3 mb-2" style="max-width: 8rem" :params="data.params" :on-change="refresh" />
    <Search
      label="Pretraži"
      autoFocus
      class="me-3 mb-2"
      style="max-width: 16rem"
      placeholder="Naziv, prikaz"
      v-model="data.params.searchTerm"
      :on-change="refresh"
    />
  </div>
  <div class="table-responsive">
    <table class="table">
      <thead>
        <tr>
          <Header :params="data.params" :on-sort="refresh" column="name" display="Naziv" />
          <Header :params="data.params" :on-sort="refresh" column="displayName" display="Prikaz" />
          <Header :params="data.params" :on-sort="refresh" column="isAdmin" display="Admin" />
          <Header :params="data.params" :on-sort="refresh" column="disabled" display="Onemogućen" />
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in data.items" :key="item.id" class="align-middle">
          <td>{{ item.name }}</td>
          <td>{{ item.displayName }}</td>
          <td>
            <CheckLgIcon v-if="item.isAdmin" />
            <XLgIcon v-else />
          </td>
          <td>{{ disabledText(item.disabled) }}</td>
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
    title="Brisanje korisnika"
    :onClose="hideDelete"
    :onConfirm="deleteUser"
    shown
  >
    Jesi siguran da želiš obrisati korisnika <b>{{ data.delete.name }}</b
    >?
  </ConfirmationModal>
</template>
