<script setup lang="ts">
import { MediaService, type MediaUM, type TagLM } from '@/api';
import Modal from '@/components/Modal.vue';
import SpinButton from '@/components/form/SpinButton.vue';
import DateTimeBox from '@/components/form/DateTimeBox.vue';
import CheckBox from '@/components/form/CheckBox.vue';
import type IModelState from '@/components/form/modelState';
import { reactive } from 'vue';

export interface IEditMedia {
    modelId: number;
    tags: TagLM[];
    onClosed: (hasTags: boolean, hidden: boolean) => void;
}

const props = defineProps<IEditMedia>();
const state = reactive<{ tagIds: Set<number>, title: string }>({ tagIds: new Set(), title: "" })
const media = reactive<IModelState<MediaUM>>({ model: { hidden: false } })
const closed = () => {
    if (props.onClosed)
        props.onClosed(state.tagIds.size > 0, media.model.hidden);
}
const submit = () => {
    media.submitting = true;
    media.error = undefined;
    MediaService.updateMedia({ mediaId: props.modelId, requestBody: media.model })
        .then(closed)
        .catch(r => media.error = r.body)
        .finally(() => media.submitting = false);
};
const toggleTag = (tagId: number) => {
    media.error = undefined;
    if (state.tagIds.has(tagId))
        MediaService.removeTag({ mediaId: props.modelId, tagId: tagId })
            .then(() => state.tagIds.delete(tagId))
            .catch(r => media.error = r.body);
    else
        MediaService.addTag({ mediaId: props.modelId, tagId: tagId })
            .then(() => state.tagIds.add(tagId))
            .catch(r => media.error = r.body);
}

MediaService.getMediaById({ mediaId: props.modelId })
    .then(r => {
        media.model.hidden = r.hidden;
        media.model.created = r.created;
        state.title = r.original.split("/").pop() ?? "";
        r.tagIds.forEach(tagId => state.tagIds.add(tagId));
    })
</script>
<template>
    <Modal :title="state.title" :onClose="closed" shown>
        <template #body>
            <form @submit.prevent="submit">
                <DateTimeBox class="mb-3" label="Snimljeno" autoFocus v-model="media.model.created" required
                    :error="media.error?.errors?.created" />
                <CheckBox class="mb-3" label="Skriveno" v-model="media.model.hidden" required
                    :error="media.error?.errors?.hidden" />
            </form>
            <ul class="list-group">
                <li v-for="tag in props.tags" :key="tag.id" class="list-group-item pointer" @click="toggleTag(tag.id)"
                    :class="{ 'active': state.tagIds.has(tag.id) }">{{ tag.name }}{{ tag.mediaCount.toLocaleString() }}</li>
            </ul>
        </template>
        <template #footer>
            <p v-if="media.error" class="text-danger">{{ media.error.message }}</p>
            <button class="btn btn-outline-danger" @click="closed">Odustani</button>
            <SpinButton class="btn-primary" :loading="media.submitting" text="Spremi" loadingText="Spremanje"
                @click="submit" />
        </template>
    </Modal>
</template>