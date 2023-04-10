<script setup lang="ts">
import { useAuth } from '@/stores/auth';
const auth = useAuth()
auth.initialize()
</script>
<template>
  <template v-if="!auth.initialized">
    <div class="vh-100 d-flex justify-content-center align-items-center">
      <div class="spinner-border" role="status"></div>
    </div>
  </template>
  <template v-else-if="auth.isAuthenticated">
    <header class="navbar navbar-expand-lg bd-navbar sticky-top bg-dark">
      <nav class="container-xxl bd-gutter flex-wrap flex-lg-nowrap">
        <div class="d-lg-none" style="width: 1.5rem;"></div>

        <RouterLink class="navbar-brand p-0 me-0 me-lg-2" to="/"><b>Obiteljski album</b></RouterLink>

        <div class="d-flex">
          <button class="navbar-toggler d-flex d-lg-none order-3 p-2" type="button" data-bs-toggle="offcanvas"
            data-bs-target="#appNavbar" aria-controls="appNavbar" aria-label="Toggle navigation">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
              class="bi bi-three-dots-vertical" viewBox="0 0 16 16">
              <path
                d="M9.5 13a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0zm0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0zm0-5a1.5 1.5 0 1 1-3 0 1.5 1.5 0 0 1 3 0z" />
            </svg>
          </button>
        </div>

        <div class="offcanvas-lg offcanvas-end flex-grow-1" tabindex="-1" id="appNavbar"
          aria-labelledby="appNavbarOffcanvasLabel" data-bs-scroll="true">
          <div class="offcanvas-header px-4 pb-0">
            <h5 class="offcanvas-title text-white" id="appNavbarOffcanvasLabel">Obiteljski album</h5>
            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="offcanvas" aria-label="Close"
              data-bs-target="#appNavbar"></button>
          </div>

          <div class="offcanvas-body p-4 pt-0 p-lg-0">
            <hr class="d-lg-none text-white-50">
            <ul class="navbar-nav flex-row flex-wrap">
              <li v-if="auth.isAdmin" class="nav-item col-6 col-lg-auto" data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas">
                <RouterLink class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'users' }">Korisnici</RouterLink>
              </li>
              <li v-if="auth.isAdmin" class="nav-item col-6 col-lg-auto" data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas">
                <RouterLink class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'uploads' }">Otprema</RouterLink>
              </li>
            </ul>
            <hr class="d-lg-none text-white-50">
            <ul class="navbar-nav flex-row flex-wrap ms-md-auto">
              <li v-if="auth.isAdmin" class="nav-item col-6 col-lg-auto" data-bs-target="#appNavbar"
                data-bs-dismiss="offcanvas">
                <a class="nav-link py-2 px-0 px-lg-2" href="/jobs" title="Jobs">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                    class="bi bi-calendar-range-fill" viewBox="0 0 16 16">
                    <path
                      d="M4 .5a.5.5 0 0 0-1 0V1H2a2 2 0 0 0-2 2v1h16V3a2 2 0 0 0-2-2h-1V.5a.5.5 0 0 0-1 0V1H4V.5zM16 7V5H0v5h5a1 1 0 1 1 0 2H0v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9h-6a1 1 0 1 1 0-2h6z" />
                  </svg>
                  <small class="d-lg-none ms-2">Jobs</small>
                </a>
              </li>
              <li class="nav-item py-2 py-lg-1 col-12 col-lg-auto">
                <div class="vr d-none d-lg-flex h-100 mx-lg-2 text-white"></div>
                <hr class="d-lg-none my-2 text-white-50">
              </li>
              <li class="nav-item col-6 col-lg-auto" data-bs-target="#appNavbar" data-bs-dismiss="offcanvas"
                title="Odjava">
                <RouterLink class="nav-link py-2 px-0 px-lg-2" :to="{ name: 'logout' }">
                  <span class="me-2">{{ auth.username }}</span>
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor"
                    class="bi bi-box-arrow-right" viewBox="0 0 16 16">
                    <path fill-rule="evenodd"
                      d="M10 12.5a.5.5 0 0 1-.5.5h-8a.5.5 0 0 1-.5-.5v-9a.5.5 0 0 1 .5-.5h8a.5.5 0 0 1 .5.5v2a.5.5 0 0 0 1 0v-2A1.5 1.5 0 0 0 9.5 2h-8A1.5 1.5 0 0 0 0 3.5v9A1.5 1.5 0 0 0 1.5 14h8a1.5 1.5 0 0 0 1.5-1.5v-2a.5.5 0 0 0-1 0v2z" />
                    <path fill-rule="evenodd"
                      d="M15.854 8.354a.5.5 0 0 0 0-.708l-3-3a.5.5 0 0 0-.708.708L14.293 7.5H5.5a.5.5 0 0 0 0 1h8.793l-2.147 2.146a.5.5 0 0 0 .708.708l3-3z" />
                  </svg>
                  <small class="d-lg-none ms-2">Odjava</small>
                </RouterLink>
              </li>
            </ul>
          </div>
        </div>
      </nav>
    </header>
    <div class="container mt-4">
      <RouterView />
    </div>
  </template>
  <template v-else>
    <div class="container-fluid">
      <RouterView />
    </div>
  </template>
</template>
<style>
.bd-navbar {
  padding: .75rem 0;
  background-color: transparent;
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15), inset 0 -1px 0 rgba(255, 255, 255, 0.15);
}

.bd-gutter {
  --bs-gutter-x: 3rem;
}
</style>