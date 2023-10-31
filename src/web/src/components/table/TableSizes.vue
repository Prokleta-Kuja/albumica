<script setup lang="ts">
import { type ITableParams, defaultPageSizes } from '.';

const props = defineProps<{ params: ITableParams, onChange: (params: ITableParams) => void }>();
const change = () => {
    props.params.page = 1;
    props.onChange(props.params);
}
</script>
<template>
    <div>
        <label for="items-per-page" class="form-label">Stavki po stranici</label>
        <select id="items-per-page" class="form-select" v-model="props.params.size" @change="change">
            <option v-for="size in defaultPageSizes" :key="size" :value="size">{{ size }}</option>
        </select>
    </div>
</template>