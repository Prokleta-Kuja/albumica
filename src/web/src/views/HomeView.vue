<script setup lang="ts">
import { MediaService, type MediaLM, TagService, type TagLM, type MediaVM, MediaView } from '@/api'
import { initParams, updateParams, type ITableParams } from '@/components/table'
import { useAuth } from '@/stores/auth'
import { useBasket } from '@/stores/basket'
import { reactive, computed } from 'vue'
import EditMedia from '@/modals/EditMedia.vue'
import RemoveMedia from '@/modals/RemoveMedia.vue'
import TagsFillIcon from '@/components/icons/TagsFillIcon.vue'
import BagXFillIcon from '@/components/icons/BagXFillIcon.vue'
import BagPlusFillIcon from '@/components/icons/BagPlusFillIcon.vue'

export interface IMediaParams extends ITableParams {
  view?: MediaView
  inBasket?: boolean
  hidden?: boolean
  tagIds?: number[]
}

const auth = useAuth()
const basket = useBasket()
const state = reactive<{
  basketOnly: boolean
  tags: TagLM[]
  tagIds: Set<number>
}>({ basketOnly: false, tags: [], tagIds: new Set() })
const data = reactive<{
  params: IMediaParams
  items: MediaLM[]
  loading: boolean
  selected?: MediaLM
}>({ params: initParams(), items: [], loading: true })
const refresh = (params?: ITableParams) => {
  if (params) data.params = params
  if (state.basketOnly) {
    if (basket.itemIds.size > 0) data.params.inBasket = true
    else {
      data.params.page = 1
      data.params.inBasket = undefined
    }
  } else data.params.inBasket = undefined
  data.params.tagIds = state.tagIds.size > 0 ? [...state.tagIds] : undefined

  data.loading = true
  MediaService.getMedia({ ...data.params }).then((r) => {
    data.items = r.items
    updateParams(data.params, r)
    data.loading = false
  })
}
const refreshToStart = () => {
  data.params.page = 1
  refresh()
}
const refreshTags = () => TagService.getTags({ size: 100 }).then((r) => (state.tags = r.items))

const hasMore = computed(() => data.params.page * data.params.size < data.params.total)
const changePage = (by: number) => {
  data.params.page = data.params.page + by
  refresh()
}
const setSelected = (item: MediaLM) => (data.selected = item)
const clearSelected = (model?: MediaVM) => {
  if (data.selected && model) {
    data.selected.hasTags = model.tagIds.length > 0
    data.selected.hidden = model.hidden
    data.selected.created = model.created
    refreshTags()
  }
  data.selected = undefined
}
const addToBasket = (item: MediaLM) => {
  item.inBasket = true
  basket.addItem(item.id)
  MediaService.addToBasket({ mediaId: item.id }).catch(() => {
    item.inBasket = false
    basket.removeItem(item.id)
    if (state.basketOnly && basket.itemIds.size === 0) {
      state.basketOnly = false
      data.params.page = 1
    }
  })
}

const removeFromBasket = (item: MediaLM) => {
  item.inBasket = false
  basket.removeItem(item.id)
  if (state.basketOnly && basket.itemIds.size === 0) {
    state.basketOnly = false
    data.params.page = 1
  }
  MediaService.removeFromBasket({ mediaId: item.id }).catch(() => {
    item.inBasket = true
    basket.addItem(item.id)
  })
}
const toggleTag = (tagId: number) => {
  if (state.tagIds.has(tagId)) state.tagIds.delete(tagId)
  else state.tagIds.add(tagId)
  refreshToStart()
}

const setMediaView = (val: MediaView) => {
  data.params.view = val
  refreshToStart()
}

const setBasketView = (val: boolean) => {
  state.basketOnly = val
  refreshToStart()
}

const dateText = (dateTime: string | null | undefined) => {
  if (!dateTime) return '-'
  var dt = new Date(dateTime)
  return dt.toLocaleDateString()
}

