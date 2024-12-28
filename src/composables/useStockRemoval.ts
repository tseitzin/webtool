import { ref } from 'vue'

export function useStockRemoval() {
  const showRemoveModal = ref(false)
  const stockToRemove = ref<string | null>(null)

  const confirmRemoval = (symbol: string) => {
    stockToRemove.value = symbol
    showRemoveModal.value = true
  }

  const cancelRemoval = () => {
    showRemoveModal.value = false
    stockToRemove.value = null
  }

  return {
    showRemoveModal,
    stockToRemove,
    confirmRemoval,
    cancelRemoval
  }
}