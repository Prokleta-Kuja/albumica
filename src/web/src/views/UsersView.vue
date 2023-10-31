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
            <svg
              v-if="item.isAdmin"
              xmlns="http://www.w3.org/2000/svg"
              width="16"
              height="16"
              fill="currentColor"
              class="bi bi-check-lg text-success"
              viewBox="0 0 16 16"
            >
              <path
                d="M12.736 3.97a.733.733 0 0 1 1.047 0c.286.289.29.756.01 1.05L7.88 12.01a.733.733 0 0 1-1.065.02L3.217 8.384a.757.757 0 0 1 0-1.06.733.733 0 0 1 1.047 0l3.052 3.093 5.4-6.425a.247.247 0 0 1 .02-.022Z"
              />
            </svg>
            <svg
              v-else
              xmlns="http://www.w3.org/2000/svg"
              width="16"
              height="16"
              fill="currentColor"
              class="bi bi-x-lg text-danger"
              viewBox="0 0 16 16"
            >
              <path
                d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z"
              />
            </svg>
          </td>
          <td>{{ disabledText(item.disabled) }}</td>
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
    title="Brisanje korisnika"
    :onClose="hideDelete"
    :onConfirm="deleteUser"
    shown
  >
    Jesi siguran da želiš obrisati korisnika <b>{{ data.delete.name }}</b
    >?
  </ConfirmationModal>
</template>
