<script setup lang="ts">
import { MediaService, type MediaLM, TagService, type TagLM } from '@/api';
import { initParams, updateParams, type ITableParams } from '@/components/table';
import { useAuth } from '@/stores/auth';
import { useBasket } from '@/stores/basket';
import { reactive, computed } from 'vue';
import EditMedia from '@/modals/EditMedia.vue'
import RemoveMedia from '@/modals/RemoveMedia.vue';

export interface IMediaParams extends ITableParams {
  noCreate?: boolean;
  inBasket?: boolean;
  hidden?: boolean;
  tagIds?: number[];
}

const auth = useAuth();
const basket = useBasket();
const state = reactive<{ basketOnly: boolean, noCreate?: boolean, tags: TagLM[], tagIds: Set<number> }>({ basketOnly: false, tags: [], tagIds: new Set() })
const data = reactive<{ params: IMediaParams, items: MediaLM[], loading: boolean, selected?: MediaLM }>({ params: initParams(), items: [], loading: true });
const refresh = (params?: ITableParams) => {
  if (params)
    data.params = params;
  data.params.noCreate = auth.isAdmin ? state.noCreate : false;
  data.params.inBasket = state.basketOnly ? true : undefined;
  data.params.tagIds = state.tagIds.size > 0 ? [...state.tagIds] : undefined;

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
  refresh();
}
const setSelected = (item: MediaLM) => data.selected = item;
const clearSelected = (hasTags: boolean, hidden: boolean) => {
  if (data.selected) {
    data.selected.hasTags = hasTags;
    data.selected.hidden = hidden;
    data.selected = undefined;
  }
}
const addToBasket = (item: MediaLM) => MediaService.addToBasket({ mediaId: item.id })
  .then(() => {
    item.inBasket = true;
    basket.addItem(item.id);
  })
  .catch(() => {/* TODO: notify  */ })

const removeFromBasket = (item: MediaLM) => MediaService.removeFromBasket({ mediaId: item.id })
  .then(() => {
    item.inBasket = false;
    basket.removeItem(item.id);
    if (state.basketOnly && basket.itemIds.size === 0)
      state.basketOnly = false;
  })
  .catch(() => {/* TODO: notify  */ })
const toggleTag = (tagId: number) => {
  if (state.tagIds.has(tagId))
    state.tagIds.delete(tagId);
  else
    state.tagIds.add(tagId);
  refresh();
}

const setBasketView = (val: boolean) => {
  state.basketOnly = val;
  refresh();
}

const toggleNoCreate = () => {
  state.noCreate = !state.noCreate;
  refresh();
}

const dateText = (dateTime: string | null | undefined) => {
  if (!dateTime)
    return '-';
  var dt = new Date(dateTime);
  return dt.toLocaleDateString();
}

data.params.size = 100;
refresh();
TagService.getTags({ size: 100, sortBy: 'Order' })
  .then(r => state.tags = r.items)
