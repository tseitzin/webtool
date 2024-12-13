import { ref, onMounted, watch } from 'vue'

export function useCollapsibleSection(sectionId: string, defaultExpanded = true) {
  const isExpanded = ref(defaultExpanded)

  onMounted(() => {
    // Load saved state from localStorage
    const savedState = localStorage.getItem(`section_${sectionId}`)
    if (savedState !== null) {
      isExpanded.value = savedState === 'true'
    }
  })

  const toggleSection = () => {
    isExpanded.value = !isExpanded.value
    // Save state to localStorage
    localStorage.setItem(`section_${sectionId}`, isExpanded.value.toString())
  }

  return {
    isExpanded,
    toggleSection
  }
}