data.params.size = 100
refresh()
refreshTags()
</script>
<template>
  <main>
    <EditMedia
      v-if="data.selected"
      :modelId="data.selected.id"
      :tags="state.tags"
      :onClosed="clearSelected"
    />
    <div class="accordion mb-3" id="home-accordion">
      <div class="accordion-item">
        <h2 class="accordion-header">
          <button
            class="accordion-button collapsed"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#home-filter"
            aria-controls="home-filter"
          >
            Filter
          </button>
        </h2>
        <div id="home-filter" class="accordion-collapse collapse" data-bs-parent="#home-accordion">
          <div class="accordion-body">
            <div v-if="auth.isAdmin" class="mb-4">
              <div class="form-check form-check-inline">
                <input
                  class="form-check-input"
                  type="radio"
                  name="view"
                  id="all"
                  :checked="data.params.view === MediaView.All"
                  @change="setMediaView(MediaView.All)"
                />
                <label class="form-check-label" for="all">Sve</label>
              </div>
              <div class="form-check form-check-inline">
                <input
                  class="form-check-input"
                  type="radio"
                  name="view"
                  id="onlycreate"
                  :checked="data.params.view === MediaView.OnlyCreate"
                  @change="setMediaView(MediaView.OnlyCreate)"
                />
                <label class="form-check-label" for="onlycreate">Sa datumima</label>
              </div>
              <div class="form-check form-check-inline">
                <input
                  class="form-check-input"
                  type="radio"
                  name="view"
                  id="nocreate"
                  :checked="data.params.view === MediaView.NoCreate"
                  @change="setMediaView(MediaView.NoCreate)"
                />
                <label class="form-check-label" for="nocreate">Bez datuma</label>
              </div>
              <div class="form-check form-check-inline">
                <input
                  class="form-check-input"
                  type="radio"
                  name="view"
                  id="notags"
                  :checked="data.params.view === MediaView.NoTags"
                  @change="setMediaView(MediaView.NoTags)"
                />
                <label class="form-check-label" for="notags">Bez oznaka</label>
              </div>
              <div class="form-check form-check-inline">
                <input
                  class="form-check-input"
                  type="radio"
                  name="view"
                  id="nopreview"
                  :checked="data.params.view === MediaView.NoPreview"
                  @change="setMediaView(MediaView.NoPreview)"
                />
                <label class="form-check-label" for="nopreview">Bez prikaza</label>
              </div>
            </div>
            <template v-if="basket.itemIds.size > 0">
              <div class="btn-group d-block mb-3" role="group">
                <button
                  type="button"
                  class="btn btn-success w-50"
                  :class="{ active: !state.basketOnly }"
                  @click="setBasketView(false)"
                >
                  Sve slike
                </button>
                <button
                  type="button"
                  class="btn btn-success w-50"
                  :class="{ active: state.basketOnly }"
                  @click="setBasketView(true)"
                >
                  Košarica {{ basket.itemIds.size.toLocaleString() }}
                </button>
              </div>
              <RemoveMedia v-if="auth.isAdmin" @removed="refreshToStart" />
            </template>
            <ul class="list-group">
              <li
                v-for="tag in state.tags"
                :key="tag.id"
                class="list-group-item d-flex justify-content-between pointer"
                @click="toggleTag(tag.id)"
                :class="{ active: state.tagIds.has(tag.id) }"
              >
                <span>{{ tag.name }}</span>
                <span> {{ tag.mediaCount.toLocaleString() }}</span>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
    <div v-if="data.loading" class="d-flex justify-content-center">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    <template v-else>
      <button
        v-if="data.params.page !== 1"
        class="btn btn-primary w-100 mb-3"
        @click="changePage(-1)"
      >
        Natrag
      </button>
      <div class="row align-items-center row-cols-sm-1 row-cols-md-2 row-cols-lg-6">
        <div v-for="item in data.items" :key="item.id">
          <div class="card mb-2" :class="{ 'text-bg-info': item.hidden }">
            <a :href="item.original" target="_blank">
              <img v-if="item.preview" :src="item.preview" class="card-img-top" />
              <img v-else src="/no-image.png" class="card-img-top" />
            </a>
            <div
              class="card-footer text-body-secondary d-flex align-items-center justify-content-between"
            >
              <span class="user-select-all">{{ dateText(item.created) }}</span>
              <div class="btn-group">
                <template v-if="auth.isAdmin">
                  <button
                    v-if="item.hasTags"
                    class="btn btn-sm btn-secondary"
                    title="Ima oznake"
                    @click="setSelected(item)"
                  >
                    <TagsFillIcon />
                  </button>
                  <button
                    v-else
                    class="btn btn-sm btn-warning"
                    title="Nema oznake"
                    @click="setSelected(item)"
                  >
                    <TagsFillIcon />
                  </button>
                </template>
                <button
                  v-if="item.inBasket"
                  class="btn btn-sm btn-outline-danger"
                  @click="removeFromBasket(item)"
                  title="Ukloni iz košarice"
                >
                  <BagXFillIcon />
                </button>
                <button
                  v-else
                  class="btn btn-sm btn-outline-success"
                  @click="addToBasket(item)"
                  title="Dodaj u košaricu"
                >
                  <BagPlusFillIcon />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
      <button v-if="hasMore" class="btn btn-primary w-100 my-3" @click="changePage(1)">
        Dalje
      </button>
      <p v-else class="my-3 text-center h5">Nema više medija</p>
    </template>
  </main>
</template>
