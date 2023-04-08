<script setup lang="ts">
import { MediaService, type MediaLM } from '@/api';
import { initParams, updateParams, type ITableParams } from '@/components/table';
import { reactive, computed } from 'vue';

const data = reactive<{ params: ITableParams, items: MediaLM[], loading: boolean }>({ params: initParams(), items: [], loading: true });
const refresh = (params?: ITableParams) => {
  if (params)
    data.params = params;

  data.loading = true;
  MediaService.getMedia({ ...data.params }).then(r => {
    data.items = r.items;
    updateParams(data.params, r);
    data.loading = false;
  });
};

const hasMore = computed(() => data.params.page * data.params.size < data.params.total)
const changePage = (by: number) => {
  data.params.page = data.params.page + by;
  // data.items = [];
  // setTimeout(refresh, 250);
  refresh();
}

//TODO: remove this
data.params.size = 5;
refresh();
</script>

<template>
  <main>
    <div v-if="data.loading" class="d-flex justify-content-center">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    <template v-else>
      <button v-if="data.params.page !== 1" class="btn btn-primary w-100 mb-3" @click="changePage(-1)">Natrag</button>
      <template v-for="item in data.items" :key="item.id">
        <a v-if="item.preview" :href="item.original" target="_blank">
          <img :src="item.preview" class="w-100">
        </a>
        <img v-else src="/no-image.png" class="w-100">
      </template>
      <button v-if="hasMore" class="btn btn-primary w-100 my-3" @click="changePage(1)">Dalje</button>
      <p v-else class="my-3 text-center h5">Nema više</p>
    </template>
  </main>
</template>