</script>
<template>
  <main>
    <EditMedia v-if="data.selected" :modelId="data.selected.id" :tags="state.tags" :onClosed="clearSelected" />
    <div v-if="data.loading" class="d-flex justify-content-center">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    <template v-else>
      <div class="accordion mb-3" id="home-accordion">
        <div class="accordion-item">
          <h2 class="accordion-header">
            <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse"
              data-bs-target="#home-filter" aria-controls="home-filter">
              Filter
            </button>
          </h2>
          <div id="home-filter" class="accordion-collapse collapse" data-bs-parent="#home-accordion">
            <div class="accordion-body">
              <template v-if="auth.isAdmin">
                <button v-if="state.noCreate" type="button" class="btn btn-warning w-100 mb-3" @click="toggleNoCreate">Sa
                  datumima</button>
                <button v-else type="button" class="btn btn-warning w-100 mb-3" @click="toggleNoCreate">Bez
                  datuma</button>
              </template>

              <template v-if="basket.itemIds.size > 0">
                <div class="btn-group d-block mb-3" role="group">
                  <button type="button" class="btn btn-success w-50" :class="{ 'active': !state.basketOnly }"
                    @click="setBasketView(false)">Sve slike</button>
                  <button type="button" class="btn btn-success w-50" :class="{ 'active': state.basketOnly }"
                    @click="setBasketView(true)">Košarica</button>
                </div>
                <RemoveMedia v-if="auth.isAdmin" />
              </template>
              <ul class="list-group">
                <li v-for="tag in state.tags" :key="tag.id" class="list-group-item pointer" @click="toggleTag(tag.id)"
                  :class="{ 'active': state.tagIds.has(tag.id) }">{{ tag.name }}{{ tag.mediaCount.toLocaleString() }}</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
      <button v-if="data.params.page !== 1" class="btn btn-primary w-100 mb-3" @click="changePage(-1)">Natrag</button>
      <div class="row align-items-center row-cols-sm-1 row-cols-md-2 row-cols-lg-6">
        <div v-for="item in data.items" :key="item.id">
          <div class="card mb-2" :class="{ 'text-bg-info': item.hidden }">
            <a :href="item.original" target="_blank">
              <img v-if="item.preview" :src="item.preview" class="card-img-top">
              <img v-else src="/no-image.png" class="card-img-top">
            </a>
            <div class="card-footer text-body-secondary d-flex align-items-center justify-content-between">
              <span class="user-select-all">{{ dateText(item.created) }}</span>
              <div class="btn-group">
                <template v-if="auth.isAdmin">
                  <button v-if="item.hasTags" class="btn btn-sm btn-secondary" title="Ima oznake"
                    @click="setSelected(item)">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                      class="bi bi-tags-fill" viewBox="0 0 16 16">
                      <path
                        d="M2 2a1 1 0 0 1 1-1h4.586a1 1 0 0 1 .707.293l7 7a1 1 0 0 1 0 1.414l-4.586 4.586a1 1 0 0 1-1.414 0l-7-7A1 1 0 0 1 2 6.586V2zm3.5 4a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3z" />
                      <path
                        d="M1.293 7.793A1 1 0 0 1 1 7.086V2a1 1 0 0 0-1 1v4.586a1 1 0 0 0 .293.707l7 7a1 1 0 0 0 1.414 0l.043-.043-7.457-7.457z" />
                    </svg>
                  </button>
                  <button v-else class="btn btn-sm btn-warning" title="Nema oznake" @click="setSelected(item)">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                      class="bi bi-tags-fill" viewBox="0 0 16 16">
                      <path
                        d="M2 2a1 1 0 0 1 1-1h4.586a1 1 0 0 1 .707.293l7 7a1 1 0 0 1 0 1.414l-4.586 4.586a1 1 0 0 1-1.414 0l-7-7A1 1 0 0 1 2 6.586V2zm3.5 4a1.5 1.5 0 1 0 0-3 1.5 1.5 0 0 0 0 3z" />
                      <path
                        d="M1.293 7.793A1 1 0 0 1 1 7.086V2a1 1 0 0 0-1 1v4.586a1 1 0 0 0 .293.707l7 7a1 1 0 0 0 1.414 0l.043-.043-7.457-7.457z" />
                    </svg>
                  </button>
                </template>
                <button v-if="item.inBasket" class="btn btn-sm btn-outline-danger" @click="removeFromBasket(item)"
                  title="Ukloni iz košarice">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                    class="bi bi-bag-x-fill" viewBox="0 0 16 16">
                    <path fill-rule="evenodd"
                      d="M10.5 3.5a2.5 2.5 0 0 0-5 0V4h5v-.5zm1 0V4H15v10a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V4h3.5v-.5a3.5 3.5 0 1 1 7 0zM6.854 8.146a.5.5 0 1 0-.708.708L7.293 10l-1.147 1.146a.5.5 0 0 0 .708.708L8 10.707l1.146 1.147a.5.5 0 0 0 .708-.708L8.707 10l1.147-1.146a.5.5 0 0 0-.708-.708L8 9.293 6.854 8.146z" />
                  </svg>
                </button>
                <button v-else class="btn btn-sm btn-outline-success" @click="addToBasket(item)" title="Dodaj u košaricu">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                    class="bi bi-bag-plus-fill" viewBox="0 0 16 16">
                    <path fill-rule="evenodd"
                      d="M10.5 3.5a2.5 2.5 0 0 0-5 0V4h5v-.5zm1 0V4H15v10a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V4h3.5v-.5a3.5 3.5 0 1 1 7 0zM8.5 8a.5.5 0 0 0-1 0v1.5H6a.5.5 0 0 0 0 1h1.5V12a.5.5 0 0 0 1 0v-1.5H10a.5.5 0 0 0 0-1H8.5V8z" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
      <button v-if="hasMore" class="btn btn-primary w-100 my-3" @click="changePage(1)">Dalje</button>
      <p v-else class="my-3 text-center h5">Nema više medija</p>
    </template>
  </main>
</template>
