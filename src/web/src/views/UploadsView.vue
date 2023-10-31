<script setup lang="ts">
import { UploadService } from '@/api';
import { reactive } from 'vue';

const state = reactive<{ percent?: string, error?: boolean, complete?: boolean }>({});
const updateProgress = (e: ProgressEvent<XMLHttpRequestEventTarget>) => {
    const val = (e.loaded / e.total) * 100;
    state.complete = val === 100;
    state.percent = state.complete ? undefined : `${val.toFixed(2)}%`;
}
const failed = () => {
    state.percent = undefined;
    state.error = true;
}
const clearComplete = (process?: boolean) => {
    state.complete = false;
    if (process)
        UploadService.processQueue()
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
    <div v-if="state.error">Pogreška</div>
    <div v-else-if="state.complete">
        <p>Slike i(li) videi su dodani.</p>
        <div class="btn-group w-100">
            <button class="btn btn-success" @click="clearComplete()">Dodaj još</button>
            <button class="btn btn-primary" @click="clearComplete(true)">Obradi dodano</button>
        </div>
    </div>
    <div v-else-if="state.percent" class="progress" role="progressbar">
        <div class="progress-bar" :style="{ width: state.percent }"></div>
    </div>

    <div v-else class="mb-3">
        <label for="add-to-queue" class="form-label">Dodaj slike ili videe</label>
        <input class="form-control" type="file" id="add-to-queue" multiple accept="*.jpg,*.jpeg,*.png,*.mov,*.avi,*.mp4"
            @change="upload">
    </div>
    <div class="accordion mb-3 mt-5" id="danger-accordion">
        <div class="accordion-item">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse"
                    data-bs-target="#danger-zone" aria-controls="danger-zone">
                    Ne diraj
                </button>
            </h2>
            <div id="danger-zone" class="accordion-collapse collapse" data-bs-parent="#danger-accordion">
                <div class="accordion-body">
                    <button type="button" class="btn btn-danger w-50" @click="clearComplete(true)">Obradi ponovno</button>
                    <button type="button" class="btn btn-warning w-50" @click="UploadService.reparseMissingCreated">Parsiraj
                        neuspjele</button>
                </div>
            </div>
        </div>
    </div>
</template>