<script setup lang="ts">
import { reactive, ref } from 'vue'
import { type UserCM, UserService, type UserVM } from '@/api'
import type IModelState from '@/components/form/modelState'
import Modal from '@/components/Modal.vue'
import SpinButton from '@/components/form/SpinButton.vue'
import Text from '@/components/form/TextBox.vue'
import CheckBox from '@/components/form/CheckBox.vue'
import PlusLgIcon from '@/components/icons/PlusLgIcon.vue'

export interface IAddUser {
  onAdded?: (addedUser: UserVM) => void
}

const props = defineProps<IAddUser>()
const blank = (): UserCM => ({ name: '', displayName: '', isAdmin: false, password: '' })
const shown = ref(false)
const user = reactive<IModelState<UserCM>>({ model: blank() })

const toggle = () => (shown.value = !shown.value)
const submit = () => {
  user.submitting = true
  user.error = undefined
  UserService.createUser({ requestBody: user.model })
    .then((r) => {
      shown.value = false
      if (props.onAdded) props.onAdded(r)
    })
    .catch((r) => (user.error = r.body))
    .finally(() => (user.submitting = false))
}
</script>
<template>
  <button class="btn btn-success" @click="toggle">
    <PlusLgIcon />
    Dodaj
  </button>
  <Modal title="Dodaj korisnika" :shown="shown" :onClose="toggle">
    <template #body>
      <form @submit.prevent="submit">
        <Text
          class="mb-3"
          label="Naziv"
          autoFocus
          v-model="user.model.name"
          required
          :error="user.error?.errors?.name"
        />
        <Text
          class="mb-3"
          label="Prikaz"
          v-model="user.model.displayName"
          :error="user.error?.errors?.displayName"
        />
        <Text
          class="mb-3"
          label="Lozinka"
          :autoComplete="'off'"
          :type="'password'"
          v-model="user.model.password"
          :error="user.error?.errors?.password"
          required
        />
        <CheckBox
          class="mb-3"
          label="Admin"
          v-model="user.model.isAdmin"
          :error="user.error?.errors?.isAdmin"
        />
      </form>
    </template>
    <template #footer>
      <p v-if="user.error" class="text-danger">{{ user.error.message }}</p>
      <button class="btn btn-outline-danger" @click="toggle">Odustani</button>
      <SpinButton
        class="btn-primary"
        :loading="user.submitting"
        text="Dodaj"
        loadingText="Dodaje se"
        @click="submit"
      />
    </template>
  </Modal>
</template>
