<script setup lang="ts">
import { reactive } from 'vue';
import { type UserUM, UserService, type UserVM } from '@/api';
import type IModelState from '@/components/form/modelState';
import Modal from '@/components/Modal.vue';
import SpinButton from '@/components/form/SpinButton.vue';
import Text from '@/components/form/TextBox.vue';
import CheckBox from '@/components/form/CheckBox.vue';
export interface IEditUser {
    model: UserVM,
    onUpdated?: (updatedDomain?: UserVM) => void
}

const mapUserModel = (m: UserVM): UserUM =>
({
    name: m.name,
    displayName: m.displayName,
    isAdmin: m.isAdmin,
    disabled: m.disabled ? true : false,
})
const props = defineProps<IEditUser>()
const user = reactive<IModelState<UserUM>>({ model: mapUserModel(props.model) })

const toggle = () => { if (props.onUpdated) props.onUpdated(); }
const submit = () => {
    user.submitting = true;
    user.error = undefined;
    UserService.updateUser({ userId: props.model.id, requestBody: user.model })
        .then(r => {
            if (props.onUpdated)
                props.onUpdated(r);
        })
        .catch(r => user.error = r.body)
        .finally(() => user.submitting = false);
};
</script>
<template>
    <Modal v-if="user.model" title="Uredi korisnika" shown :onClose="toggle">
        <template #body>
            <form @submit.prevent="submit">
                <Text class="mb-3" label="Naziv" autoFocus v-model="user.model.name" required
                    :error="user.error?.errors?.name" />
                <Text class="mb-3" label="Prikaz" v-model="user.model.displayName"
                    :error="user.error?.errors?.displayName" />
                <Text class="mb-3" label="Promijeni lozinku" :autoComplete="'off'" :type="'password'"
                    v-model="user.model.newPassword" :error="user.error?.errors?.newPassword" />
                <CheckBox class="mb-3" label="Admin" v-model="user.model.isAdmin" :error="user.error?.errors?.isAdmin" />
                <CheckBox class="mb-3" label="Onemogućen" v-model="user.model.disabled"
                    :error="user.error?.errors?.disabled" />
            </form>
        </template>
        <template #footer>
            <p v-if="user.error" class="text-danger">{{ user.error.message }}</p>
            <button class="btn btn-outline-danger" @click="toggle">Odustani</button>
            <SpinButton class="btn-primary" :loading="user.submitting" text="Spremi" loadingText="Spremanje"
                @click="submit" />
        </template>
    </Modal>
</template>