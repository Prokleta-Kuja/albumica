<script setup lang="ts">
import { useRouter } from 'vue-router'
import { AuthService } from '@/api'
import { ref } from 'vue'
import { useAuth } from '@/stores/auth'

const auth = useAuth()
const router = useRouter()
const loading = ref(true)
const goToLogin = () => router.replace({ name: 'login' })
if (auth.isAuthenticated) AuthService.logout().then(() => auth.clearLoginInfo())
else loading.value = false
</script>
<template>
  <div class="row vh-100 g-0">
    <div class="col-lg-8 position-relative d-none d-lg-block">
      <div class="bg-holder"></div>
    </div>
    <div class="col-lg-4">
      <div class="row d-flex align-items-center justify-content-center h-100 g-0 px-4 px-sm-0">
        <div class="col col-sm-6 col-lg-8 col-xl-8">
          <div class="d-flex justify-content-center">
            <img src="/android-chrome-192x192.png" width="58" />
          </div>
          <template v-if="loading">
            <div class="text-center mt-5">
              <div class="spinner-border" role="status"></div>
            </div>
          </template>
          <template v-else>
            <div class="text-center mb-7">
              <h3 class="text-1000">Odjava</h3>
              <p class="text-700">Odjavljeni ste</p>
            </div>
            <button class="btn btn-primary w-100 mb-3" @click="goToLogin">Prijavi se opet</button>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>
<style>
.bg-holder {
  background-image: url(/login.jpg);
  position: absolute;
  width: 100%;
  min-height: 100%;
  top: 0;
  left: 0;
  background-size: cover;
  background-position: center;
  overflow: hidden;
  will-change: transform, opacity, filter;
  backface-visibility: hidden;
  background-repeat: no-repeat;
  z-index: 0;
}
</style>
