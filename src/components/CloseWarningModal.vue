<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'
import { SessionService } from '../services/sessionService'

const showModal = ref(false)
const auth = useAuthStore()
const router = useRouter()
const sessionService = SessionService.getInstance()

const handleClose = () => {
  if (auth.isAuthenticated && !showModal.value) {
    showModal.value = true
  }
}

onMounted(() => {
  sessionService.onClose(handleClose)
})

onBeforeUnmount(() => {
  sessionService.removeCloseHandler(handleClose)
})

const handleLogout = () => {
  auth.logout()
  showModal.value = false
  router.push('/login')
  window.close()
}

const handleStayLoggedIn = () => {
  showModal.value = false
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="showModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
      @click.self="handleStayLoggedIn"
    >
      <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
        <h2 class="text-xl font-bold mb-4">Warning</h2>
        <p class="mb-6">
          You are about to leave the page while still logged in. Would you like to log out first?
        </p>
        <div class="flex justify-end space-x-4">
          <button
            @click="handleStayLoggedIn"
            class="px-4 py-2 text-gray-600 hover:text-gray-800"
          >
            Stay Logged In
          </button>
          <button
            @click="handleLogout"
            class="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
          >
            Logout
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>