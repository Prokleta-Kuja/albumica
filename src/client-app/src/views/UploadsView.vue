<script setup lang="ts">
import { reactive } from 'vue';

const state = reactive<{ percent?: string, error?: boolean }>({});
const updateProgress = (e: ProgressEvent<XMLHttpRequestEventTarget>) => {
    const val = (e.loaded / e.total) * 100;
    state.percent = val === 100 ? undefined : `${val.toFixed(2)}%`;
}
const failed = () => {
    state.percent = undefined;
    state.error = true;
}
const upload = (e: Event) => {
    var input = e.target as HTMLInputElement;
    if (!input.files)
        return;

    state.percent = '0px';
    state.error = false;
    const fd = new FormData();
    for (let i = 0; i < input.files.length; i++) {
        const file = input.files[i];
        fd.append("file", file, file.name);
    }

    const request = new XMLHttpRequest();
    request.open("POST", "/api/uploads");
    request.upload.addEventListener('progress', updateProgress)
    request.upload.addEventListener('error', failed)
    request.send(fd);
}
</script>
<template>
    <div v-if="state.error">Error occured</div>
    <div v-else-if="state.percent" class="progress" role="progressbar">
        <div class="progress-bar" :style="{ width: state.percent }"></div>
    </div>

    <div v-else class="mb-3">
        <label for="formFileMultiple" class="form-label">Dodaj slike ili videe</label>
        <input class="form-control" type="file" id="formFileMultiple" multiple @change="upload">
    </div>
</template>